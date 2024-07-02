using System.Security.Cryptography;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using WutheringDownloader.Models;

namespace WutheringDownloader
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length <= 1)
            {
                Console.WriteLine("Args: WutheringDownloader [Resource URL] [Output Folder]");
            }
            else
            {
                string basePath = args[1];

                HttpClient client = new HttpClient(new HttpClientHandler() { AutomaticDecompression = System.Net.DecompressionMethods.All });

                string json = await client.GetStringAsync(args[0]);
                string zip = args[0].Replace("resource.json", "zip");

                ResourceRoot? root = JsonConvert.DeserializeObject<ResourceRoot>(json);
                Console.WriteLine("Json retrieved");

                string namePattern = @"/([^/]+)$";
                string pathPattern = @"^(.*)/[^/]+$";

                foreach (Resource resource in root.resource)
                {
                    Match nameMatch = Regex.Match(resource.dest, namePattern);
                    Match pathMatch = Regex.Match(resource.dest, pathPattern);

                    string path = resource.dest.Replace("/", @"\");
                    string folder = pathMatch.Groups[1].Value.Replace("/", @"\");

                    if (!Directory.Exists(basePath + folder))
                        Directory.CreateDirectory(basePath + folder);

                    if (!File.Exists(basePath + path))
                    {
                        Stream stream = await client.GetStreamAsync(zip + resource.dest);
                        FileStream fileStream = new FileStream(basePath + path, FileMode.OpenOrCreate);

                        Console.WriteLine("Downloading: " + nameMatch.Groups[1].Value);

                        await stream.CopyToAsync(fileStream);

                        await fileStream.DisposeAsync();

                        Thread.Sleep(1000);
                    }
                    Stream checkStream = File.OpenRead(basePath + path);
                    MD5 md5 = MD5.Create();
                    string hash = BitConverter.ToString(md5.ComputeHash(checkStream)).Replace("-", "").ToLower();
                    await checkStream.DisposeAsync();

                    if (hash == resource.md5)
                    {
                        Console.WriteLine("Valid MD5: " + nameMatch.Groups[1].Value);
                    }
                    else
                    {
                        while (hash != resource.md5)
                        {
                            Console.WriteLine($"Invalid MD5: {nameMatch.Groups[1].Value}, Expected: {resource.md5}, Got: {hash}");

                            File.Delete(basePath + path);

                            Stream stream = await client.GetStreamAsync(zip + resource.dest);
                            FileStream fileStream = new FileStream(basePath + path, FileMode.OpenOrCreate);

                            Console.WriteLine("Downloading: " + nameMatch.Groups[1].Value);

                            await stream.CopyToAsync(fileStream);

                            fileStream.Dispose();

                            checkStream = File.OpenRead(basePath + path);
                            hash = BitConverter.ToString(md5.ComputeHash(checkStream)).Replace("-", "").ToLower();
                            await checkStream.DisposeAsync();

                            Thread.Sleep(1000);
                        }
                    }
                }
            }
        }
    }
}
