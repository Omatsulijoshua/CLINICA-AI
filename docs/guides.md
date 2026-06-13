# Clinica AI – Operations & Guides Manual

This manual contains the Developer Setup Guide, Deployment Instructions, Administration Manual, and End-User Guide.

---

## 1. Developer Setup Guide

Follow these steps to run Clinica AI locally on your machine.

### Prerequisites:
- **.NET SDK 10.0+**
- **Node.js 20+** & npm
- **Docker Desktop** (Docker Compose)

### Environment Configuration:
1. In the project root directory, copy `.env.example` to `.env`.
2. Configure settings. If you want to connect to a live OpenAI account, supply your `OPENAI_API_KEY`. If left blank, Clinica AI will automatically engage its fallback clinical reasoning rules engine.

### Running Backend:
1. Navigate to `backend/`.
2. Run `dotnet restore`.
3. Run `dotnet run --project ClinicaAI.Api`. The backend will start on `http://localhost:5000` and automatically create/seed the database if PostgreSQL is running locally or in Docker.

### Running Frontend:
1. Navigate to `frontend/`.
2. Run `npm install`.
3. Run `npm run dev`. The Next.js dashboard will start on `http://localhost:3000`.

---

## 2. Deployment Instructions

### Docker Compose Production Run:
To build and spin up the complete stack (Postgres, Redis, Qdrant, MinIO, Backend, and Frontend) in production containers:
```bash
docker-compose up -d --build
```
Verify container statuses using:
```bash
docker compose ps
```

### Kubernetes Configuration:
Kubernetes configurations are available in the `/k8s` folder. Deploy them using `kubectl`:
```bash
kubectl apply -f k8s/postgres-deployment.yaml
kubectl apply -f k8s/redis-deployment.yaml
kubectl apply -f k8s/qdrant-deployment.yaml
# Deploy gateway services
kubectl apply -f k8s/backend-deployment.yaml
kubectl apply -f k8s/frontend-deployment.yaml
```

---

## 3. Administration Manual

Clinica AI provides an analytics panel accessible to accounts with the **Admin** role.

### Dashboard Capabilities:
1. **Analytics Counters**: Tracks total user counts, monthly recurring revenue projections, conversation metrics, and report upload actions.
2. **EHR Database Health Status**: Displays active health checks for Postgres, Redis cache latency, MinIO storage availability, and the active LLM engine mode.
3. **User Access Control**: Administrators can modify user roles (downgrade or upgrade to Free, Premium, Professional, or Admin).
4. **Audit Logs View**: Renders the HIPAA security logs, capturing user action descriptions, dates, and IP addresses.

---

## 4. End-User Guide

Clinica AI is designed as a three-column clinical education interface:

### Consulting the Assistant:
1. Tap **Start New Consultation** in the left sidebar to open a chat.
2. Enter your inquiry, symptoms, or upload documents in the central input box.
3. Select any **Prompt Suggestion** or click on **Follow-up Questions** bubbles to expand details.

### Reviewing Evidence Sources:
1. Every medical summary shows a **Confidence Gauge** and active references in the right panel.
2. Click on **Citations** links to view full academic publications (PubMed, WHO, CDC, etc.).
3. Browse related conditions lists and click on **Educational Videos** to watch medical explainers on YouTube.
4. **Emergency Escalations**: Life-threatening symptoms immediately lock input, triggering Emergency Mode, which highlights first-aid guidance.
