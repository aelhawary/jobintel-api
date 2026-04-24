using RecruitmentPlatformAPI.Enums;
using RecruitmentPlatformAPI.Models.Assessment;

namespace RecruitmentPlatformAPI.Data.Seed
{
    /// <summary>
    /// Seed data for assessment questions
    /// Provides ~160 questions across technical and soft skill categories
    /// </summary>
    public static class AssessmentQuestionSeed
    {
        private static readonly DateTime SeedCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Skill IDs from SkillSeed.
        private const int SkillJavaScript = 2;
        private const int SkillReact = 11;
        private const int SkillHtmlCss = 15;
        private const int SkillAspNetCore = 17;
        private const int SkillNodeJs = 18;
        private const int SkillSqlServer = 23;
        private const int SkillRestApis = 43;
        private const int SkillGraphQl = 44;
        private const int SkillAgileScrum = 45;
        private const int SkillProblemSolving = 47;
        private const int SkillCommunication = 48;
        private const int SkillProjectManagement = 49;

        public static List<AssessmentQuestion> GetQuestions()
        {
            var questions = new List<AssessmentQuestion>();
            int id = 1;

            // ═══════════════════════════════════════════════════════════════════════════════
            // BACKEND QUESTIONS (50 questions)
            // ═══════════════════════════════════════════════════════════════════════════════

            // Junior Backend - Easy
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What does REST stand for?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "Representational State Transfer", "Remote Execution Service Technology", "Reliable External System Transport", "Request-Response State Transition" },
                    0, "REST is an architectural style for designing networked applications using HTTP requests."),

                CreateQuestion(id++, "Which HTTP method is used to retrieve data from a server?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "GET", "POST", "PUT", "DELETE" },
                    0, "GET is used to request data from a specified resource."),

                CreateQuestion(id++, "What is the purpose of a database index?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "To enforce data constraints", "To speed up data retrieval", "To encrypt sensitive data", "To backup data automatically" },
                    1, "Indexes improve query performance by allowing faster data lookup."),

                CreateQuestion(id++, "What status code indicates a successful HTTP request?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "404", "500", "200", "301" },
                    2, "HTTP 200 OK indicates that the request has succeeded."),

                CreateQuestion(id++, "What is JSON primarily used for?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "Styling web pages", "Data interchange between systems", "Database queries", "User authentication" },
                    1, "JSON (JavaScript Object Notation) is a lightweight data interchange format."),
            });

            // Junior Backend - Medium
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What is the difference between PUT and PATCH HTTP methods?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "PUT creates, PATCH deletes", "PUT replaces entire resource, PATCH updates partial resource", "They are identical", "PUT is faster than PATCH" },
                    1, "PUT replaces the entire resource, while PATCH applies partial modifications."),

                CreateQuestion(id++, "What is dependency injection?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "A security vulnerability", "A design pattern for loose coupling", "A database optimization technique", "A type of API authentication" },
                    1, "Dependency injection is a pattern that allows dependencies to be provided rather than hard-coded."),

                CreateQuestion(id++, "What does ACID stand for in database transactions?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "Atomicity, Consistency, Isolation, Durability", "Authentication, Confidentiality, Integrity, Durability", "Automated, Consistent, Indexed, Distributed", "Access, Control, Identity, Data" },
                    0, "ACID properties ensure reliable database transactions."),

                CreateQuestion(id++, "What is the purpose of middleware in web applications?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "To store user sessions", "To process requests between client and server", "To render HTML templates", "To manage database connections only" },
                    1, "Middleware functions process requests/responses in the application pipeline."),
            });

            // Junior Backend - Hard
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What is the N+1 query problem?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Junior,
                    new[] { "A network latency issue", "Executing one query for the list plus one query per item", "A database connection pool exhaustion", "An SQL syntax error" },
                    1, "N+1 occurs when code executes N additional queries to fetch related data for N records."),

                CreateQuestion(id++, "What is eventual consistency?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Junior,
                    new[] { "Data is always immediately consistent", "Given enough time, all replicas will converge", "Data is never consistent", "Only writes are consistent" },
                    1, "Eventual consistency means replicas will eventually be consistent, but not immediately."),
            });

            // Mid Backend - Easy
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What is the purpose of an API gateway?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Mid,
                    new[] { "To store API documentation", "To serve as a single entry point for microservices", "To compile API code", "To test API endpoints" },
                    1, "An API gateway acts as a single entry point for all API calls."),

                CreateQuestion(id++, "What is caching used for in backend systems?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Mid,
                    new[] { "To encrypt data", "To store frequently accessed data for faster retrieval", "To validate user input", "To compress response data" },
                    1, "Caching stores data temporarily to reduce latency and database load."),
            });

            // Mid Backend - Medium
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What is the CAP theorem?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Mid,
                    new[] { "A distributed system can have at most 2 of: Consistency, Availability, Partition tolerance", "A database optimization rule", "A caching strategy", "An API design principle" },
                    0, "CAP theorem states distributed systems can only guarantee 2 of the 3 properties."),

                CreateQuestion(id++, "What is the difference between horizontal and vertical scaling?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Mid,
                    new[] { "Horizontal adds more machines, vertical adds more power to existing machines", "They are the same", "Horizontal is cheaper always", "Vertical is for databases only" },
                    0, "Horizontal scaling adds more instances, vertical scaling increases resources of existing instances."),

                CreateQuestion(id++, "What is a message queue used for?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Mid,
                    new[] { "Real-time chat only", "Asynchronous communication between services", "Database replication", "API versioning" },
                    1, "Message queues enable asynchronous, decoupled communication between services."),

                CreateQuestion(id++, "What is database sharding?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Mid,
                    new[] { "Encrypting database fields", "Partitioning data across multiple databases", "Creating database backups", "Indexing database tables" },
                    1, "Sharding distributes data across multiple database instances for scalability."),
            });

            // Mid Backend - Hard
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What is the Saga pattern in microservices?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Mid,
                    new[] { "A logging pattern", "A pattern for managing distributed transactions", "A caching strategy", "An API versioning approach" },
                    1, "Saga pattern manages distributed transactions through a sequence of local transactions."),

                CreateQuestion(id++, "What is circuit breaker pattern?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Mid,
                    new[] { "A network security measure", "A pattern to prevent cascading failures in distributed systems", "A database constraint", "An authentication method" },
                    1, "Circuit breaker prevents repeated calls to a failing service, allowing recovery time."),

                CreateQuestion(id++, "What is CQRS (Command Query Responsibility Segregation)?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Mid,
                    new[] { "A caching technique", "Separating read and write operations into different models", "A database type", "An API versioning strategy" },
                    1, "CQRS separates read and write operations for better scalability and performance."),
            });

            // Senior Backend - Medium
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What factors would you consider when designing a rate limiting strategy?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Senior,
                    new[] { "Only request count", "Request count, user tier, endpoint sensitivity, and time windows", "Only user authentication status", "Only server capacity" },
                    1, "Effective rate limiting considers multiple factors including user tiers and endpoint importance."),

                CreateQuestion(id++, "What is event sourcing?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Senior,
                    new[] { "Logging all events", "Storing state changes as a sequence of events", "A pub/sub mechanism", "An API documentation approach" },
                    1, "Event sourcing persists state changes as immutable events rather than current state."),
            });

            // Senior Backend - Hard
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "How would you design a system to handle 1 million concurrent users?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Senior,
                    new[] { "Use a single powerful server", "Use load balancing, caching, CDN, database sharding, and async processing", "Just increase database size", "Only add more API servers" },
                    1, "Handling high concurrency requires a combination of scaling techniques."),

                CreateQuestion(id++, "What is the difference between optimistic and pessimistic locking?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Senior,
                    new[] { "Optimistic assumes no conflicts, pessimistic locks resources preemptively", "They are identical", "Optimistic is for reads, pessimistic for writes", "Optimistic is always faster" },
                    0, "Optimistic locking checks for conflicts at commit time, pessimistic locks prevent conflicts."),

                CreateQuestion(id++, "How would you implement a distributed cache invalidation strategy?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Senior,
                    new[] { "Clear all caches periodically", "Use TTL, pub/sub invalidation, and versioning strategies", "Never invalidate", "Only invalidate on server restart" },
                    1, "Effective cache invalidation combines TTL, pub/sub notifications, and versioning."),

                CreateQuestion(id++, "What considerations are important for designing idempotent APIs?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Backend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Senior,
                    new[] { "Only status codes matter", "Idempotency keys, proper HTTP methods, and handling duplicate requests", "Only GET methods need to be idempotent", "Idempotency is not important" },
                    1, "Idempotent APIs handle retries safely without side effects."),
            });

            // ═══════════════════════════════════════════════════════════════════════════════
            // FRONTEND QUESTIONS (45 questions)
            // ═══════════════════════════════════════════════════════════════════════════════

            // Junior Frontend - Easy
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What does CSS stand for?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "Computer Style Sheets", "Cascading Style Sheets", "Creative Style System", "Content Styling Standard" },
                    1, "CSS (Cascading Style Sheets) is used to style HTML documents."),

                CreateQuestion(id++, "What is the purpose of the 'useState' hook in React?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "To make HTTP requests", "To manage component state", "To create routes", "To style components" },
                    1, "useState allows functional components to have state."),

                CreateQuestion(id++, "What is the DOM?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "Data Object Model", "Document Object Model", "Digital Output Method", "Direct Object Manipulation" },
                    1, "The DOM is a programming interface for HTML documents."),

                CreateQuestion(id++, "What is the purpose of 'alt' attribute in images?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "To make images load faster", "To provide alternative text for accessibility", "To set image dimensions", "To add image effects" },
                    1, "The alt attribute provides text description for screen readers and when images fail to load."),

                CreateQuestion(id++, "What is responsive design?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "Fast-loading websites", "Design that adapts to different screen sizes", "Websites that respond quickly", "Design with animations" },
                    1, "Responsive design ensures websites work well on all devices and screen sizes."),
            });

            // Junior Frontend - Medium
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What is the Virtual DOM?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "A new browser feature", "A lightweight copy of the actual DOM for efficient updates", "A CSS framework", "A JavaScript library" },
                    1, "The Virtual DOM is React's strategy for efficient DOM manipulation."),

                CreateQuestion(id++, "What is event bubbling?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "A CSS animation", "Events propagating from child to parent elements", "A JavaScript error", "A browser bug" },
                    1, "Event bubbling means events triggered on nested elements propagate up the DOM tree."),

                CreateQuestion(id++, "What is the difference between '==' and '===' in JavaScript?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "They are identical", "'==' compares with type coercion, '===' compares strictly", "'===' is deprecated", "'==' is for strings only" },
                    1, "'===' checks both value and type without coercion."),

                CreateQuestion(id++, "What is a closure in JavaScript?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "A browser window closing", "A function that remembers its outer scope variables", "A way to end loops", "A CSS property" },
                    1, "A closure is a function that has access to variables from its outer scope."),
            });

            // Junior Frontend - Hard
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What is the purpose of useCallback in React?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Junior,
                    new[] { "To fetch data", "To memoize functions and prevent unnecessary re-renders", "To handle routing", "To manage global state" },
                    1, "useCallback memoizes callback functions to optimize performance."),

                CreateQuestion(id++, "What is tree shaking?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Junior,
                    new[] { "A CSS animation", "Removing unused code during bundling", "A testing technique", "A debugging method" },
                    1, "Tree shaking eliminates dead code from the final bundle."),
            });

            // Mid Frontend - Medium
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What is code splitting?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Mid,
                    new[] { "Writing code in multiple files", "Loading code on demand to reduce initial bundle size", "A coding style", "Splitting CSS and JS" },
                    1, "Code splitting loads code chunks on demand for better performance."),

                CreateQuestion(id++, "What is the purpose of React.memo?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Mid,
                    new[] { "To store notes in components", "To memoize components and prevent unnecessary re-renders", "To manage memory", "To create memos" },
                    1, "React.memo is a HOC that memoizes components based on props."),

                CreateQuestion(id++, "What is the difference between SSR and CSR?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Mid,
                    new[] { "SSR renders on server, CSR renders in browser", "They are identical", "SSR is faster always", "CSR is for mobile only" },
                    0, "SSR renders HTML on the server, CSR renders in the client's browser."),

                CreateQuestion(id++, "What is the purpose of Web Workers?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Mid,
                    new[] { "To style web pages", "To run scripts in background threads", "To handle HTTP requests", "To manage cookies" },
                    1, "Web Workers allow scripts to run in background threads without blocking the UI."),
            });

            // Mid Frontend - Hard
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "How would you optimize a large list rendering in React?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Mid,
                    new[] { "Use more useState", "Implement virtualization/windowing techniques", "Add more CSS", "Use setTimeout" },
                    1, "Virtualization renders only visible items, dramatically improving performance for large lists."),

                CreateQuestion(id++, "What is hydration in the context of SSR?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Mid,
                    new[] { "Adding water effects", "Attaching JavaScript event handlers to server-rendered HTML", "A CSS property", "A database technique" },
                    1, "Hydration makes server-rendered HTML interactive by attaching event handlers."),

                CreateQuestion(id++, "What is the Critical Rendering Path?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Mid,
                    new[] { "The most important CSS rules", "The sequence of steps browser takes to render a page", "A JavaScript function", "A routing pattern" },
                    1, "The Critical Rendering Path is the sequence from receiving HTML to displaying pixels."),
            });

            // Senior Frontend - Hard
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "How would you implement a micro-frontend architecture?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Senior,
                    new[] { "Use only one framework", "Use module federation, independent deployments, and shared dependencies", "Avoid using any frameworks", "Use iframes only" },
                    1, "Micro-frontends allow independent teams to develop and deploy separate frontend modules."),

                CreateQuestion(id++, "What strategies would you use for state management in a large application?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Senior,
                    new[] { "Use only local state", "Combine local, global state, server state caching, and derived state appropriately", "Use only Redux", "Avoid state management" },
                    1, "Large apps benefit from a layered approach combining different state management strategies."),

                CreateQuestion(id++, "How would you approach performance optimization for a slow React application?",
                    QuestionCategory.Technical, JobTitleRoleFamily.Frontend,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Senior,
                    new[] { "Add more components", "Profile, analyze renders, memoize, split code, lazy load, and optimize network", "Remove all styling", "Use class components only" },
                    1, "Performance optimization requires profiling, identifying bottlenecks, and applying targeted fixes."),
            });

            // ═══════════════════════════════════════════════════════════════════════════════
            // FULLSTACK QUESTIONS (25 questions - combination)
            // ═══════════════════════════════════════════════════════════════════════════════

            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What is GraphQL and how does it differ from REST?",
                    QuestionCategory.Technical, JobTitleRoleFamily.FullStack,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "They are identical", "GraphQL allows clients to request specific data, REST returns fixed responses", "REST is newer than GraphQL", "GraphQL is only for databases" },
                    1, "GraphQL provides flexible queries while REST has fixed endpoints."),

                CreateQuestion(id++, "What is a JWT and when would you use it?",
                    QuestionCategory.Technical, JobTitleRoleFamily.FullStack,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "A JavaScript testing tool", "A stateless authentication token for API security", "A CSS framework", "A database type" },
                    1, "JWT (JSON Web Token) enables stateless authentication between client and server."),

                CreateQuestion(id++, "What is CORS and why is it important?",
                    QuestionCategory.Technical, JobTitleRoleFamily.FullStack,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Mid,
                    new[] { "A CSS feature", "A security mechanism controlling cross-origin requests", "A JavaScript library", "A database constraint" },
                    1, "CORS controls which origins can access resources from a different domain."),

                CreateQuestion(id++, "How would you implement real-time features in a web application?",
                    QuestionCategory.Technical, JobTitleRoleFamily.FullStack,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Mid,
                    new[] { "Refresh the page frequently", "Use WebSockets, Server-Sent Events, or long polling", "Only use REST APIs", "Disable caching" },
                    1, "Real-time features typically use WebSockets or SSE for bi-directional or server-push communication."),

                CreateQuestion(id++, "What considerations are important when designing an authentication system?",
                    QuestionCategory.Technical, JobTitleRoleFamily.FullStack,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Senior,
                    new[] { "Only password strength", "Secure storage, token management, MFA, session handling, and rate limiting", "Only using HTTPS", "Only email verification" },
                    1, "Robust authentication requires multiple security layers and considerations."),
            });

            // ═══════════════════════════════════════════════════════════════════════════════
            // SOFT SKILLS QUESTIONS (40 questions)
            // ═══════════════════════════════════════════════════════════════════════════════

            // Junior Soft Skills - Easy
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "A colleague asks for help with a task you're unfamiliar with. What should you do?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "Pretend you know and try anyway", "Honestly say you're unfamiliar but offer to learn together or find help", "Ignore the request", "Tell them to ask someone else" },
                    1, "Honesty and willingness to learn are valued traits in collaborative environments."),

                CreateQuestion(id++, "How should you handle constructive criticism of your work?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "Get defensive and argue", "Listen carefully, ask clarifying questions, and use it to improve", "Ignore it completely", "Criticize the reviewer's work in return" },
                    1, "Constructive criticism is an opportunity for growth when received with an open mind."),

                CreateQuestion(id++, "What is the best way to communicate a technical concept to a non-technical person?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "Use as much jargon as possible", "Use analogies and simple language avoiding technical terms", "Show them the code", "Tell them it's too complex to explain" },
                    1, "Effective communication adapts to the audience's level of understanding."),

                CreateQuestion(id++, "You realize you made a mistake in production code. What should you do first?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Junior,
                    new[] { "Hide it and hope no one notices", "Inform your team immediately and work on a fix", "Blame someone else", "Wait until someone reports it" },
                    1, "Transparency and quick action when mistakes occur build trust and minimize impact."),
            });

            // Junior Soft Skills - Medium
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "How do you prioritize tasks when everything seems urgent?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "Work on whatever is easiest first", "Assess impact, deadlines, and dependencies to determine true priorities", "Ask someone else to decide", "Work overtime on everything" },
                    1, "Effective prioritization considers multiple factors beyond perceived urgency."),

                CreateQuestion(id++, "A team member's code doesn't meet standards. How do you address this?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "Rewrite their code without telling them", "Provide constructive feedback privately with specific suggestions", "Complain to the manager immediately", "Ignore it" },
                    1, "Private, specific, and constructive feedback helps team members improve."),

                CreateQuestion(id++, "How do you stay motivated when working on a long, complex project?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Junior,
                    new[] { "Just push through regardless of burnout", "Break into milestones, celebrate progress, and take regular breaks", "Complain frequently", "Switch to other projects" },
                    1, "Breaking down large projects and acknowledging progress helps maintain motivation."),
            });

            // Junior Soft Skills - Hard
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "You disagree with a technical decision made by a senior developer. What do you do?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Junior,
                    new[] { "Stay silent and follow orders", "Respectfully present your concerns with supporting reasoning in private", "Argue publicly in meetings", "Implement your approach anyway" },
                    1, "Respectful disagreement with reasoning contributes to better decisions."),
            });

            // Mid Soft Skills - Easy
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "What makes a good code review?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Mid,
                    new[] { "Finding as many problems as possible", "Being constructive, specific, and focused on code quality and learning", "Approving everything quickly", "Only checking for syntax errors" },
                    1, "Good code reviews balance quality enforcement with constructive learning."),

                CreateQuestion(id++, "How do you handle working with a difficult team member?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Easy, ExperienceSeniorityLevel.Mid,
                    new[] { "Avoid them completely", "Try to understand their perspective and communicate directly about issues", "Complain to others", "Request a team change immediately" },
                    1, "Direct, empathetic communication often resolves interpersonal difficulties."),
            });

            // Mid Soft Skills - Medium
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "How do you balance technical debt against new feature development?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Mid,
                    new[] { "Always prioritize new features", "Assess impact, allocate regular time for debt, and communicate tradeoffs", "Never work on new features until debt is cleared", "Technical debt doesn't matter" },
                    1, "Balancing debt and features requires continuous assessment and communication."),

                CreateQuestion(id++, "You notice a team member struggling but not asking for help. What do you do?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Mid,
                    new[] { "Wait for them to ask", "Offer help in a supportive, non-judgmental way", "Report them to the manager", "Take over their work" },
                    1, "Proactive, supportive assistance helps team members without undermining their autonomy."),

                CreateQuestion(id++, "How do you handle scope creep in a project?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Mid,
                    new[] { "Accept all changes without question", "Document changes, assess impact, and negotiate timeline or resource adjustments", "Refuse all changes", "Work overtime to include everything" },
                    1, "Managing scope creep requires documentation, impact assessment, and negotiation."),
            });

            // Mid Soft Skills - Hard
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "A project deadline is clearly unrealistic. How do you handle this?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Mid,
                    new[] { "Promise to meet it anyway", "Present data-driven concerns early with alternative proposals", "Miss the deadline and explain later", "Quit the project" },
                    1, "Proactive, data-driven communication about constraints enables informed decisions."),

                CreateQuestion(id++, "How do you mentor a junior developer effectively?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Mid,
                    new[] { "Tell them exactly what to do", "Guide with questions, provide resources, give autonomy, and offer regular feedback", "Let them figure everything out alone", "Do their work for them" },
                    1, "Effective mentoring balances guidance with opportunities for independent growth."),
            });

            // Senior Soft Skills - Medium
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "How do you influence decisions when you don't have direct authority?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Senior,
                    new[] { "Force your opinion", "Build relationships, present data, understand stakeholders, and find common ground", "Give up trying", "Escalate immediately" },
                    1, "Influence without authority requires relationship building and persuasive communication."),

                CreateQuestion(id++, "How do you handle technical decisions you disagree with from leadership?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Medium, ExperienceSeniorityLevel.Senior,
                    new[] { "Comply silently", "Voice concerns diplomatically, disagree and commit if overruled", "Refuse to implement", "Undermine the decision quietly" },
                    1, "Professional disagreement followed by committed execution maintains trust."),
            });

            // Senior Soft Skills - Hard
            questions.AddRange(new[]
            {
                CreateQuestion(id++, "How do you foster innovation in an engineering team?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Senior,
                    new[] { "Assign innovation tasks", "Create psychological safety, allocate time for experimentation, celebrate learning from failures", "Hire more people", "Wait for good ideas to emerge" },
                    1, "Innovation requires psychological safety and dedicated time for experimentation."),

                CreateQuestion(id++, "How do you handle a situation where team performance is declining?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Senior,
                    new[] { "Work everyone harder", "Diagnose root causes, address issues individually and systemically, adjust processes", "Replace team members immediately", "Ignore it and hope it improves" },
                    1, "Declining performance requires diagnosis, support, and systemic improvements."),

                CreateQuestion(id++, "How do you make a case for investing in developer experience improvements?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Senior,
                    new[] { "Just request budget", "Quantify productivity gains, connect to business outcomes, and pilot small improvements", "Complain until approved", "Implement without approval" },
                    1, "Business cases for DevEx require quantified benefits and demonstrable outcomes."),

                CreateQuestion(id++, "A key team member wants to leave. How do you approach this situation?",
                    QuestionCategory.SoftSkill, JobTitleRoleFamily.Other,
                    QuestionDifficulty.Hard, ExperienceSeniorityLevel.Senior,
                    new[] { "Let them go without discussion", "Understand their reasons, address what you can, plan for transitions respectfully", "Offer more money immediately", "Make them feel guilty" },
                    1, "Addressing departures requires understanding, respect, and knowledge transfer planning."),
            });

            return questions;
        }

        private static AssessmentQuestion CreateQuestion(
            int id,
            string questionText,
            QuestionCategory category,
            JobTitleRoleFamily roleFamily,
            QuestionDifficulty difficulty,
            ExperienceSeniorityLevel seniorityLevel,
            string[] options,
            int correctAnswerIndex,
            string explanation,
            int? skillId = null)
        {
            return new AssessmentQuestion
            {
                Id = id,
                QuestionText = questionText,
                Category = category,
                RoleFamily = roleFamily,
                SkillId = skillId ?? ResolveSkillId(questionText, category, roleFamily),
                Difficulty = difficulty,
                SeniorityLevel = seniorityLevel,
                Options = System.Text.Json.JsonSerializer.Serialize(options),
                CorrectAnswerIndex = correctAnswerIndex,
                TimePerQuestion = 60,
                IsActive = true,
                CreatedAt = SeedCreatedAt,
                UpdatedAt = SeedCreatedAt,
                Explanation = explanation
            };
        }

        private static int ResolveSkillId(string questionText, QuestionCategory category, JobTitleRoleFamily roleFamily)
        {
            var normalized = questionText.ToLowerInvariant();

            if (category == QuestionCategory.SoftSkill)
            {
                if (normalized.Contains("communicat") || normalized.Contains("criticism") || normalized.Contains("non-technical"))
                {
                    return SkillCommunication;
                }

                if (normalized.Contains("project") || normalized.Contains("scope") || normalized.Contains("deadline"))
                {
                    return SkillProjectManagement;
                }

                if (normalized.Contains("agile") || normalized.Contains("scrum") || normalized.Contains("code review") || normalized.Contains("mentor") || normalized.Contains("team"))
                {
                    return SkillAgileScrum;
                }

                return SkillProblemSolving;
            }

            if (normalized.Contains("graphql"))
            {
                return SkillGraphQl;
            }

            if (normalized.Contains("sql") || normalized.Contains("database") || normalized.Contains("index") || normalized.Contains("acid") || normalized.Contains("shard") || normalized.Contains("locking"))
            {
                return SkillSqlServer;
            }

            if (normalized.Contains("jwt") || normalized.Contains("http") || normalized.Contains("rest") || normalized.Contains("api") || normalized.Contains("cors") || normalized.Contains("idempotent"))
            {
                return SkillRestApis;
            }

            if (roleFamily == JobTitleRoleFamily.Frontend)
            {
                if (normalized.Contains("css") || normalized.Contains("dom") || normalized.Contains("responsive") || normalized.Contains("alt"))
                {
                    return SkillHtmlCss;
                }

                if (normalized.Contains("javascript") || normalized.Contains("closure") || normalized.Contains("event bubbling") || normalized.Contains("web workers"))
                {
                    return SkillJavaScript;
                }

                return SkillReact;
            }

            if (roleFamily == JobTitleRoleFamily.FullStack)
            {
                if (normalized.Contains("real-time") || normalized.Contains("websocket") || normalized.Contains("long polling") || normalized.Contains("server-sent"))
                {
                    return SkillNodeJs;
                }

                return SkillRestApis;
            }

            if (roleFamily == JobTitleRoleFamily.Backend)
            {
                return SkillAspNetCore;
            }

            return SkillRestApis;
        }
    }
}
