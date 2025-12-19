namespace Fitness_Center_Web_Project.Models
{
    public class AiOneriModel
    {
        // Kullanıcıdan alacağımız bilgiler
        public int Yas { get; set; }
        public int Boy { get; set; }   // cm
        public int Kilo { get; set; }  // kg
        public string Cinsiyet { get; set; } = "Belirtilmemiş";
        public string Hedef { get; set; } = "Kilo Vermek";

        // Yapay zekadan gelen cevabı buraya yazacağız
        public string? YapayZekaCevabi { get; set; }
    }
}