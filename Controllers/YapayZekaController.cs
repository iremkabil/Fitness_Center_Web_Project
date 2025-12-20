using Fitness_Center_Web_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Fitness_Center_Web_Project.Controllers
{
    public class YapayZekaController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string ModelName = "gemini-2.5-flash-image";
        private const string Endpoint =
            "https://generativelanguage.googleapis.com/v1beta/models/" + ModelName + ":generateContent";

        public YapayZekaController(IConfiguration config, IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _env = env;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new AiOneriModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AiOneriModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Foto zorunlu olsun istiyorsan:
            if (model.Foto == null || model.Foto.Length == 0)
            {
                ModelState.AddModelError("", "Lütfen bir fotoğraf yükleyin.");
                return View(model);
            }

            // Basit güvenlik kontrolleri
            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowed.Contains(model.Foto.ContentType))
            {
                ModelState.AddModelError("", "Sadece JPG/PNG/WEBP yükleyebilirsiniz.");
                return View(model);
            }

            if (model.Foto.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("", "Maksimum 5MB fotoğraf yükleyebilirsiniz.");
                return View(model);
            }

            var apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                model.YapayZekaCevabi = "Gemini API key bulunamadı. User-Secrets/appsettings kontrol edin.";
                return View(model);
            }

            byte[] inputImageBytes;
            using (var ms = new MemoryStream())
            {
                await model.Foto.CopyToAsync(ms);
                inputImageBytes = ms.ToArray();
            }

            var base64Image = Convert.ToBase64String(inputImageBytes);

            // Prompt: hem plan üretmesini hem de “dönüşüm görseli” üretmesini istiyoruz.
            // (Bu görsel temsili olacak; gerçek sonucu garanti etmez.)
            var prompt = $@"
                Kullanıcı bilgileri:
                - Yaş: {model.Yas}
                - Boy: {model.Boy} cm
                - Kilo: {model.Kilo} kg
                - Cinsiyet: {model.Cinsiyet}
                - Hedef: {model.Hedef}

                İSTEK 1 (METİN): 8 haftalık kısa bir plan yaz:
                - Haftalık antrenman programı (gün gün)
                - Basit beslenme önerileri (madde madde)
                - Güvenli uyarılar (sakatlık/sağlık için doktora danış vb.)
                Cevabı Türkçe ver ve çok uzun yazma.

                İSTEK 2 (GÖRSEL): Yüklenen fotoğrafı referans alarak, kişiyi İNSAN GÖRÜNÜMÜ KORUNACAK ŞEKİLDE (aynı kişi hissi),
                tamamen giyinik, gerçekçi bir tarzda, 8 hafta sonunda hedefe uygun “daha fit” temsili bir görsel üret.
                Arka planı mümkünse benzer tut, abartı kas/çarpıtma yapma.
                ";

            // Gemini REST: text + inlineData(image) birlikte gönderilir. :contentReference[oaicite:2]{index=2}
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt },
                            new
                            {
                                inlineData = new
                                {
                                    mimeType = model.Foto.ContentType,
                                    data = base64Image
                                }
                            }
                        }
                    }
                }
                // İstersen burada generationConfig ekleyebilirsin.
                // Örn: safetySettings vs. (dokümana göre)
            };

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await client.PostAsync(Endpoint, httpContent);
                var respText = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    model.YapayZekaCevabi = $"HATA! Durum: {(int)resp.StatusCode} {resp.StatusCode}\nDetay: {respText}";
                    return View(model);
                }

                // Response parse
                using var doc = JsonDocument.Parse(respText);

                // candidates[0].content.parts[*].text / inlineData.data
                var parts = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts");

                string? planText = null;
                byte[]? outImageBytes = null;
                string? outMime = null;

                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var txt))
                    {
                        var t = txt.GetString();
                        if (!string.IsNullOrWhiteSpace(t))
                            planText = (planText == null) ? t : (planText + "\n" + t);
                    }

                    // inlineData: { mimeType, data(base64) }
                    if (part.TryGetProperty("inlineData", out var inline))
                    {
                        outMime = inline.TryGetProperty("mimeType", out var mt) ? mt.GetString() : "image/png";
                        var data = inline.GetProperty("data").GetString();
                        if (!string.IsNullOrWhiteSpace(data))
                            outImageBytes = Convert.FromBase64String(data);
                    }
                }

                model.YapayZekaCevabi = planText ?? "Plan üretilemedi.";

                if (outImageBytes != null)
                {
                    var webFolder = Path.Combine(_env.WebRootPath, "ai");
                    if (!Directory.Exists(webFolder))
                        Directory.CreateDirectory(webFolder);

                    var fileName = $"{Guid.NewGuid():N}.png";
                    var filePath = Path.Combine(webFolder, fileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, outImageBytes);

                    model.DonusumGorselUrl = "/ai/" + fileName;
                }
                else
                {
                    // Bazı yanıtlarda sadece text dönebilir; o durumda bunu göster.
                    model.DonusumGorselUrl = null;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                model.YapayZekaCevabi = "Bağlantı/işlem hatası: " + ex.Message;
                return View(model);
            }
        }
    }
}
