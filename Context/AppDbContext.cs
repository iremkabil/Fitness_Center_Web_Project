using Microsoft.EntityFrameworkCore;
using Fitness_Center_Web_Project.Models;

namespace Fitness_Center_Web_Project.Context
{ 
    public class AppDbContext : DbContext
    {
        public DbSet<Islem> Islemler { get; set; }
        public DbSet<Mesai> Mesailer { get; set; }
        public DbSet<MesaiGunu> MesaiGunleri { get; set; }
        public DbSet<Personel> Personeller { get; set; }
        public DbSet<PersonelUzmanlik> Uzmanliklar { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Kazanc> Kazanclar { get; set; }
        public DbSet<AiOneri> AiOneriler { get; set; }



        public AppDbContext(DbContextOptions<AppDbContext>options) : base(options)
        {
                
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal alanlar (truncate uyarılarını kaldırır)
            modelBuilder.Entity<Islem>()
                .Property(i => i.Ucret)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Randevu>()
                .Property(r => r.Ucret)
                .HasPrecision(18, 2);

            // Uyarıda geçen property adı "kazanc" (küçük harf)
            modelBuilder.Entity<Kazanc>()
                .Property(k => k.kazanc)
                .HasPrecision(18, 2);

            // Personel <-> Uzmanlik (Many-to-Many)
            modelBuilder.Entity<Personel>()
                .HasMany(p => p.Uzmanliklar)
                .WithMany(u => u.Personeller)
                .UsingEntity(j => j.ToTable("PersonelUzmanliklar"));
        }

    }
}
