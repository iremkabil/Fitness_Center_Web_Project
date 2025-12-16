using System;
using System.Collections.Generic;

namespace WebProje.Helpers
{
    public static class GunHelper
    {
        public static readonly Dictionary<DayOfWeek, string> Gunler = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Monday, "Pazartesi" },
            { DayOfWeek.Tuesday, "Salı" },
            { DayOfWeek.Wednesday, "Çarşamba" },
            { DayOfWeek.Thursday, "Perşembe" },
            { DayOfWeek.Friday, "Cuma" },
            { DayOfWeek.Saturday, "Cumartesi" },
            { DayOfWeek.Sunday, "Pazar" }
        };

        public static string GetTurkceGun(DayOfWeek gun)
        {
            return Gunler.TryGetValue(gun, out var turkceGun) ? turkceGun : gun.ToString();
        }
    }
}
