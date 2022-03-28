using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static async Task Main()
        {
            var urls = new[] {
                "https://www.youtube.com/watch?v=GZ6ltKuGY9Q",
                "https://www.youtube.com/watch?v=FD_-b06JJtE",
                "https://www.youtube.com/watch?v=Huljt0g0Y3Q",
                "https://www.youtube.com/watch?v=-naJR_rhkUM",
                "https://www.youtube.com/watch?v=ByRKF1INOcU",
                "https://www.youtube.com/watch?v=89sOyZdtVA0",
                "https://www.youtube.com/watch?v=2-gQCZL8VA8"
            };

            foreach (var url in urls)
            {
                await DownloadYouTubeVideo(url, ARGUMENT_TYPE.M4A_VIDEO_AND_AUDIO);
            }
        }

        private static async Task DownloadYouTubeVideo(string url, ARGUMENT_TYPE argumentType)
        {
            url = url.Trim();
            if (!url.StartsWith('\"')) url = "\"" + url;
            if (!url.EndsWith('\"')) url = url + "\"";

            var argument = argumentType switch
            {
                ARGUMENT_TYPE.MP3_AUDIO_ONLY => $"-f bestaudio --extract-audio --audio-format mp3 {url}",
                ARGUMENT_TYPE.MP4_AUDIO_ONLY => $"{url} -f bestaudio[ext=m4a]",
                ARGUMENT_TYPE.M4A_VIDEO_AND_AUDIO => $"{url} -f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\"",
                _ => throw new NotImplementedException()
            };

            if (!File.Exists("yt-dlp.exe"))
            {
                try
                {
                    Console.WriteLine("Downloading yt-dlp.exe...");
                    using var client = new WebClient();
                    var task = client.DownloadFileTaskAsync("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe", "yt-dlp.exe");
                    await task;
                    if (task.IsCompletedSuccessfully) Console.WriteLine("yt-dlp downloaded successfully!");
                    else throw new Exception("Did not succesfully download file!");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    File.Delete("yt-dlp.exe");
                    Environment.Exit(0);
                }
            }

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp.exe",
                    Arguments = argument,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            var previousLineLength = 0;
            var addNewline = false;
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.StartsWith("[download]"))
                {
                    Console.Write($"\r{new string(' ', previousLineLength)}");
                    Console.Write($"\r{line}");
                    addNewline = true;
                    previousLineLength = line.Length;
                }
                else
                {
                    if (addNewline && !string.Empty.Equals(line)) Console.WriteLine();
                    Console.WriteLine(line);
                    addNewline = false;
                }
            }
        }

        enum ARGUMENT_TYPE
        {
            MP3_AUDIO_ONLY,
            MP4_AUDIO_ONLY,
            M4A_VIDEO_AND_AUDIO
        }
    }
}
