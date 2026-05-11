BEGIN TRANSACTION;
DELETE FROM [AssessmentQuestion]
WHERE [Id] = 100;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 101;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 102;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 103;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 104;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 105;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 106;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 107;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 108;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 109;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 110;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 111;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 112;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 113;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 114;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 115;
SELECT @@ROWCOUNT;


DELETE FROM [AssessmentQuestion]
WHERE [Id] = 116;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Options] = N'["Remote Execution Service Technology","Request-Response State Transition","Reliable External System Transport","Representational State Transfer"]'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Options] = N'["GET","PUT","DELETE","POST"]'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Options] = N'["To enforce data constraints","To backup data automatically","To encrypt sensitive data","To speed up data retrieval"]'
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Options] = N'["200","301","500","404"]'
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 2, [Options] = N'["Database queries","Styling web pages","Data interchange between systems","User authentication"]'
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 2, [Options] = N'["PUT is faster than PATCH","They are identical","PUT replaces entire resource, PATCH updates partial resource","PUT creates, PATCH deletes"]'
WHERE [Id] = 6;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Options] = N'["A security vulnerability","A type of API authentication","A database optimization technique","A design pattern for loose coupling"]'
WHERE [Id] = 7;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Options] = N'["Authentication, Confidentiality, Integrity, Durability","Atomicity, Consistency, Isolation, Durability","Access, Control, Identity, Data","Automated, Consistent, Indexed, Distributed"]'
WHERE [Id] = 8;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 1, [Options] = N'["To render HTML templates","To process requests between client and server","To manage database connections only","To store user sessions"]'
WHERE [Id] = 9;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Options] = N'["Executing one query for the list plus one query per item","A database connection pool exhaustion","An SQL syntax error","A network latency issue"]'
WHERE [Id] = 10;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 2, [Options] = N'["Data is always immediately consistent","Data is never consistent","Only writes are consistent","Given enough time, all replicas will converge"]'
WHERE [Id] = 11;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Options] = N'["To compile API code","To store API documentation","To test API endpoints","To serve as a single entry point for microservices"]'
WHERE [Id] = 12;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Options] = N'["To validate user input","To compress response data","To encrypt data","To store frequently accessed data for faster retrieval"]'
WHERE [Id] = 13;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Options] = N'["An API design principle","A distributed system can have at most 2 of: Consistency, Availability, Partition tolerance","A caching strategy","A database optimization rule"]'
WHERE [Id] = 14;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 1, [Options] = N'["They are the same","Horizontal adds more machines, vertical adds more power to existing machines","Vertical is for databases only","Horizontal is always cheaper"]'
WHERE [Id] = 15;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Options] = N'["Database replication","Real-time chat only","API versioning","Asynchronous communication between services"]'
WHERE [Id] = 16;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 2, [Options] = N'["Indexing database tables","Creating database backups","Partitioning data across multiple databases","Encrypting database fields"]'
WHERE [Id] = 17;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Options] = N'["A pattern to prevent cascading failures in distributed systems","An authentication method","A database constraint","A network security measure"]'
WHERE [Id] = 19;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 2, [Options] = N'["A database type","An API versioning strategy","Separating read and write operations into different models","A caching technique"]'
WHERE [Id] = 20;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Options] = N'["Only user authentication status","Request count, user tier, endpoint sensitivity, and time windows","Only request count","Only server capacity"]'
WHERE [Id] = 21;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Options] = N'["An API documentation approach","Storing state changes as a sequence of events","Logging all events","A pub/sub mechanism"]'
WHERE [Id] = 22;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Options] = N'["Use load balancing, caching, CDN, database sharding, and async processing","Just increase database size","Use a single powerful server","Only add more API servers"]'
WHERE [Id] = 23;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 2, [Options] = N'["They are identical","Optimistic is for reads, pessimistic for writes","Optimistic assumes no conflicts, pessimistic locks resources preemptively","Optimistic is always faster"]'
WHERE [Id] = 24;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Options] = N'["Use TTL, pub/sub invalidation, and versioning strategies","Only invalidate on server restart","Clear all caches periodically","Never invalidate"]'
WHERE [Id] = 25;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 1, [Options] = N'["Only GET methods need to be idempotent","Idempotency keys, proper HTTP methods, and handling duplicate requests","Idempotency is not important","Only status codes matter"]'
WHERE [Id] = 26;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 2, [Difficulty] = 1, [Explanation] = N'CSS stands for Cascading Style Sheets, the language used to describe the presentation of a document written in HTML. The acronym ''Computer Style Sheets'' sounds plausible but is incorrect; knowing the full term is foundational for frontend developers. Mastering CSS ensures precise control over layout and visual design.', [Options] = N'["Computer Style Sheets","Creative Style System","Cascading Style Sheets","Content Styling Standard"]', [QuestionText] = N'What does CSS stand for?', [RoleFamily] = 1, [SkillId] = 15
WHERE [Id] = 27;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 1, [Difficulty] = 1, [Explanation] = N'The useState hook allows functional components to hold and update local state, replacing the need for class components. A common mistake is thinking it handles data fetching, but hooks like useEffect serve that purpose. Proper state management is essential for building interactive React applications.', [Options] = N'["To make HTTP requests","To manage component state","To create routes","To style components"]', [QuestionText] = N'What is the purpose of the ''useState'' hook in React?', [RoleFamily] = 1, [SkillId] = 11
WHERE [Id] = 28;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Difficulty] = 1, [Explanation] = N'The DOM (Document Object Model) is a programming interface that represents the page so that programs can change the document structure, style, and content. The tempting wrong answer ''Data Object Model'' muddles the concept with data structures. Understanding the DOM is crucial for effective client‑side scripting.', [Options] = N'["Direct Object Manipulation","Data Object Model","Digital Output Method","Document Object Model"]', [QuestionText] = N'What is the DOM?', [RoleFamily] = 1, [SkillId] = 15
WHERE [Id] = 29;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Difficulty] = 1, [Explanation] = N'The alt attribute supplies alternative text when an image cannot be displayed and is vital for screen readers and SEO. Thinking it speeds up loading confuses function with performance; always including meaningful alt text improves accessibility compliance and user experience.', [Options] = N'["To make images load faster","To set image dimensions","To provide alternative text for accessibility","To add image effects"]', [QuestionText] = N'What is the purpose of the ''alt'' attribute in images?', [RoleFamily] = 1, [SkillId] = 15
WHERE [Id] = 30;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 1, [Difficulty] = 1, [Explanation] = N'Responsive design uses fluid grids, flexible images, and media queries to adapt layouts to varying viewport sizes. The misleading choice ''fast-loading websites'' overlaps with performance concerns, but responsiveness is about adaptability. Implementing responsive design is fundamental for modern multi‑device web experiences.', [Options] = N'["Fast-loading websites","Design that adapts to different screen sizes","Design with animations","Websites that respond quickly"]', [QuestionText] = N'What is responsive design?', [RoleFamily] = 1, [SkillId] = 15
WHERE [Id] = 31;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 1, [Difficulty] = 2, [Explanation] = N'The Virtual DOM is an in‑memory representation of the real DOM, enabling React to batch changes and update only what is necessary. Confusing it with a browser feature misses that it''s a library‑specific optimisation. Understanding the Virtual DOM helps developers write performant React code.', [Options] = N'["A CSS framework","A lightweight copy of the actual DOM for efficient updates","A new browser feature","A JavaScript library"]', [QuestionText] = N'What is the Virtual DOM?', [RoleFamily] = 1, [SkillId] = 11
WHERE [Id] = 32;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Difficulty] = 2, [Explanation] = N'Event bubbling is the process where an event triggered on a nested element travels up through its ancestors, allowing delegated handling. The distraction ''A CSS animation'' misattributes the name to visual effects. Mastering event flow is essential for building robust and scalable UI interactions.', [Options] = N'["Events propagating from child to parent elements","A browser bug","A CSS animation","A JavaScript error"]', [QuestionText] = N'What is event bubbling?', [RoleFamily] = 1, [SeniorityLevel] = 1, [SkillId] = 15
WHERE [Id] = 33;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Difficulty] = 2, [Explanation] = N'The strict equality operator ''==='' checks both value and type without coercion, while ''=='' may convert types leading to unexpected results. Believing they are identical is a common source of difficult‑to‑debug bugs. Using strict comparison by default promotes predictable and safer code.', [Options] = N'["They are identical","\u0027===\u0027 is deprecated","\u0027==\u0027 is for strings only","\u0027==\u0027 compares with type coercion, \u0027===\u0027 compares strictly"]', [QuestionText] = N'What is the difference between ''=='' and ''==='' in JavaScript?', [RoleFamily] = 1, [SeniorityLevel] = 1, [SkillId] = 2
WHERE [Id] = 34;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Explanation] = N'A closure is a function bundled with its lexical environment, allowing it to access variables from an enclosing scope even after that scope has closed. The wrong answer ''A browser window closing'' is a literal misinterpretation. Closures underpin many patterns like data privacy and callbacks in JavaScript.', [Options] = N'["A way to end loops","A function that remembers its outer scope variables","A CSS property","A browser window closing"]', [QuestionText] = N'What is a closure in JavaScript?', [RoleFamily] = 1, [SeniorityLevel] = 1, [SkillId] = 2
WHERE [Id] = 35;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Difficulty] = 3, [Explanation] = N'useCallback returns a memoized version of a callback that only changes if its dependencies change, preventing child components from re‑rendering unnecessarily. It is often confused with useEffect because they share a dependency array, but useCallback is specifically for function reference stability. Using useCallback correctly is key to optimizing React performance in large component trees.', [Options] = N'["To fetch data","To handle routing","To manage global state","To memoize functions and prevent unnecessary re-renders"]', [QuestionText] = N'What is the purpose of useCallback in React?', [RoleFamily] = 1, [SeniorityLevel] = 1, [SkillId] = 11
WHERE [Id] = 36;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Difficulty] = 3, [Explanation] = N'Tree shaking is a build‑time process that eliminates dead code from the final bundle, relying on ES module static structure. The misconception that it''s a debugging technique overlooks its role in production optimisation. Understanding tree shaking encourages developers to write modular imports that reduce payload size.', [Options] = N'["Removing unused code during bundling","A debugging method","A CSS animation","A testing technique"]', [QuestionText] = N'What is tree shaking?', [RoleFamily] = 1, [SeniorityLevel] = 1, [SkillId] = 2
WHERE [Id] = 37;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 1, [Explanation] = N'Code splitting defers loading non‑critical code until needed, improving initial load time. The trap answer ''Writing code in multiple files'' is just modular programming, not on‑demand loading. Implementing code splitting is essential for keeping large applications fast.', [Options] = N'["Splitting CSS and JS","Loading code on demand to reduce initial bundle size","Writing code in multiple files","A coding style"]', [QuestionText] = N'What is code splitting?', [RoleFamily] = 1, [SkillId] = 2
WHERE [Id] = 38;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 2, [Difficulty] = 2, [Explanation] = N'React.memo is a higher‑order component that prevents re‑renders when props remain the same, boosting performance. It is sometimes mistaken for memory management, but its role is purely about render optimisation. Applying React.memo effectively can significantly reduce wasted renders in complex UIs.', [Options] = N'["To store notes in components","To create memos","To memoize components and prevent unnecessary re-renders","To manage memory"]', [QuestionText] = N'What is the purpose of React.memo?', [RoleFamily] = 1, [SkillId] = 11
WHERE [Id] = 39;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 1, [Difficulty] = 2, [Explanation] = N'Server‑Side Rendering (SSR) generates HTML on the server, while Client‑Side Rendering (CSR) builds it in the browser. The fallacy that SSR is always faster ignores that CSR can outperform for subsequent interactions. Choosing the right rendering strategy affects SEO, Time‑to‑Interactive, and user experience.', [Options] = N'["They are identical","SSR renders on server, CSR renders in browser","SSR is always faster","CSR is for mobile only"]', [QuestionText] = N'What is the difference between SSR and CSR?', [RoleFamily] = 1, [SkillId] = 11
WHERE [Id] = 40;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Difficulty] = 2, [Explanation] = N'Web Workers execute JavaScript in background threads, preventing CPU‑heavy tasks from blocking the main UI thread. Confusing them with HTTP requests misses their concurrency model. Using Web Workers keeps interfaces responsive during intensive computations.', [Options] = N'["To run scripts in background threads","To manage cookies","To style web pages","To handle HTTP requests"]', [QuestionText] = N'What is the purpose of Web Workers?', [RoleFamily] = 1, [SkillId] = 2
WHERE [Id] = 41;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 2, [Difficulty] = 3, [Explanation] = N'Virtualization (windowing) renders only the visible items in a long list, drastically reducing DOM nodes and improving performance. Adding more CSS or state hooks does not address the rendering bottleneck. Mastering virtualization is crucial for maintaining smooth scrolling in data‑heavy applications.', [Options] = N'["Use more useState","Add more CSS","Implement virtualization/windowing techniques","Use setTimeout"]', [QuestionText] = N'How would you optimize a large list rendering in React?', [RoleFamily] = 1, [SeniorityLevel] = 2, [SkillId] = 11
WHERE [Id] = 42;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Difficulty] = 3, [Explanation] = N'Hydration is the process of making server‑rendered HTML interactive by attaching event listeners and React state. The literal misinterpretation ''Adding water effects'' is a common joke but shows a misunderstanding. Proper hydration bridges the gap between static SSR content and a fully interactive SPA.', [Options] = N'["Attaching JavaScript event handlers to server-rendered HTML","Adding water effects","A CSS property","A database technique"]', [QuestionText] = N'What is hydration in the context of SSR?', [RoleFamily] = 1, [SeniorityLevel] = 2, [SkillId] = 11
WHERE [Id] = 43;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Difficulty] = 3, [Explanation] = N'The Critical Rendering Path covers the steps from receiving HTML to painting pixels, including DOM, CSSOM, render tree, layout, and paint. The tempting distractor narrows it to CSS alone. Understanding this path is vital for diagnosing and resolving rendering performance bottlenecks.', [Options] = N'["A JavaScript function","A routing pattern","The most important CSS rules","The sequence of steps the browser takes to render a page"]', [QuestionText] = N'What is the Critical Rendering Path?', [SeniorityLevel] = 2
WHERE [Id] = 44;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Difficulty] = 3, [Explanation] = N'A successful micro‑frontend approach leverages module federation to compose independent modules at runtime, enabling teams to deploy autonomously while sharing common dependencies. Relying solely on iframes ignores modern runtime orchestration and can hamper performance. Implementing micro‑frontends scales development across large organizations.', [Options] = N'["Use module federation, independent deployments, and shared dependencies","Avoid using any frameworks","Use only one framework","Use iframes only"]', [QuestionText] = N'How would you implement a micro-frontend architecture?', [SeniorityLevel] = 3, [SkillId] = 2
WHERE [Id] = 45;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 2, [Difficulty] = 3, [Explanation] = N'Large applications benefit from a stratified approach: local UI state, global shared state (via Context/Redux), server cache (React Query/SWR), and computed selectors. The ''only Redux'' dogma oversimplifies the problem and leads to boilerplate. Thoughtful state separation improves maintainability and performance.', [Options] = N'["Avoid state management","Use only Redux","Combine local, global state, server state caching, and derived state appropriately","Use only local state"]', [QuestionText] = N'What strategies would you use for state management in a large application?', [SeniorityLevel] = 3, [SkillId] = 11
WHERE [Id] = 46;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Difficulty] = 3, [Explanation] = N'Performance optimization begins with profiling (React DevTools, Lighthouse) to pinpoint bottlenecks, then applying techniques like memoization, code splitting, lazy loading, and efficient data fetching. Simply piling on more components exacerbates the issue. A structured, data‑driven optimization process reliably improves user experience.', [Options] = N'["Add more components","Profile, analyze renders, memoize, split code, lazy load, and optimize network","Use class components only","Remove all styling"]', [QuestionText] = N'How would you approach performance optimization for a slow React application?', [SeniorityLevel] = 3, [SkillId] = 11
WHERE [Id] = 47;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Difficulty] = 2, [Explanation] = N'GraphQL gives clients the power to ask for exactly the data they need, avoiding over/under‑fetching common in REST. The belief that it replaces databases confuses the query language with storage systems. Adopting GraphQL can streamline data fetching in complex frontend‑backend integrations.', [Options] = N'["GraphQL is only for databases","GraphQL allows clients to request specific data, REST returns fixed responses","They are identical","REST is newer than GraphQL"]', [QuestionText] = N'What is GraphQL and how does it differ from REST?', [RoleFamily] = 3, [SkillId] = 44
WHERE [Id] = 48;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Explanation] = N'JWT (JSON Web Token) encodes claims in a compact, self‑contained token, enabling stateless authentication across distributed services. Mistaking it for a testing tool overlooks its critical role in securing APIs. Using JWTs correctly requires careful handling of secrets, expiration, and HTTPS.', [Options] = N'["A CSS framework","A database type","A JavaScript testing tool","A stateless authentication token for API security"]', [QuestionText] = N'What is a JWT and when would you use it?', [RoleFamily] = 3, [SkillId] = 43
WHERE [Id] = 49;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Explanation] = N'CORS (Cross-Origin Resource Sharing) allows servers to specify who can access their resources, preventing unauthorized cross‑site requests. The wrong answer ''A CSS feature'' sounds plausible but relates to styling. Properly configuring CORS is essential for secure API consumption from browsers.', [Options] = N'["A JavaScript library","A security mechanism controlling cross-origin requests","A CSS feature","A database constraint"]', [QuestionText] = N'What is CORS and why is it important?', [RoleFamily] = 3, [SeniorityLevel] = 2, [SkillId] = 43
WHERE [Id] = 50;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Difficulty] = 3, [Explanation] = N'Real‑time communication relies on WebSockets for full‑duplex channels, SSE for server‑to‑client streams, or long polling as a fallback. Simply refreshing the page is inefficient and delivers poor UX. Choosing the right real‑time transport ensures responsive, collaborative features.', [Options] = N'["Use WebSockets, Server-Sent Events, or long polling","Only use REST APIs","Disable caching","Refresh the page frequently"]', [QuestionText] = N'How would you implement real-time features in a web application?', [RoleFamily] = 3, [SeniorityLevel] = 2
WHERE [Id] = 51;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Difficulty] = 3, [Explanation] = N'Robust authentication requires layered security: hashed passwords, secure token lifecycle (JWT expiry/rotation), multi‑factor authentication, session invalidation, and brute‑force protection. The oversimplified focus on password strength alone invites breaches. A holistic approach minimizes attack surfaces and protects user data.', [Options] = N'["Only using HTTPS","Secure storage, token management, MFA, session handling, and rate limiting","Only password strength","Only email verification"]', [QuestionText] = N'What considerations are important when designing an authentication system?', [RoleFamily] = 3, [SeniorityLevel] = 3, [SkillId] = 43
WHERE [Id] = 52;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [CorrectAnswerIndex] = 0, [Difficulty] = 1, [Explanation] = N'Admitting unfamiliarity and offering to collaborate builds trust and fosters a learning culture. Pretending to know can lead to mistakes and erodes credibility. In real teams, honesty and a willingness to learn together strengthen team resilience.', [Options] = N'["Honestly say you\u0027re unfamiliar but offer to learn together or find help","Pretend you know and try anyway","Ignore the request","Tell them to ask someone else"]', [QuestionText] = N'A colleague asks for help with a task you''re unfamiliar with. What should you do?', [RoleFamily] = 9, [SkillId] = 48
WHERE [Id] = 53;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [CorrectAnswerIndex] = 0, [Difficulty] = 1, [Explanation] = N'Receiving feedback with an open mind turns criticism into a growth opportunity. Defensiveness shuts down dialogue and prevents improvement. Embracing constructive feedback is a hallmark of a professional developer who continuously improves product quality.', [Options] = N'["Listen carefully, ask clarifying questions, and use it to improve","Get defensive and argue","Criticize the reviewer\u0027s work in return","Ignore it completely"]', [QuestionText] = N'How should you handle constructive criticism of your work?', [RoleFamily] = 9, [SkillId] = 47
WHERE [Id] = 54;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [Difficulty] = 1, [Explanation] = N'Simplifying with analogies makes technical ideas accessible to stakeholders, aligning expectations and reducing misunderstandings. Using jargon alienates the audience and hinders collaboration. Effective communication bridges the gap between developers and business, ensuring shared vision.', [Options] = N'["Use as much jargon as possible","Use analogies and simple language avoiding technical terms","Tell them it\u0027s too complex to explain","Show them the code"]', [QuestionText] = N'What is the best way to communicate a technical concept to a non-technical person?', [RoleFamily] = 9, [SeniorityLevel] = 1, [SkillId] = 48
WHERE [Id] = 55;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [CorrectAnswerIndex] = 3, [Difficulty] = 1, [Explanation] = N'Promptly owning up to mistakes and initiating a fix minimizes impact and maintains team trust. Concealing errors can compound problems and damage reputation. Transparency in incident response is critical for reliable engineering cultures.', [Options] = N'["Hide it and hope no one notices","Blame someone else","Wait until someone reports it","Inform your team immediately and work on a fix"]', [QuestionText] = N'You realize you made a mistake in production code. What should you do first?', [RoleFamily] = 9, [SeniorityLevel] = 1, [SkillId] = 47
WHERE [Id] = 56;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [CorrectAnswerIndex] = 2, [Explanation] = N'Effective prioritization evaluates business impact, deadlines, and task dependencies, not just ease. The easy‑first approach may neglect critical high‑impact tasks, causing project bottlenecks. Using a structured prioritisation framework ensures resources are allocated to what truly matters.', [Options] = N'["Work on whatever is easiest first","Ask someone else to decide","Assess impact, deadlines, and dependencies to determine true priorities","Work overtime on everything"]', [QuestionText] = N'How do you prioritize tasks when everything seems urgent?', [RoleFamily] = 9, [SeniorityLevel] = 1, [SkillId] = 47
WHERE [Id] = 57;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [CorrectAnswerIndex] = 3, [Explanation] = N'Private, specific, and kind feedback encourages improvement without embarrassment. Rewriting secretly undermines trust and skips a teaching moment. Constructive code reviews strengthen the team''s collective code quality.', [Options] = N'["Ignore it","Rewrite their code without telling them","Complain to the manager immediately","Provide constructive feedback privately with specific suggestions"]', [QuestionText] = N'A team member''s code doesn''t meet standards. How do you address this?', [RoleFamily] = 9, [SeniorityLevel] = 1, [SkillId] = 48
WHERE [Id] = 58;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [CorrectAnswerIndex] = 3, [Difficulty] = 2, [Explanation] = N'A sustainable approach dedicates a portion of each cycle to pay down debt while still delivering value, with transparent communication about tradeoffs. Ignoring debt entirely accelerates entropy and slows future velocity. Managing this balance is a core senior responsibility.', [Options] = N'["Always prioritize new features","Never work on new features until debt is cleared","Technical debt doesn\u0027t matter","Assess impact, allocate regular time for debt, and communicate tradeoffs"]', [QuestionText] = N'How do you balance technical debt against new feature development?', [RoleFamily] = 9, [SkillId] = 49
WHERE [Id] = 59;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [CorrectAnswerIndex] = 3, [Difficulty] = 2, [Explanation] = N'Proactively offering support respects their autonomy while preventing prolonged blockers. Waiting can delay the project and increase frustration. A supportive culture encourages everyone to seek and receive help, enhancing overall team throughput.', [Options] = N'["Wait for them to ask","Report them to the manager","Take over their work","Offer help in a supportive, non-judgmental way"]', [QuestionText] = N'You notice a team member struggling but not asking for help. What do you do?', [RoleFamily] = 9, [SkillId] = 48
WHERE [Id] = 60;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [Difficulty] = 2, [Explanation] = N'Managing scope creep requires documenting requested changes, analyzing their effect on schedule/budget, and renegotiating commitments. Simply refusing everything can damage stakeholder relationships, while accepting all leads to missed deadlines. Controlled scope management protects the team and delivers reliable outcomes.', [Options] = N'["Work overtime to include everything","Document changes, assess impact, and negotiate timeline or resource adjustments","Refuse all changes","Accept all changes without question"]', [QuestionText] = N'How do you handle scope creep in a project?', [RoleFamily] = 9, [SkillId] = 49
WHERE [Id] = 61;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [CorrectAnswerIndex] = 2, [Explanation] = N'Raising evidence‑based concerns early with viable alternatives allows stakeholders to make informed tradeoffs. Promising the impossible erodes trust later; failing silently damages delivery. Proactive risk communication is a hallmark of engineering maturity.', [Options] = N'["Quit the project","Miss the deadline and explain later","Present data-driven concerns early with alternative proposals","Promise to meet it anyway"]', [QuestionText] = N'A project deadline is clearly unrealistic. How do you handle this?', [RoleFamily] = 9, [SeniorityLevel] = 2, [SkillId] = 49
WHERE [Id] = 62;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [CorrectAnswerIndex] = 0, [Explanation] = N'Effective mentoring uses guided discovery, allowing juniors to solve problems while providing a safety net. Prescriptive instructions stifle growth; abandoning them to struggle is demoralizing. Mentorship accelerates learning and builds a stronger, more autonomous team.', [Options] = N'["Guide with questions, provide resources, give autonomy, and offer regular feedback","Tell them exactly what to do","Do their work for them","Let them figure everything out alone"]', [QuestionText] = N'How do you mentor a junior developer effectively?', [RoleFamily] = 9, [SeniorityLevel] = 2, [SkillId] = 47
WHERE [Id] = 63;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [Difficulty] = 2, [Explanation] = N'Influence without authority relies on credibility, empathy, and aligning proposals with stakeholders'' goals. Forcing opinions or escalating prematurely undermines collaboration. This skill is essential for architects and senior engineers driving technical direction.', [Options] = N'["Force your opinion","Build relationships, present data, understand stakeholders, and find common ground","Give up trying","Escalate immediately"]', [QuestionText] = N'How do you influence decisions when you don''t have direct authority?', [RoleFamily] = 9, [SkillId] = 48
WHERE [Id] = 64;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [CorrectAnswerIndex] = 2, [Explanation] = N'Professional ''disagree and commit'' preserves team cohesion after decisions are made, while ensuring your perspective is heard. Refusing to implement or passive sabotage breeds toxicity. This balance maintains respect and enables execution.', [Options] = N'["Refuse to implement","Comply silently","Voice concerns diplomatically, disagree and commit if overruled","Undermine the decision quietly"]', [QuestionText] = N'How do you handle technical decisions you disagree with from leadership?', [RoleFamily] = 9, [SeniorityLevel] = 3, [SkillId] = 49
WHERE [Id] = 65;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [CorrectAnswerIndex] = 0, [Difficulty] = 3, [Explanation] = N'Innovation thrives when team members feel safe to propose and test ideas without fear of blame. Merely assigning ''innovation'' without cultural support yields few results. Deliberate practices like hackathons and blameless post‑mortems nurture creative problem‑solving.', [Options] = N'["Create psychological safety, allocate time for experimentation, celebrate learning from failures","Assign innovation tasks","Hire more people","Wait for good ideas to emerge"]', [QuestionText] = N'How do you foster innovation in an engineering team?', [RoleFamily] = 9, [SeniorityLevel] = 3, [SkillId] = 47
WHERE [Id] = 66;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [CorrectAnswerIndex] = 3, [Difficulty] = 3, [Explanation] = N'Decline often stems from systemic issues, burnout, or unclear goals; a leader investigates, supports, and improves processes. Driving harder only accelerates burnout. Thoughtful intervention that tackles both people and process restores sustainable performance.', [Options] = N'["Replace team members immediately","Work everyone harder","Ignore it and hope it improves","Diagnose root causes, address issues individually and systemically, adjust processes"]', [QuestionText] = N'How do you handle a situation where team performance is declining?', [RoleFamily] = 9, [SeniorityLevel] = 3, [SkillId] = 49
WHERE [Id] = 67;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [Explanation] = N'A compelling business case translates faster build times, reduced errors, and developer satisfaction into revenue or cost savings. Vague requests lack credibility; piloting shows tangible results. Linking DevEx to business metrics secures investment and aligns engineering with company goals.', [Options] = N'["Complain until approved","Quantify productivity gains, connect to business outcomes, and pilot small improvements","Just request budget","Implement without approval"]', [QuestionText] = N'How do you make a case for investing in developer experience improvements?', [RoleFamily] = 9, [SeniorityLevel] = 3, [SkillId] = 49
WHERE [Id] = 68;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 2, [Explanation] = N'Listening to exit motivations may reveal fixable issues and shows respect, while proper knowledge transfer mitigates risk. Only throwing money at the problem may ignore deeper concerns. Handling departures gracefully preserves relationships and maintains team morale.', [Options] = N'["Let them go without discussion","Understand their reasons, address what you can, plan for transitions respectfully","Make them feel guilty","Offer more money immediately"]', [QuestionText] = N'A key team member wants to leave. How do you approach this situation?', [RoleFamily] = 9, [SkillId] = 48
WHERE [Id] = 69;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [Explanation] = N'@Component declares a class as a component with a template and selector. @Directive is for attribute/structural directives without a view, a common confusion. Mastering component decorators is the first step to building any Angular UI.', [Options] = N'["@NgModule","@Component","@Directive","@Injectable"]', [QuestionText] = N'What decorator is used to define a component class in Angular?', [RoleFamily] = 1, [SkillId] = 12
WHERE [Id] = 70;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [CorrectAnswerIndex] = 0, [Explanation] = N'The `percent` pipe formats a number as a percentage (e.g., 0.5 → ''50%''). `currency` is for money formatting, which is a frequent mix-up. Correct pipe usage ensures data is presented in a locale-sensitive and readable way.', [Options] = N'["percent","currency","number","decimal"]', [QuestionText] = N'Which Angular pipe transforms a number into a percentage string?', [RoleFamily] = 1, [SkillId] = 12
WHERE [Id] = 71;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [CorrectAnswerIndex] = 0, [Explanation] = N'*ngIf adds/removes elements from the DOM, while [hidden] just toggles CSS visibility, leaving elements in the DOM and potentially hurting performance. Understanding this distinction avoids unnecessary DOM bloat.', [Options] = N'["*ngIf","*ngSwitch","[hidden]","*ngFor"]', [QuestionText] = N'In Angular templates, which structural directive conditionally includes a template based on a truthy expression?', [RoleFamily] = 1, [SkillId] = 12
WHERE [Id] = 72;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [CorrectAnswerIndex] = 0, [Explanation] = N'Property binding syntax `[attr.title]` sets a native attribute from a component field. Interpolation `{{property}}` is for text content, not attributes. Knowing the correct binding syntax is essential for dynamic and accessible HTML.', [Options] = N'["[attr.title]=\u0022property\u0022","(click)=\u0022property\u0022","{{property}}","[(ngModel)]=\u0022property\u0022"]', [QuestionText] = N'How do you bind a component property to an HTML attribute in one-way binding?', [RoleFamily] = 1, [SkillId] = 12
WHERE [Id] = 73;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [CorrectAnswerIndex] = 0, [Difficulty] = 1, [Explanation] = N'ngOnInit fires after the first ngOnChanges, signalling that data-bound inputs are set. Attempting to access @Input in the constructor leads to undefined values, a common pitfall. Initialisation logic belongs in ngOnInit.', [Options] = N'["ngOnInit","ngDoCheck","ngAfterViewInit","ngOnChanges"]', [QuestionText] = N'Which lifecycle hook is called once after the first `ngOnChanges` and is used for component initialization?', [RoleFamily] = 1, [SkillId] = 12
WHERE [Id] = 74;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [CorrectAnswerIndex] = 2, [Difficulty] = 1, [Explanation] = N'Declarations make components, directives, and pipes available inside the module. Mixing it up with `imports` results in ''not a known element'' errors. Accurate module declaration is the foundation of template compilation.', [Options] = N'["To bootstrap the root component","To define services provided by the module","To register components, directives, and pipes that belong to the module","To import other modules"]', [QuestionText] = N'What is the primary purpose of the `declarations` array in an NgModule?', [RoleFamily] = 1, [SkillId] = 12
WHERE [Id] = 75;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [CorrectAnswerIndex] = 2, [Difficulty] = 1, [Explanation] = N'HttpClient from @angular/common/http is the modern API with interceptors and typed responses. The deprecated `Http` still appears in legacy code, causing compatibility issues. Using HttpClient ensures robust and testable HTTP communication.', [Options] = N'["XHRService","Http","HttpClient","FetchService"]', [QuestionText] = N'Which service is used to make HTTP requests in Angular?', [RoleFamily] = 1, [SeniorityLevel] = 1, [SkillId] = 12
WHERE [Id] = 76;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [CorrectAnswerIndex] = 2, [Difficulty] = 1, [Explanation] = N'Angular uses the `let item of items` microsyntax. Using `in` instead of `of` is a mistake carried over from other frameworks. Proper *ngFor usage avoids template compilation errors and renders lists correctly.', [Options] = N'["*ngRepeat=\u0022item in items\u0022","*ngFor=\u0022item in items\u0022","*ngFor=\u0022let item of items\u0022","*ngFor=\u0022#item of items\u0022"]', [QuestionText] = N'What is the correct syntax to iterate over an array `items` and display each item in a template?', [RoleFamily] = 1, [SeniorityLevel] = 1, [SkillId] = 12
WHERE [Id] = 77;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [Difficulty] = 1, [Explanation] = N'ngModel creates a FormControl instance and enables two-way binding in template-driven forms. `formControl` is for reactive forms, and mixing them leads to inconsistent form APIs. Understanding ngModel helps build quick, reliable forms.', [Options] = N'["formGroup","ngModel","ngControl","formControl"]', [QuestionText] = N'In template-driven forms, which directive links an input element to a form control?', [RoleFamily] = 1, [SeniorityLevel] = 1, [SkillId] = 12
WHERE [Id] = 78;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [CorrectAnswerIndex] = 2, [Difficulty] = 1, [Explanation] = N'Constructor injection with an access modifier (private) is the standard DI pattern. The `@Inject` decorator is for custom injection tokens, not simple class dependencies. This pattern promotes testability and clean separation of concerns.', [Options] = N'["providers: [DataService] inside component","@Inject property dataService: DataService","constructor(private dataService: DataService)","inject(DataService) as a property"]', [QuestionText] = N'What is the correct way to inject a service named `DataService` into a component?', [RoleFamily] = 1, [SeniorityLevel] = 1, [SkillId] = 12
WHERE [Id] = 79;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [CorrectAnswerIndex] = 3, [Difficulty] = 2, [Explanation] = N'`map` applies a projection function to each value, like Array.map. `tap` is often misused for transformations but only performs side effects. Correct use of `map` is essential for shaping data in reactive streams.', [Options] = N'["switchMap","tap","filter","map"]', [QuestionText] = N'Which RxJS operator would you use to transform values emitted by an observable before they reach subscribers?', [RoleFamily] = 1, [SkillId] = 12
WHERE [Id] = 80;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [CorrectAnswerIndex] = 3, [Explanation] = N'`takeUntil` completes the observable when the notifier emits, providing a single cleanup point. The array-loop approach is error-prone and still requires manual steps. This pattern prevents memory leaks in complex components with many subscriptions.', [Options] = N'["Letting the framework handle it via zone.js","Calling \u0060unsubscribeAll()\u0060 on a composite subscription","Storing subscriptions in an array and looping to unsubscribe","Using a \u0060takeUntil\u0060 notifier with a subject that completes in \u0060ngOnDestroy\u0060"]', [QuestionText] = N'What is the recommended pattern to unsubscribe from multiple subscriptions in a component without calling unsubscribe() on each?', [RoleFamily] = 1, [SeniorityLevel] = 2, [SkillId] = 12
WHERE [Id] = 81;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [Explanation] = N'OnPush marks the component to be checked only when an input reference changes (or events/async pipe trigger it), drastically reducing checks. The misconception is that it disables CD completely—it still responds to observable emissions via async pipe. Adopting OnPush improves rendering performance in large apps.', [Options] = N'["It batches all change detection cycles into one","It skips change detection for the component and its subtree unless an @Input reference changes or an event originates internally","It only checks primitive input values","It disables change detection entirely, requiring manual updates"]', [QuestionText] = N'What performance benefit does `ChangeDetectionStrategy.OnPush` provide?', [RoleFamily] = 1, [SeniorityLevel] = 2, [SkillId] = 12
WHERE [Id] = 82;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [Difficulty] = 2, [Explanation] = N'`providedIn: ''root''` registers the service with the root injector, making it a tree-shakeable singleton. `@Singleton()` does not exist in Angular, and using it would cause a compilation error. This approach simplifies DI and improves bundle optimization.', [Options] = N'["@Singleton()","@Injectable({ providedIn: \u0027root\u0027 })","@ProvideInRoot()","@Injectable({ scope: \u0027root\u0027 })"]', [QuestionText] = N'Which decorator configures a service to be a singleton available application-wide without adding it to a module’s providers?', [RoleFamily] = 1, [SeniorityLevel] = 2, [SkillId] = 12
WHERE [Id] = 83;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [Difficulty] = 2, [Explanation] = N'`forRoot()` initialises the router service and declares root routes; `forChild()` registers additional routes without duplicating the router service. Misusing them can create multiple router instances, causing navigation glitches. This pattern ensures a single router service across the application.', [Options] = N'["\u0060forRoot()\u0060 and \u0060forChild()\u0060 are identical","\u0060forRoot()\u0060 provides the router service and route configuration; \u0060forChild()\u0060 only adds routes","\u0060forRoot()\u0060 is for lazy loading, \u0060forChild()\u0060 for eager loading","\u0060forRoot()\u0060 is for AppModule, \u0060forChild()\u0060 for feature modules with separate routing instance"]', [QuestionText] = N'In Angular routing, what is the difference between `RouterModule.forRoot()` and `RouterModule.forChild()`?', [RoleFamily] = 1, [SeniorityLevel] = 2, [SkillId] = 12
WHERE [Id] = 84;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [CorrectAnswerIndex] = 0, [Difficulty] = 2, [Explanation] = N'The dynamic import syntax enables code splitting and on-demand loading. The deprecated string syntax still appears in older codebases but is discouraged. Lazy loading drastically improves initial load time by deferring non-critical bundles.', [Options] = N'["loadChildren: () =\u003E import(\u0027./feature/feature.module\u0027).then(m =\u003E m.FeatureModule)","lazy: true, module: FeatureModule","component: FeatureModule","loadChildren: \u0027./feature/feature.module#FeatureModule\u0027"]', [QuestionText] = N'How do you configure a route to lazy-load a feature module?', [RoleFamily] = 1, [SeniorityLevel] = 2, [SkillId] = 12
WHERE [Id] = 85;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Category] = 1, [CorrectAnswerIndex] = 3, [Difficulty] = 2, [Explanation] = N'`trackBy` returns a unique identifier so Angular can reuse existing DOM elements instead of re-rendering the entire list. Without it, object identity comparisons cause unnecessary DOM updates, degrading performance on large lists. Using `trackBy` is a key optimisation for dynamic collections.', [Options] = N'["It enables two-way binding for each item","It specifies the order of iteration","It tracks the number of iterations for debugging","It helps Angular identify which items have changed, added, or removed, minimizing DOM manipulations"]', [QuestionText] = N'What is the purpose of the `trackBy` function in an `*ngFor` loop?', [RoleFamily] = 1, [SeniorityLevel] = 2, [SkillId] = 12
WHERE [Id] = 86;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Difficulty] = 2, [Explanation] = N'This error indicates a property was modified after the change detection pass completed, often in lifecycle hooks like `ngAfterViewInit`. Forgetting to wrap such changes in `setTimeout` or `Promise.resolve()` is a frequent mistake. Recognising this error helps developers maintain a unidirectional data flow and avoid view inconsistencies.', [Options] = N'["Calling \u0060detectChanges()\u0060 manually","Missing \u0060@Input\u0060 decorator on a property","Using an Observable without the async pipe","Changing a component\u0027s property in \u0060ngAfterViewInit\u0060 that affects the view after Angular has already checked it"]', [QuestionText] = N'What commonly causes the `ExpressionChangedAfterItHasBeenCheckedError` in development mode?', [SeniorityLevel] = 2
WHERE [Id] = 87;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Difficulty] = 2, [Explanation] = N'The async pipe handles subscription lifecycle, eliminating manual unsubscription and preventing memory leaks. A common mistake is subscribing manually and storing a subscription reference. Embracing the async pipe leads to cleaner, reactive template code.', [Options] = N'["Multicasts the Observable to multiple subscribers","Emits values synchronously","Converts the Observable into a Promise","Automatically subscribes, returns the latest emitted value, and unsubscribes when the component is destroyed"]', [QuestionText] = N'What does the `async` pipe do when used with an Observable in a template?', [SeniorityLevel] = 2
WHERE [Id] = 88;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Difficulty] = 2, [Explanation] = N'The `CanActivate` interface''s `canActivate()` method controls navigation. Forgetting to list the guard in the route’s `canActivate` array leaves the route unprotected. Route guards are essential for enforcing authentication and authorisation.', [Options] = N'["Create a service that implements the \u0060CanActivate\u0060 interface and provide it in the route\u2019s \u0060canActivate\u0060 array","Implement \u0060onActivate\u0060 method in the component","Use the \u0060@Guard\u0060 decorator","Set \u0060canActivate: true\u0060 in the route configuration"]', [QuestionText] = N'How do you implement a route guard that checks if a user can activate a route?', [SeniorityLevel] = 2
WHERE [Id] = 89;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Difficulty] = 3, [Explanation] = N'Angular’s hierarchical injector delegates from the component injector up through parents and ultimately the root module injector. A common misconception is that services are always globally singleton, but providing a service at a component level scopes a new instance. Mastering this hierarchy allows intentional service scoping for state management.', [Options] = N'["It only looks at the root module injector","It checks providers of the component\u2019s parent directive only","It first checks its own providers; if not found, walks up the component injector tree to the root, then to the module injector","It merges all ancestor providers into a flat list"]', [QuestionText] = N'In Angular''s hierarchical dependency injection, how does a child component’s injector resolve a dependency?', [SeniorityLevel] = 3
WHERE [Id] = 90;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Difficulty] = 3, [Explanation] = N'`static: true` tells Angular the queried element is not inside a structural directive and can be resolved early, useful for elements always present. Using `static: false` when the element is within `*ngIf` is required because the element doesn’t exist until later. Misconfiguring leads to undefined references or logic timing issues.', [Options] = N'["With \u0060static: true\u0060 the query result is available in \u0060ngOnInit\u0060; with \u0060false\u0060 it\u2019s available in \u0060ngAfterViewInit\u0060","\u0060static: true\u0060 resolves synchronously, \u0060false\u0060 asynchronously","\u0060static: true\u0060 is for template-driven forms, \u0060false\u0060 for reactive","There is no difference; it\u0027s a deprecated option"]', [QuestionText] = N'What is the difference between `@ViewChild` with `{ static: true }` versus `{ static: false }`?', [SeniorityLevel] = 3
WHERE [Id] = 91;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 0, [Difficulty] = 3, [Explanation] = N'`markForCheck` traverses up marking the path for the next detection cycle, ideal for OnPush components with asynchronous updates. `detectChanges` runs CD synchronously, useful for imperative DOM reads/writes. Confusing the two can cause either performance degradation or missed view updates.', [Options] = N'["Call \u0060markForCheck()\u0060 to mark the component and ancestors for a check in the next cycle; call \u0060detectChanges()\u0060 to run change detection immediately on the component and its children","Use \u0060ApplicationRef.tick()\u0060 to detect changes only in the current component","Call \u0060runChangeDetection()\u0060 to trigger global CD","Call \u0060checkChanges()\u0060 on the component instance"]', [QuestionText] = N'How can you manually control change detection using `ChangeDetectorRef`?', [SeniorityLevel] = 3
WHERE [Id] = 92;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Difficulty] = 3, [Explanation] = N'Zone.js intercepts async callbacks (setTimeout, XHR, events) so Angular knows to run change detection. This patching can lead to excessive CD cycles, especially with many microtasks. Running code outside Angular''s zone with `ngZone.runOutsideAngular` is a common optimisation technique for heavy non‑Angular async operations.', [Options] = N'["It enables template type-checking","Zone.js monkey-patches browser APIs to notify Angular when asynchronous operations complete, triggering change detection; it can cause performance overhead by running CD too frequently","It handles HTTP caching","It provides a virtual DOM diffing algorithm"]', [QuestionText] = N'Why does Angular use zone.js and what is a key drawback?', [SeniorityLevel] = 3
WHERE [Id] = 93;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 2, [Difficulty] = 3, [Explanation] = N'Retained Observables prevent garbage collection of components. Heap snapshots reveal detached DOM elements and component instances still in memory. Developers often forget to unsubscribe when navigating away, causing cumulative leaks; proper teardown logic is essential for long-running SPAs.', [Options] = N'["Rely on zone.js to automatically clean up subscriptions","Set \u0060subscription = null\u0060 in \u0060ngOnInit\u0060","Use browser dev tools to take heap snapshots and look for retained component instances; check that subscriptions are unsubscribed using \u0060takeUntil\u0060 or \u0060async\u0060 pipe","Check the console for \u0027memory leak\u0027 warnings"]', [QuestionText] = N'How can you debug a memory leak caused by a long-lived Observable subscription?', [SeniorityLevel] = 3
WHERE [Id] = 94;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 1, [Difficulty] = 3, [Explanation] = N'`APP_INITIALIZER` accepts a factory that returns a Promise, and Angular waits for all initializers to resolve before starting the app. A common mistake is using it for module preloading; that is handled by preloading strategies. Loading runtime configuration dynamically avoids hardcoding environment-specific values.', [Options] = N'["It pre-compiles templates at build time","It allows running functions before the application bootstraps, often used to load configuration from a server","It initializes lazy-loaded modules","It registers service workers"]', [QuestionText] = N'What is the purpose of the `APP_INITIALIZER` injection token?', [SeniorityLevel] = 3
WHERE [Id] = 95;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 1, [Difficulty] = 3, [Explanation] = N'AOT removes the Angular compiler from the client bundle and resolves templates ahead of time, improving security and performance. JIT is convenient for development but leads to larger bundles and slower boot in production. Enabling AOT is a fundamental production optimisation step.', [Options] = N'["AOT only minimizes JavaScript files, JIT reduces CSS","AOT compiles templates at build time, resulting in smaller bundles, faster rendering, and catching template errors early; JIT compiles in the browser, increasing load and render times","AOT enables dynamic component loading, JIT disables it","AOT and JIT produce identical output; AOT is just a convention"]', [QuestionText] = N'Compare AOT (Ahead-of-Time) compilation with JIT (Just-in-Time) and explain an advantage for production.', [SeniorityLevel] = 3
WHERE [Id] = 96;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 3, [Difficulty] = 3, [Explanation] = N'`switchMap` unsubscribes from the previous inner observable on each new value, perfect for typeahead to avoid outdated responses. `mergeMap` would cause race conditions and potentially display wrong results. Choosing the correct flattening operator is critical for building responsive search UIs.', [Options] = N'["\u0060concatMap\u0060 to preserve order but not cancel","\u0060exhaustMap\u0060 to ignore new emissions while a request is in-flight","\u0060mergeMap\u0060 to allow all requests in parallel","\u0060switchMap\u0060 to cancel the previous inner observable when a new emission arrives"]', [QuestionText] = N'Which combination of higher-order mapping operators would you use for a typeahead where the user types, and you want to cancel the previous search request when a new character is typed?', [SeniorityLevel] = 3
WHERE [Id] = 97;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [CorrectAnswerIndex] = 1, [Difficulty] = 3, [Explanation] = N'A combination of lazy loading, tree-shaking, and bundle analysis directly shrinks initial payloads. Simply removing all third-party libraries is impractical and ignores the real gains from code splitting. Proactive bundle management is essential for fast load times and good user experience.', [Options] = N'["Using JIT compilation in production","Lazy-loading feature modules, enabling tree-shaking, using the Angular CLI budget settings, and analyzing bundles with tools like \u0060source-map-explorer\u0060","Removing all third-party libraries","Setting \u0060enableProdMode(false)\u0060"]', [QuestionText] = N'Which strategy reduces Angular application bundle size effectively?', [SeniorityLevel] = 3
WHERE [Id] = 98;
SELECT @@ROWCOUNT;


UPDATE [AssessmentQuestion] SET [Difficulty] = 3, [Explanation] = N'Ivy compiles component metadata to static fields, enabling superior tree-shaking and reduced bundle size, plus clearer error locations. Developers sometimes assume Ivy changes the component API, but it’s largely backward compatible. Understanding Ivy’s advantages helps teams leverage modern Angular performance features.', [Options] = N'["Ivy replaced RxJS with native promises","Ivy introduced Shadow DOM encapsulation for all components","Ivy produces smaller and more tree-shakeable bundles, faster compilation, and better debugging with template errors directly pointing to the source file","Ivy removed the need for modules"]', [QuestionText] = N'What is a key improvement brought by the Ivy renderer over the previous ViewEngine?', [SeniorityLevel] = 3
WHERE [Id] = 99;
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260511003009_UpdateAssessmentQuestionsSeedReordered', N'9.0.10');

COMMIT;
GO

