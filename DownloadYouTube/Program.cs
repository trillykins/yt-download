using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DownloadYouTube
{
    class Program
    {
        static async Task Main()
        {
            var urls = new[] {
                "https://www.youtube.com/watch?v=GZ6ltKuGY9Q",
                //"https://www.youtube.com/watch?v=FD_-b06JJtE",
                //"https://www.youtube.com/watch?v=Huljt0g0Y3Q",
                //"https://www.youtube.com/watch?v=-naJR_rhkUM",
                //"https://www.youtube.com/watch?v=ByRKF1INOcU",
                //"https://www.youtube.com/watch?v=89sOyZdtVA0",
                //"https://www.youtube.com/watch?v=2-gQCZL8VA8"
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
            if (!url.EndsWith('\"')) url += "\"";

            var argument = argumentType switch
            {
                ARGUMENT_TYPE.MP3_AUDIO_ONLY => $"-f bestaudio --extract-audio --audio-format mp3 {url}",
                ARGUMENT_TYPE.MP4_AUDIO_ONLY => $"{url} -f bestaudio[ext=m4a]",
                ARGUMENT_TYPE.M4A_VIDEO_AND_AUDIO => $"{url} -f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\"",
                _ => throw new NotImplementedException()
            };

            if (!File.Exists("yt-dlp.exe"))
            {
                await DownloadRequiredFile.DownloadAsync(url: "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe", filename: "yt-dlp.exe");
            }

            if (!File.Exists("ffmpeg.exe"))
            {
                await DownloadRequiredFile.DownloadAndUnzipAsync(url: "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip", filename: "ffmpeg-release-essentials.zip", specificFileToUnzipPath: "bin/ffmpeg.exe");
            }

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp.exe",
                    Arguments = argument,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            var previousLineLength = 0;
            var addNewline = false;
            var regex = new Regex(@"^(\[download\])\s+[0-9.]+(%)\s+(of)", RegexOptions.Compiled);
            var errors = new List<string>();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                string errorLine = proc.StandardError.ReadLine();
                if (regex.IsMatch(line))
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
                if (errorLine != null && errorLine != string.Empty)
                {
                    errors.Add(errorLine);
                }
            }
            // Adding errors to the end of the output for readability
            Console.WriteLine(string.Join('\n', errors));
        }
    }
}
