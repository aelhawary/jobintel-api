using Bogus;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Identity;
using RecruitmentPlatformAPI.Models.Jobs;
using RecruitmentPlatformAPI.Models.JobSeeker;
using RecruitmentPlatformAPI.Models.Recruiter;
using RecruitmentPlatformAPI.Models.Reference;
using RecruitmentPlatformAPI.Models.Assessment;

namespace RecruitmentPlatformAPI.Data.Seed
{
    public static class MockDataSeeder
    {
        private const int JobSeekerCount = 45;
        private const int RecruiterCount = 5;
        private const int JobCount = 18;
        private const int BogusSeed = 42;
        private const string MockPassword = "Mock@123";

        // ── 8 tech job titles from DB: Backend(1), Frontend(2), FullStack(3), Mobile(4), DevOps(7), DataScientist(8), MLEngineer(10), AIEngineer(11)
        private static readonly int[] TechJobTitleIds = [1, 2, 3, 4, 7, 8, 10, 11];
        private static readonly string[] TechJobTitleNames = ["Backend Developer", "Frontend Developer", "Full Stack Developer", "Mobile Developer", "DevOps Engineer", "Data Scientist", "Machine Learning Engineer", "AI Engineer"];

        private static readonly Dictionary<int, int[]> SkillPools = new()
        {
            [1] = [1, 4, 17, 18, 19, 20, 21, 23, 24, 25, 26, 27, 28, 43, 46, 68, 69, 73, 82, 83, 84, 85, 88, 93, 98],
            [2] = [3, 11, 12, 13, 14, 15, 16, 61, 62, 65, 66, 67, 91, 92],
            [3] = [1, 2, 3, 4, 11, 12, 13, 14, 15, 16, 17, 18, 21, 23, 24, 25, 28, 34, 43, 44, 46, 82, 85, 92, 98],
            [4] = [10, 36, 37, 38, 39, 51, 54, 96, 97],
            [7] = [9, 29, 30, 31, 32, 33, 34, 35, 138, 139, 140, 142, 143, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 173],
            [8] = [4, 40, 41, 42, 55, 56, 106, 107, 110, 111, 115, 116, 117, 118, 119, 120, 121, 124, 125, 126, 127, 128, 131, 132, 133, 134, 136, 137],
            [10] = [4, 29, 30, 31, 32, 40, 102, 103, 104, 108, 112, 115, 116, 117, 118, 119, 120, 122, 123, 124, 127, 128, 129, 134],
            [11] = [4, 29, 30, 31, 34, 40, 43, 82, 102, 103, 104, 106, 107, 108, 112, 113, 114, 115, 116, 122, 123, 148],
        };

        private static readonly string[][] BioTemplates =
        [
            ["Passionate Backend Developer with {exp} years of experience building scalable microservices in {tech}. Skilled in designing RESTful APIs, optimizing database performance, and leading cross-functional engineering teams to deliver high-impact solutions."],
            ["Senior Backend Engineer with {exp}+ years crafting distributed systems using {tech}. Focused on clean architecture, API-first design, and mentoring junior developers. Proven track record of reducing infrastructure costs and improving system reliability."],
            ["Backend Developer with strong expertise in {tech}. I love solving complex problems — from building real-time data pipelines to designing fault-tolerant microservices. {exp} years of delivering production-grade software."],
            ["Results-driven Backend Developer with {exp} years of experience in {tech}. Led the redesign of a core payment system handling $50M+ annually. Passionate about performance optimization and code quality."],
            ["Backend engineer specializing in {tech} with {exp} years of experience. Built and maintained systems serving 1M+ users. Advocate for automated testing, CI/CD, and domain-driven design."],
            ["Dedicated Backend Developer with {exp} years building high-throughput APIs and event-driven architectures in {tech}. Strong background in SQL optimization, caching strategies, and cloud-native development."],
            ["Frontend Developer with {exp} years of experience crafting responsive, accessible web applications using {tech}. Expert in component architecture, state management, and modern CSS. Passionate about delivering pixel-perfect UI."],
            ["Creative Frontend Developer specializing in {tech}. {exp}+ years turning complex designs into performant, user-friendly interfaces. Experienced in building design systems and optimizing Core Web Vitals."],
            ["Frontend Engineer with {exp} years building production React applications using {tech}. Strong focus on accessibility, testability, and developer experience. Led the migration of a legacy jQuery app to modern React."],
            ["UI-focused Frontend Developer with {exp} years in {tech}. I bridge design and engineering — prototyping in Figma and implementing with clean, maintainable code. Built interfaces used by 500K+ daily active users."],
            ["Frontend Developer passionate about performance and UX. {exp} years of experience with {tech}. Spearheaded a frontend rewrite that reduced bundle size by 60% and improved Lighthouse scores from 45 to 95."],
            ["Senior Frontend Developer with {exp} years mastering {tech}. Built and maintained component libraries used across 5 product teams. Advocate for TypeScript strict mode, automated visual regression testing, and inclusive design."],
            ["Full Stack Developer with {exp} years building end-to-end applications using {tech}. Experienced in both frontend architecture and backend API design. Delivered 10+ production applications from concept to deployment."],
            ["Versatile Full Stack Engineer proficient in {tech}. {exp} years of experience spanning React SPAs, Node.js APIs, and cloud infrastructure. Passionate about developer productivity and clean, well-documented code."],
            ["Full Stack Developer with {exp}+ years designing and building scalable web applications. Stack includes {tech}. Led the development of a SaaS platform serving 200+ enterprise clients."],
            ["Full Stack Developer specializing in {tech}. {exp} years of experience across the entire stack — from database schema design to responsive UI. Strong advocate for automated testing and DevOps best practices."],
            ["Full Stack Engineer with {exp} years delivering production applications. Tech stack: {tech}. Built a real-time collaboration platform handling 10K concurrent users. Focused on system reliability and team velocity."],
            ["Experienced Full Stack Developer with {exp} years. Worked with {tech} to build everything from internal tools to customer-facing platforms. Enjoy mentoring, code reviews, and driving engineering excellence."],
            ["Mobile Developer with {exp} years building cross-platform and native applications using {tech}. Published 8 apps on App Store and Google Play with 100K+ combined downloads. Focused on performance and smooth UX."],
            ["Senior Mobile Developer specializing in {tech}. {exp} years of experience delivering polished mobile experiences. Built a ride-sharing app serving 50K+ active users across 3 countries."],
            ["Mobile Engineer with {exp} years using {tech}. Expertise in state management, offline-first architecture, and platform-specific UI/UX patterns. Maintained a top-rated fintech app with 4.8 star rating."],
            ["Mobile Developer passionate about creating intuitive mobile experiences. {exp} years with {tech}. Led the mobile team that shipped a social networking app reaching 500K downloads in the first quarter."],
            ["iOS and Android developer with {exp} years of experience using {tech}. Built enterprise mobile solutions for logistics and healthcare sectors. Strong focus on accessibility, security, and performance optimization."],
            ["DevOps Engineer with {exp} years of experience managing cloud infrastructure using {tech}. Designed and implemented CI/CD pipelines reducing deployment time from 2 hours to 12 minutes."],
            ["Site Reliability Engineer specializing in {tech}. {exp} years ensuring 99.99% uptime for production systems. Built monitoring dashboards, automated incident response, and led migration to Kubernetes."],
            ["DevOps Engineer with {exp} years using {tech}. Infrastructure-as-Code advocate who reduced manual operations by 80%. Managed multi-cloud deployments serving 2M+ users."],
            ["Cloud Infrastructure Engineer with {exp} years of experience in {tech}. Designed a zero-downtime deployment strategy for a microservices ecosystem. Certified in AWS and Azure."],
            ["Platform Engineer focused on developer experience and infrastructure reliability. {exp} years with {tech}. Built an internal developer platform used by 15 engineering teams."],
            ["Data Scientist with {exp} years extracting insights from complex datasets using {tech}. Built ML models that improved customer retention by 25%. Experienced in full data pipeline from ingestion to visualization."],
            ["Data Scientist with strong background in statistics and machine learning. {exp} years using {tech} to solve business problems — from demand forecasting to fraud detection. PhD in Computer Science."],
            ["Senior Data Scientist specializing in {tech}. {exp} years experience building predictive models and recommendation systems. Deployed ML pipelines processing 10TB+ of data daily."],
            ["Data Scientist passionate about using data to drive product decisions. {exp} years with {tech}. Designed A/B testing frameworks and built dashboards used by executive leadership."],
            ["Analytics-focused Data Scientist with {exp} years in {tech}. Converted raw data into actionable insights for marketing, product, and operations teams. Skilled in communicating complex findings to non-technical stakeholders."],
            ["Data Scientist with {exp} years experience deploying production ML models. Expert in {tech}. Built a real-time fraud detection system that saved $2M annually."],
            ["Machine Learning Engineer with {exp} years building and deploying production ML systems using {tech}. Experienced in MLOps, model monitoring, and scalable training pipelines."],
            ["ML Engineer specializing in deep learning and natural language processing. {exp} years with {tech}. Built a multilingual sentiment analysis system processing 1M+ documents daily."],
            ["Machine Learning Engineer with {exp} years deploying models at scale. Expertise in {tech} including distributed training, model quantization, and A/B testing frameworks."],
            ["ML Engineer passionate about bridging research and production. {exp} years of experience in {tech}. Published papers at NeurIPS and ICML. Built recommendation systems for e-commerce platforms."],
            ["Applied Machine Learning Engineer with {exp} years using {tech}. Developed computer vision solutions for autonomous navigation and quality inspection. Reduced defect detection time by 90%."],
            ["Machine Learning Engineer focused on NLP and LLMs. {exp} years experience with {tech}. Fine-tuned and deployed large language models for enterprise document analysis."],
            ["AI Engineer with {exp} years building intelligent systems using {tech}. Specializing in LLM fine-tuning, RAG pipelines, and prompt engineering. Deployed AI solutions serving 100K+ users."],
            ["AI Engineer passionate about applied AI. {exp} years experience in {tech}. Built a generative AI platform for automated content creation, reducing content production time by 70%."],
            ["Senior AI Engineer with expertise in {tech}. {exp} years designing and deploying machine learning systems at scale. Led the development of an AI-powered recommendation engine that increased revenue by 35%."],
            ["AI Engineer focused on bridging LLMs with production systems. {exp} years using {tech}. Built agentic workflows and RAG architectures for enterprise knowledge management."],
            ["AI Engineer experienced in full ML lifecycle. {exp} years with {tech}. From data collection to model deployment and monitoring. Built real-time NLP pipelines for customer support automation."],
        ];

        private static readonly string[][] ExperienceResponsibilities =
        [
            // Backend (index 0)
            [
                "Designed and implemented RESTful APIs using ASP.NET Core, handling 50K+ daily requests with 99.9% uptime.",
                "Led the migration of a legacy monolith to a microservices architecture, reducing deployment time by 60%.",
                "Optimized SQL Server queries and introduced Redis caching, reducing API response times by 40%.",
                "Built an event-driven notification system using Kafka and SignalR serving 100K+ concurrent users.",
                "Implemented CI/CD pipelines with GitHub Actions and Azure DevOps, automating testing and deployment.",
                "Designed database schemas for a multi-tenant SaaS platform supporting 500+ enterprise clients.",
                "Developed a real-time data processing pipeline processing 1M+ events per hour.",
                "Refactored critical code paths improving system throughput by 3x under peak load.",
                "Mentored 5 junior developers through code reviews, pair programming, and technical design sessions.",
                "Architected a role-based access control system with OAuth 2.0 and JWT authentication.",
            ],
            // Frontend (index 1)
            [
                "Built a responsive React dashboard used by 10K+ daily active users with material UI and Tailwind CSS.",
                "Led the frontend architecture for a SaaS platform, establishing component patterns and state management conventions.",
                "Reduced bundle size by 55% through code splitting, lazy loading, and tree shaking.",
                "Implemented a design system with Storybook, ensuring consistent UI across 5 product teams.",
                "Migrated a legacy jQuery application to React, improving page load times by 70%.",
                "Developed real-time data visualization components using D3.js and Chart.js.",
                "Improved Core Web Vitals scores from poor to excellent, boosting organic search traffic by 30%.",
                "Built an accessible component library meeting WCAG 2.1 AA standards.",
                "Implemented end-to-end testing with Cypress, achieving 95% test coverage on critical user flows.",
                "Optimized rendering performance for large data tables handling 100K+ rows with virtual scrolling.",
            ],
            // FullStack (index 2)
            [
                "Built a full-stack e-commerce platform with React frontend and ASP.NET Core backend, serving 200K+ monthly users.",
                "Designed and implemented a real-time collaboration feature using WebSockets and React state management.",
                "Led development of a customer portal from concept to production, handling the full stack.",
                "Implemented CI/CD and automated testing for both frontend and backend codebases.",
                "Built RESTful APIs with Node.js and Express, consumed by a React SPA with server-side rendering.",
                "Designed database schema, wrote complex SQL queries, and built the corresponding admin dashboard.",
                "Developed a multi-tenant architecture supporting white-labeling for 50+ enterprise clients.",
                "Reduced infrastructure costs by 40% through serverless migration and resource optimization.",
                "Built a micro-frontend architecture allowing 3 independent teams to deploy independently.",
                "Integrated third-party payment gateways, SSO providers, and analytics platforms.",
            ],
            // Mobile (index 3)
            [
                "Developed a cross-platform mobile app using Flutter and Dart, achieving 4.8 star rating on both stores.",
                "Built native iOS and Android features for a fintech app serving 100K+ users.",
                "Implemented offline-first architecture with local database synchronization and conflict resolution.",
                "Optimized app startup time by 60% through lazy initialization and code splitting.",
                "Integrated push notifications, deep linking, and biometric authentication.",
                "Led the migration of a native codebase to React Native, reducing development time by 40%.",
                "Built custom UI components and animations for a social media platform.",
                "Implemented A/B testing framework for mobile features, driving 15% user engagement improvement.",
                "Designed and built a mobile CI/CD pipeline with automated testing and beta distribution.",
                "Developed AR features using ARKit and ARCore for a retail shopping app.",
            ],
            // DevOps (index 4)
            [
                "Designed and managed Kubernetes clusters handling 500+ microservices across multiple environments.",
                "Built GitOps workflows with ArgoCD, reducing deployment failures by 80%.",
                "Implemented comprehensive monitoring with Prometheus, Grafana, and ELK stack for 200+ services.",
                "Led migration from on-premise to AWS, reducing infrastructure costs by 45%.",
                "Automated infrastructure provisioning using Terraform and Ansible across 3 cloud providers.",
                "Designed disaster recovery strategy achieving RPO of 5 minutes and RTO of 30 minutes.",
                "Built internal developer platform with self-service CI/CD, reducing onboarding time from 2 weeks to 2 days.",
                "Implemented security scanning and compliance automation in CI/CD pipelines.",
                "Optimized container images reducing average size by 70% and improving deployment speed.",
                "Managed multi-cloud networking, VPNs, and service mesh (Istio) for hybrid infrastructure.",
            ],
            // Data Scientist (index 5)
            [
                "Built machine learning models for customer churn prediction, reducing churn by 22%.",
                "Developed a real-time fraud detection system processing 10K transactions per second.",
                "Designed and deployed A/B testing framework used by 5 product teams.",
                "Created interactive dashboards in Tableau and Power BI for executive reporting.",
                "Built ETL pipelines processing 10TB+ of data daily using Apache Spark and Airflow.",
                "Developed recommendation systems that increased cross-sell revenue by 18%.",
                "Performed deep-dive analysis on user behavior, identifying key drivers of retention and engagement.",
                "Built time series forecasting models for inventory and demand planning.",
                "Led data migration to Snowflake data warehouse, improving query performance by 10x.",
                "Collaborated with product teams to define metrics, funnels, and experimentation frameworks.",
            ],
            // ML Engineer (index 6)
            [
                "Designed and deployed production ML pipelines using TensorFlow and PyTorch, serving 1M+ predictions daily.",
                "Built a real-time NLP system for sentiment analysis and entity extraction.",
                "Developed computer vision models for automated quality inspection, reducing defect escape rate by 95%.",
                "Implemented MLOps practices including model versioning, monitoring, and automated retraining.",
                "Fine-tuned large language models for domain-specific question answering.",
                "Built feature engineering pipelines using Apache Spark, processing 100+ features at scale.",
                "Deployed models to production using Docker and Kubernetes with automated canary deployments.",
                "Optimized model inference latency from 500ms to 15ms using quantization and ONNX runtime.",
                "Led research and development of novel approaches for document understanding using transformers.",
                "Built and maintained a model registry and experimentation platform for the ML team.",
            ],
            // AI Engineer (index 7)
            [
                "Built RAG pipelines combining vector databases (Pinecone) with LLMs for enterprise knowledge retrieval.",
                "Developed a generative AI platform for automated report generation, saving 200+ engineer hours monthly.",
                "Fine-tuned open-source LLMs using LoRA and QLoRA for domain-specific applications.",
                "Implemented agentic workflows using LangChain and AutoGPT for business process automation.",
                "Designed and deployed prompt engineering frameworks used across 3 product teams.",
                "Built real-time AI-powered customer support chatbot handling 5K+ conversations daily.",
                "Developed multi-modal AI systems combining text, image, and audio processing.",
                "Created evaluation frameworks for LLM outputs including factuality, safety, and bias testing.",
                "Led integration of AI features into existing products, improving user engagement by 40%.",
                "Built model serving infrastructure with GPU optimization and auto-scaling.",
            ],
        ];

        private static readonly string[][] ProjectTemplates =
        [
            // Backend projects
            [
                "Event-Driven Order Processing Pipeline",
                "Real-Time Analytics Dashboard Backend",
                "Microservices API Gateway",
                "Distributed Task Scheduler",
                "Multi-Tenant SaaS Backend Platform",
            ],
            // Frontend projects
            [
                "Interactive Data Visualization Dashboard",
                "Design System Component Library",
                "Real-Time Collaboration Whiteboard",
                "E-Commerce Storefront with SSR",
                "Accessible Admin Panel Framework",
            ],
            // FullStack projects
            [
                "Full-Stack Project Management Tool",
                "Real-Time Chat Application",
                "SaaS Customer Portal",
                "Content Management System",
                "Marketplace Platform with Payment Integration",
            ],
            // Mobile projects
            [
                "Cross-Platform Fitness Tracking App",
                "Food Delivery Mobile Application",
                "Social Media Content Creator App",
                "Mobile Banking Application",
                "Real-Time Ride Sharing Platform",
            ],
            // DevOps projects
            [
                "GitOps Deployment Pipeline",
                "Infrastructure Monitoring Platform",
                "Multi-Cloud Resource Orchestrator",
                "Container Security Scanner",
                "Automated Disaster Recovery System",
            ],
            // Data Science projects
            [
                "Customer Churn Prediction Engine",
                "Real-Time Fraud Detection System",
                "Product Recommendation Platform",
                "Sales Forecasting Dashboard",
                "User Segmentation Pipeline",
            ],
            // ML Engineering projects
            [
                "Production ML Model Serving Platform",
                "Real-Time Object Detection System",
                "NLP Document Classification Pipeline",
                "ML Feature Store Platform",
                "Automated Model Retraining Pipeline",
            ],
            // AI Engineering projects
            [
                "LLM-Powered Knowledge Retrieval System",
                "Generative Content Creation Platform",
                "Multi-Agent Workflow Orchestrator",
                "AI Customer Support Automation",
                "RAG Pipeline for Enterprise Search",
            ],
        ];

        private static readonly string[][] ProjectDescriptions =
        [
            [
                "Built a high-throughput event processing system using Kafka and .NET, handling 1M+ events daily with exactly-once delivery guarantees.",
                "Designed and implemented a real-time analytics backend using ClickHouse and Redis, supporting sub-second queries on 100M+ event records.",
                "Developed an API gateway handling authentication, rate limiting, and routing for 20+ microservices using Ocelot and ASP.NET Core.",
                "Created a distributed task scheduler with cron-like expressions, retry logic, and dead-letter queues using SQL Server and Hangfire.",
                "Architected a multi-tenant SaaS backend with isolated databases, tenant-aware caching, and usage-based billing integration.",
            ],
            [
                "An interactive dashboard built with React and D3.js, visualizing real-time data streams for 10K+ concurrent users with dynamic filtering.",
                "A comprehensive design system with 50+ reusable components built in React with Storybook documentation and automated visual regression testing.",
                "Real-time collaborative whiteboard with WebSocket-based sync, supporting 100+ simultaneous users with operational transform for conflict resolution.",
                "High-performance e-commerce storefront with Next.js, featuring ISR, streaming SSR, and edge caching for sub-100ms page loads.",
                "An accessible admin panel built with React and TypeScript, achieving WCAG 2.1 AA compliance and supporting screen readers and keyboard navigation.",
            ],
            [
                "A full-stack project management tool with React frontend, ASP.NET Core API, and real-time updates via SignalR for team collaboration.",
                "Real-time chat application supporting group conversations, file sharing, and message search using WebSockets and MongoDB for message storage.",
                "A customer portal with self-service capabilities, built with React and .NET, serving 500+ enterprise clients with role-based access control.",
                "Headless CMS with React admin panel and public API, supporting markdown editing, media management, and version history.",
                "A marketplace platform with Stripe payment integration, seller dashboards, and real-time order tracking using event-driven architecture.",
            ],
            [
                "Cross-platform fitness tracker built with Flutter, featuring workout logging, progress charts, and social challenges with 50K+ users.",
                "Food delivery app with real-time driver tracking, push notifications, and in-app payments using React Native and Firebase.",
                "Content creator app for short-form video editing and sharing, built with Swift and Kotlin, reaching 100K+ downloads.",
                "Mobile banking app with biometric authentication, real-time transaction monitoring, and budgeting tools using Flutter and secure APIs.",
                "Ride sharing platform with live location tracking, fare estimation, and driver matching algorithm using MapKit and Google Maps SDK.",
            ],
            [
                "Complete GitOps pipeline using ArgoCD and GitHub Actions, enabling automated deployments across 5 environments with rollback capabilities.",
                "Infrastructure monitoring platform built with Prometheus, Grafana, and custom exporters, providing real-time visibility into 200+ services.",
                "Multi-cloud resource orchestrator using Terraform and Pulumi, managing infrastructure across AWS, Azure, and GCP from a single codebase.",
                "Container vulnerability scanner integrated into CI/CD pipeline, scanning 500+ images daily with automated remediation workflows.",
                "Automated disaster recovery system with cross-region replication, failover testing, and RTO monitoring for critical production services.",
            ],
            [
                "Customer churn prediction engine using gradient boosting, achieving 85% accuracy and reducing churn by 22% through targeted interventions.",
                "Real-time fraud detection system processing 10K transactions/second using streaming ML models with automatic model retraining.",
                "Product recommendation platform using collaborative filtering and matrix factorization, increasing cross-sell revenue by 18%.",
                "Interactive sales forecasting dashboard combining time series models with external market data for quarterly revenue predictions.",
                "User segmentation pipeline using K-means clustering and PCA, enabling personalized marketing campaigns across 50+ segments.",
            ],
            [
                "Production ML serving platform with model versioning, canary deployments, and automated rollbacks using Kubernetes and TensorFlow Serving.",
                "Real-time object detection system using YOLOv8 and NVIDIA Triton Inference Server, processing 30 FPS video streams.",
                "Document classification pipeline using fine-tuned BERT models, categorizing 50K+ documents daily with 94% accuracy.",
                "ML feature store enabling feature sharing and reuse across teams, with online and offline serving for training and inference.",
                "Automated model retraining pipeline triggered by data drift detection, with performance monitoring and alerting.",
            ],
            [
                "Enterprise knowledge retrieval system using RAG architecture with Pinecone vector database and fine-tuned LLMs for document Q&A.",
                "Generative content platform using GPT models with custom prompts and safety filters, producing marketing copy and product descriptions.",
                "Multi-agent orchestration system where specialized AI agents collaborate on complex business workflows using LangChain.",
                "AI customer support system combining intent classification, retrieval-augmented generation, and sentiment analysis.",
                "Enterprise search RAG pipeline ingesting 100K+ documents with hybrid search combining dense and sparse embeddings.",
            ],
        ];

        private static readonly string[] RecruiterCompanies =
        [
            "NexGen AI Solutions",
            "CloudBase Technologies",
            "PixelCraft Digital",
            "InnoVault Systems",
            "StackForge Engineering",
        ];

        private static readonly string[] CompanyDescriptions =
        [
            "Next-generation AI platform building enterprise-grade machine learning solutions for Fortune 500 companies. Specializing in NLP, computer vision, and predictive analytics.",
            "Cloud infrastructure company providing multi-cloud management, DevOps consulting, and platform engineering services to fast-growing startups and enterprises.",
            "Digital product agency crafting beautiful, performant web and mobile applications. We help startups and enterprises design and build user-centric digital experiences.",
            "Enterprise software company building scalable backend systems, data platforms, and AI-powered solutions for the financial services and healthcare industries.",
            "Full-stack development consultancy specializing in modern web applications, microservices architecture, and cloud-native solutions for mid-market companies.",
        ];

        private static readonly string[] RecruiterIndustries =
            ["Artificial Intelligence", "Cloud Infrastructure", "Digital Agency", "Enterprise Software", "Technology Consulting"];

        private static readonly string[] CompanySizes = ["51-200", "201-500", "51-200", "501-1000", "51-200"];

        private static readonly string[][] ExperienceCompanies =
        [
            ["Microsoft", "Amazon", "Google", "Meta", "Apple", "Netflix", "Uber", "Spotify", "Stripe", "Shopify"],
            ["Google", "Meta", "Airbnb", "Figma", "Vercel", "Netflix", "Pinterest", "Canva", "Atlassian", "Shopify"],
            ["Amazon", "Google", "Microsoft", "Shopify", "Uber", "Airbnb", "Stripe", "Spotify", "Atlassian", "Salesforce"],
            ["Uber", "Snapchat", "TikTok", "Spotify", "Duolingo", "Venmo", "Robinhood", "Waze", "Instagram", "WhatsApp"],
            ["Amazon Web Services", "Google Cloud", "Microsoft Azure", "Cloudflare", "Datadog", "HashiCorp", "GitLab", "DigitalOcean", "Fastly", "MongoDB"],
            ["Netflix", "Spotify", "Airbnb", "Uber", "Palantir", "Zynga", "Twitter", "LinkedIn", "Pinterest", "eBay"],
            ["Google DeepMind", "OpenAI", "Meta AI", "NVIDIA", "Tesla AI", "Anthropic", "Scale AI", "Hugging Face", "Databricks", "Cohere"],
            ["OpenAI", "Google DeepMind", "Anthropic", "Scale AI", "Hugging Face", "Meta AI", "NVIDIA", "Cohere", "Stability AI", "Midjourney"],
        ];

        private static readonly string[][] CertificateNames =
        [
            ["AWS Certified Solutions Architect", "Microsoft Certified: Azure Developer Associate", "Oracle Certified Professional: Java SE"],
            ["Meta Front-End Developer Certificate", "Google UX Design Certificate", "AWS Certified Developer – Associate"],
            ["AWS Certified Developer – Associate", "Microsoft Certified: Azure Developer Associate", "Google Cloud Professional Cloud Developer"],
            ["Google Associate Android Developer", "Apple Certified iOS Developer", "Flutter Certified Application Developer"],
            ["AWS Certified DevOps Engineer", "Certified Kubernetes Administrator", "HashiCorp Certified: Terraform Associate"],
            ["Google Professional Data Engineer", "AWS Certified Machine Learning Specialty", "Microsoft Certified: Azure Data Scientist"],
            ["AWS Certified Machine Learning Specialty", "Google Professional Machine Learning Engineer", "TensorFlow Developer Certificate"],
            ["AWS Certified Machine Learning Specialty", "Microsoft Certified: Azure AI Engineer Associate", "Google Cloud AI/ML Certification"],
        ];

        private static readonly string[][] CertIssuers =
        [
            ["Amazon Web Services", "Microsoft", "Oracle"],
            ["Meta", "Google", "Amazon Web Services"],
            ["Amazon Web Services", "Microsoft", "Google Cloud"],
            ["Google", "Apple", "Flutter"],
            ["Amazon Web Services", "CNCF", "HashiCorp"],
            ["Google Cloud", "Amazon Web Services", "Microsoft"],
            ["Amazon Web Services", "Google Cloud", "TensorFlow"],
            ["Amazon Web Services", "Microsoft", "Google Cloud"],
        ];

        private static readonly string[] Universities =
            ["MIT", "Stanford University", "Carnegie Mellon University", "UC Berkeley", "Georgia Tech", "University of Illinois Urbana-Champaign", "University of Washington", "Caltech", "University of Michigan", "Cornell University", "University of Texas at Austin", "Purdue University", "University of California San Diego", "University of Wisconsin-Madison", "University of California Los Angeles", "University of Southern California", "University of Maryland", "University of Pennsylvania", "University of Toronto", "University of British Columbia"];

        private static readonly int[] FieldOfStudyIds = [1, 15, 16, 17, 18, 14, 4]; // Computer Science, Software Engineering, Data Science, AI, Cybersecurity, IT, Electrical Engineering

        private static readonly string[] FirstNames = ["James", "Sarah", "Michael", "Emily", "David", "Jessica", "Daniel", "Ashley", "Christopher", "Amanda", "Matthew", "Jennifer", "Andrew", "Stephanie", "Ryan", "Nicole", "Joshua", "Elizabeth", "Kevin", "Megan", "Brandon", "Lauren", "Tyler", "Rachel", "Justin", "Brittany", "Nicholas", "Heather", "Jacob", "Victoria", "Ethan", "Amber", "Aaron", "Danielle", "Nathan", "Gabrielle", "Samuel", "Samantha", "Benjamin", "Alexis", "Cody", "Michelle", "Zachary", "Katherine", "Kyle", "Christina", "Brian", "Sara", "Derek", "Tiffany"];

        private static readonly string[] LastNames = ["Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez", "Wilson", "Anderson", "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "White", "Harris", "Clark", "Lewis", "Robinson", "Walker", "Young", "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Adams", "Baker", "Green", "Campbell", "Mitchell", "Carter", "Roberts", "Phillips", "Cruz", "Reed", "Cook", "Bailey", "Howard", "Ward", "Morgan", "Fisher", "Perry"];

        private static readonly string[] JobTitlePrefixes = ["Junior ", "Senior ", "", "Lead ", "Principal "];

        public static async Task SeedAsync(AppDbContext context, ILogger logger)
        {
            var jobSeekerCount = await context.JobSeekers.CountAsync();
            var jobCount = await context.Jobs.CountAsync();
            if (jobSeekerCount >= 45 && jobCount >= 18)
            {
                logger.LogInformation("Mock data already exists (>=45 seekers, >=18 jobs). Skipping MockDataSeeder.");
                return;
            }

            logger.LogInformation("Purging existing mock data...");
            await context.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID('[Recommendations]', 'U') IS NOT NULL DELETE FROM [Recommendations];
                IF OBJECT_ID('[ProfileView]', 'U') IS NOT NULL DELETE FROM [ProfileView];
                IF OBJECT_ID('[AssessmentAnswer]', 'U') IS NOT NULL DELETE FROM [AssessmentAnswer];
                IF OBJECT_ID('[AssessmentAttempt]', 'U') IS NOT NULL DELETE FROM [AssessmentAttempt];
                IF OBJECT_ID('[AssessmentAnswersV2]', 'U') IS NOT NULL DELETE FROM [AssessmentAnswersV2];
                IF OBJECT_ID('[AssessmentAttemptsV2]', 'U') IS NOT NULL DELETE FROM [AssessmentAttemptsV2];
                IF OBJECT_ID('[JobSkills]', 'U') IS NOT NULL DELETE FROM [JobSkills];
                IF OBJECT_ID('[Jobs]', 'U') IS NOT NULL DELETE FROM [Jobs];
                IF OBJECT_ID('[JobSeekerSkills]', 'U') IS NOT NULL DELETE FROM [JobSeekerSkills];
                IF OBJECT_ID('[Certificates]', 'U') IS NOT NULL DELETE FROM [Certificates];
                IF OBJECT_ID('[Projects]', 'U') IS NOT NULL DELETE FROM [Projects];
                IF OBJECT_ID('[Experiences]', 'U') IS NOT NULL DELETE FROM [Experiences];
                IF OBJECT_ID('[Educations]', 'U') IS NOT NULL DELETE FROM [Educations];
                IF OBJECT_ID('[SocialAccounts]', 'U') IS NOT NULL DELETE FROM [SocialAccounts];
                IF OBJECT_ID('[Resumes]', 'U') IS NOT NULL DELETE FROM [Resumes];
                IF OBJECT_ID('[JobSeekers]', 'U') IS NOT NULL DELETE FROM [JobSeekers];
                IF OBJECT_ID('[Recruiters]', 'U') IS NOT NULL DELETE FROM [Recruiters];
                IF OBJECT_ID('[EmailVerifications]', 'U') IS NOT NULL DELETE FROM [EmailVerifications];
                IF OBJECT_ID('[PasswordResets]', 'U') IS NOT NULL DELETE FROM [PasswordResets];
                IF OBJECT_ID('[User]', 'U') IS NOT NULL DELETE FROM [User];
            """);
            logger.LogInformation("Purge complete.");

            logger.LogInformation("Seeding realistic mock data...");

            var countriesWithCities = await context.Countries
                .Include(c => c.Cities)
                .Where(c => c.Cities.Any())
                .ToListAsync();

            if (!countriesWithCities.Any())
            {
                logger.LogWarning("No countries with cities found. Cannot seed.");
                return;
            }

            var languageIds = await context.Languages.Select(l => l.Id).ToListAsync();
            var fieldOfStudyIds = await context.FieldsOfStudy.Select(f => f.Id).ToListAsync();

            if (!languageIds.Any() || !fieldOfStudyIds.Any())
            {
                logger.LogWarning("Reference data (Languages/FieldsOfStudy) not found.");
                return;
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(MockPassword);
            var faker = new Faker { Random = new Randomizer(BogusSeed) };

            // ── Build title->index lookup ──
            var titleIndexMap = new Dictionary<int, int>();
            for (int i = 0; i < TechJobTitleIds.Length; i++)
                titleIndexMap[TechJobTitleIds[i]] = i;

            // ── Assign job title IDs to each jobseeker (roughly even distribution) ──
            var seekerTitleAssignments = new int[JobSeekerCount];
            {
                int idx = 0;
                for (int t = 0; t < TechJobTitleIds.Length; t++)
                {
                    int count = t < 5 ? 6 : 5; // 6 for first 5 titles, 5 for last 3 = 45
                    for (int i = 0; i < count && idx < JobSeekerCount; i++)
                        seekerTitleAssignments[idx++] = TechJobTitleIds[t];
                }
                faker.Random.Shuffle(seekerTitleAssignments);
            }

            // ══════════════════════════════════════════
            // STEP 1: Create Users
            // ══════════════════════════════════════════
            var allUsers = new List<User>();
            var recUserEmails = new List<string>();

            for (int i = 0; i < RecruiterCount; i++)
            {
                var domain = RecruiterCompanies[i].ToLower().Replace(" ", "").Replace("-", "") + ".com";
                var email = $"{FirstNames[i].ToLower()}.{LastNames[i].ToLower()}@{domain}";
                recUserEmails.Add(email);

                allUsers.Add(new User
                {
                    FirstName = FirstNames[i],
                    LastName = LastNames[i],
                    Email = email,
                    PasswordHash = passwordHash,
                    AccountType = AccountType.Recruiter,
                    AuthProvider = AuthProvider.Email,
                    IsEmailVerified = true,
                    IsActive = true,
                    ProfileCompletionStep = 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(30, 180)),
                    UpdatedAt = DateTime.UtcNow,
                });
            }

            for (int i = 0; i < JobSeekerCount; i++)
            {
                var firstName = FirstNames[RecruiterCount + i];
                var lastName = LastNames[i];
                var email = $"{firstName.ToLower()}.{lastName.ToLower()}{faker.Random.Int(1, 99)}@gmail.com";

                allUsers.Add(new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    PasswordHash = passwordHash,
                    AccountType = AccountType.JobSeeker,
                    AuthProvider = AuthProvider.Email,
                    IsEmailVerified = true,
                    IsActive = true,
                    ProfileCompletionStep = 4,
                    CreatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(30, 365)),
                    UpdatedAt = DateTime.UtcNow,
                });
            }

            await context.Users.AddRangeAsync(allUsers);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} users.", allUsers.Count);

            // ══════════════════════════════════════════
            // STEP 2: Create Recruiters
            // ══════════════════════════════════════════
            var recruiters = new List<Recruiter>();
            for (int i = 0; i < RecruiterCount; i++)
            {
                var user = allUsers[i];
                var country = faker.PickRandom(countriesWithCities);
                var city = faker.PickRandom(country.Cities.ToList());

                recruiters.Add(new Recruiter
                {
                    UserId = user.Id,
                    CompanyName = RecruiterCompanies[i],
                    CompanySize = CompanySizes[i],
                    Industry = RecruiterIndustries[i],
                    CountryId = country.Id,
                    CityId = city.Id,
                    Website = $"https://www.{RecruiterCompanies[i].ToLower().Replace(" ", "").Replace("-", "")}.com",
                    LinkedIn = $"https://linkedin.com/company/{RecruiterCompanies[i].ToLower().Replace(" ", "").Replace("-", "")}",
                    CompanyDescription = CompanyDescriptions[i],
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                });
            }

            await context.Recruiters.AddRangeAsync(recruiters);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} recruiters.", recruiters.Count);

            // ══════════════════════════════════════════
            // STEP 3: Create JobSeekers & related entities
            // ══════════════════════════════════════════
            var workModelValues = Enum.GetValues<WorkModel>();
            var proficiencyValues = Enum.GetValues<LanguageProficiency>();
            var employmentTypeValues = Enum.GetValues<EmploymentType>();
            var degreeValues = Enum.GetValues<Degree>();

            var jobSeekers = new List<JobSeeker>();
            var allSkills = new List<JobSeekerSkill>();
            var allExperiences = new List<Experience>();
            var allEducation = new List<Education>();
            var allCertificates = new List<Certificate>();
            var allProjects = new List<Project>();
            var allSocialAccounts = new List<SocialAccount>();
            var allAssessmentAttempts = new List<AssessmentAttempt>();
            var skillIdToName = await context.Skills.ToDictionaryAsync(s => s.Id, s => s.Name);

            for (int i = 0; i < JobSeekerCount; i++)
            {
                var user = allUsers[RecruiterCount + i];
                var jobTitleId = seekerTitleAssignments[i];
                var titleIndex = titleIndexMap[jobTitleId];
                var titleName = TechJobTitleNames[titleIndex];
                var expYears = faker.Random.Int(1, 12);

                var country = faker.PickRandom(countriesWithCities);
                var city = faker.PickRandom(country.Cities.ToList());

                var workPrefs = faker.PickRandom(workModelValues, faker.Random.Int(1, 3)).Distinct().ToList();
                var empTypes = faker.PickRandom(employmentTypeValues, faker.Random.Int(1, 3)).Distinct().ToList();

                int? secondLangId = faker.Random.Bool(0.7f) ? faker.PickRandom(languageIds) : null;

                // Build bio
                var bioPattern = faker.PickRandom(BioTemplates[titleIndex]);
                var bio = bioPattern
                    .Replace("{exp}", expYears.ToString())
                    .Replace("{tech}", string.Join(", ", faker.PickRandom(SkillPools[jobTitleId], faker.Random.Int(2, 4)).Select(id => skillIdToName.GetValueOrDefault(id, ""))));
                if (bio.Length > 500) bio = bio[..497] + "...";

                var js = new JobSeeker
                {
                    UserId = user.Id,
                    JobTitleId = jobTitleId,
                    YearsOfExperience = expYears,
                    CountryId = country.Id,
                    CityId = city.Id,
                    PhoneNumber = $"+1-{faker.Random.Int(200, 999)}-{faker.Random.Int(100, 999)}-{faker.Random.Int(1000, 9999)}",
                    FirstLanguageId = faker.PickRandom(languageIds),
                    FirstLanguageProficiency = faker.PickRandom(proficiencyValues),
                    SecondLanguageId = secondLangId,
                    SecondLanguageProficiency = secondLangId.HasValue ? faker.PickRandom(proficiencyValues) : null,
                    Bio = bio,
                    WorkPreferences = workPrefs,
                    DesiredEmploymentTypes = empTypes,
                    CurrentAssessmentScore = Math.Round(faker.Random.Decimal(45m, 97m), 2),
                    LastAssessmentDate = DateTime.UtcNow.AddDays(-faker.Random.Int(1, 60)),
                    AssessmentJobTitleId = jobTitleId,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                };

                jobSeekers.Add(js);
            }

            await context.JobSeekers.AddRangeAsync(jobSeekers);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} job seekers.", jobSeekers.Count);

            // ══════════════════════════════════════════
            // STEP 4: Create JobSeekerSkills (aligned to job title)
            // ══════════════════════════════════════════
            for (int i = 0; i < JobSeekerCount; i++)
            {
                var js = jobSeekers[i];
                var jobTitleId = seekerTitleAssignments[i];
                var pool = SkillPools[jobTitleId];
                var skillCount = faker.Random.Int(4, 8);
                var selected = faker.PickRandom(pool, skillCount).Distinct().ToList();

                foreach (var skillId in selected)
                {
                    allSkills.Add(new JobSeekerSkill
                    {
                        JobSeekerId = js.Id,
                        SkillId = skillId,
                        Source = "Self",
                    });
                }
            }

            await context.JobSeekerSkills.AddRangeAsync(allSkills);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} job seeker skills.", allSkills.Count);

            // ══════════════════════════════════════════
            // STEP 5: Create Experiences (2-4 per seeker)
            // ══════════════════════════════════════════
            for (int i = 0; i < JobSeekerCount; i++)
            {
                var js = jobSeekers[i];
                var titleIndex = titleIndexMap[seekerTitleAssignments[i]];
                var expCount = faker.Random.Int(2, 4);

                var companies = faker.PickRandom(ExperienceCompanies[titleIndex], expCount).ToList();
                var responsibilities = faker.PickRandom(ExperienceResponsibilities[titleIndex], expCount).ToList();

                int accumulatedExp = 0;
                for (int e = 0; e < expCount; e++)
                {
                    var country = faker.PickRandom(countriesWithCities);
                    var city = faker.PickRandom(country.Cities.ToList());
                    var isCurrent = e == 0;
                    var durationMonths = faker.Random.Int(8, 36);
                    var endDate = isCurrent ? (DateTime?)null : DateTime.UtcNow.AddDays(-faker.Random.Int(1, 365) - accumulatedExp * 30);
                    var startDate = endDate.HasValue
                        ? endDate.Value.AddMonths(-durationMonths)
                        : DateTime.UtcNow.AddMonths(-durationMonths);
                    accumulatedExp += durationMonths;

                    var resp = responsibilities[e];
                    if (resp.Length > 2000) resp = resp[..1997] + "...";

                    allExperiences.Add(new Experience
                    {
                        JobSeekerId = js.Id,
                        JobTitle = companies[e].Contains("AI") || companies[e].Contains("DeepMind") || companies[e].Contains("OpenAI")
                            ? $"{TechJobTitleNames[titleIndex]}"
                            : $"{faker.PickRandom(JobTitlePrefixes)}{TechJobTitleNames[titleIndex]}",
                        CompanyName = companies[e],
                        CountryId = country.Id,
                        CityId = city.Id,
                        EmploymentType = EmploymentType.FullTime,
                        StartDate = startDate,
                        EndDate = endDate,
                        IsCurrent = isCurrent,
                        Responsibilities = resp,
                        DisplayOrder = expCount - 1 - e,
                        CreatedAt = startDate,
                        UpdatedAt = DateTime.UtcNow,
                    });
                }
            }

            await context.Experiences.AddRangeAsync(allExperiences);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} experiences.", allExperiences.Count);

            // ══════════════════════════════════════════
            // STEP 6: Create Education (1-2 per seeker)
            // ══════════════════════════════════════════
            for (int i = 0; i < JobSeekerCount; i++)
            {
                var js = jobSeekers[i];
                var eduCount = faker.Random.Int(1, 2);

                for (int e = 0; e < eduCount; e++)
                {
                    var isCurrent = e == 0;
                    var startYear = DateTime.UtcNow.AddYears(-(4 + e * 2));
                    var endDate = isCurrent ? (DateTime?)null : startYear.AddYears(faker.Random.Int(2, 5));

                    allEducation.Add(new Education
                    {
                        JobSeekerId = js.Id,
                        Institution = faker.PickRandom(Universities),
                        Degree = degreeValues.GetValue(faker.Random.Int(3, 5)) switch
                        {
                            Degree d when d == Degree.Bachelor => Degree.Bachelor,
                            Degree d when d == Degree.Master => Degree.Master,
                            _ => Degree.Bachelor,
                        },
                        FieldOfStudyId = faker.PickRandom(FieldOfStudyIds),
                        GradeOrGPA = $"{faker.Random.Decimal(2.5m, 4.0m):F2}",
                        StartDate = startYear,
                        EndDate = endDate,
                        IsCurrent = isCurrent,
                        DisplayOrder = eduCount - 1 - e,
                        CreatedAt = startYear,
                        UpdatedAt = DateTime.UtcNow,
                    });
                }
            }

            await context.Educations.AddRangeAsync(allEducation);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} education entries.", allEducation.Count);

            // ══════════════════════════════════════════
            // STEP 7: Create Certificates (1-2 per seeker)
            // ══════════════════════════════════════════
            for (int i = 0; i < JobSeekerCount; i++)
            {
                var titleIndex = titleIndexMap[seekerTitleAssignments[i]];
                var certCount = faker.Random.Int(0, 2);
                if (certCount == 0) continue;

                var pickedCerts = faker.PickRandom(CertificateNames[titleIndex], certCount).ToList();
                var pickedIssuers = faker.PickRandom(CertIssuers[titleIndex], certCount).ToList();

                for (int c = 0; c < certCount; c++)
                {
                    var issueDate = DateTime.UtcNow.AddDays(-faker.Random.Int(30, 730));
                    allCertificates.Add(new Certificate
                    {
                        JobSeekerId = jobSeekers[i].Id,
                        Title = pickedCerts[c],
                        IssuingOrganization = pickedIssuers[c],
                        IssueDate = issueDate,
                        ExpirationDate = faker.Random.Bool(0.3f) ? issueDate.AddYears(3) : (DateTime?)null,
                        DisplayOrder = c,
                        CreatedAt = issueDate,
                        UpdatedAt = DateTime.UtcNow,
                    });
                }
            }

            await context.Certificates.AddRangeAsync(allCertificates);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} certificates.", allCertificates.Count);

            // ══════════════════════════════════════════
            // STEP 8: Create Projects (1-3 per seeker)
            // ══════════════════════════════════════════
            var descsByTitle = new Dictionary<int, string[]>();
            for (int t = 0; t < TechJobTitleIds.Length; t++)
                descsByTitle[TechJobTitleIds[t]] = ProjectDescriptions[t];

            for (int i = 0; i < JobSeekerCount; i++)
            {
                var titleIndex = titleIndexMap[seekerTitleAssignments[i]];
                var projCount = faker.Random.Int(1, 3);
                var pickedTitles = faker.PickRandom(ProjectTemplates[titleIndex], projCount).ToList();
                var pickedDescs = faker.PickRandom(descsByTitle[seekerTitleAssignments[i]], projCount).ToList();

                for (int p = 0; p < projCount; p++)
                {
                    var techs = string.Join(", ", faker.PickRandom(SkillPools[seekerTitleAssignments[i]], faker.Random.Int(2, 4)).Select(id => skillIdToName.GetValueOrDefault(id, "")));
                    if (techs.Length > 300) techs = techs[..297] + "...";
                    var desc = pickedDescs[p];
                    if (desc.Length > 1200) desc = desc[..1197] + "...";

                    allProjects.Add(new Project
                    {
                        JobSeekerId = jobSeekers[i].Id,
                        Title = pickedTitles[p],
                        TechnologiesUsed = techs,
                        Description = desc,
                        ProjectLink = faker.Random.Bool(0.5f) ? $"https://github.com/{FirstNames[i + RecruiterCount].ToLower()}/{pickedTitles[p].ToLower().Replace(" ", "-")}" : null,
                        DisplayOrder = p,
                        CreatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(30, 365)),
                        UpdatedAt = DateTime.UtcNow,
                    });
                }
            }

            await context.Projects.AddRangeAsync(allProjects);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} projects.", allProjects.Count);

            // ══════════════════════════════════════════
            // STEP 9: Create SocialAccounts
            // ══════════════════════════════════════════
            for (int i = 0; i < JobSeekerCount; i++)
            {
                var firstName = allUsers[RecruiterCount + i].FirstName.ToLower();
                var lastName = allUsers[RecruiterCount + i].LastName.ToLower();

                allSocialAccounts.Add(new SocialAccount
                {
                    JobSeekerId = jobSeekers[i].Id,
                    LinkedIn = $"https://linkedin.com/in/{firstName}-{lastName}-{faker.Random.Int(1000, 9999)}",
                    Github = $"https://github.com/{firstName}{lastName}",
                    PersonalWebsite = faker.Random.Bool(0.6f) ? $"https://{firstName}{lastName}.dev" : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(30, 365)),
                    UpdatedAt = DateTime.UtcNow,
                });
            }

            await context.SocialAccounts.AddRangeAsync(allSocialAccounts);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} social accounts.", allSocialAccounts.Count);

            // ══════════════════════════════════════════
            // STEP 10: Create AssessmentAttempts
            // ══════════════════════════════════════════
            for (int i = 0; i < JobSeekerCount; i++)
            {
                var js = jobSeekers[i];
                var jobTitleId = seekerTitleAssignments[i];
                var completedAt = js.LastAssessmentDate!.Value;
                var startedAt = completedAt.AddMinutes(-faker.Random.Int(10, 25));
                var score = js.CurrentAssessmentScore!.Value;
                var techScore = Math.Round(score - faker.Random.Decimal(-8m, 8m), 2);
                var softScore = Math.Round(score - faker.Random.Decimal(-8m, 8m), 2);
                if (techScore > 100) techScore = 99.99m; if (techScore < 0) techScore = 0;
                if (softScore > 100) softScore = 99.99m; if (softScore < 0) softScore = 0;

                allAssessmentAttempts.Add(new AssessmentAttempt
                {
                    JobSeekerId = js.Id,
                    JobTitleId = jobTitleId,
                    OverallScore = score,
                    TechnicalScore = techScore,
                    SoftSkillsScore = softScore,
                    Status = AssessmentStatus.Completed,
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    TimeLimitMinutes = 30,
                    TotalQuestions = 20,
                    QuestionsAnswered = 20,
                    ResumeCount = 0,
                    ExpiresAt = startedAt.AddMinutes(30),
                    ScoreExpiresAt = completedAt.AddMonths(18),
                    IsActive = true,
                    RetakeNumber = 1,
                    AlgorithmVersion = 1,
                });
            }

            await context.AssessmentAttempts.AddRangeAsync(allAssessmentAttempts);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} assessment attempts.", allAssessmentAttempts.Count);

            // ══════════════════════════════════════════
            // STEP 11: Create Jobs (18 jobs across 5 recruiters)
            // ══════════════════════════════════════════
            var jobs = new List<Job>();
            for (int i = 0; i < JobCount; i++)
            {
                var recruiter = recruiters[i % RecruiterCount];
                var jobTitleId = TechJobTitleIds[i % TechJobTitleIds.Length];
                var titleIndex = titleIndexMap[jobTitleId];

                var workModel = faker.PickRandom(workModelValues);
                int? countryId = null;
                int? cityId = null;
                if (workModel != WorkModel.Remote)
                {
                    var c = faker.PickRandom(countriesWithCities);
                    countryId = c.Id;
                    cityId = faker.PickRandom(c.Cities.ToList()).Id;
                }

                var skillNames = string.Join(", ", faker.PickRandom(SkillPools[jobTitleId], faker.Random.Int(3, 5)).Select(id => skillIdToName.GetValueOrDefault(id, "")));

                jobs.Add(new Job
                {
                    RecruiterId = recruiter.Id,
                    Title = TechJobTitleNames[titleIndex],
                    JobTitleId = jobTitleId,
                    Description = $"We are looking for a talented {TechJobTitleNames[titleIndex]} to join our growing team at {RecruiterCompanies[i % RecruiterCount]}. The ideal candidate has strong experience with {skillNames} and a passion for building high-quality software. You will work with a team of experienced engineers on challenging problems that impact millions of users.",
                    Requirements = $"• {faker.Random.Int(2, 5)}+ years of experience as a {TechJobTitleNames[titleIndex]}\n• Strong proficiency in {skillNames}\n• Experience with Agile/Scrum methodologies\n• Excellent problem-solving and communication skills\n• Bachelor's degree in Computer Science or related field",
                    EmploymentType = faker.PickRandom(employmentTypeValues),
                    MinYearsOfExperience = faker.Random.Int(0, 3),
                    WorkModel = workModel,
                    CountryId = countryId,
                    CityId = cityId,
                    IsActive = true,
                    PostedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(0, 30)),
                    UpdatedAt = DateTime.UtcNow,
                });
            }

            await context.Jobs.AddRangeAsync(jobs);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} jobs.", jobs.Count);

            // ══════════════════════════════════════════
            // STEP 12: Create JobSkills (aligned to job title)
            // ══════════════════════════════════════════
            var jobSkills = new List<JobSkill>();
            for (int i = 0; i < JobCount; i++)
            {
                var job = jobs[i];
                var jobTitleId = job.JobTitleId!.Value;
                var pool = SkillPools[jobTitleId];
                var skillCount = faker.Random.Int(4, 7);
                var selected = faker.PickRandom(pool, skillCount).Distinct().ToList();

                foreach (var skillId in selected)
                {
                    jobSkills.Add(new JobSkill
                    {
                        JobId = job.Id,
                        SkillId = skillId,
                    });
                }
            }

            await context.JobSkills.AddRangeAsync(jobSkills);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} job skills.", jobSkills.Count);

            // ══════════════════════════════════════════
            // STEP 13: Create ProfileViews (for engagement stats)
            // ══════════════════════════════════════════
            var profileViews = new List<ProfileView>();
            var recruiterIds = recruiters.Select(r => r.Id).ToList();

            foreach (var js in jobSeekers)
            {
                var viewCount = faker.Random.Int(0, 5);
                for (int v = 0; v < viewCount; v++)
                {
                    profileViews.Add(new ProfileView
                    {
                        JobSeekerId = js.Id,
                        ViewerRecruiterId = faker.PickRandom(recruiterIds),
                        ViewType = faker.Random.Bool(0.4f) ? "ProfileClick" : "Search",
                        ViewedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(0, 30)).AddHours(-faker.Random.Int(0, 23)),
                    });
                }
            }

            await context.ProfileViews.AddRangeAsync(profileViews);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} profile views.", profileViews.Count);

            logger.LogInformation(
                "MockDataSeeder complete — {JsCount} job seekers, {RCount} recruiters, {JCount} jobs, {PvCount} profile views.",
                jobSeekers.Count, recruiters.Count, jobs.Count, profileViews.Count);
        }
    }
}
