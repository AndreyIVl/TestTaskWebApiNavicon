using System;
using System.Collections.Generic;
using System.Net;

namespace Client
{
    class Program
    {
        public class ImageResponsJson
        {
            public string Host { set; get; }
            public List<ImageInfo> images { set; get; }
            public ImageResponsJson()
            {
                images = new List<ImageInfo>();
            }
        }
        public class ImageInfo
        {
            public string Alt { set; get; }
            public string Src { set; get; }
            public long Size { set; get; }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Введите url");
            string url = Console.ReadLine();
            Console.WriteLine("Введите количество изображений");
            int imageCount = Int32.Parse(Console.ReadLine());
            string CallImageService = $"https://localhost:44335/Home/api/Image?url={url}&imageCount={imageCount}";
            if (!string.IsNullOrEmpty(url) && imageCount > 0)
            {
                var webClient = new WebClient();
                var content = webClient.DownloadString(CallImageService);

                if (content == null)
                {
                    throw new ArgumentException("Не верный адрес, или сервер недоступен");
                }
                else
                {
                    var trade = System.Text.Json.JsonSerializer.Deserialize<ImageResponsJson>(content);
                    Console.WriteLine($"Хост: {trade.Host}");
                    foreach (var item in trade.images)
                    {
                        Console.WriteLine($"alt:{item.Alt} {Environment.NewLine}" +
                            $"src: {item.Src} {Environment.NewLine}" +
                            $"size: {item.Size} {Environment.NewLine}" +
                            $" {Environment.NewLine} ");
                    }
                }
            }
        }
    }
}
