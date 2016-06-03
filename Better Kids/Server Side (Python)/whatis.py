#!/usr/bin/env python3
import json
import tornado.web
import requests
import server

CV_KEY = 'bffac94c45ce4b8ba345446223b2fc6c'
CV_URL = 'https://api.projectoxford.ai/vision/v1.0/describe'

TEXT_KEY = '930485c13ba34038852ed9fddcc375a9'
TEXT_URL = 'https://api.projectoxford.ai/linguistics/v1.0/analyze'
TEXT_UUID = '4fa79af1-f22c-408d-98bb-b7d7aeef7f04'

TRANS_URL = 'http://dict.youdao.com/w/'


def get_description(img):
    req = requests.post(CV_URL, data=img,
                        headers={
                            'Content-Type': 'application/octet-stream',
                            server.KEY_NAME: CV_KEY,
                        })
    cv_reply = json.loads(req.text)
    return cv_reply['description']['captions'][0]['text']


def get_pos(text):
    text_post = {
        'language': 'en',
        'analyzerIds': [TEXT_UUID],
        'text': text}
    req = requests.post(TEXT_URL, data=json.dumps(text_post),
                        headers={'Content-Type': 'application/json',
                                 server.KEY_NAME: TEXT_KEY})
    text_reply = json.loads(req.text)
    return text_reply[0]['result'][0]


def get_translation(text):
    from lxml import etree
    trans = ''
    try:
        req = requests.get(TRANS_URL + text)
        tree = etree.HTML(req.text)
        trans = tree.xpath('.//div[@id="tWebTrans"]//span/text()')[0]
    except:
        pass
    return trans.strip()


class WhatIsHandler(tornado.web.RequestHandler):
    def post(self, *args, **kwargs):
        img = server.get_img(self)
        text = get_description(img)
        pos = get_pos(text)
        word = text.split(' ')
        outputs = list()

        i = 0
        while i < len(pos):
            section = list()
            while i < len(pos):
                if pos[i].startswith('NN'):
                    section.append(word[i])
                else:
                    break
                i += 1
            if section:
                outputs.append(' '.join(section))
            i += 1

        ret = [{'en-US': i, 'zh-CN': get_translation(i)} for i in outputs]
        print(text, ret)
        self.write(json.dumps({'result': ret}))
        self.set_header('Content-Type', 'application/json')

    def data_received(self, data):
        pass
