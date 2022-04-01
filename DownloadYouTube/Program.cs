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

            using Process proc = Process.Start(new ProcessStartInfo
            {
                FileName = "yt-dlp.exe",
                Arguments = argument,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            var previousLineLength = 0;
            var addNewline = false;
            var regex = new Regex(@"^(\[download\])\s+[0-9.]+(%)\s+(of)", RegexOptions.Compiled);
            var errors = new List<string>();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
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
            }
            while (!proc.StandardError.EndOfStream)
            {
                string line = proc.StandardError.ReadLine();
                Console.WriteLine(line);
            }

        }
    }
}
