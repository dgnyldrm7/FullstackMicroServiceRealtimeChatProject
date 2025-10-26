# 🧩 Fullstack Microservice Realtime Chat Project

A full-stack, microservice-based **Realtime Chat Application** built with **Angular, .NET, Redis, RabbitMQ, Worker Service, and Docker**.  
This project demonstrates distributed messaging, JWT-based authentication, and scalable service orchestration.

---

## 🏗️ System Architecture

The system consists of **three main components**:

| Layer | Project | Description |
|-------|----------|-------------|
| 🖥️ Frontend | `App.Angular` | Angular client that communicates with the backend via REST APIs. |
| ⚙️ Backend | `App.Backend` | ASP.NET Core Web API handling authentication, validation, message publishing, and business logic. |
| 🔧 Worker | `App.WorkerService` | Background worker consuming messages from RabbitMQ and persisting them to SQL Server. |

---
## ⚡ Quick Start


# 1. Clone the repository
git clone https://github.com/dgnyldrm7/FullstackMicroServiceRealtimeChatProject.git
cd FullstackMicroServiceRealtimeChatProject

# 2. Start Seq using Docker
docker run -d -e ACCEPT_EULA=Y -p 5341:80 datalust/seq

# 3. Run Backend (.NET API)
cd App.Backend
dotnet restore
dotnet run

# 4. Run Worker Service
cd ../App.WorkerService
dotnet restore
dotnet run

# 5. Run Angular Frontend
cd ../App.Angular
npm install
ng serve

---

## 🧭 System Overview

### 1️⃣ Authentication Flow
<img width="1709" height="613" alt="image" src="https://github.com/user-attachments/assets/8f1ab63e-b58a-46b6-bbc4-4738944c11fd" />

- The user sends a login request to `/api/auth/login`.
- The backend validates the credentials.
- If valid, **AccessToken** and **RefreshToken** are generated.
- Tokens are stored in **Redis**, enabling JTI (JWT ID) validation.
- Middleware dynamically validates tokens for each request.

---

### 2️⃣ Message Publishing Flow
<img width="1702" height="680" alt="image" src="https://github.com/user-attachments/assets/670abb17-ca98-4b29-8019-633e792cddb7" />
- The Angular client sends a chat message to the backend via REST API.
- The backend publishes the message to **RabbitMQ** (`chat-messages-save` queue).
- **Serilog + Seq** are used for centralized logging and monitoring.
- The backend performs validation using **FluentValidation** and JWT authorization.

---

### 3️⃣ Worker Service Flow
<img width="1709" height="627" alt="image" src="https://github.com/user-attachments/assets/24948372-03ec-4d71-88e3-e3e9341347d8" />

- The **Worker Service** listens to the RabbitMQ `chat-queue`.
- Each message is validated and persisted to **SQL Server**.
- The worker authenticates through `/apiworker/WorkerAuth/login`.
- Upon success, it connects to **SignalR Hub** (`/workerhub`) to deliver messages in real-time.



# 🔍 Health Checks

The backend includes built-in **health check endpoints** to monitor service availability and connection status.

You can integrate this with:

- **Docker** `HEALTHCHECK` instructions  
- **Kubernetes** liveness probes  
- **Monitoring tools** like **Seq**, **Grafana**, or **Azure Monitor**


### 🩺 Accessing the Endpoint

To view health status, simply navigate to one of the following in your backend server:
- **URL** `/healt or /healt-ui`   



---


## ⚙️ Configuration

Below is the main `appsettings.json` structure used by the backend:

```json
{
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=DESKTOP-5ESD6TM\\SQLEXPRESS;Initial Catalog=RealTimeChatApp;Integrated Security=True;Encrypt=True;Trust Server Certificate=True",
    "RedisConnection": "redis-18165.c323.us-east-1-2.ec2.redns.redis-cloud.com:18165,password=<your-redis-password>,abortConnect=false"
  },
  "RabbitMQ": {
    "User": "vqnjwdew",
    "Password": "KGZj5RK3SNVr1gpdw7JZ64VYpLNvWhli",
    "Uri": "amqps://vqnjwdew:KGZj5RK3SNVr1gpdw7JZ64VYpLNvWhli@mouse.rmq5.cloudamqp.com/vqnjwdew",
    "QueueName": "chat-messages-save"
  },
  "JwtSettings": {
    "Key": "v8$2Lp9#TxG7wQz!NfYkRsDmB4VcEj1H",
    "Issuer": "MyApp",
    "Audience": "MyAppUsers",
    "AccessTokenExpireMinutes": 20,
    "RefreshTokenExpireDays": 1
  },
  "WorkerAuth": {
    "ServiceUser": "WorkerService",
    "ServiceSecret": "61884063Aa*"
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.Seq" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ],
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "Seq", "Args": { "serverUrl": "http://localhost:5341" } }
    ],
    "Properties": { "Application": "RealTimeChatApp" }
  }
}



