using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace DownloadYouTube
{
    internal class DownloadRequiredFile
    {

        internal static async Task<bool> DownloadAndUnzipAsync(string url, string filename, string specificFileToUnzipPath = null)
        {
            if (await DownloadAsync(url: url, filename: filename))
            {
                return Unzip(filename, specificFileToUnzipPath: specificFileToUnzipPath);
            }
            return false;
        }

        internal static async Task<bool> DownloadAsync(string url, string filename)
        {
            try
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    throw new InvalidOperationException("Invalid uri!");
                }

                Console.WriteLine($"Downloading {filename}...");
                using var client = new HttpClient();
                var task = client.GetByteArrayAsync(url);
                await task;
                if (task.IsCompletedSuccessfully)
                {
                    Console.WriteLine($"Successully downloaded {filename} from {url}");
                    await File.WriteAllBytesAsync(filename, task.Result);
                    return true;
                }
                else throw new Exception("Did not succesfully download ffmpeg!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                File.Delete(filename);
                return false;
            }
        }

        private static bool Unzip(string filename, string specificFileToUnzipPath = null)
        {
            try
            {
                Console.WriteLine($"Unzipping {filename}...");
                using ZipArchive archive = ZipFile.OpenRead(filename);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (specificFileToUnzipPath != null)
                    {
                        if (entry.FullName.ToLower().EndsWith(specificFileToUnzipPath))
                        {
                            Console.WriteLine("Found file to extract!");
                            entry.ExtractToFile(entry.Name);
                            return true;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Unzipping: {entry.Name}");
                    }

                }
                if (specificFileToUnzipPath != null) return false;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            finally
            {
                Console.WriteLine($"Deleting {filename}");
                File.Delete(filename);
            }
        }
    }
}
