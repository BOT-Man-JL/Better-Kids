import json
import tornado.web
import requests
import server

EMOTION_KEY = 'b1c3c671c6a545459048cf19f0a131ec'
EMOTION_URL = 'https://api.projectoxford.ai/emotion/v1.0/recognize'
EMOTION_THRESHOLD = 0.6


class EmotionHandler(tornado.web.RequestHandler):
    def post(self, *args, **kwargs):
        req = requests.post(EMOTION_URL, data=server.get_img(self),
                            headers={
                                'Content-Type': 'application/octet-stream',
                                server.KEY_NAME: EMOTION_KEY,
                            })
        result = None
        scores = json.loads(req.text)
        if scores:
            best = sorted(scores[0]['scores'].items(), key=lambda x: x[1])[-1]
            if best[1] > EMOTION_THRESHOLD:
                result = best
        if result:
            self.write(json.dumps({
                'success': True,
                'confidence': best[1],
                'emotion': best[0]}))
        else:
            self.write(json.dumps({'success': False}))
        self.set_header('Content-Type', 'application/json')

    def data_received(self, data):
        print(data)
