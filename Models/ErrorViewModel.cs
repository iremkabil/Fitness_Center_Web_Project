namespace Fitness_Center_Web_Project.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);

        // Opsiyonel: kullanıcıya daha anlaşılır mesaj göstermek için
        public string? Message { get; set; }

        // Opsiyonel: hata kodu gibi (404, 500 vb.)
        public int? StatusCode { get; set; }
    }
}
