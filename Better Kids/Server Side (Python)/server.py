#!/usr/bin/env python3
import io
import tornado.ioloop
import PIL.Image
import emotion
import guess
import whatis

KEY_NAME = 'Ocp-Apim-Subscription-Key'


def get_img(obj):
    bytes_in = io.BytesIO(obj.request.files['file'][0]['body'])
    img = PIL.Image.open(bytes_in)
    img.thumbnail((512, 512), PIL.Image.ANTIALIAS)

    bytes_out = io.BytesIO()
    img.save(bytes_out, 'JPEG')
    bytes_out.seek(0, io.SEEK_SET)
    data = bytes_out.read()
    open('/tmp/demo.jpg', 'wb').write(data)
    return data


def run_web():
    app = tornado.web.Application([
        (r"/guess", guess.GuessHandler),
        (r"/emotion", emotion.EmotionHandler),
        (r"/whatis", whatis.WhatIsHandler),
    ])
    app.listen(8888)
    tornado.ioloop.IOLoop.current().start()


def main():
    from sys import argv
    {'web': run_web,
     'crawl': guess.crawl_all,
     'trans': lambda: print(whatis.get_translation(argv[2]))}.get(argv[1])()

if __name__ == "__main__":
    main()
