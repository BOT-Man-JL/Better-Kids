import os
import json
import tornado.web
import requests
import server

BING_KEY = 'd9c59f6186204b4b8c12d4c26e6e84e4'
BING_URL = 'https://bingapis.azure-api.net/api/v5/images/search'
TYPES = ['anger', 'contempt', 'disgust', 'fear', 'happiness', 'neutral',
         'sadness', 'surprise']
NUM_FETCH = 20


class GuessHandler(tornado.web.RequestHandler):
    def get(self, *args, **kwargs):
        pic_type = self.get_argument('type')
        if pic_type not in TYPES:
            print('GuessHandler: Invalid pic type')
            return

        import random
        path = 'guess/' + pic_type + '/' + str(random.randrange(NUM_FETCH))
        self.write(open(path, 'rb').read())
        self.set_header('Cache-Control', 'no-cache')

    def data_received(self, data):
        pass


def crawl(keyword):
    basedir = 'guess/' + keyword + '/'
    os.mkdir(basedir)

    query = keyword + ' face'
    req = requests.get(BING_URL, params={'q': query, 'count': NUM_FETCH},
                       headers={server.KEY_NAME: BING_KEY})
    items = json.loads(req.text)['value']

    i = 0
    for item in items:
        url = item['thumbnailUrl']
        print('Crawl: [%d] %s' % (i, url))
        open(basedir + str(i), 'wb').write(requests.get(url).content)
        i += 1


def crawl_all():
    for keyword in TYPES:
        print('Crawling %s' % keyword)
        crawl(keyword)
