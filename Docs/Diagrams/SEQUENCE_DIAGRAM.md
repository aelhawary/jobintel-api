## Sequence Diagrams

Key system workflows illustrated with simplified sequence diagrams.

---

### 1. User Registration

```mermaid
sequenceDiagram
    participant User
    participant Backend
    participant Database
    participant Email

    User->>Backend: Register (email, password)
    Backend->>Database: Save user
    Backend->>Email: Send verification code
    Backend-->>User: Success + check email
```

---

### 2. User Login

```mermaid
sequenceDiagram
    participant User
    participant Backend
    participant Database

    User->>Backend: Login (email, password)
    Backend->>Database: Find user
    Backend->>Backend: Verify password
    Backend->>Backend: Generate JWT
    Backend-->>User: JWT Token
```

---

### 3. Google OAuth

```mermaid
sequenceDiagram
    participant User
    participant Frontend
    participant Google
    participant Backend
    participant Database

    User->>Frontend: Click "Sign in with Google"
    Frontend->>Google: Request authorization
    Google-->>Frontend: ID Token
    Frontend->>Backend: Send ID Token
    Backend->>Google: Verify token
    Google-->>Backend: User info
    Backend->>Database: Find or create user
    Backend-->>Frontend: JWT Token
    Frontend-->>User: Logged in
```

---

### 4. Password Reset

```mermaid
sequenceDiagram
    participant User
    participant Backend
    participant Database
    participant Email

    User->>Backend: Forgot password (email)
    Backend->>Database: Generate reset token
    Backend->>Email: Send reset link
    Backend-->>User: Check email

    User->>Backend: Reset password (token, newPassword)
    Backend->>Database: Validate token
    Backend->>Database: Update password
    Backend-->>User: Password changed
```

---

### 5. Profile Management

```mermaid
sequenceDiagram
    participant User
    participant Backend
    participant Database

    User->>Backend: Update profile (JWT + data)
    Backend->>Backend: Validate JWT
    Backend->>Database: Update JobSeeker
    Backend-->>User: Updated profile
```

---

### 6. Resume Upload & Parsing

```mermaid
sequenceDiagram
    participant User
    participant Backend
    participant Storage
    participant NLP Service
    participant Database

    User->>Backend: Upload resume (file)
    Backend->>Storage: Save file
    Backend->>Database: Create resume record
    Backend-->>User: Upload success

    Note over Backend,NLP Service: Background processing
    Backend->>NLP Service: Parse resume
    NLP Service-->>Backend: Extracted skills
    Backend->>Database: Save skills
```

---

### 7. Assessment Quiz

```mermaid
sequenceDiagram
    participant JobSeeker
    participant Backend
    participant Database

    JobSeeker->>Backend: Start assessment
    Backend->>Database: Select questions by role
    Backend-->>JobSeeker: Quiz started

    loop Each Question
        JobSeeker->>Backend: Submit answer
        Backend->>Database: Save answer
        Backend-->>JobSeeker: Next question
    end

    JobSeeker->>Backend: Complete quiz
    Backend->>Backend: Calculate scores
    Backend->>Database: Save results
    Backend-->>JobSeeker: Final score
```

---

### 8. AI Job Matching

```mermaid
sequenceDiagram
    participant Recruiter
    participant Backend
    participant MatchingEngine
    participant Database

    Recruiter->>Backend: Post job
    Backend->>Database: Save job + required skills
    Backend-->>Recruiter: Job created

    Note over MatchingEngine: Matching runs periodically
    MatchingEngine->>Database: Get job requirements
    MatchingEngine->>Database: Get all candidates
    
    loop Each Candidate
        MatchingEngine->>MatchingEngine: Calculate skill match
        MatchingEngine->>MatchingEngine: Calculate experience score
        MatchingEngine->>MatchingEngine: Apply assessment boost
        MatchingEngine->>MatchingEngine: Compute final score
    end
    
    MatchingEngine->>Database: Save recommendations
```

---

### 9. Get Recommendations

```mermaid
sequenceDiagram
    participant Recruiter
    participant Backend
    participant Database

    Recruiter->>Backend: GET /recommendations/candidates/{jobId}
    Backend->>Database: Get matched candidates
    Backend-->>Recruiter: Ranked candidate list

```

---

### AI Matching Algorithm

```mermaid
flowchart LR
    subgraph Inputs
        A[Candidate Skills]
        B[Job Requirements]
        C[Experience]
        D[Assessment Score]
    end

    subgraph Matching
        E[Skill Match %]
        F[Experience Score]
        G[Assessment Boost]
    end

    subgraph Output
        H[Match Score]
    end

    A --> E
    B --> E
    C --> F
    D --> G
    E --> H
    F --> H
    G --> H
```

---
