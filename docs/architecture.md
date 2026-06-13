# Clinica AI – Architecture & Database Schema

This document outlines the high-level architecture designs, service integrations, and relational database schema configurations for the **Clinica AI** SaaS platform.

## Architecture Topology

The application uses a containerized multi-tier model orchestrated by Docker Compose (and deployable to Kubernetes).

```mermaid
graph TD
    subgraph Client Layer
        A[Next.js 15 Client]
    end

    subgraph Service API Layer
        B[ASP.NET Core 9 Web API Gateway]
        C[Rate Limiter Middleware]
        D[HIPAA Audit Filter]
        E[Multi-Agent Pipeline Coordinator]
    end

    subgraph Core Agents
        G1[Symptom Agent]
        G2[Lab Agent]
        G3[Drug Agent]
        G4[Nutrition Agent]
        G5[Research Agent]
        G6[Memory Agent]
        G7[Video Agent]
        G8[Emergency Agent]
    end

    subgraph Persistent Data Layer
        P1[(PostgreSQL)]
        P2[(Redis Cache)]
        P3[(Qdrant Vector DB)]
        P4[(MinIO Cloud Storage S3)]
    end

    A -->|HTTPS / JWT| B
    B --> C
    C --> D
    D --> E
    E --> G1 & G2 & G3 & G4 & G5 & G6 & G7 & G8
    B -->|EF Core| P1
    B -->|StackExchange| P2
    B -->|gRPC/REST| P3
    B -->|S3 API| P4
```

### Key Subsystems:
1. **Three-Column Dashboard**: Left side sidebar settings, Center chat stream upload actions, Right side citations learning assets.
2. **HIPAA Security Auditing**: Middleware intercepting every route that handles or reads PHI (Protected Health Information), saving logs to `audit_logs`.
3. **Dual-Mode LLM Client**: Fail-safe clinical routing in C# that automatically defaults to deterministic medical rule matching and NLP diagnostics when external OpenAI credentials are unset.

---

## Database Relational Schema (PostgreSQL)

```mermaid
erDiagram
    users ||--o| profiles : "has clinical attributes"
    users ||--o{ conversations : "initiates"
    users ||--o{ subscriptions : "owns billing status"
    users ||--o{ audit_logs : "triggers"
    profiles ||--o{ medical_history : "records chronic conditions"
    profiles ||--o{ uploaded_reports : "contains interpreted lab files"
    profiles ||--o{ memory_store : "saves long-term summaries"
    conversations ||--o{ messages : "stores log"
    conversations ||--o{ uploaded_files : "contains attachments"
    uploaded_files ||--o| uploaded_reports : "interprets biomarkers"
    messages ||--o{ source_references : "contains citations"
    messages ||--o{ video_references : "displays learn assets"
```

### Table Specifications:

#### 1. `users`
- `Id`: `UUID` (Primary Key)
- `Email`: `VARCHAR(150)` (Unique Index)
- `PasswordHash`: `VARCHAR(255)`
- `FullName`: `VARCHAR(100)`
- `Role`: `INT` (0=Free, 1=Premium, 2=Professional, 3=Admin)
- `IsEmailVerified`: `BOOLEAN`
- `EmailVerificationToken`: `VARCHAR(100)`
- `PasswordResetToken`: `VARCHAR(100)`
- `ResetTokenExpiry`: `TIMESTAMP`
- `CreatedAt` / `UpdatedAt`: `TIMESTAMP`

#### 2. `profiles`
- `Id`: `UUID` (Primary Key)
- `UserId`: `UUID` (Foreign Key -> `users.Id`, Cascade)
- `Age`: `INT`
- `Gender`: `VARCHAR(50)`
- `Country`: `VARCHAR(100)`
- `StateRegion`: `VARCHAR(100)`
- `Height`: `DOUBLE PRECISION`
- `Weight`: `DOUBLE PRECISION`
- `BloodGroup`: `VARCHAR(10)`
- `KnownConditions`: `TEXT`
- `Allergies`: `TEXT`
- `CurrentMedications`: `TEXT`
- `MedicalHistory`: `TEXT`
- `CreatedAt` / `UpdatedAt`: `TIMESTAMP`

#### 3. `conversations`
- `Id`: `UUID` (Primary Key)
- `UserId`: `UUID` (Foreign Key -> `users.Id`, Cascade)
- `Title`: `VARCHAR(200)`
- `CreatedAt` / `UpdatedAt`: `TIMESTAMP`

#### 4. `messages`
- `Id`: `UUID` (Primary Key)
- `ConversationId`: `UUID` (Foreign Key -> `conversations.Id`, Cascade)
- `Sender`: `VARCHAR(50)` ("User", "AI")
- `Content`: `TEXT`
- `StructuredResponseJson`: `TEXT` (Houses details: causes, risk levels, lifestyle etc.)
- `CreatedAt`: `TIMESTAMP`

#### 5. `medical_history`
- `Id`: `UUID` (Primary Key)
- `ProfileId`: `UUID` (Foreign Key -> `profiles.Id`, Cascade)
- `Condition`: `VARCHAR(200)`
- `Status`: `VARCHAR(50)` (Active, Resolved, Managed)
- `DiagnosedDate`: `TIMESTAMP`
- `Notes`: `TEXT`
- `CreatedAt`: `TIMESTAMP`

#### 6. `uploaded_files`
- `Id`: `UUID` (Primary Key)
- `ConversationId`: `UUID` (Foreign Key -> `conversations.Id`, Cascade)
- `FileName`: `VARCHAR(255)`
- `FilePath`: `VARCHAR(500)` (Cloud Key/Proxy URL)
- `FileSize`: `BIGINT`
- `MimeType`: `VARCHAR(100)`
- `ExtractedText`: `TEXT`
- `UploadedAt`: `TIMESTAMP`

#### 7. `uploaded_reports`
- `Id`: `UUID` (Primary Key)
- `FileId`: `UUID` (Foreign Key -> `uploaded_files.Id`, Cascade)
- `ProfileId`: `UUID` (Foreign Key -> `profiles.Id`, Cascade)
- `ReportType`: `VARCHAR(100)` (CBC, Thyroid, etc.)
- `Summary`: `TEXT`
- `AbnormalFindings`: `TEXT`
- `FullInterpretationJson`: `TEXT`
- `InterpretedAt`: `TIMESTAMP`

#### 8. `memory_store`
- `Id`: `UUID` (Primary Key)
- `ProfileId`: `UUID` (Foreign Key -> `profiles.Id`, Cascade)
- `Key`: `VARCHAR(100)` ("past_diagnoses", "allergies", etc.)
- `Value`: `TEXT`
- `UpdatedAt`: `TIMESTAMP`

#### 9. `source_references`
- `Id`: `UUID` (Primary Key)
- `MessageId`: `UUID` (Foreign Key -> `messages.Id`, Cascade)
- `SourceName`: `VARCHAR(100)`
- `Title`: `VARCHAR(250)`
- `URL`: `VARCHAR(500)`
- `Snippet`: `TEXT`

#### 10. `video_references`
- `Id`: `UUID` (Primary Key)
- `MessageId`: `UUID` (Foreign Key -> `messages.Id`, Cascade)
- `Title`: `VARCHAR(250)`
- `ThumbnailUrl`: `VARCHAR(500)`
- `DurationString`: `VARCHAR(20)`
- `ChannelName`: `VARCHAR(100)`
- `VideoUrl`: `VARCHAR(500)`

#### 11. `subscriptions`
- `Id`: `UUID` (Primary Key)
- `UserId`: `UUID` (Foreign Key -> `users.Id`, Cascade)
- `PlanType`: `INT` (Free, Premium, Pro, Admin)
- `StartDate` / `EndDate`: `TIMESTAMP`
- `IsActive`: `BOOLEAN`

#### 12. `audit_logs`
- `Id`: `UUID` (Primary Key)
- `UserId`: `UUID` (Foreign Key -> `users.Id`, Nullable, SetNull)
- `Action`: `VARCHAR(150)`
- `IpAddress`: `VARCHAR(100)`
- `Details`: `TEXT`
- `Timestamp`: `TIMESTAMP`
