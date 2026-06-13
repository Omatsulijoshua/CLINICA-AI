# Clinica AI – REST API Documentation

All endpoints respond with JSON payloads. Authenticated endpoints require a JWT header: `Authorization: Bearer <token>`.

---

## 1. Authentication Endpoints (`/api/auth`)

### Register Account
- **Route**: `POST /api/auth/register`
- **Request Body**:
```json
{
  "email": "patient@clinica.ai",
  "password": "SecurePassword123!",
  "fullName": "Sarah Carter"
}
```
- **Response (200 OK)**:
```json
{
  "message": "Registration successful. Please verify your email.",
  "verificationToken": "abc123verificationcode"
}
```

### Sign In
- **Route**: `POST /api/auth/login`
- **Request Body**:
```json
{
  "email": "patient@clinica.ai",
  "password": "SecurePassword123!"
}
```
- **Response (200 OK)**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "e44d32a4-7935-4cb3-96b6-3a78bc51b5c4",
    "email": "patient@clinica.ai",
    "fullName": "Sarah Carter",
    "role": "Free",
    "isEmailVerified": false
  }
}
```

### Google OAuth Mock Sign In
- **Route**: `POST /api/auth/google-signin`
- **Request Body**:
```json
{
  "email": "google_patient@clinica.ai",
  "name": "Sarah Google Patient"
}
```
- **Response (200 OK)**: (Returns JWT + User info similar to standard Login endpoint)

---

## 2. Clinical Profile Endpoints (`/api/profile`)

### Fetch Profile
- **Route**: `GET /api/profile` (Auth Required)
- **Response (200 OK)**:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "e44d32a4-7935-4cb3-96b6-3a78bc51b5c4",
  "age": 30,
  "gender": "Female",
  "country": "USA",
  "stateRegion": "California",
  "height": 170.0,
  "weight": 65.0,
  "bloodGroup": "O+",
  "knownConditions": "Asthma",
  "allergies": "Peanuts",
  "currentMedications": "Albuterol",
  "medicalHistory": "Diagnosed in 2018.",
  "historyEntries": []
}
```

### Update Profile
- **Route**: `PUT /api/profile` (Auth Required)
- **Request Body**: (JSON properties matching profile parameters above)
- **Response (200 OK)**: Returns updated profile block.

---

## 3. Conversation & Consultation Endpoints (`/api/chat`)

### List Conversations
- **Route**: `GET /api/chat/conversations?search=<query>` (Auth Required)
- **Response (200 OK)**: Array of conversation logs.

### Create Conversation
- **Route**: `POST /api/chat/conversations` (Auth Required)
- **Response (200 OK)**: Returns new conversation object with unique GUID.

### Delete Conversation
- **Route**: `DELETE /api/chat/conversations/{id}` (Auth Required)

### List Messages
- **Route**: `GET /api/chat/conversations/{id}/messages` (Auth Required)

### Send Consultation Message
- **Route**: `POST /api/chat/conversations/{id}/message` (Auth Required)
- **Request Body**:
```json
{
  "userInput": "I have had a high fever for 3 days and joint aches"
}
```
- **Response (200 OK)**: Returns AI response message containing detailed structured reasoning data:
```json
{
  "id": "a4c6a798-cb15-46f0-a178-b118742b78b5",
  "conversationId": "7e3b1456-f1c5-4d6f-9988-cc77ee88ff99",
  "sender": "AI",
  "content": "Based on your report, symptoms suggest Malaria or Typhoid Fever adjusted for your country context...",
  "structuredResponseJson": "{\"Summary\":\"...\",\"PossibleCauses\":[{\"Name\":\"Malaria\",\"ConfidenceScore\":0.75}],\"RiskLevel\":\"Medium\",\"LifestyleRecommendations\":[],\"NutritionSuggestions\":[],\"SupplementInformation\":[],\"MedicationEducation\":[],\"Sources\":[],\"EducationalVideos\":[],\"MedicalDisclaimer\":\"...\"}",
  "createdAt": "2026-06-13T10:00:00Z"
}
```

### Upload Report / Image
- **Route**: `POST /api/chat/conversations/{id}/upload` (Auth Required)
- **Payload**: `multipart/form-data` containing `file` parameter.
- **Response (200 OK)**: Returns file details and OCR/Biomarker parsed summary.

---

## 4. Administration Endpoints (`/api/admin`)

*These routes require Admin role: `Role == Admin`*

### Get Metrics
- **Route**: `GET /api/admin/analytics`

### List Users
- **Route**: `GET /api/admin/users`

### Update User Role
- **Route**: `PUT /api/admin/users/role`
- **Request Body**:
```json
{
  "email": "patient@clinica.ai",
  "newRole": 1
}
```

### System Diagnostics
- **Route**: `GET /api/admin/system-health`

### View Audit Trail
- **Route**: `GET /api/admin/audit-logs`
