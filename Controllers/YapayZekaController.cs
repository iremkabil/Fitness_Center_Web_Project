using Fitness_Center_Web_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fitness_Center_Web_Project.Controllers
{
    public class YapayZekaController : Controller
    {
        private const string ApiKey = "AIzaSyAzLGOHnInP31O5K2VHxIV2GCGA0jEq-lk";
        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=";

        [HttpGet]
        public IActionResult Index()
        {
            return View(new AiOneriModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(AiOneriModel model)
        {
            string prompt = $"Ben {model.Yas} yaşında, {model.Boy} cm boyunda, {model.Kilo} kg ağırlığında bir {model.Cinsiyet} bireyim. " +
                            $"Hedefim: {model.Hedef}. " +
                            $"Bana fitness uzmanı gibi davran. Maddeler halinde günlük kısa bir beslenme tüyosu ve evde yapabileceğim 3 tane egzersiz hareketini (set sayılarıyla) öner. Cevabı Türkçe ver.";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.PostAsync(ApiUrl + ApiKey, httpContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        var geminiResponse = JsonSerializer.Deserialize<GeminiResponseRoot>(responseString);
                        string gelenCevap = geminiResponse?.Candidates?[0]?.Content?.Parts?[0]?.Text;
                        model.YapayZekaCevabi = gelenCevap;
                    }
                    else
                    {
                        model.YapayZekaCevabi = "Hata oluştu. Google sunucusu cevap vermedi. Kod: " + response.StatusCode;
                    }
                }
                catch (Exception ex)
                {
                    model.YapayZekaCevabi = "Bağlantı hatası: " + ex.Message;
                }
            }

            return View(model);
        }
    }

    public class GeminiResponseRoot
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public List<Part>? Parts { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}