using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Data.Seed
{
    /// <summary>
    /// Seed data for Country reference table (65 countries with bilingual support)
    /// Arab countries prioritized with SortOrder 1-18
    /// </summary>
    public static class CountrySeed
    {
        private static readonly DateTime SeedCreatedAt = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static List<Country> GetCountries()
        {
            return new List<Country>
            {
                // Middle East & North Africa (prioritized for region)
                new() { Id = 1, IsoCode = "EG", NameEn = "Egypt", NameAr = "مصر", PhoneCode = "+20", SortOrder = 1, CreatedAt = SeedCreatedAt },
                new() { Id = 2, IsoCode = "SA", NameEn = "Saudi Arabia", NameAr = "المملكة العربية السعودية", PhoneCode = "+966", SortOrder = 2, CreatedAt = SeedCreatedAt },
                new() { Id = 3, IsoCode = "AE", NameEn = "United Arab Emirates", NameAr = "الإمارات العربية المتحدة", PhoneCode = "+971", SortOrder = 3, CreatedAt = SeedCreatedAt },
                new() { Id = 4, IsoCode = "KW", NameEn = "Kuwait", NameAr = "الكويت", PhoneCode = "+965", SortOrder = 4, CreatedAt = SeedCreatedAt },
                new() { Id = 5, IsoCode = "QA", NameEn = "Qatar", NameAr = "قطر", PhoneCode = "+974", SortOrder = 5, CreatedAt = SeedCreatedAt },
                new() { Id = 6, IsoCode = "BH", NameEn = "Bahrain", NameAr = "البحرين", PhoneCode = "+973", SortOrder = 6, CreatedAt = SeedCreatedAt },
                new() { Id = 7, IsoCode = "OM", NameEn = "Oman", NameAr = "عمان", PhoneCode = "+968", SortOrder = 7, CreatedAt = SeedCreatedAt },
                new() { Id = 8, IsoCode = "JO", NameEn = "Jordan", NameAr = "الأردن", PhoneCode = "+962", SortOrder = 8, CreatedAt = SeedCreatedAt },
                new() { Id = 9, IsoCode = "LB", NameEn = "Lebanon", NameAr = "لبنان", PhoneCode = "+961", SortOrder = 9, CreatedAt = SeedCreatedAt },
                new() { Id = 10, IsoCode = "IQ", NameEn = "Iraq", NameAr = "العراق", PhoneCode = "+964", SortOrder = 10, CreatedAt = SeedCreatedAt },
                new() { Id = 11, IsoCode = "SY", NameEn = "Syria", NameAr = "سوريا", PhoneCode = "+963", SortOrder = 11, CreatedAt = SeedCreatedAt },
                new() { Id = 12, IsoCode = "YE", NameEn = "Yemen", NameAr = "اليمن", PhoneCode = "+967", SortOrder = 12, CreatedAt = SeedCreatedAt },
                new() { Id = 13, IsoCode = "PS", NameEn = "Palestine", NameAr = "فلسطين", PhoneCode = "+970", SortOrder = 13, CreatedAt = SeedCreatedAt },
                new() { Id = 14, IsoCode = "LY", NameEn = "Libya", NameAr = "ليبيا", PhoneCode = "+218", SortOrder = 14, CreatedAt = SeedCreatedAt },
                new() { Id = 15, IsoCode = "TN", NameEn = "Tunisia", NameAr = "تونس", PhoneCode = "+216", SortOrder = 15, CreatedAt = SeedCreatedAt },
                new() { Id = 16, IsoCode = "DZ", NameEn = "Algeria", NameAr = "الجزائر", PhoneCode = "+213", SortOrder = 16, CreatedAt = SeedCreatedAt },
                new() { Id = 17, IsoCode = "MA", NameEn = "Morocco", NameAr = "المغرب", PhoneCode = "+212", SortOrder = 17, CreatedAt = SeedCreatedAt },
                new() { Id = 18, IsoCode = "SD", NameEn = "Sudan", NameAr = "السودان", PhoneCode = "+249", SortOrder = 18, CreatedAt = SeedCreatedAt },

                // Major Global Countries
                new() { Id = 19, IsoCode = "US", NameEn = "United States", NameAr = "الولايات المتحدة", PhoneCode = "+1", SortOrder = 100, CreatedAt = SeedCreatedAt },
                new() { Id = 20, IsoCode = "GB", NameEn = "United Kingdom", NameAr = "المملكة المتحدة", PhoneCode = "+44", SortOrder = 101, CreatedAt = SeedCreatedAt },
                new() { Id = 21, IsoCode = "CA", NameEn = "Canada", NameAr = "كندا", PhoneCode = "+1", SortOrder = 102, CreatedAt = SeedCreatedAt },
                new() { Id = 22, IsoCode = "AU", NameEn = "Australia", NameAr = "أستراليا", PhoneCode = "+61", SortOrder = 103, CreatedAt = SeedCreatedAt },
                new() { Id = 23, IsoCode = "DE", NameEn = "Germany", NameAr = "ألمانيا", PhoneCode = "+49", SortOrder = 104, CreatedAt = SeedCreatedAt },
                new() { Id = 24, IsoCode = "FR", NameEn = "France", NameAr = "فرنسا", PhoneCode = "+33", SortOrder = 105, CreatedAt = SeedCreatedAt },
                new() { Id = 25, IsoCode = "IT", NameEn = "Italy", NameAr = "إيطاليا", PhoneCode = "+39", SortOrder = 106, CreatedAt = SeedCreatedAt },
                new() { Id = 26, IsoCode = "ES", NameEn = "Spain", NameAr = "إسبانيا", PhoneCode = "+34", SortOrder = 107, CreatedAt = SeedCreatedAt },
                new() { Id = 27, IsoCode = "NL", NameEn = "Netherlands", NameAr = "هولندا", PhoneCode = "+31", SortOrder = 108, CreatedAt = SeedCreatedAt },
                new() { Id = 28, IsoCode = "SE", NameEn = "Sweden", NameAr = "السويد", PhoneCode = "+46", SortOrder = 109, CreatedAt = SeedCreatedAt },
                new() { Id = 29, IsoCode = "NO", NameEn = "Norway", NameAr = "النرويج", PhoneCode = "+47", SortOrder = 110, CreatedAt = SeedCreatedAt },
                new() { Id = 30, IsoCode = "CH", NameEn = "Switzerland", NameAr = "سويسرا", PhoneCode = "+41", SortOrder = 111, CreatedAt = SeedCreatedAt },

                // Asia
                new() { Id = 31, IsoCode = "CN", NameEn = "China", NameAr = "الصين", PhoneCode = "+86", SortOrder = 200, CreatedAt = SeedCreatedAt },
                new() { Id = 32, IsoCode = "JP", NameEn = "Japan", NameAr = "اليابان", PhoneCode = "+81", SortOrder = 201, CreatedAt = SeedCreatedAt },
                new() { Id = 33, IsoCode = "IN", NameEn = "India", NameAr = "الهند", PhoneCode = "+91", SortOrder = 202, CreatedAt = SeedCreatedAt },
                new() { Id = 34, IsoCode = "KR", NameEn = "South Korea", NameAr = "كوريا الجنوبية", PhoneCode = "+82", SortOrder = 203, CreatedAt = SeedCreatedAt },
                new() { Id = 35, IsoCode = "SG", NameEn = "Singapore", NameAr = "سنغافورة", PhoneCode = "+65", SortOrder = 204, CreatedAt = SeedCreatedAt },
                new() { Id = 36, IsoCode = "MY", NameEn = "Malaysia", NameAr = "ماليزيا", PhoneCode = "+60", SortOrder = 205, CreatedAt = SeedCreatedAt },
                new() { Id = 37, IsoCode = "TH", NameEn = "Thailand", NameAr = "تايلاند", PhoneCode = "+66", SortOrder = 206, CreatedAt = SeedCreatedAt },
                new() { Id = 38, IsoCode = "PH", NameEn = "Philippines", NameAr = "الفلبين", PhoneCode = "+63", SortOrder = 207, CreatedAt = SeedCreatedAt },
                new() { Id = 39, IsoCode = "ID", NameEn = "Indonesia", NameAr = "إندونيسيا", PhoneCode = "+62", SortOrder = 208, CreatedAt = SeedCreatedAt },
                new() { Id = 40, IsoCode = "VN", NameEn = "Vietnam", NameAr = "فيتنام", PhoneCode = "+84", SortOrder = 209, CreatedAt = SeedCreatedAt },
                new() { Id = 41, IsoCode = "PK", NameEn = "Pakistan", NameAr = "باكستان", PhoneCode = "+92", SortOrder = 210, CreatedAt = SeedCreatedAt },
                new() { Id = 42, IsoCode = "BD", NameEn = "Bangladesh", NameAr = "بنغلاديش", PhoneCode = "+880", SortOrder = 211, CreatedAt = SeedCreatedAt },
                new() { Id = 43, IsoCode = "TR", NameEn = "Turkey", NameAr = "تركيا", PhoneCode = "+90", SortOrder = 212, CreatedAt = SeedCreatedAt },
                new() { Id = 44, IsoCode = "IR", NameEn = "Iran", NameAr = "إيران", PhoneCode = "+98", SortOrder = 214, CreatedAt = SeedCreatedAt },

                // Africa
                new() { Id = 45, IsoCode = "ZA", NameEn = "South Africa", NameAr = "جنوب أفريقيا", PhoneCode = "+27", SortOrder = 300, CreatedAt = SeedCreatedAt },
                new() { Id = 46, IsoCode = "NG", NameEn = "Nigeria", NameAr = "نيجيريا", PhoneCode = "+234", SortOrder = 301, CreatedAt = SeedCreatedAt },
                new() { Id = 47, IsoCode = "KE", NameEn = "Kenya", NameAr = "كينيا", PhoneCode = "+254", SortOrder = 302, CreatedAt = SeedCreatedAt },
                new() { Id = 48, IsoCode = "ET", NameEn = "Ethiopia", NameAr = "إثيوبيا", PhoneCode = "+251", SortOrder = 303, CreatedAt = SeedCreatedAt },
                new() { Id = 49, IsoCode = "GH", NameEn = "Ghana", NameAr = "غانا", PhoneCode = "+233", SortOrder = 304, CreatedAt = SeedCreatedAt },

                // Americas
                new() { Id = 50, IsoCode = "BR", NameEn = "Brazil", NameAr = "البرازيل", PhoneCode = "+55", SortOrder = 400, CreatedAt = SeedCreatedAt },
                new() { Id = 51, IsoCode = "MX", NameEn = "Mexico", NameAr = "المكسيك", PhoneCode = "+52", SortOrder = 401, CreatedAt = SeedCreatedAt },
                new() { Id = 52, IsoCode = "AR", NameEn = "Argentina", NameAr = "الأرجنتين", PhoneCode = "+54", SortOrder = 402, CreatedAt = SeedCreatedAt },
                new() { Id = 53, IsoCode = "CL", NameEn = "Chile", NameAr = "تشيلي", PhoneCode = "+56", SortOrder = 403, CreatedAt = SeedCreatedAt },
                new() { Id = 54, IsoCode = "CO", NameEn = "Colombia", NameAr = "كولومبيا", PhoneCode = "+57", SortOrder = 404, CreatedAt = SeedCreatedAt },

                // Europe (Additional)
                new() { Id = 55, IsoCode = "PL", NameEn = "Poland", NameAr = "بولندا", PhoneCode = "+48", SortOrder = 500, CreatedAt = SeedCreatedAt },
                new() { Id = 56, IsoCode = "RO", NameEn = "Romania", NameAr = "رومانيا", PhoneCode = "+40", SortOrder = 501, CreatedAt = SeedCreatedAt },
                new() { Id = 57, IsoCode = "GR", NameEn = "Greece", NameAr = "اليونان", PhoneCode = "+30", SortOrder = 502, CreatedAt = SeedCreatedAt },
                new() { Id = 58, IsoCode = "PT", NameEn = "Portugal", NameAr = "البرتغال", PhoneCode = "+351", SortOrder = 503, CreatedAt = SeedCreatedAt },
                new() { Id = 59, IsoCode = "BE", NameEn = "Belgium", NameAr = "بلجيكا", PhoneCode = "+32", SortOrder = 504, CreatedAt = SeedCreatedAt },
                new() { Id = 60, IsoCode = "AT", NameEn = "Austria", NameAr = "النمسا", PhoneCode = "+43", SortOrder = 505, CreatedAt = SeedCreatedAt },
                new() { Id = 61, IsoCode = "DK", NameEn = "Denmark", NameAr = "الدنمارك", PhoneCode = "+45", SortOrder = 506, CreatedAt = SeedCreatedAt },
                new() { Id = 62, IsoCode = "FI", NameEn = "Finland", NameAr = "فنلندا", PhoneCode = "+358", SortOrder = 507, CreatedAt = SeedCreatedAt },
                new() { Id = 63, IsoCode = "IE", NameEn = "Ireland", NameAr = "أيرلندا", PhoneCode = "+353", SortOrder = 508, CreatedAt = SeedCreatedAt },
                new() { Id = 64, IsoCode = "NZ", NameEn = "New Zealand", NameAr = "نيوزيلندا", PhoneCode = "+64", SortOrder = 509, CreatedAt = SeedCreatedAt },
                new() { Id = 65, IsoCode = "RU", NameEn = "Russia", NameAr = "روسيا", PhoneCode = "+7", SortOrder = 510, CreatedAt = SeedCreatedAt }
            };
        }
    }
}
