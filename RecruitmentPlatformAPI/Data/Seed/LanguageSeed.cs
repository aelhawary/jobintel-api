using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Data.Seed
{
    /// <summary>
    /// Seed data for Language reference table (50 languages with bilingual support)
    /// Arabic and English prioritized with SortOrder 1-2
    /// </summary>
    public static class LanguageSeed
    {
        private static readonly DateTime SeedCreatedAt = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static List<Language> GetLanguages()
        {
            return new List<Language>
            {
                // Top Priority - Arabic & English (Most Important)
                new() { Id = 1, IsoCode = "ara", NameEn = "Arabic", NameAr = "العربية", SortOrder = 1, CreatedAt = SeedCreatedAt },
                new() { Id = 2, IsoCode = "eng", NameEn = "English", NameAr = "الإنجليزية", SortOrder = 2, CreatedAt = SeedCreatedAt },

                // Middle East & North Africa Languages
                new() { Id = 3, IsoCode = "tur", NameEn = "Turkish", NameAr = "التركية", SortOrder = 10, CreatedAt = SeedCreatedAt },
                new() { Id = 4, IsoCode = "fas", NameEn = "Persian (Farsi)", NameAr = "الفارسية", SortOrder = 11, CreatedAt = SeedCreatedAt },
                new() { Id = 5, IsoCode = "heb", NameEn = "Hebrew", NameAr = "العبرية", SortOrder = 12, CreatedAt = SeedCreatedAt },
                new() { Id = 6, IsoCode = "kur", NameEn = "Kurdish", NameAr = "الكردية", SortOrder = 13, CreatedAt = SeedCreatedAt },
                new() { Id = 7, IsoCode = "urd", NameEn = "Urdu", NameAr = "الأردية", SortOrder = 14, CreatedAt = SeedCreatedAt },

                // Major European Languages
                new() { Id = 8, IsoCode = "fra", NameEn = "French", NameAr = "الفرنسية", SortOrder = 20, CreatedAt = SeedCreatedAt },
                new() { Id = 9, IsoCode = "deu", NameEn = "German", NameAr = "الألمانية", SortOrder = 21, CreatedAt = SeedCreatedAt },
                new() { Id = 10, IsoCode = "spa", NameEn = "Spanish", NameAr = "الإسبانية", SortOrder = 22, CreatedAt = SeedCreatedAt },
                new() { Id = 11, IsoCode = "ita", NameEn = "Italian", NameAr = "الإيطالية", SortOrder = 23, CreatedAt = SeedCreatedAt },
                new() { Id = 12, IsoCode = "por", NameEn = "Portuguese", NameAr = "البرتغالية", SortOrder = 24, CreatedAt = SeedCreatedAt },
                new() { Id = 13, IsoCode = "rus", NameEn = "Russian", NameAr = "الروسية", SortOrder = 25, CreatedAt = SeedCreatedAt },
                new() { Id = 14, IsoCode = "nld", NameEn = "Dutch", NameAr = "الهولندية", SortOrder = 26, CreatedAt = SeedCreatedAt },
                new() { Id = 15, IsoCode = "pol", NameEn = "Polish", NameAr = "البولندية", SortOrder = 27, CreatedAt = SeedCreatedAt },
                new() { Id = 16, IsoCode = "ukr", NameEn = "Ukrainian", NameAr = "الأوكرانية", SortOrder = 28, CreatedAt = SeedCreatedAt },
                new() { Id = 17, IsoCode = "swe", NameEn = "Swedish", NameAr = "السويدية", SortOrder = 29, CreatedAt = SeedCreatedAt },
                new() { Id = 18, IsoCode = "nor", NameEn = "Norwegian", NameAr = "النرويجية", SortOrder = 30, CreatedAt = SeedCreatedAt },
                new() { Id = 19, IsoCode = "dan", NameEn = "Danish", NameAr = "الدنماركية", SortOrder = 31, CreatedAt = SeedCreatedAt },
                new() { Id = 20, IsoCode = "fin", NameEn = "Finnish", NameAr = "الفنلندية", SortOrder = 32, CreatedAt = SeedCreatedAt },
                new() { Id = 21, IsoCode = "ell", NameEn = "Greek", NameAr = "اليونانية", SortOrder = 33, CreatedAt = SeedCreatedAt },

                // Major Asian Languages
                new() { Id = 22, IsoCode = "zho", NameEn = "Chinese (Mandarin)", NameAr = "الصينية (الماندرين)", SortOrder = 40, CreatedAt = SeedCreatedAt },
                new() { Id = 23, IsoCode = "jpn", NameEn = "Japanese", NameAr = "اليابانية", SortOrder = 41, CreatedAt = SeedCreatedAt },
                new() { Id = 24, IsoCode = "kor", NameEn = "Korean", NameAr = "الكورية", SortOrder = 42, CreatedAt = SeedCreatedAt },
                new() { Id = 25, IsoCode = "hin", NameEn = "Hindi", NameAr = "الهندية", SortOrder = 43, CreatedAt = SeedCreatedAt },
                new() { Id = 26, IsoCode = "ben", NameEn = "Bengali", NameAr = "البنغالية", SortOrder = 44, CreatedAt = SeedCreatedAt },
                new() { Id = 27, IsoCode = "vie", NameEn = "Vietnamese", NameAr = "الفيتنامية", SortOrder = 45, CreatedAt = SeedCreatedAt },
                new() { Id = 28, IsoCode = "tha", NameEn = "Thai", NameAr = "التايلاندية", SortOrder = 46, CreatedAt = SeedCreatedAt },
                new() { Id = 29, IsoCode = "ind", NameEn = "Indonesian", NameAr = "الإندونيسية", SortOrder = 47, CreatedAt = SeedCreatedAt },
                new() { Id = 30, IsoCode = "msa", NameEn = "Malay", NameAr = "الماليزية", SortOrder = 48, CreatedAt = SeedCreatedAt },
                new() { Id = 31, IsoCode = "fil", NameEn = "Filipino (Tagalog)", NameAr = "الفلبينية (تاغالوغ)", SortOrder = 49, CreatedAt = SeedCreatedAt },

                // African Languages
                new() { Id = 32, IsoCode = "swa", NameEn = "Swahili", NameAr = "السواحيلية", SortOrder = 50, CreatedAt = SeedCreatedAt },
                new() { Id = 33, IsoCode = "amh", NameEn = "Amharic", NameAr = "الأمهرية", SortOrder = 51, CreatedAt = SeedCreatedAt },
                new() { Id = 34, IsoCode = "hau", NameEn = "Hausa", NameAr = "الهوسا", SortOrder = 52, CreatedAt = SeedCreatedAt },
                new() { Id = 35, IsoCode = "yor", NameEn = "Yoruba", NameAr = "اليوروبا", SortOrder = 53, CreatedAt = SeedCreatedAt },
                new() { Id = 36, IsoCode = "zul", NameEn = "Zulu", NameAr = "الزولو", SortOrder = 54, CreatedAt = SeedCreatedAt },

                // Other Widely Spoken Languages
                new() { Id = 37, IsoCode = "ron", NameEn = "Romanian", NameAr = "الرومانية", SortOrder = 60, CreatedAt = SeedCreatedAt },
                new() { Id = 38, IsoCode = "ces", NameEn = "Czech", NameAr = "التشيكية", SortOrder = 61, CreatedAt = SeedCreatedAt },
                new() { Id = 39, IsoCode = "hun", NameEn = "Hungarian", NameAr = "المجرية", SortOrder = 62, CreatedAt = SeedCreatedAt },
                new() { Id = 40, IsoCode = "bul", NameEn = "Bulgarian", NameAr = "البلغارية", SortOrder = 63, CreatedAt = SeedCreatedAt },
                new() { Id = 41, IsoCode = "hrv", NameEn = "Croatian", NameAr = "الكرواتية", SortOrder = 64, CreatedAt = SeedCreatedAt },
                new() { Id = 42, IsoCode = "srp", NameEn = "Serbian", NameAr = "الصربية", SortOrder = 65, CreatedAt = SeedCreatedAt },
                new() { Id = 43, IsoCode = "slk", NameEn = "Slovak", NameAr = "السلوفاكية", SortOrder = 66, CreatedAt = SeedCreatedAt },
                new() { Id = 44, IsoCode = "slv", NameEn = "Slovenian", NameAr = "السلوفينية", SortOrder = 67, CreatedAt = SeedCreatedAt },
                new() { Id = 45, IsoCode = "lit", NameEn = "Lithuanian", NameAr = "الليتوانية", SortOrder = 68, CreatedAt = SeedCreatedAt },
                new() { Id = 46, IsoCode = "lav", NameEn = "Latvian", NameAr = "اللاتفية", SortOrder = 69, CreatedAt = SeedCreatedAt },
                new() { Id = 47, IsoCode = "est", NameEn = "Estonian", NameAr = "الإستونية", SortOrder = 70, CreatedAt = SeedCreatedAt },
                new() { Id = 48, IsoCode = "cat", NameEn = "Catalan", NameAr = "الكتالونية", SortOrder = 71, CreatedAt = SeedCreatedAt },
                new() { Id = 49, IsoCode = "glg", NameEn = "Galician", NameAr = "الجاليكية", SortOrder = 72, CreatedAt = SeedCreatedAt },
                new() { Id = 50, IsoCode = "eus", NameEn = "Basque", NameAr = "الباسكية", SortOrder = 73, CreatedAt = SeedCreatedAt }
            };
        }
    }
}
