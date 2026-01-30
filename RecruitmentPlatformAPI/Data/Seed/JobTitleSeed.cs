using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Data.Seed
{
    /// <summary>
    /// Seed data for JobTitle reference table (90 job titles across 8 categories)
    /// </summary>
    public static class JobTitleSeed
    {
        private static readonly DateTime SeedCreatedAt = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static List<JobTitle> GetJobTitles()
        {
            return new List<JobTitle>
            {
                // Technology (40 titles)
                new() { Id = 1, Title = "Backend Developer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Backend, CreatedAt = SeedCreatedAt },
                new() { Id = 2, Title = "Frontend Developer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Frontend, CreatedAt = SeedCreatedAt },
                new() { Id = 3, Title = "Full Stack Developer", Category = "Technology", RoleFamily = JobTitleRoleFamily.FullStack, CreatedAt = SeedCreatedAt },
                new() { Id = 4, Title = "Mobile Developer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Mobile, CreatedAt = SeedCreatedAt },
                new() { Id = 5, Title = "iOS Developer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Mobile, CreatedAt = SeedCreatedAt },
                new() { Id = 6, Title = "Android Developer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Mobile, CreatedAt = SeedCreatedAt },
                new() { Id = 7, Title = "DevOps Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.DevOps, CreatedAt = SeedCreatedAt },
                new() { Id = 8, Title = "Data Scientist", Category = "Technology", RoleFamily = JobTitleRoleFamily.Data, CreatedAt = SeedCreatedAt },
                new() { Id = 9, Title = "Data Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Data, CreatedAt = SeedCreatedAt },
                new() { Id = 10, Title = "Machine Learning Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Data, CreatedAt = SeedCreatedAt },
                new() { Id = 11, Title = "AI Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Data, CreatedAt = SeedCreatedAt },
                new() { Id = 12, Title = "Software Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.FullStack, CreatedAt = SeedCreatedAt },
                new() { Id = 13, Title = "QA Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.QA, CreatedAt = SeedCreatedAt },
                new() { Id = 14, Title = "Test Automation Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.QA, CreatedAt = SeedCreatedAt },
                new() { Id = 15, Title = "Cloud Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.DevOps, CreatedAt = SeedCreatedAt },
                new() { Id = 16, Title = "Security Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.DevOps, CreatedAt = SeedCreatedAt },
                new() { Id = 17, Title = "Cybersecurity Analyst", Category = "Technology", RoleFamily = JobTitleRoleFamily.DevOps, CreatedAt = SeedCreatedAt },
                new() { Id = 18, Title = "Network Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.DevOps, CreatedAt = SeedCreatedAt },
                new() { Id = 19, Title = "Systems Administrator", Category = "Technology", RoleFamily = JobTitleRoleFamily.DevOps, CreatedAt = SeedCreatedAt },
                new() { Id = 20, Title = "Database Administrator", Category = "Technology", RoleFamily = JobTitleRoleFamily.Backend, CreatedAt = SeedCreatedAt },
                new() { Id = 21, Title = "Solutions Architect", Category = "Technology", RoleFamily = JobTitleRoleFamily.FullStack, CreatedAt = SeedCreatedAt },
                new() { Id = 22, Title = "Technical Architect", Category = "Technology", RoleFamily = JobTitleRoleFamily.FullStack, CreatedAt = SeedCreatedAt },
                new() { Id = 23, Title = "Site Reliability Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.DevOps, CreatedAt = SeedCreatedAt },
                new() { Id = 24, Title = "Embedded Systems Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 25, Title = "Game Developer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 26, Title = "Blockchain Developer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Backend, CreatedAt = SeedCreatedAt },
                new() { Id = 27, Title = "IoT Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 28, Title = "Computer Vision Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Data, CreatedAt = SeedCreatedAt },
                new() { Id = 29, Title = "NLP Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Data, CreatedAt = SeedCreatedAt },
                new() { Id = 30, Title = "Business Intelligence Analyst", Category = "Technology", RoleFamily = JobTitleRoleFamily.Data, CreatedAt = SeedCreatedAt },
                new() { Id = 31, Title = "Data Analyst", Category = "Technology", RoleFamily = JobTitleRoleFamily.Data, CreatedAt = SeedCreatedAt },
                new() { Id = 32, Title = "IT Support Specialist", Category = "Technology", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 33, Title = "Technical Support Engineer", Category = "Technology", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 34, Title = "IT Manager", Category = "Technology", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 35, Title = "CTO", Category = "Technology", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 36, Title = "Engineering Manager", Category = "Technology", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 37, Title = "Technical Lead", Category = "Technology", RoleFamily = JobTitleRoleFamily.FullStack, CreatedAt = SeedCreatedAt },
                new() { Id = 38, Title = "Scrum Master", Category = "Technology", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 39, Title = "Product Manager", Category = "Technology", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 40, Title = "Technical Product Manager", Category = "Technology", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },

                // Design (15 titles)
                new() { Id = 41, Title = "UX Designer", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 42, Title = "UI Designer", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 43, Title = "UX/UI Designer", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 44, Title = "Graphic Designer", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 45, Title = "Web Designer", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 46, Title = "Visual Designer", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 47, Title = "Product Designer", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 48, Title = "Interaction Designer", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 49, Title = "Motion Designer", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 50, Title = "3D Designer", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 51, Title = "Game Designer", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 52, Title = "UX Researcher", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 53, Title = "Creative Director", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 54, Title = "Art Director", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },
                new() { Id = 55, Title = "Brand Designer", Category = "Design", RoleFamily = JobTitleRoleFamily.Design, CreatedAt = SeedCreatedAt },

                // Marketing (10 titles)
                new() { Id = 56, Title = "Digital Marketing Specialist", Category = "Marketing", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 57, Title = "SEO Specialist", Category = "Marketing", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 58, Title = "Content Marketing Manager", Category = "Marketing", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 59, Title = "Social Media Manager", Category = "Marketing", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 60, Title = "Marketing Manager", Category = "Marketing", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 61, Title = "Brand Manager", Category = "Marketing", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 62, Title = "Growth Manager", Category = "Marketing", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 63, Title = "Email Marketing Specialist", Category = "Marketing", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 64, Title = "Marketing Analyst", Category = "Marketing", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 65, Title = "Content Writer", Category = "Marketing", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },

                // Sales (5 titles)
                new() { Id = 66, Title = "Sales Representative", Category = "Sales", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 67, Title = "Account Executive", Category = "Sales", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 68, Title = "Sales Manager", Category = "Sales", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 69, Title = "Business Development Manager", Category = "Sales", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 70, Title = "Customer Success Manager", Category = "Sales", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },

                // Finance (5 titles)
                new() { Id = 71, Title = "Accountant", Category = "Finance", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 72, Title = "Financial Analyst", Category = "Finance", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 73, Title = "Finance Manager", Category = "Finance", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 74, Title = "CFO", Category = "Finance", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 75, Title = "Investment Analyst", Category = "Finance", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },

                // HR (5 titles)
                new() { Id = 76, Title = "HR Manager", Category = "Human Resources", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 77, Title = "Recruiter", Category = "Human Resources", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 78, Title = "Talent Acquisition Specialist", Category = "Human Resources", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 79, Title = "HR Business Partner", Category = "Human Resources", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 80, Title = "People Operations Manager", Category = "Human Resources", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },

                // Operations (5 titles)
                new() { Id = 81, Title = "Operations Manager", Category = "Operations", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 82, Title = "Project Manager", Category = "Operations", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 83, Title = "Program Manager", Category = "Operations", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 84, Title = "Supply Chain Manager", Category = "Operations", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 85, Title = "Logistics Coordinator", Category = "Operations", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },

                // Executive (5 titles)
                new() { Id = 86, Title = "CEO", Category = "Executive", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 87, Title = "COO", Category = "Executive", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 88, Title = "VP of Engineering", Category = "Executive", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 89, Title = "VP of Product", Category = "Executive", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt },
                new() { Id = 90, Title = "VP of Sales", Category = "Executive", RoleFamily = JobTitleRoleFamily.Other, CreatedAt = SeedCreatedAt }
            };
        }
    }
}
