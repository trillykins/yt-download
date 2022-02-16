# Run me first!
from __future__ import unicode_literals
import youtube_dl

def youtubeDownload(urls):
    ydl_opts = {
        'format': 'bestaudio/best',
        'postprocessors': [{
            'key': 'FFmpegExtractAudio',
            'preferredcodec': 'mp3',
            'preferredquality': '192',
        }],
    }
    for a in urls:
        print("Beginning:", a)
        with youtube_dl.YoutubeDL(ydl_opts) as ydl:
            ydl.download([a])
        print("Finished:", a)
    print("Finished!")


ydl_opts = {}
with youtube_dl.YoutubeDL(ydl_opts) as ydl:
    ydl.download(['https://www.youtube.com/watch?v=GZ6ltKuGY9Q'])