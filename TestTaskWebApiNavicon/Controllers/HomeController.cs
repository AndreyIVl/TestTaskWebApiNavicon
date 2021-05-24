using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TestTaskWebApiNavicon.Models;

namespace TestTaskWebApiNavicon.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger _logger;
        private static readonly HttpClient client = new HttpClient();
        public HomeController(IWebHostEnvironment webHostEnvironment, ILogger<HomeController> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }
        [HttpGet]
        [Route("api/Image")]
        public async Task<string> GetImages(string url, int imageCount)
        {
            if (!string.IsNullOrEmpty(url) && imageCount > 0)
            {
                //Cписок ссылок на изображения из src.
                List<string> ImgeUrl = new List<string>();
                List<Task> tasks = new List<Task>();
                ImageResponsJson responsJson = new ImageResponsJson();
                responsJson.Host = url;

                WebRequest reqest = HttpWebRequest.Create(url);
                reqest.Method = "GET";

                int maxCurrentTasks = Environment.ProcessorCount * 2;

                try
                {
                    using (StreamReader reader = new StreamReader(reqest.GetResponse().GetResponseStream()))
                    {
                        //Код html страницы
                        string source = reader.ReadToEnd();

                        var parser = new HtmlParser();
                        var document = parser.ParseDocument(source);
                        int i = 0;
                        foreach (var element in document.QuerySelectorAll("img"))
                        {
                            //Добавление url изображений
                            ImgeUrl.Add(element.GetAttribute("src"));

                            responsJson.images.Add(new ImageInfo()
                            {
                                Alt = element.GetAttribute("alt"),
                                Src = element.GetAttribute("src"),
                                Size = HttpWebRequest.Create(element.GetAttribute("src")).GetResponse().ContentLength
                            });

                            i++;
                            if (i >= imageCount)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    _logger.LogError(ex.Message);
                }

                using (SemaphoreSlim semaphore = new SemaphoreSlim(maxCurrentTasks))
                {
                    foreach (var imageUrl in ImgeUrl)
                    {
                        await semaphore.WaitAsync();
                        tasks.Add(DownloadAndSaveFileAsync(imageUrl, semaphore));
                    }
                    try
                    {
                        await Task.WhenAll(tasks);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex.Message);
                    }
                }
                string json = JsonSerializer.Serialize(responsJson);
                return json;
            }
            else throw new ArgumentException("Не валидные параметры");
            
        }
        private async Task DownloadAndSaveFileAsync(string ImageUrl, SemaphoreSlim semaphore)
        {
            string rootPath = _webHostEnvironment.WebRootPath + @"\Images\";
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(ImageUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string filePath = Path.Combine(rootPath, Path.GetFileName(ImageUrl));
                        using (Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        using (FileStream fileStream = System.IO.File.Create(filePath))
                        {
                            await responseStream.CopyToAsync(fileStream).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogDebug(ex.Message);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
