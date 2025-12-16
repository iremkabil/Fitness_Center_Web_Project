using Microsoft.AspNetCore.Mvc;
using WebProje.Models;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;


namespace WebProje.Controllers
{
    public class AiOneriController : Controller
    {
        // Ana sayfa (Saç modeli denemesi formu)
        public IActionResult Index()
        {
            var model = new AiOneri();
            return View(model);
        }

        // Saç modeli işlemi
        [HttpPost]
        public async Task<IActionResult> ProcessHairStyle(AiOneri model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                // Fotoğrafı Base64 formatına çevirme
                string base64Image;
                using (var stream = new MemoryStream())
                {
                    await model.Photo!.CopyToAsync(stream);
                    base64Image = Convert.ToBase64String(stream.ToArray());
                }

                // API'ye istek gönderme
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://hairstyle-changer.p.rapidapi.com/huoshan/facebody/hairstyle"),
                    Headers =
                    {
                        { "x-rapidapi-key", "91af12c3e4msh784b40d16a5ae4dp12a5b7jsnfa367660c4fc" },
                        { "x-rapidapi-host", "hairstyle-changer.p.rapidapi.com" },
                    },
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "image", base64Image },
                        { "style", model.HairStyle?.ToString()! } // Saç tipi numarası
                    })
                };

                // API yanıtını al
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Yanıtı JSON olarak ayrıştır
                    var jsonResponse = JObject.Parse(responseBody);
                    var base64Result = jsonResponse["data"]?["image"]?.ToString();

                    if (!string.IsNullOrEmpty(base64Result))
                    {
                        ViewBag.ResultImage = base64Result;
                    }
                    else
                    {
                        ModelState.AddModelError("", "API yanıtında görsel bulunamadı.");
                    }
                }

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
                return View("Index", model);
            }
        }
    
    }
}
