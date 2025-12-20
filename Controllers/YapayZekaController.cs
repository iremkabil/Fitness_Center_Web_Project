using Fitness_Center_Web_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;

namespace Fitness_Center_Web_Project.Controllers
{
    public class YapayZekaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public YapayZekaController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // SENİN ÇALIŞAN ANAHTARIN
        private const string ApiKey = "AIzaSyBbxy5x_pi2mpNqxwcbH3BAu3DZqg8a_RE";

        [HttpGet]
        public IActionResult Index()
        {
            return View(new AiOneriModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(AiOneriModel model)
        {
            string prompt = $"Ben {model.Yas} yaşında, {model.Boy} cm, {model.Kilo} kg, {model.Cinsiyet} biriyim. Hedefim: {model.Hedef}. Bana kısa bir beslenme tüyosu ve 3 tane evde yapılacak egzersiz öner. Türkçe cevap ver.";

            using (var httpClient = new HttpClient())
            {
                try
                {
                    // 1. ADIM: Modelleri listele
                    var listModelsUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={ApiKey}";
                    var listResponse = await httpClient.GetAsync(listModelsUrl);

                    string modelName = "";

                    if (listResponse.IsSuccessStatusCode)
                    {
                        var listString = await listResponse.Content.ReadAsStringAsync();
                        var modelList = JsonSerializer.Deserialize<ModelListRoot>(listString);

                        // ÖNCE KARARLI MODELLERİ ARA (Sırasıyla bunları kontrol et)
                        var kararliModeller = new[] { "models/gemini-1.5-flash", "models/gemini-1.5-flash-001", "models/gemini-pro" };

                        var bulunanModel = modelList?.Models?
                            .FirstOrDefault(m => kararliModeller.Contains(m.Name));

                        if (bulunanModel != null)
                        {
                            modelName = bulunanModel.Name; // Kararlı model bulundu!
                        }
                        else
                        {
                            // Kararlıları bulamazsa herhangi bir Gemini modelini al (Son çare)
                            modelName = modelList?.Models?
                                .FirstOrDefault(m => m.Name.Contains("gemini") && m.SupportedGenerationMethods.Contains("generateContent"))?.Name
                                ?? "models/gemini-pro";
                        }
                    }
                    else
                    {
                        // Liste alamazsak manuel olarak bunu dene
                        modelName = "models/gemini-1.5-flash";
                    }

                    // 2. ADIM: Seçilen sağlam model ile isteği gönder
                    var generateUrl = $"https://generativelanguage.googleapis.com/v1beta/{modelName}:generateContent?key={ApiKey}";

                    var requestBody = new
                    {
                        contents = new[] { new { parts = new[] { new { text = prompt } } } }
                    };

                    var jsonContent = JsonSerializer.Serialize(requestBody);
                    var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(generateUrl, httpContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        var geminiResponse = JsonSerializer.Deserialize<GeminiResponseRoot>(responseString);
                        model.YapayZekaCevabi = geminiResponse?.Candidates?[0]?.Content?.Parts?[0]?.Text;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        model.YapayZekaCevabi = $"HATA! \nSeçilen Model: {modelName} \nDurum: {response.StatusCode} \nDetay: {errorContent}";
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

    // --- JSON Sınıfları ---
    public class ModelListRoot { [JsonPropertyName("models")] public List<ModelInfo>? Models { get; set; } }
    public class ModelInfo { [JsonPropertyName("name")] public string Name { get; set; } = ""; [JsonPropertyName("supportedGenerationMethods")] public List<string> SupportedGenerationMethods { get; set; } = new(); }
    public class GeminiResponseRoot { [JsonPropertyName("candidates")] public List<Candidate>? Candidates { get; set; } }
    public class Candidate { [JsonPropertyName("content")] public Content? Content { get; set; } }
    public class Content { [JsonPropertyName("parts")] public List<Part>? Parts { get; set; } }
    public class Part { [JsonPropertyName("text")] public string? Text { get; set; } }
}