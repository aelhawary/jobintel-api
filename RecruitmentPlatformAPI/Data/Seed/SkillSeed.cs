using RecruitmentPlatformAPI.Models.Reference;

namespace RecruitmentPlatformAPI.Data.Seed
{
    /// <summary>
    /// Seed data for Skills reference table.
    /// Covers ~350 in-demand skills across:
    ///   Software Engineering · AI and Data · Cloud and DevOps · Cybersecurity
    ///   UI/UX and Design · Marketing · Product Management · Business and Strategy
    ///   Sales · Content Creation · Operations · Soft Skills
    /// </summary>
    public static class SkillSeed
    {
        private static readonly DateTime SeedCreatedAt = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static List<Skill> GetSkills()
        {
            return new List<Skill>
            {
                // ─────────────────────────────────────────────
                // SOFTWARE ENGINEERING — Programming Languages
                // ─────────────────────────────────────────────
                new() { Id = 1,  Name = "C#",             CreatedAt = SeedCreatedAt },
                new() { Id = 2,  Name = "JavaScript",     CreatedAt = SeedCreatedAt },
                new() { Id = 3,  Name = "TypeScript",     CreatedAt = SeedCreatedAt },
                new() { Id = 4,  Name = "Python",         CreatedAt = SeedCreatedAt },
                new() { Id = 5,  Name = "Java",           CreatedAt = SeedCreatedAt },
                new() { Id = 6,  Name = "C++",            CreatedAt = SeedCreatedAt },
                new() { Id = 7,  Name = "PHP",            CreatedAt = SeedCreatedAt },
                new() { Id = 8,  Name = "Ruby",           CreatedAt = SeedCreatedAt },
                new() { Id = 9,  Name = "Go",             CreatedAt = SeedCreatedAt },
                new() { Id = 10, Name = "Swift",          CreatedAt = SeedCreatedAt },
                new() { Id = 51, Name = "Kotlin",         CreatedAt = SeedCreatedAt },
                new() { Id = 52, Name = "Rust",           CreatedAt = SeedCreatedAt },
                new() { Id = 53, Name = "Scala",          CreatedAt = SeedCreatedAt },
                new() { Id = 54, Name = "Dart",           CreatedAt = SeedCreatedAt },
                new() { Id = 55, Name = "R",              CreatedAt = SeedCreatedAt },
                new() { Id = 56, Name = "MATLAB",         CreatedAt = SeedCreatedAt },
                new() { Id = 57, Name = "Perl",           CreatedAt = SeedCreatedAt },
                new() { Id = 58, Name = "Lua",            CreatedAt = SeedCreatedAt },
                new() { Id = 59, Name = "Haskell",        CreatedAt = SeedCreatedAt },
                new() { Id = 60, Name = "Solidity",       CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // SOFTWARE ENGINEERING — Frontend
                // ─────────────────────────────────────────────
                new() { Id = 11, Name = "React",          CreatedAt = SeedCreatedAt },
                new() { Id = 12, Name = "Angular",        CreatedAt = SeedCreatedAt },
                new() { Id = 13, Name = "Vue.js",         CreatedAt = SeedCreatedAt },
                new() { Id = 14, Name = "Next.js",        CreatedAt = SeedCreatedAt },
                new() { Id = 15, Name = "HTML/CSS",       CreatedAt = SeedCreatedAt },
                new() { Id = 16, Name = "Tailwind CSS",   CreatedAt = SeedCreatedAt },
                new() { Id = 61, Name = "Svelte",         CreatedAt = SeedCreatedAt },
                new() { Id = 62, Name = "Nuxt.js",        CreatedAt = SeedCreatedAt },
                new() { Id = 63, Name = "Gatsby",         CreatedAt = SeedCreatedAt },
                new() { Id = 64, Name = "Remix",          CreatedAt = SeedCreatedAt },
                new() { Id = 65, Name = "Webpack",        CreatedAt = SeedCreatedAt },
                new() { Id = 66, Name = "Vite",           CreatedAt = SeedCreatedAt },
                new() { Id = 67, Name = "Storybook",      CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // SOFTWARE ENGINEERING — Backend and Frameworks
                // ─────────────────────────────────────────────
                new() { Id = 17, Name = "ASP.NET Core",   CreatedAt = SeedCreatedAt },
                new() { Id = 18, Name = "Node.js",        CreatedAt = SeedCreatedAt },
                new() { Id = 19, Name = "Django",         CreatedAt = SeedCreatedAt },
                new() { Id = 20, Name = "Spring Boot",    CreatedAt = SeedCreatedAt },
                new() { Id = 21, Name = "Express.js",     CreatedAt = SeedCreatedAt },
                new() { Id = 22, Name = "Flask",          CreatedAt = SeedCreatedAt },
                new() { Id = 68, Name = "FastAPI",        CreatedAt = SeedCreatedAt },
                new() { Id = 69, Name = "NestJS",         CreatedAt = SeedCreatedAt },
                new() { Id = 70, Name = "Ruby on Rails",  CreatedAt = SeedCreatedAt },
                new() { Id = 71, Name = "Laravel",        CreatedAt = SeedCreatedAt },
                new() { Id = 72, Name = "Hibernate",      CreatedAt = SeedCreatedAt },
                new() { Id = 73, Name = "SignalR",        CreatedAt = SeedCreatedAt },
                new() { Id = 74, Name = "Blazor",         CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // SOFTWARE ENGINEERING — Databases and Storage
                // ─────────────────────────────────────────────
                new() { Id = 23, Name = "SQL Server",         CreatedAt = SeedCreatedAt },
                new() { Id = 24, Name = "PostgreSQL",         CreatedAt = SeedCreatedAt },
                new() { Id = 25, Name = "MySQL",              CreatedAt = SeedCreatedAt },
                new() { Id = 26, Name = "MongoDB",            CreatedAt = SeedCreatedAt },
                new() { Id = 27, Name = "Redis",              CreatedAt = SeedCreatedAt },
                new() { Id = 28, Name = "Entity Framework",   CreatedAt = SeedCreatedAt },
                new() { Id = 75, Name = "Elasticsearch",      CreatedAt = SeedCreatedAt },
                new() { Id = 76, Name = "Cassandra",          CreatedAt = SeedCreatedAt },
                new() { Id = 77, Name = "Firebase",           CreatedAt = SeedCreatedAt },
                new() { Id = 78, Name = "SQLite",             CreatedAt = SeedCreatedAt },
                new() { Id = 79, Name = "DynamoDB",           CreatedAt = SeedCreatedAt },
                new() { Id = 80, Name = "Prisma",             CreatedAt = SeedCreatedAt },
                new() { Id = 81, Name = "Neo4j",              CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // SOFTWARE ENGINEERING — Architecture and APIs
                // ─────────────────────────────────────────────
                new() { Id = 43, Name = "REST APIs",                   CreatedAt = SeedCreatedAt },
                new() { Id = 44, Name = "GraphQL",                     CreatedAt = SeedCreatedAt },
                new() { Id = 82, Name = "Microservices",               CreatedAt = SeedCreatedAt },
                new() { Id = 83, Name = "gRPC",                        CreatedAt = SeedCreatedAt },
                new() { Id = 84, Name = "WebSockets",                  CreatedAt = SeedCreatedAt },
                new() { Id = 85, Name = "OAuth/JWT",                   CreatedAt = SeedCreatedAt },
                new() { Id = 86, Name = "Event-Driven Architecture",   CreatedAt = SeedCreatedAt },
                new() { Id = 87, Name = "Domain-Driven Design",        CreatedAt = SeedCreatedAt },
                new() { Id = 88, Name = "CQRS",                        CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // SOFTWARE ENGINEERING — Testing and Quality
                // ─────────────────────────────────────────────
                new() { Id = 46, Name = "Unit Testing",   CreatedAt = SeedCreatedAt },
                new() { Id = 89, Name = "Selenium",       CreatedAt = SeedCreatedAt },
                new() { Id = 90, Name = "Playwright",     CreatedAt = SeedCreatedAt },
                new() { Id = 91, Name = "Cypress",        CreatedAt = SeedCreatedAt },
                new() { Id = 92, Name = "Jest",           CreatedAt = SeedCreatedAt },
                new() { Id = 93, Name = "xUnit",          CreatedAt = SeedCreatedAt },
                new() { Id = 94, Name = "Postman",        CreatedAt = SeedCreatedAt },
                new() { Id = 95, Name = "Test Automation",CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // SOFTWARE ENGINEERING — Mobile
                // ─────────────────────────────────────────────
                new() { Id = 36, Name = "React Native",   CreatedAt = SeedCreatedAt },
                new() { Id = 37, Name = "Flutter",        CreatedAt = SeedCreatedAt },
                new() { Id = 38, Name = "Android",        CreatedAt = SeedCreatedAt },
                new() { Id = 39, Name = "iOS",            CreatedAt = SeedCreatedAt },
                new() { Id = 96, Name = "SwiftUI",        CreatedAt = SeedCreatedAt },
                new() { Id = 97, Name = "Jetpack Compose",CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // SOFTWARE ENGINEERING — Tools and General
                // ─────────────────────────────────────────────
                new() { Id = 34, Name = "Git",            CreatedAt = SeedCreatedAt },
                new() { Id = 35, Name = "Linux",          CreatedAt = SeedCreatedAt },
                new() { Id = 45, Name = "Agile/Scrum",    CreatedAt = SeedCreatedAt },
                new() { Id = 47, Name = "Problem Solving",CreatedAt = SeedCreatedAt },
                new() { Id = 98, Name = "Design Patterns",CreatedAt = SeedCreatedAt },
                new() { Id = 99, Name = "Data Structures & Algorithms", CreatedAt = SeedCreatedAt },
                new() { Id = 100, Name = "Embedded Systems",            CreatedAt = SeedCreatedAt },
                new() { Id = 101, Name = "IoT Development",             CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // AI and DATA — Core
                // ─────────────────────────────────────────────
                new() { Id = 40,  Name = "Machine Learning",              CreatedAt = SeedCreatedAt },
                new() { Id = 41,  Name = "Data Analysis",                 CreatedAt = SeedCreatedAt },
                new() { Id = 102, Name = "Deep Learning",                 CreatedAt = SeedCreatedAt },
                new() { Id = 103, Name = "Natural Language Processing",   CreatedAt = SeedCreatedAt },
                new() { Id = 104, Name = "Computer Vision",               CreatedAt = SeedCreatedAt },
                new() { Id = 105, Name = "Data Science",                  CreatedAt = SeedCreatedAt },
                new() { Id = 106, Name = "Statistics",                    CreatedAt = SeedCreatedAt },
                new() { Id = 107, Name = "Feature Engineering",           CreatedAt = SeedCreatedAt },
                new() { Id = 108, Name = "MLOps",                         CreatedAt = SeedCreatedAt },
                new() { Id = 109, Name = "Reinforcement Learning",        CreatedAt = SeedCreatedAt },
                new() { Id = 110, Name = "Time Series Analysis",          CreatedAt = SeedCreatedAt },
                new() { Id = 111, Name = "A/B Testing",                   CreatedAt = SeedCreatedAt },
                new() { Id = 112, Name = "LLM Fine-tuning",               CreatedAt = SeedCreatedAt },
                new() { Id = 113, Name = "Prompt Engineering",            CreatedAt = SeedCreatedAt },
                new() { Id = 114, Name = "Generative AI",                 CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // AI and DATA — Frameworks and Libraries
                // ─────────────────────────────────────────────
                new() { Id = 115, Name = "TensorFlow",    CreatedAt = SeedCreatedAt },
                new() { Id = 116, Name = "PyTorch",       CreatedAt = SeedCreatedAt },
                new() { Id = 117, Name = "Keras",         CreatedAt = SeedCreatedAt },
                new() { Id = 118, Name = "Scikit-learn",  CreatedAt = SeedCreatedAt },
                new() { Id = 119, Name = "Pandas",        CreatedAt = SeedCreatedAt },
                new() { Id = 120, Name = "NumPy",         CreatedAt = SeedCreatedAt },
                new() { Id = 121, Name = "Matplotlib",    CreatedAt = SeedCreatedAt },
                new() { Id = 122, Name = "Hugging Face",  CreatedAt = SeedCreatedAt },
                new() { Id = 123, Name = "LangChain",     CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // AI and DATA — Data Engineering and BI
                // ─────────────────────────────────────────────
                new() { Id = 42,  Name = "Power BI",          CreatedAt = SeedCreatedAt },
                new() { Id = 124, Name = "Data Engineering",  CreatedAt = SeedCreatedAt },
                new() { Id = 125, Name = "Data Warehousing",  CreatedAt = SeedCreatedAt },
                new() { Id = 126, Name = "ETL Pipelines",     CreatedAt = SeedCreatedAt },
                new() { Id = 127, Name = "Apache Spark",      CreatedAt = SeedCreatedAt },
                new() { Id = 128, Name = "Apache Kafka",      CreatedAt = SeedCreatedAt },
                new() { Id = 129, Name = "Apache Airflow",    CreatedAt = SeedCreatedAt },
                new() { Id = 130, Name = "Hadoop",            CreatedAt = SeedCreatedAt },
                new() { Id = 131, Name = "Tableau",           CreatedAt = SeedCreatedAt },
                new() { Id = 132, Name = "Looker",            CreatedAt = SeedCreatedAt },
                new() { Id = 133, Name = "Snowflake",         CreatedAt = SeedCreatedAt },
                new() { Id = 134, Name = "Databricks",        CreatedAt = SeedCreatedAt },
                new() { Id = 135, Name = "dbt",               CreatedAt = SeedCreatedAt },
                new() { Id = 136, Name = "SQL",               CreatedAt = SeedCreatedAt },
                new() { Id = 137, Name = "Big Data",          CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // CLOUD & DEVOPS — Platforms
                // ─────────────────────────────────────────────
                new() { Id = 29,  Name = "AWS",                          CreatedAt = SeedCreatedAt },
                new() { Id = 30,  Name = "Azure",                        CreatedAt = SeedCreatedAt },
                new() { Id = 138, Name = "Google Cloud Platform (GCP)",  CreatedAt = SeedCreatedAt },
                new() { Id = 139, Name = "DigitalOcean",                 CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // CLOUD & DEVOPS — Containers & Orchestration
                // ─────────────────────────────────────────────
                new() { Id = 31,  Name = "Docker",               CreatedAt = SeedCreatedAt },
                new() { Id = 32,  Name = "Kubernetes",           CreatedAt = SeedCreatedAt },
                new() { Id = 140, Name = "Helm",                 CreatedAt = SeedCreatedAt },
                new() { Id = 141, Name = "Service Mesh / Istio", CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // CLOUD & DEVOPS — IaC & Automation
                // ─────────────────────────────────────────────
                new() { Id = 142, Name = "Terraform",                 CreatedAt = SeedCreatedAt },
                new() { Id = 143, Name = "Ansible",                   CreatedAt = SeedCreatedAt },
                new() { Id = 144, Name = "Infrastructure as Code",    CreatedAt = SeedCreatedAt },
                new() { Id = 145, Name = "HashiCorp Vault",           CreatedAt = SeedCreatedAt },
                new() { Id = 146, Name = "AWS CloudFormation",        CreatedAt = SeedCreatedAt },
                new() { Id = 147, Name = "AWS Lambda",                CreatedAt = SeedCreatedAt },
                new() { Id = 148, Name = "Serverless Architecture",   CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // CLOUD & DEVOPS — CI/CD & Monitoring
                // ─────────────────────────────────────────────
                new() { Id = 33,  Name = "CI/CD",            CreatedAt = SeedCreatedAt },
                new() { Id = 149, Name = "Jenkins",          CreatedAt = SeedCreatedAt },
                new() { Id = 150, Name = "GitHub Actions",   CreatedAt = SeedCreatedAt },
                new() { Id = 151, Name = "GitLab CI/CD",     CreatedAt = SeedCreatedAt },
                new() { Id = 152, Name = "ArgoCD",           CreatedAt = SeedCreatedAt },
                new() { Id = 153, Name = "Azure DevOps",     CreatedAt = SeedCreatedAt },
                new() { Id = 154, Name = "Prometheus",       CreatedAt = SeedCreatedAt },
                new() { Id = 155, Name = "Grafana",          CreatedAt = SeedCreatedAt },
                new() { Id = 156, Name = "ELK Stack",        CreatedAt = SeedCreatedAt },
                new() { Id = 157, Name = "Nginx",            CreatedAt = SeedCreatedAt },
                new() { Id = 158, Name = "Site Reliability Engineering (SRE)", CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // CYBERSECURITY
                // ─────────────────────────────────────────────
                new() { Id = 159, Name = "Network Security",          CreatedAt = SeedCreatedAt },
                new() { Id = 160, Name = "Penetration Testing",       CreatedAt = SeedCreatedAt },
                new() { Id = 161, Name = "Ethical Hacking",           CreatedAt = SeedCreatedAt },
                new() { Id = 162, Name = "Vulnerability Assessment",  CreatedAt = SeedCreatedAt },
                new() { Id = 163, Name = "Incident Response",         CreatedAt = SeedCreatedAt },
                new() { Id = 164, Name = "Threat Intelligence",       CreatedAt = SeedCreatedAt },
                new() { Id = 165, Name = "SIEM",                      CreatedAt = SeedCreatedAt },
                new() { Id = 166, Name = "SOC Analysis",              CreatedAt = SeedCreatedAt },
                new() { Id = 167, Name = "Cryptography",              CreatedAt = SeedCreatedAt },
                new() { Id = 168, Name = "Identity & Access Management (IAM)", CreatedAt = SeedCreatedAt },
                new() { Id = 169, Name = "Zero Trust Security",       CreatedAt = SeedCreatedAt },
                new() { Id = 170, Name = "Cloud Security",            CreatedAt = SeedCreatedAt },
                new() { Id = 171, Name = "Application Security",      CreatedAt = SeedCreatedAt },
                new() { Id = 172, Name = "OWASP",                     CreatedAt = SeedCreatedAt },
                new() { Id = 173, Name = "DevSecOps",                 CreatedAt = SeedCreatedAt },
                new() { Id = 174, Name = "Digital Forensics",         CreatedAt = SeedCreatedAt },
                new() { Id = 175, Name = "Malware Analysis",          CreatedAt = SeedCreatedAt },
                new() { Id = 176, Name = "Security Architecture",     CreatedAt = SeedCreatedAt },
                new() { Id = 177, Name = "Firewall Management",       CreatedAt = SeedCreatedAt },
                new() { Id = 178, Name = "Endpoint Security",         CreatedAt = SeedCreatedAt },
                new() { Id = 179, Name = "Risk Management",           CreatedAt = SeedCreatedAt },
                new() { Id = 180, Name = "ISO 27001",                 CreatedAt = SeedCreatedAt },
                new() { Id = 181, Name = "GDPR Compliance",           CreatedAt = SeedCreatedAt },
                new() { Id = 182, Name = "Bug Bounty Hunting",        CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // UI/UX & DESIGN — Tools
                // ─────────────────────────────────────────────
                new() { Id = 50,  Name = "UI/UX Design",       CreatedAt = SeedCreatedAt },
                new() { Id = 183, Name = "Figma",              CreatedAt = SeedCreatedAt },
                new() { Id = 184, Name = "Adobe XD",           CreatedAt = SeedCreatedAt },
                new() { Id = 185, Name = "Sketch",             CreatedAt = SeedCreatedAt },
                new() { Id = 186, Name = "InVision",           CreatedAt = SeedCreatedAt },
                new() { Id = 187, Name = "Adobe Photoshop",    CreatedAt = SeedCreatedAt },
                new() { Id = 188, Name = "Adobe Illustrator",  CreatedAt = SeedCreatedAt },
                new() { Id = 189, Name = "Adobe After Effects",CreatedAt = SeedCreatedAt },
                new() { Id = 190, Name = "Canva",              CreatedAt = SeedCreatedAt },
                new() { Id = 191, Name = "Blender (3D)",       CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // UI/UX & DESIGN — Disciplines
                // ─────────────────────────────────────────────
                new() { Id = 192, Name = "Prototyping",           CreatedAt = SeedCreatedAt },
                new() { Id = 193, Name = "Wireframing",           CreatedAt = SeedCreatedAt },
                new() { Id = 194, Name = "User Research",         CreatedAt = SeedCreatedAt },
                new() { Id = 195, Name = "Usability Testing",     CreatedAt = SeedCreatedAt },
                new() { Id = 196, Name = "Information Architecture", CreatedAt = SeedCreatedAt },
                new() { Id = 197, Name = "Interaction Design",    CreatedAt = SeedCreatedAt },
                new() { Id = 198, Name = "Visual Design",         CreatedAt = SeedCreatedAt },
                new() { Id = 199, Name = "Motion Design",         CreatedAt = SeedCreatedAt },
                new() { Id = 200, Name = "Design Systems",        CreatedAt = SeedCreatedAt },
                new() { Id = 201, Name = "Accessibility (WCAG)",  CreatedAt = SeedCreatedAt },
                new() { Id = 202, Name = "Responsive Design",     CreatedAt = SeedCreatedAt },
                new() { Id = 203, Name = "Brand Identity",        CreatedAt = SeedCreatedAt },
                new() { Id = 204, Name = "Typography",            CreatedAt = SeedCreatedAt },
                new() { Id = 205, Name = "Graphic Design",        CreatedAt = SeedCreatedAt },
                new() { Id = 206, Name = "Video Editing",         CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // MARKETING
                // ─────────────────────────────────────────────
                new() { Id = 207, Name = "Digital Marketing",              CreatedAt = SeedCreatedAt },
                new() { Id = 208, Name = "SEO",                            CreatedAt = SeedCreatedAt },
                new() { Id = 209, Name = "SEM/PPC",                        CreatedAt = SeedCreatedAt },
                new() { Id = 210, Name = "Google Ads",                     CreatedAt = SeedCreatedAt },
                new() { Id = 211, Name = "Facebook Ads",                   CreatedAt = SeedCreatedAt },
                new() { Id = 212, Name = "LinkedIn Ads",                   CreatedAt = SeedCreatedAt },
                new() { Id = 213, Name = "TikTok Ads",                     CreatedAt = SeedCreatedAt },
                new() { Id = 214, Name = "Social Media Marketing",         CreatedAt = SeedCreatedAt },
                new() { Id = 215, Name = "Email Marketing",                CreatedAt = SeedCreatedAt },
                new() { Id = 216, Name = "Content Marketing",              CreatedAt = SeedCreatedAt },
                new() { Id = 217, Name = "Influencer Marketing",           CreatedAt = SeedCreatedAt },
                new() { Id = 218, Name = "Affiliate Marketing",            CreatedAt = SeedCreatedAt },
                new() { Id = 219, Name = "Performance Marketing",          CreatedAt = SeedCreatedAt },
                new() { Id = 220, Name = "Growth Hacking",                 CreatedAt = SeedCreatedAt },
                new() { Id = 221, Name = "Conversion Rate Optimization",   CreatedAt = SeedCreatedAt },
                new() { Id = 222, Name = "Marketing Analytics",            CreatedAt = SeedCreatedAt },
                new() { Id = 223, Name = "Marketing Automation",           CreatedAt = SeedCreatedAt },
                new() { Id = 224, Name = "HubSpot",                        CreatedAt = SeedCreatedAt },
                new() { Id = 225, Name = "Salesforce Marketing Cloud",     CreatedAt = SeedCreatedAt },
                new() { Id = 226, Name = "Brand Strategy",                 CreatedAt = SeedCreatedAt },
                new() { Id = 227, Name = "Market Research",                CreatedAt = SeedCreatedAt },
                new() { Id = 228, Name = "Customer Segmentation",          CreatedAt = SeedCreatedAt },
                new() { Id = 229, Name = "Public Relations",               CreatedAt = SeedCreatedAt },
                new() { Id = 230, Name = "Copywriting",                    CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // PRODUCT MANAGEMENT
                // ─────────────────────────────────────────────
                new() { Id = 49,  Name = "Project Management",        CreatedAt = SeedCreatedAt },
                new() { Id = 231, Name = "Product Strategy",          CreatedAt = SeedCreatedAt },
                new() { Id = 232, Name = "Product Roadmapping",       CreatedAt = SeedCreatedAt },
                new() { Id = 233, Name = "User Story Mapping",        CreatedAt = SeedCreatedAt },
                new() { Id = 234, Name = "OKRs",                      CreatedAt = SeedCreatedAt },
                new() { Id = 235, Name = "Product Analytics",         CreatedAt = SeedCreatedAt },
                new() { Id = 236, Name = "Stakeholder Management",    CreatedAt = SeedCreatedAt },
                new() { Id = 237, Name = "Competitive Analysis",      CreatedAt = SeedCreatedAt },
                new() { Id = 238, Name = "Go-to-Market Strategy",     CreatedAt = SeedCreatedAt },
                new() { Id = 239, Name = "Customer Discovery",        CreatedAt = SeedCreatedAt },
                new() { Id = 240, Name = "Prioritization Frameworks", CreatedAt = SeedCreatedAt },
                new() { Id = 241, Name = "JIRA",                      CreatedAt = SeedCreatedAt },
                new() { Id = 242, Name = "Confluence",                CreatedAt = SeedCreatedAt },
                new() { Id = 243, Name = "Product-Led Growth",        CreatedAt = SeedCreatedAt },
                new() { Id = 244, Name = "Customer Journey Mapping",  CreatedAt = SeedCreatedAt },
                new() { Id = 245, Name = "Sprint Planning",           CreatedAt = SeedCreatedAt },
                new() { Id = 246, Name = "Backlog Management",        CreatedAt = SeedCreatedAt },
                new() { Id = 247, Name = "Feature Flagging",          CreatedAt = SeedCreatedAt },
                new() { Id = 248, Name = "Requirements Gathering",    CreatedAt = SeedCreatedAt },
                new() { Id = 249, Name = "Product Marketing",         CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // BUSINESS & STRATEGY
                // ─────────────────────────────────────────────
                new() { Id = 250, Name = "Business Analysis",         CreatedAt = SeedCreatedAt },
                new() { Id = 251, Name = "Business Development",      CreatedAt = SeedCreatedAt },
                new() { Id = 252, Name = "Strategic Planning",        CreatedAt = SeedCreatedAt },
                new() { Id = 253, Name = "Financial Modeling",        CreatedAt = SeedCreatedAt },
                new() { Id = 254, Name = "Financial Analysis",        CreatedAt = SeedCreatedAt },
                new() { Id = 255, Name = "P&L Management",            CreatedAt = SeedCreatedAt },
                new() { Id = 256, Name = "Budgeting & Forecasting",   CreatedAt = SeedCreatedAt },
                new() { Id = 257, Name = "Change Management",         CreatedAt = SeedCreatedAt },
                new() { Id = 258, Name = "Management Consulting",     CreatedAt = SeedCreatedAt },
                new() { Id = 259, Name = "Process Improvement",       CreatedAt = SeedCreatedAt },
                new() { Id = 260, Name = "Lean Six Sigma",            CreatedAt = SeedCreatedAt },
                new() { Id = 261, Name = "Business Intelligence",     CreatedAt = SeedCreatedAt },
                new() { Id = 262, Name = "Mergers & Acquisitions",    CreatedAt = SeedCreatedAt },
                new() { Id = 263, Name = "Market Analysis",           CreatedAt = SeedCreatedAt },
                new() { Id = 264, Name = "Corporate Strategy",        CreatedAt = SeedCreatedAt },
                new() { Id = 265, Name = "Risk Assessment",           CreatedAt = SeedCreatedAt },
                new() { Id = 266, Name = "Executive Presentations",   CreatedAt = SeedCreatedAt },
                new() { Id = 267, Name = "Venture Capital",           CreatedAt = SeedCreatedAt },
                new() { Id = 268, Name = "Operations Management",     CreatedAt = SeedCreatedAt },
                new() { Id = 269, Name = "KPI Development",           CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // SALES
                // ─────────────────────────────────────────────
                new() { Id = 270, Name = "B2B Sales",                 CreatedAt = SeedCreatedAt },
                new() { Id = 271, Name = "B2C Sales",                 CreatedAt = SeedCreatedAt },
                new() { Id = 272, Name = "Enterprise Sales",          CreatedAt = SeedCreatedAt },
                new() { Id = 273, Name = "SaaS Sales",                CreatedAt = SeedCreatedAt },
                new() { Id = 274, Name = "Account Management",        CreatedAt = SeedCreatedAt },
                new() { Id = 275, Name = "Lead Generation",           CreatedAt = SeedCreatedAt },
                new() { Id = 276, Name = "Pipeline Management",       CreatedAt = SeedCreatedAt },
                new() { Id = 277, Name = "Sales Strategy",            CreatedAt = SeedCreatedAt },
                new() { Id = 278, Name = "Negotiation",               CreatedAt = SeedCreatedAt },
                new() { Id = 279, Name = "Cold Outreach",             CreatedAt = SeedCreatedAt },
                new() { Id = 280, Name = "Sales Enablement",          CreatedAt = SeedCreatedAt },
                new() { Id = 281, Name = "Customer Success",          CreatedAt = SeedCreatedAt },
                new() { Id = 282, Name = "Upselling & Cross-selling", CreatedAt = SeedCreatedAt },
                new() { Id = 283, Name = "Solution Selling",          CreatedAt = SeedCreatedAt },
                new() { Id = 284, Name = "Sales Analytics",           CreatedAt = SeedCreatedAt },
                new() { Id = 285, Name = "Revenue Operations (RevOps)",CreatedAt = SeedCreatedAt },
                new() { Id = 286, Name = "Customer Retention",        CreatedAt = SeedCreatedAt },
                new() { Id = 287, Name = "CRM",                       CreatedAt = SeedCreatedAt },
                new() { Id = 288, Name = "Salesforce CRM",            CreatedAt = SeedCreatedAt },
                new() { Id = 289, Name = "Territory Management",      CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // CONTENT CREATION
                // ─────────────────────────────────────────────
                new() { Id = 290, Name = "Content Writing",           CreatedAt = SeedCreatedAt },
                new() { Id = 291, Name = "Technical Writing",         CreatedAt = SeedCreatedAt },
                new() { Id = 292, Name = "SEO Writing",               CreatedAt = SeedCreatedAt },
                new() { Id = 293, Name = "UX Writing",                CreatedAt = SeedCreatedAt },
                new() { Id = 294, Name = "Ghostwriting",              CreatedAt = SeedCreatedAt },
                new() { Id = 295, Name = "Creative Writing",          CreatedAt = SeedCreatedAt },
                new() { Id = 296, Name = "Scriptwriting",             CreatedAt = SeedCreatedAt },
                new() { Id = 297, Name = "Blogging",                  CreatedAt = SeedCreatedAt },
                new() { Id = 298, Name = "Newsletter Writing",        CreatedAt = SeedCreatedAt },
                new() { Id = 299, Name = "Proofreading & Editing",    CreatedAt = SeedCreatedAt },
                new() { Id = 300, Name = "Storytelling",              CreatedAt = SeedCreatedAt },
                new() { Id = 301, Name = "Content Strategy",          CreatedAt = SeedCreatedAt },
                new() { Id = 302, Name = "Social Media Content",      CreatedAt = SeedCreatedAt },
                new() { Id = 303, Name = "Video Production",          CreatedAt = SeedCreatedAt },
                new() { Id = 304, Name = "Podcast Production",        CreatedAt = SeedCreatedAt },
                new() { Id = 305, Name = "YouTube Content Creation",  CreatedAt = SeedCreatedAt },
                new() { Id = 306, Name = "Translation & Localization",CreatedAt = SeedCreatedAt },
                new() { Id = 307, Name = "Grant Writing",             CreatedAt = SeedCreatedAt },
                new() { Id = 308, Name = "Documentation Writing",     CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // OPERATIONS
                // ─────────────────────────────────────────────
                new() { Id = 309, Name = "Supply Chain Management",   CreatedAt = SeedCreatedAt },
                new() { Id = 310, Name = "Logistics",                 CreatedAt = SeedCreatedAt },
                new() { Id = 311, Name = "Inventory Management",      CreatedAt = SeedCreatedAt },
                new() { Id = 312, Name = "Procurement",               CreatedAt = SeedCreatedAt },
                new() { Id = 313, Name = "Vendor Management",         CreatedAt = SeedCreatedAt },
                new() { Id = 314, Name = "Process Automation",        CreatedAt = SeedCreatedAt },
                new() { Id = 315, Name = "ERP Systems (SAP)",         CreatedAt = SeedCreatedAt },
                new() { Id = 316, Name = "Quality Assurance",         CreatedAt = SeedCreatedAt },
                new() { Id = 317, Name = "Customer Support",          CreatedAt = SeedCreatedAt },
                new() { Id = 318, Name = "HR Operations",             CreatedAt = SeedCreatedAt },
                new() { Id = 319, Name = "Talent Acquisition",        CreatedAt = SeedCreatedAt },
                new() { Id = 320, Name = "Onboarding",                CreatedAt = SeedCreatedAt },
                new() { Id = 321, Name = "Performance Management",    CreatedAt = SeedCreatedAt },
                new() { Id = 322, Name = "Workforce Planning",        CreatedAt = SeedCreatedAt },
                new() { Id = 323, Name = "Contract Management",       CreatedAt = SeedCreatedAt },
                new() { Id = 324, Name = "Event Planning",            CreatedAt = SeedCreatedAt },
                new() { Id = 325, Name = "Compliance Management",     CreatedAt = SeedCreatedAt },
                new() { Id = 326, Name = "Business Process Improvement", CreatedAt = SeedCreatedAt },
                new() { Id = 327, Name = "No-Code/Low-Code Development", CreatedAt = SeedCreatedAt },
                new() { Id = 328, Name = "Power Automate",            CreatedAt = SeedCreatedAt },
                new() { Id = 329, Name = "SharePoint",                CreatedAt = SeedCreatedAt },
                new() { Id = 330, Name = "Microsoft 365",             CreatedAt = SeedCreatedAt },
                new() { Id = 331, Name = "Zapier",                    CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // EMERGING TECH
                // ─────────────────────────────────────────────
                new() { Id = 332, Name = "Blockchain",                CreatedAt = SeedCreatedAt },
                new() { Id = 333, Name = "Web3",                      CreatedAt = SeedCreatedAt },
                new() { Id = 334, Name = "Smart Contracts",           CreatedAt = SeedCreatedAt },
                new() { Id = 335, Name = "AR/VR Development",         CreatedAt = SeedCreatedAt },
                new() { Id = 336, Name = "Game Development",          CreatedAt = SeedCreatedAt },
                new() { Id = 337, Name = "Unity",                     CreatedAt = SeedCreatedAt },
                new() { Id = 338, Name = "Unreal Engine",             CreatedAt = SeedCreatedAt },
                new() { Id = 339, Name = "Quantum Computing",         CreatedAt = SeedCreatedAt },

                // ─────────────────────────────────────────────
                // SOFT SKILLS & CROSS-DOMAIN
                // ─────────────────────────────────────────────
                new() { Id = 48,  Name = "Communication",                      CreatedAt = SeedCreatedAt },
                new() { Id = 340, Name = "Leadership",                         CreatedAt = SeedCreatedAt },
                new() { Id = 341, Name = "Team Management",                    CreatedAt = SeedCreatedAt },
                new() { Id = 342, Name = "Mentoring & Coaching",               CreatedAt = SeedCreatedAt },
                new() { Id = 343, Name = "Critical Thinking",                  CreatedAt = SeedCreatedAt },
                new() { Id = 344, Name = "Time Management",                    CreatedAt = SeedCreatedAt },
                new() { Id = 345, Name = "Emotional Intelligence",             CreatedAt = SeedCreatedAt },
                new() { Id = 346, Name = "Conflict Resolution",                CreatedAt = SeedCreatedAt },
                new() { Id = 347, Name = "Public Speaking",                    CreatedAt = SeedCreatedAt },
                new() { Id = 348, Name = "Research & Analysis",                CreatedAt = SeedCreatedAt },
                new() { Id = 349, Name = "Data-Driven Decision Making",        CreatedAt = SeedCreatedAt },
                new() { Id = 350, Name = "Cross-functional Collaboration",     CreatedAt = SeedCreatedAt },
            };
        }
    }
}
