using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RecruitmentPlatformAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAssessmentQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AssessmentQuestion",
                columns: new[] { "Id", "Category", "CorrectAnswerIndex", "CreatedAt", "Difficulty", "Explanation", "IsActive", "Options", "QuestionText", "RoleFamily", "SeniorityLevel", "SkillId", "TimePerQuestion", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "REST is an architectural style for designing networked applications using HTTP requests.", true, "[\"Representational State Transfer\",\"Remote Execution Service Technology\",\"Reliable External System Transport\",\"Request-Response State Transition\"]", "What does REST stand for?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "GET is used to request data from a specified resource.", true, "[\"GET\",\"POST\",\"PUT\",\"DELETE\"]", "Which HTTP method is used to retrieve data from a server?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Indexes improve query performance by allowing faster data lookup.", true, "[\"To enforce data constraints\",\"To speed up data retrieval\",\"To encrypt sensitive data\",\"To backup data automatically\"]", "What is the purpose of a database index?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 1, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "HTTP 200 OK indicates that the request has succeeded.", true, "[\"404\",\"500\",\"200\",\"301\"]", "What status code indicates a successful HTTP request?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "JSON (JavaScript Object Notation) is a lightweight data interchange format.", true, "[\"Styling web pages\",\"Data interchange between systems\",\"Database queries\",\"User authentication\"]", "What is JSON primarily used for?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "PUT replaces the entire resource, while PATCH applies partial modifications.", true, "[\"PUT creates, PATCH deletes\",\"PUT replaces entire resource, PATCH updates partial resource\",\"They are identical\",\"PUT is faster than PATCH\"]", "What is the difference between PUT and PATCH HTTP methods?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Dependency injection is a pattern that allows dependencies to be provided rather than hard-coded.", true, "[\"A security vulnerability\",\"A design pattern for loose coupling\",\"A database optimization technique\",\"A type of API authentication\"]", "What is dependency injection?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "ACID properties ensure reliable database transactions.", true, "[\"Atomicity, Consistency, Isolation, Durability\",\"Authentication, Confidentiality, Integrity, Durability\",\"Automated, Consistent, Indexed, Distributed\",\"Access, Control, Identity, Data\"]", "What does ACID stand for in database transactions?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Middleware functions process requests/responses in the application pipeline.", true, "[\"To store user sessions\",\"To process requests between client and server\",\"To render HTML templates\",\"To manage database connections only\"]", "What is the purpose of middleware in web applications?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "N+1 occurs when code executes N additional queries to fetch related data for N records.", true, "[\"A network latency issue\",\"Executing one query for the list plus one query per item\",\"A database connection pool exhaustion\",\"An SQL syntax error\"]", "What is the N+1 query problem?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Eventual consistency means replicas will eventually be consistent, but not immediately.", true, "[\"Data is always immediately consistent\",\"Given enough time, all replicas will converge\",\"Data is never consistent\",\"Only writes are consistent\"]", "What is eventual consistency?", 2, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "An API gateway acts as a single entry point for all API calls.", true, "[\"To store API documentation\",\"To serve as a single entry point for microservices\",\"To compile API code\",\"To test API endpoints\"]", "What is the purpose of an API gateway?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Caching stores data temporarily to reduce latency and database load.", true, "[\"To encrypt data\",\"To store frequently accessed data for faster retrieval\",\"To validate user input\",\"To compress response data\"]", "What is caching used for in backend systems?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "CAP theorem states distributed systems can only guarantee 2 of the 3 properties.", true, "[\"A distributed system can have at most 2 of: Consistency, Availability, Partition tolerance\",\"A database optimization rule\",\"A caching strategy\",\"An API design principle\"]", "What is the CAP theorem?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Horizontal scaling adds more instances, vertical scaling increases resources of existing instances.", true, "[\"Horizontal adds more machines, vertical adds more power to existing machines\",\"They are the same\",\"Horizontal is cheaper always\",\"Vertical is for databases only\"]", "What is the difference between horizontal and vertical scaling?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Message queues enable asynchronous, decoupled communication between services.", true, "[\"Real-time chat only\",\"Asynchronous communication between services\",\"Database replication\",\"API versioning\"]", "What is a message queue used for?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Sharding distributes data across multiple database instances for scalability.", true, "[\"Encrypting database fields\",\"Partitioning data across multiple databases\",\"Creating database backups\",\"Indexing database tables\"]", "What is database sharding?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Saga pattern manages distributed transactions through a sequence of local transactions.", true, "[\"A logging pattern\",\"A pattern for managing distributed transactions\",\"A caching strategy\",\"An API versioning approach\"]", "What is the Saga pattern in microservices?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 19, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Circuit breaker prevents repeated calls to a failing service, allowing recovery time.", true, "[\"A network security measure\",\"A pattern to prevent cascading failures in distributed systems\",\"A database constraint\",\"An authentication method\"]", "What is circuit breaker pattern?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 20, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "CQRS separates read and write operations for better scalability and performance.", true, "[\"A caching technique\",\"Separating read and write operations into different models\",\"A database type\",\"An API versioning strategy\"]", "What is CQRS (Command Query Responsibility Segregation)?", 2, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 21, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Effective rate limiting considers multiple factors including user tiers and endpoint importance.", true, "[\"Only request count\",\"Request count, user tier, endpoint sensitivity, and time windows\",\"Only user authentication status\",\"Only server capacity\"]", "What factors would you consider when designing a rate limiting strategy?", 2, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 22, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Event sourcing persists state changes as immutable events rather than current state.", true, "[\"Logging all events\",\"Storing state changes as a sequence of events\",\"A pub/sub mechanism\",\"An API documentation approach\"]", "What is event sourcing?", 2, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 23, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Handling high concurrency requires a combination of scaling techniques.", true, "[\"Use a single powerful server\",\"Use load balancing, caching, CDN, database sharding, and async processing\",\"Just increase database size\",\"Only add more API servers\"]", "How would you design a system to handle 1 million concurrent users?", 2, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 24, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Optimistic locking checks for conflicts at commit time, pessimistic locks prevent conflicts.", true, "[\"Optimistic assumes no conflicts, pessimistic locks resources preemptively\",\"They are identical\",\"Optimistic is for reads, pessimistic for writes\",\"Optimistic is always faster\"]", "What is the difference between optimistic and pessimistic locking?", 2, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 25, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Effective cache invalidation combines TTL, pub/sub notifications, and versioning.", true, "[\"Clear all caches periodically\",\"Use TTL, pub/sub invalidation, and versioning strategies\",\"Never invalidate\",\"Only invalidate on server restart\"]", "How would you implement a distributed cache invalidation strategy?", 2, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 26, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Idempotent APIs handle retries safely without side effects.", true, "[\"Only status codes matter\",\"Idempotency keys, proper HTTP methods, and handling duplicate requests\",\"Only GET methods need to be idempotent\",\"Idempotency is not important\"]", "What considerations are important for designing idempotent APIs?", 2, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 27, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "CSS (Cascading Style Sheets) is used to style HTML documents.", true, "[\"Computer Style Sheets\",\"Cascading Style Sheets\",\"Creative Style System\",\"Content Styling Standard\"]", "What does CSS stand for?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 28, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "useState allows functional components to have state.", true, "[\"To make HTTP requests\",\"To manage component state\",\"To create routes\",\"To style components\"]", "What is the purpose of the 'useState' hook in React?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 29, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "The DOM is a programming interface for HTML documents.", true, "[\"Data Object Model\",\"Document Object Model\",\"Digital Output Method\",\"Direct Object Manipulation\"]", "What is the DOM?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 30, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "The alt attribute provides text description for screen readers and when images fail to load.", true, "[\"To make images load faster\",\"To provide alternative text for accessibility\",\"To set image dimensions\",\"To add image effects\"]", "What is the purpose of 'alt' attribute in images?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 31, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Responsive design ensures websites work well on all devices and screen sizes.", true, "[\"Fast-loading websites\",\"Design that adapts to different screen sizes\",\"Websites that respond quickly\",\"Design with animations\"]", "What is responsive design?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 32, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "The Virtual DOM is React's strategy for efficient DOM manipulation.", true, "[\"A new browser feature\",\"A lightweight copy of the actual DOM for efficient updates\",\"A CSS framework\",\"A JavaScript library\"]", "What is the Virtual DOM?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 33, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Event bubbling means events triggered on nested elements propagate up the DOM tree.", true, "[\"A CSS animation\",\"Events propagating from child to parent elements\",\"A JavaScript error\",\"A browser bug\"]", "What is event bubbling?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 34, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "'===' checks both value and type without coercion.", true, "[\"They are identical\",\"\\u0027==\\u0027 compares with type coercion, \\u0027===\\u0027 compares strictly\",\"\\u0027===\\u0027 is deprecated\",\"\\u0027==\\u0027 is for strings only\"]", "What is the difference between '==' and '===' in JavaScript?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 35, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "A closure is a function that has access to variables from its outer scope.", true, "[\"A browser window closing\",\"A function that remembers its outer scope variables\",\"A way to end loops\",\"A CSS property\"]", "What is a closure in JavaScript?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 36, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "useCallback memoizes callback functions to optimize performance.", true, "[\"To fetch data\",\"To memoize functions and prevent unnecessary re-renders\",\"To handle routing\",\"To manage global state\"]", "What is the purpose of useCallback in React?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 37, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Tree shaking eliminates dead code from the final bundle.", true, "[\"A CSS animation\",\"Removing unused code during bundling\",\"A testing technique\",\"A debugging method\"]", "What is tree shaking?", 1, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 38, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Code splitting loads code chunks on demand for better performance.", true, "[\"Writing code in multiple files\",\"Loading code on demand to reduce initial bundle size\",\"A coding style\",\"Splitting CSS and JS\"]", "What is code splitting?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 39, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "React.memo is a HOC that memoizes components based on props.", true, "[\"To store notes in components\",\"To memoize components and prevent unnecessary re-renders\",\"To manage memory\",\"To create memos\"]", "What is the purpose of React.memo?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 40, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "SSR renders HTML on the server, CSR renders in the client's browser.", true, "[\"SSR renders on server, CSR renders in browser\",\"They are identical\",\"SSR is faster always\",\"CSR is for mobile only\"]", "What is the difference between SSR and CSR?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 41, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Web Workers allow scripts to run in background threads without blocking the UI.", true, "[\"To style web pages\",\"To run scripts in background threads\",\"To handle HTTP requests\",\"To manage cookies\"]", "What is the purpose of Web Workers?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 42, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Virtualization renders only visible items, dramatically improving performance for large lists.", true, "[\"Use more useState\",\"Implement virtualization/windowing techniques\",\"Add more CSS\",\"Use setTimeout\"]", "How would you optimize a large list rendering in React?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 43, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Hydration makes server-rendered HTML interactive by attaching event handlers.", true, "[\"Adding water effects\",\"Attaching JavaScript event handlers to server-rendered HTML\",\"A CSS property\",\"A database technique\"]", "What is hydration in the context of SSR?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 44, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "The Critical Rendering Path is the sequence from receiving HTML to displaying pixels.", true, "[\"The most important CSS rules\",\"The sequence of steps browser takes to render a page\",\"A JavaScript function\",\"A routing pattern\"]", "What is the Critical Rendering Path?", 1, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 45, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Micro-frontends allow independent teams to develop and deploy separate frontend modules.", true, "[\"Use only one framework\",\"Use module federation, independent deployments, and shared dependencies\",\"Avoid using any frameworks\",\"Use iframes only\"]", "How would you implement a micro-frontend architecture?", 1, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 46, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Large apps benefit from a layered approach combining different state management strategies.", true, "[\"Use only local state\",\"Combine local, global state, server state caching, and derived state appropriately\",\"Use only Redux\",\"Avoid state management\"]", "What strategies would you use for state management in a large application?", 1, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 47, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Performance optimization requires profiling, identifying bottlenecks, and applying targeted fixes.", true, "[\"Add more components\",\"Profile, analyze renders, memoize, split code, lazy load, and optimize network\",\"Remove all styling\",\"Use class components only\"]", "How would you approach performance optimization for a slow React application?", 1, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 48, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "GraphQL provides flexible queries while REST has fixed endpoints.", true, "[\"They are identical\",\"GraphQL allows clients to request specific data, REST returns fixed responses\",\"REST is newer than GraphQL\",\"GraphQL is only for databases\"]", "What is GraphQL and how does it differ from REST?", 3, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 49, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "JWT (JSON Web Token) enables stateless authentication between client and server.", true, "[\"A JavaScript testing tool\",\"A stateless authentication token for API security\",\"A CSS framework\",\"A database type\"]", "What is a JWT and when would you use it?", 3, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 50, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "CORS controls which origins can access resources from a different domain.", true, "[\"A CSS feature\",\"A security mechanism controlling cross-origin requests\",\"A JavaScript library\",\"A database constraint\"]", "What is CORS and why is it important?", 3, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 51, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Real-time features typically use WebSockets or SSE for bi-directional or server-push communication.", true, "[\"Refresh the page frequently\",\"Use WebSockets, Server-Sent Events, or long polling\",\"Only use REST APIs\",\"Disable caching\"]", "How would you implement real-time features in a web application?", 3, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 52, 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Robust authentication requires multiple security layers and considerations.", true, "[\"Only password strength\",\"Secure storage, token management, MFA, session handling, and rate limiting\",\"Only using HTTPS\",\"Only email verification\"]", "What considerations are important when designing an authentication system?", 3, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 53, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Honesty and willingness to learn are valued traits in collaborative environments.", true, "[\"Pretend you know and try anyway\",\"Honestly say you\\u0027re unfamiliar but offer to learn together or find help\",\"Ignore the request\",\"Tell them to ask someone else\"]", "A colleague asks for help with a task you're unfamiliar with. What should you do?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 54, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Constructive criticism is an opportunity for growth when received with an open mind.", true, "[\"Get defensive and argue\",\"Listen carefully, ask clarifying questions, and use it to improve\",\"Ignore it completely\",\"Criticize the reviewer\\u0027s work in return\"]", "How should you handle constructive criticism of your work?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 55, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Effective communication adapts to the audience's level of understanding.", true, "[\"Use as much jargon as possible\",\"Use analogies and simple language avoiding technical terms\",\"Show them the code\",\"Tell them it\\u0027s too complex to explain\"]", "What is the best way to communicate a technical concept to a non-technical person?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 56, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Transparency and quick action when mistakes occur build trust and minimize impact.", true, "[\"Hide it and hope no one notices\",\"Inform your team immediately and work on a fix\",\"Blame someone else\",\"Wait until someone reports it\"]", "You realize you made a mistake in production code. What should you do first?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 57, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Effective prioritization considers multiple factors beyond perceived urgency.", true, "[\"Work on whatever is easiest first\",\"Assess impact, deadlines, and dependencies to determine true priorities\",\"Ask someone else to decide\",\"Work overtime on everything\"]", "How do you prioritize tasks when everything seems urgent?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 58, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Private, specific, and constructive feedback helps team members improve.", true, "[\"Rewrite their code without telling them\",\"Provide constructive feedback privately with specific suggestions\",\"Complain to the manager immediately\",\"Ignore it\"]", "A team member's code doesn't meet standards. How do you address this?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 59, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Breaking down large projects and acknowledging progress helps maintain motivation.", true, "[\"Just push through regardless of burnout\",\"Break into milestones, celebrate progress, and take regular breaks\",\"Complain frequently\",\"Switch to other projects\"]", "How do you stay motivated when working on a long, complex project?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 60, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Respectful disagreement with reasoning contributes to better decisions.", true, "[\"Stay silent and follow orders\",\"Respectfully present your concerns with supporting reasoning in private\",\"Argue publicly in meetings\",\"Implement your approach anyway\"]", "You disagree with a technical decision made by a senior developer. What do you do?", 9, 1, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 61, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Good code reviews balance quality enforcement with constructive learning.", true, "[\"Finding as many problems as possible\",\"Being constructive, specific, and focused on code quality and learning\",\"Approving everything quickly\",\"Only checking for syntax errors\"]", "What makes a good code review?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 62, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Direct, empathetic communication often resolves interpersonal difficulties.", true, "[\"Avoid them completely\",\"Try to understand their perspective and communicate directly about issues\",\"Complain to others\",\"Request a team change immediately\"]", "How do you handle working with a difficult team member?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 63, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Balancing debt and features requires continuous assessment and communication.", true, "[\"Always prioritize new features\",\"Assess impact, allocate regular time for debt, and communicate tradeoffs\",\"Never work on new features until debt is cleared\",\"Technical debt doesn\\u0027t matter\"]", "How do you balance technical debt against new feature development?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 64, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Proactive, supportive assistance helps team members without undermining their autonomy.", true, "[\"Wait for them to ask\",\"Offer help in a supportive, non-judgmental way\",\"Report them to the manager\",\"Take over their work\"]", "You notice a team member struggling but not asking for help. What do you do?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 65, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Managing scope creep requires documentation, impact assessment, and negotiation.", true, "[\"Accept all changes without question\",\"Document changes, assess impact, and negotiate timeline or resource adjustments\",\"Refuse all changes\",\"Work overtime to include everything\"]", "How do you handle scope creep in a project?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 66, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Proactive, data-driven communication about constraints enables informed decisions.", true, "[\"Promise to meet it anyway\",\"Present data-driven concerns early with alternative proposals\",\"Miss the deadline and explain later\",\"Quit the project\"]", "A project deadline is clearly unrealistic. How do you handle this?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 67, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Effective mentoring balances guidance with opportunities for independent growth.", true, "[\"Tell them exactly what to do\",\"Guide with questions, provide resources, give autonomy, and offer regular feedback\",\"Let them figure everything out alone\",\"Do their work for them\"]", "How do you mentor a junior developer effectively?", 9, 2, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 68, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Influence without authority requires relationship building and persuasive communication.", true, "[\"Force your opinion\",\"Build relationships, present data, understand stakeholders, and find common ground\",\"Give up trying\",\"Escalate immediately\"]", "How do you influence decisions when you don't have direct authority?", 9, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 69, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Professional disagreement followed by committed execution maintains trust.", true, "[\"Comply silently\",\"Voice concerns diplomatically, disagree and commit if overruled\",\"Refuse to implement\",\"Undermine the decision quietly\"]", "How do you handle technical decisions you disagree with from leadership?", 9, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 70, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Innovation requires psychological safety and dedicated time for experimentation.", true, "[\"Assign innovation tasks\",\"Create psychological safety, allocate time for experimentation, celebrate learning from failures\",\"Hire more people\",\"Wait for good ideas to emerge\"]", "How do you foster innovation in an engineering team?", 9, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 71, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Declining performance requires diagnosis, support, and systemic improvements.", true, "[\"Work everyone harder\",\"Diagnose root causes, address issues individually and systemically, adjust processes\",\"Replace team members immediately\",\"Ignore it and hope it improves\"]", "How do you handle a situation where team performance is declining?", 9, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 72, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Business cases for DevEx require quantified benefits and demonstrable outcomes.", true, "[\"Just request budget\",\"Quantify productivity gains, connect to business outcomes, and pilot small improvements\",\"Complain until approved\",\"Implement without approval\"]", "How do you make a case for investing in developer experience improvements?", 9, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 73, 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "Addressing departures requires understanding, respect, and knowledge transfer planning.", true, "[\"Let them go without discussion\",\"Understand their reasons, address what you can, plan for transitions respectfully\",\"Offer more money immediately\",\"Make them feel guilty\"]", "A key team member wants to leave. How do you approach this situation?", 9, 3, null, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 61);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 62);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 63);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 66);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 70);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 71);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 72);

            migrationBuilder.DeleteData(
                table: "AssessmentQuestion",
                keyColumn: "Id",
                keyValue: 73);
        }
    }
}
