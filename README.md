# 🍽️ EatOClock — Online Food Delivery Platform

> _Order Smarter. Eat Better. Delivered Faster._

EatOClock is a production-grade **microservices-based Online Food Delivery Platform** built with **ASP.NET Core (.NET 8)**. It connects **Customers**, **Restaurant Owners**, and **Delivery Agents** on a single unified backend, orchestrated through an API Gateway (YARP) and backed by **PostgreSQL (Supabase-hosted)**. Real-time delivery tracking is powered by **ASP.NET Core SignalR**.

---

## 📑 Table of Contents

- [About the Project](#about-the-project)
- [Tech Stack](#tech-stack)
- [System Architecture](#system-architecture)
- [Microservices Overview](#microservices-overview)
- [Core Features](#core-features)
- [System Architectural Diagram](#system-architectural-diagram)
- [Use Case Diagram](#use-case-diagram)
- [Microservices Communication Flow](#microservices-communication-flow)
- [ER Diagram](#er-diagram)
- [Project Flow Diagram](#project-flow-diagram)
- [Architecture Layers (Per Microservice)](#architecture-layers-per-microservice)
- [Project Structure](#project-structure)
- [API Endpoints (Per Service)](#api-endpoints-per-service)
- [Infrastructure Components](#infrastructure-components)
- [Installation & Setup](#installation--setup)
- [How to Run](#how-to-run)
- [How to Test](#how-to-test)
- [Database Schema](#database-schema)
- [Security Features](#security-features)
- [API Security](#api-security)
- [Performance Optimization](#performance-optimization)
- [Event-Driven Architecture](#event-driven-architecture)
- [API Documentation](#api-documentation)
- [Docker Configuration](#docker-configuration)
- [Roadmap](#roadmap)

---

## 🧭 About the Project

EatOClock is a **full-stack backend platform** inspired by Zomato and Swiggy, rebuilt from the ground up using modern ASP.NET Core patterns and clean microservices principles.

### The Problem It Solves

Traditional monolithic food delivery backends are hard to scale, deploy independently, or maintain at team level. EatOClock breaks every domain boundary into its own autonomous microservice — each with its own database, its own API surface, its own Docker container — connected through a single YARP-powered API Gateway that handles JWT authentication, rate limiting, and request routing transparently.

### What's Been Implemented

| Domain                                        | Status           |
| --------------------------------------------- | ---------------- |
| Authentication & User Management              | ✅ Implemented   |
| Restaurant Registration & Management          | ✅ Implemented   |
| Menu Categories & Items                       | ✅ Implemented   |
| Order Lifecycle (Place → Deliver)             | ✅ Implemented   |
| Delivery Agent Registration & Assignment      | ✅ Implemented   |
| Real-time Location (SignalR Hub)              | ✅ Implemented   |
| Redis Caching (Building Block)                | ✅ Implemented   |
| JWT Auth Middleware (Building Block)          | ✅ Implemented   |
| Health Checks (Building Block)                | ✅ Implemented   |
| Structured Logging - Serilog (Building Block) | ✅ Implemented   |
| EventBus (Stub / Foundation)                  | ✅ Stub in place |
| API Gateway (YARP)                            | ✅ Implemented   |
| Integration Tests                             | ✅ Implemented   |
| Cart Service                                  | 🔜 Roadmap       |
| Payment / Wallet Service                      | 🔜 Roadmap       |
| Notification Service                          | 🔜 Roadmap       |
| Review & Ratings Service                      | 🔜 Roadmap       |

### Architecture Philosophy

- **Database-per-Service** — every microservice owns its own PostgreSQL schema; no shared tables across services.
- **Single Responsibility** — each service has one bounded context and does exactly one job well.
- **Contract-First** — every service exposes a C# interface (`IXxxService`) that the controller depends on, not the concrete implementation.
- **Shared Nothing** — cross-service communication happens via **direct HTTP calls**, never shared memory or database joins.
- **Observability First** — every service plugs into the shared `Logging` building block (Serilog) and the `HealthChecks` building block at startup.

---

## 🛠️ Tech Stack

| Category             | Technology                                             |
| -------------------- | ------------------------------------------------------ |
| **Runtime**          | .NET 8 / ASP.NET Core Web API                          |
| **Language**         | C# 12                                                  |
| **API Gateway**      | YARP (Yet Another Reverse Proxy)                       |
| **ORM**              | Entity Framework Core 8                                |
| **Database**         | PostgreSQL (hosted on Supabase)                        |
| **Real-time**        | ASP.NET Core SignalR                                   |
| **Caching**          | Redis (IRedisCache Building Block)                     |
| **Authentication**   | JWT Bearer Tokens (custom middleware building block)   |
| **Logging**          | Serilog (structured logging building block)            |
| **Health Checks**    | ASP.NET Core Health Checks (building block)            |
| **Event Bus**        | Custom EventBus stub (foundation for future messaging) |
| **Containerisation** | Docker + Docker Compose                                |
| **Code Quality**     | SonarQube analysis                                     |
| **Testing**          | xUnit / NUnit (EatOClock.Tests project)                |
| **API Docs**         | Swagger / OpenAPI (per service)                        |

---

## 🏗️ System Architecture

EatOClock follows a **microservices architecture** where every service is independently deployable, owns its own data store, and communicates through well-defined HTTP contracts routed via the API Gateway.

```
Clients (Browser / Mobile)
        │
        ▼
┌──────────────────────────────┐
│    API Gateway (YARP :5000)  │  ← JWT Validation, Rate Limiting, Routing
└────────────┬─────────────────┘
             │  routes to →
    ┌─────────────────────────────────────────────┐
    │         Microservices Layer                  │
    ├──────────┬──────────┬───────────┬────────────┤
    │Auth Svc  │Rest. Svc │Menu Svc   │Order Svc   │  Delivery Svc
    │:8081     │:8082     │:8083      │:8084       │  :8085
    └────┬─────┴────┬─────┴────┬──────┴────┬───────┘
         │          │          │           │
    ┌────▼──┐ ┌─────▼──┐ ┌────▼──┐  ┌────▼──────┐
    │Auth DB│ │Rest. DB│ │Menu DB│  │Order DB   │ Delivery DB
    │(PG)   │ │(PG)    │ │(PG)   │  │(PG)       │ (PG)
    └───────┘ └────────┘ └───────┘  └───────────┘
         │
    ┌────▼─────────────────────────────────────┐
    │         Building Blocks (Shared)          │
    │  JWT Auth · Redis Cache · EventBus        │
    │  Serilog Logging · Health Checks          │
    └───────────────────────────────────────────┘
```

---

## 🔬 Microservices Overview

### 1. Auth Service (:8081)

Handles all identity concerns: user registration, login, JWT issuance & refresh, profile management, password change, and account deactivation. Users carry a `Role` enum (`CUSTOMER / OWNER / AGENT / ADMIN`) that downstream services read from the JWT claims.

**Key files:** `AuthController.cs`, `AuthServiceImpl.cs`, `IAuthService.cs`, `User.cs`, `AppDbContext.cs`

---

### 2. Restaurant Service (:8082)

Manages restaurant profiles. Stores GPS coordinates for geo-proximity search, cuisine tags, delivery radius, minimum order amount, and estimated delivery time. Restaurants require **Admin approval** before becoming visible to customers.

**Key files:** `RestaurantController.cs`, `RestaurantService.cs`, `IRestaurantService.cs`, `Restaurant.cs`

---

### 3. Menu Service (:8083)

Owns the full menu structure for every restaurant. Organises items under `MenuCategory` entities. Items carry price, discounted price, availability toggle, veg/non-veg flag, calories, and tags. Supports keyword search and veg-only filtering.

**Key files:** `MenuController.cs`, `MenuService.cs`, `IMenuService.cs`, `MenuItem.cs`, `MenuCategory.cs`

---

### 4. Order Service (:8084)

The **central orchestration service**. Converts a placed order into an `Order` record with an immutable `OrderItem` snapshot. Manages the complete status lifecycle: `PLACED → CONFIRMED → PREPARING → PICKED_UP → DELIVERED / CANCELLED`. Calls the Delivery Service to assign agents.

**Key files:** `OrderController.cs`, `OrderServiceImpl.cs`, `IOrderService.cs`, `OrderModels.cs`

---

### 5. Delivery Agent Service (:8085)

Handles agent registration, availability toggling, order assignment, GPS location updates, delivery records, and ratings. Exposes a **SignalR Hub (`LocationHub`)** for real-time location streaming to customers.

**Key files:** `AgentController.cs`, `AgentServiceImpl.cs`, `IAgentService.cs`, `LocationHub.cs`, `AgentModels.cs`

---

### API Gateway (YARP :5000)

Single ingress point. Reads YARP route config from `appsettings.json`, validates JWT tokens, applies rate-limiting policies, and proxies requests to the correct downstream service. No business logic lives here.

**Key files:** `Program.cs`, `appsettings.json`

---

### Building Blocks (Shared Libraries)

| Library          | Purpose                                                          |
| ---------------- | ---------------------------------------------------------------- |
| `Authentication` | `JwtBearerExtensions` — one-line JWT setup for any service       |
| `Caching`        | `IRedisCache` — typed Redis interface for distributed caching    |
| `EventBus`       | Foundation stub for future async messaging (RabbitMQ / Azure SB) |
| `HealthChecks`   | Registers standard ASP.NET Core health check endpoints           |
| `Logging`        | Serilog configuration shared across all services                 |

---

## ✨ Core Features

### Authentication & Identity

- JWT Bearer token authentication with refresh token support
- Role-based access control (`Customer / Owner / Agent / Admin`)
- Profile update and password change
- Account deactivation

### Restaurant Discovery

- Search restaurants by city or cuisine
- Geo-proximity search using latitude/longitude (Haversine-like distance)
- Filter by open status and admin approval
- Full restaurant CRUD for owners

### Menu Management

- Hierarchical categories → items structure
- Toggle item availability in real time
- Veg / non-veg filtering
- Keyword search across menu items
- Discounted price field per item

### Order Lifecycle

- Place orders referencing restaurant + items with quantity
- Full status pipeline: `PLACED → CONFIRMED → PREPARING → PICKED_UP → DELIVERED`
- Cancel orders with request reason
- Assign delivery agent to order
- View order history by customer or restaurant

### Delivery & Real-time Tracking

- Agent registration with vehicle details
- Availability toggling (online / offline)
- GPS location updates stored per agent
- Real-time location streaming via **SignalR `LocationHub`**
- Delivery record with pickup time, delivery time, and customer rating
- Nearby agent search (distance-based)

### Admin Operations

- Approve / reject restaurant registrations
- Approve delivery agents
- Role-based endpoints locked behind `[Authorize(Roles="Admin")]`

---

## 📊 System Architectural Diagram

![System Architecture](01_system_architecture.png)

---

## 📋 Use Case Diagram

![Use Case Diagram](05_use_case.png)

---

## 🔄 Microservices Communication Flow

![Communication Flow](02_comm_flow.png)

---

## 🗃️ ER Diagram

![ER Diagram](03_er_diagram.png)

---

## 🌊 Project Flow Diagram

![Project Flow](04_project_flow.png)

---

## 🏛️ Architecture Layers (Per Microservice)

![Architecture Layers](05_architecture_layers.png)

Every microservice follows the same 5-layer pattern:

```
┌──────────────────────────────────┐
│  Controller Layer                │  HTTP surface · DTO in/out · [Authorize] attributes
├──────────────────────────────────┤
│  Service Interface (IXxxService) │  Business contract · DI target
├──────────────────────────────────┤
│  Service Implementation          │  Business logic · Orchestration · Cross-service calls
├──────────────────────────────────┤
│  Data Access (EF Core DbContext) │  LINQ queries · Migrations · DbSet<T>
├──────────────────────────────────┤
│  Domain Models (POCOs)           │  C# entities mapped to PostgreSQL tables
└──────────────────────────────────┘
         ↑ shared by all services ↑
┌──────────────────────────────────┐
│  Building Blocks                 │  JWT · Redis · EventBus · Serilog · HealthChecks
└──────────────────────────────────┘
```

---

## 📂 Project Structure

```
EatOClock/
│
├── API-Gateway/
│   └── EatOClock.Gateway/
│       ├── Program.cs                    # YARP + JWT + rate-limit setup
│       ├── appsettings.json              # Route config for all 5 services
│       └── Dockerfile
│
├── BuildingBlocks/
│   ├── Authentication/
│   │   └── JwtBearerExtensions.cs        # Shared JWT middleware
│   ├── Caching/
│   │   └── IRedisCache.cs               # Redis interface
│   ├── EventBus/
│   │   └── Class1.cs                    # EventBus stub
│   ├── HealthChecks/
│   └── Logging/                         # Serilog config
│
├── Services/
│   ├── Auth_Service/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   └── UpdateProfileRequest.cs
│   │   ├── Data/AppDbContext.cs
│   │   ├── DTOs/                        # LoginDTO, RegisterDTO, UserDTO, etc.
│   │   ├── Enums/AllowedRegistrationRole.cs
│   │   ├── Interfaces/IAuthService.cs
│   │   ├── Migrations/
│   │   ├── Models/User.cs
│   │   ├── Services/
│   │   │   ├── AuthServiceImpl.cs
│   │   │   └── AuthResult.cs
│   │   ├── Program.cs
│   │   └── Dockerfile
│   │
│   ├── Restaurant_Service/
│   │   ├── Controllers/RestaurantController.cs
│   │   ├── Data/AppDbContext.cs
│   │   ├── DTOs/                        # CreateRestaurantDTO, NearbySearchDTO, etc.
│   │   ├── Interfaces/IRestaurantService.cs
│   │   ├── Migrations/
│   │   ├── Models/Restaurant.cs
│   │   ├── Services/RestaurantService.cs
│   │   ├── Program.cs
│   │   └── Dockerfile
│   │
│   ├── Menu_Service/
│   │   ├── Controllers/MenuController.cs
│   │   ├── Data/AppDbContext.cs
│   │   ├── DTOs/                        # CreateCategoryRequest, CreateMenuItemRequest, etc.
│   │   ├── Interfaces/IMenuService.cs
│   │   ├── Migrations/
│   │   ├── Models/
│   │   │   ├── MenuCategory.cs
│   │   │   └── MenuItem.cs
│   │   ├── Services/MenuService.cs
│   │   ├── Program.cs
│   │   └── Dockerfile
│   │
│   ├── Order_Service/
│   │   ├── Controllers/OrderController.cs
│   │   ├── Data/AppDbContext.cs
│   │   ├── DTOs/                        # PlaceOrderRequest, UpdateStatusRequest, etc.
│   │   ├── Enums/OrderStatus.cs
│   │   ├── Interfaces/IOrderService.cs
│   │   ├── Migrations/
│   │   ├── Models/OrderModels.cs
│   │   ├── Services/OrderServiceImpl.cs
│   │   ├── Program.cs
│   │   └── Dockerfile
│   │
│   └── DeliveryAgent_Service/
│       ├── Controllers/AgentController.cs
│       ├── Data/AppDbContext.cs
│       ├── DTOs/                        # AgentDTOs, AssignOrderRequest, NearbyAgentDTO, etc.
│       ├── Enums/AgentEnums.cs
│       ├── Hubs/LocationHub.cs          # SignalR real-time hub
│       ├── Interfaces/IAgentService.cs
│       ├── Migrations/
│       ├── Models/AgentModels.cs
│       ├── Services/AgentServiceImpl.cs
│       ├── Program.cs
│       └── Dockerfile
│
├── Shared/
│   └── Shared.csproj                   # Shared DTOs / utilities across services
│
├── Tests/
│   └── EatOClock.Tests/
│       └── EatOClock.Tests.csproj
│
└── docker-compose.yml
```

---

## 🌐 API Endpoints (Per Service)

All routes pass through the **API Gateway at `:5000`** which proxies to the respective service.

### 🔐 Auth Service — `/api/auth`

| Method   | Endpoint               | Auth Required | Description                                    |
| -------- | ---------------------- | ------------- | ---------------------------------------------- |
| `POST`   | `/api/auth/register`   | No            | Register a new user (Customer / Owner / Agent) |
| `POST`   | `/api/auth/login`      | No            | Login and receive JWT + refresh token          |
| `POST`   | `/api/auth/refresh`    | No            | Refresh an expired JWT                         |
| `GET`    | `/api/auth/profile`    | Yes           | Get own profile                                |
| `PUT`    | `/api/auth/profile`    | Yes           | Update profile details                         |
| `PUT`    | `/api/auth/password`   | Yes           | Change password                                |
| `DELETE` | `/api/auth/deactivate` | Yes           | Deactivate account                             |

---

### 🍜 Restaurant Service — `/api/restaurants`

| Method   | Endpoint                             | Auth Required     | Description                             |
| -------- | ------------------------------------ | ----------------- | --------------------------------------- |
| `POST`   | `/api/restaurants`                   | Yes (Owner)       | Register a new restaurant               |
| `GET`    | `/api/restaurants/{id}`              | No                | Get restaurant by ID                    |
| `GET`    | `/api/restaurants/owner/{ownerId}`   | Yes               | Get restaurants by owner                |
| `GET`    | `/api/restaurants/city/{city}`       | No                | Get restaurants by city                 |
| `GET`    | `/api/restaurants/cuisine/{cuisine}` | No                | Filter by cuisine                       |
| `GET`    | `/api/restaurants/nearby`            | No                | Get nearby restaurants (lat/lng/radius) |
| `GET`    | `/api/restaurants/search`            | No                | Search restaurants by name              |
| `PUT`    | `/api/restaurants/{id}`              | Yes (Owner)       | Update restaurant details               |
| `PUT`    | `/api/restaurants/{id}/approve`      | Yes (Admin)       | Approve restaurant                      |
| `PUT`    | `/api/restaurants/{id}/toggle-open`  | Yes (Owner)       | Toggle open/closed                      |
| `DELETE` | `/api/restaurants/{id}`              | Yes (Admin/Owner) | Delete restaurant                       |

---

### 🍕 Menu Service — `/api/menu`

| Method   | Endpoint                              | Auth Required | Description                    |
| -------- | ------------------------------------- | ------------- | ------------------------------ |
| `POST`   | `/api/menu/category`                  | Yes (Owner)   | Create a menu category         |
| `POST`   | `/api/menu/item`                      | Yes (Owner)   | Add a menu item                |
| `GET`    | `/api/menu/restaurant/{restaurantId}` | No            | Get full menu for a restaurant |
| `GET`    | `/api/menu/categories/{restaurantId}` | No            | Get categories only            |
| `GET`    | `/api/menu/item/{itemId}`             | No            | Get single item                |
| `PUT`    | `/api/menu/item/{itemId}`             | Yes (Owner)   | Update menu item               |
| `PUT`    | `/api/menu/item/{itemId}/toggle`      | Yes (Owner)   | Toggle item availability       |
| `DELETE` | `/api/menu/item/{itemId}`             | Yes (Owner)   | Delete menu item               |
| `DELETE` | `/api/menu/category/{categoryId}`     | Yes (Owner)   | Delete category                |
| `GET`    | `/api/menu/search`                    | No            | Search menu items by keyword   |
| `GET`    | `/api/menu/veg/{restaurantId}`        | No            | Get veg items only             |

---

### 📦 Order Service — `/api/orders`

| Method | Endpoint                                | Auth Required      | Description                     |
| ------ | --------------------------------------- | ------------------ | ------------------------------- |
| `POST` | `/api/orders`                           | Yes (Customer)     | Place a new order               |
| `GET`  | `/api/orders/{id}`                      | Yes                | Get order by ID                 |
| `GET`  | `/api/orders/customer/{customerId}`     | Yes                | Get all orders for a customer   |
| `GET`  | `/api/orders/restaurant/{restaurantId}` | Yes (Owner)        | Get all orders for a restaurant |
| `GET`  | `/api/orders/active`                    | Yes                | Get active (in-progress) orders |
| `PUT`  | `/api/orders/{id}/status`               | Yes (Owner/Agent)  | Update order status             |
| `PUT`  | `/api/orders/{id}/assign-agent`         | Yes (Admin/System) | Assign delivery agent           |
| `PUT`  | `/api/orders/{id}/cancel`               | Yes (Customer)     | Cancel an order                 |
| `POST` | `/api/orders/{id}/reorder`              | Yes (Customer)     | Reorder from history            |
| `GET`  | `/api/orders/count`                     | Yes (Admin)        | Get total order count           |

---

### 🛵 Delivery Agent Service — `/api/agents`

| Method | Endpoint                        | Auth Required  | Description                  |
| ------ | ------------------------------- | -------------- | ---------------------------- |
| `POST` | `/api/agents/register`          | Yes            | Register as delivery agent   |
| `GET`  | `/api/agents/{id}`              | Yes            | Get agent profile            |
| `PUT`  | `/api/agents/{id}/availability` | Yes (Agent)    | Toggle online/offline        |
| `PUT`  | `/api/agents/{id}/location`     | Yes (Agent)    | Update GPS location          |
| `POST` | `/api/agents/{id}/assign-order` | Yes            | Assign an order to agent     |
| `GET`  | `/api/agents/nearby`            | Yes            | Find nearby available agents |
| `GET`  | `/api/agents/{id}/deliveries`   | Yes            | Get delivery history         |
| `POST` | `/api/agents/{id}/rate`         | Yes (Customer) | Rate a delivery              |
| `PUT`  | `/api/agents/{agentId}/approve` | Yes (Admin)    | Approve agent                |

---

## 🏗️ Infrastructure Components

| Component     | Technology                | Purpose                                              |
| ------------- | ------------------------- | ---------------------------------------------------- |
| API Gateway   | YARP (Microsoft)          | Single ingress, routing, JWT validation              |
| Database      | PostgreSQL on Supabase    | Persistent relational storage (1 DB per service)     |
| Redis         | Redis Server              | Distributed caching via `IRedisCache` building block |
| SignalR       | ASP.NET Core SignalR      | Real-time WebSocket location push from `LocationHub` |
| Serilog       | Serilog + sinks           | Structured JSON logging (console + file)             |
| Health Checks | ASP.NET Core HealthChecks | `/health` endpoint per service                       |
| Docker        | Docker + Compose          | Container packaging for all services                 |
| SonarQube     | SonarQube scanner         | Static code analysis and quality gate                |

---

## ⚙️ Installation & Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [PostgreSQL](https://www.postgresql.org/) — or a Supabase project
- [Redis](https://redis.io/) — local or Docker
- Git

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/EatOClock.git
cd EatOClock
```

### 2. Configure Connection Strings

Each service has its own `appsettings.json`. Update the connection string for your PostgreSQL instance:

```json
// Services/Auth_Service/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=your-host;Port=5432;Database=eatoclock_auth;Username=postgres;Password=your-password"
  },
  "JwtSettings": {
    "Secret": "your-super-secret-key-min-32-chars",
    "Issuer": "EatOClock",
    "Audience": "EatOClock",
    "ExpiryMinutes": 60
  }
}
```

Repeat for `Restaurant_Service`, `Menu_Service`, `Order_Service`, and `DeliveryAgent_Service` using separate database names:

| Service    | Database Name           |
| ---------- | ----------------------- |
| Auth       | `eatoclock_auth`        |
| Restaurant | `eatoclock_restaurants` |
| Menu       | `eatoclock_menu`        |
| Order      | `eatoclock_orders`      |
| Delivery   | `eatoclock_delivery`    |

### 3. Run Database Migrations

Run migrations for each service from the solution root:

```bash
dotnet ef database update --project Services/Auth_Service
dotnet ef database update --project Services/Restaurant_Service
dotnet ef database update --project Services/Menu_Service
dotnet ef database update --project Services/Order_Service
dotnet ef database update --project Services/DeliveryAgent_Service
```

### 4. Configure Redis

If running Redis locally via Docker:

```bash
docker run -d -p 6379:6379 --name eatoclock-redis redis:alpine
```

Update each service's `appsettings.json`:

```json
"Redis": {
  "ConnectionString": "localhost:6379"
}
```

---

## 🚀 How to Run

### Option A — Run All Services via Docker Compose (Recommended)

```bash
docker-compose up --build
```

This starts all 5 microservices + the API Gateway. Default port mapping:

| Service                | Port   |
| ---------------------- | ------ |
| API Gateway            | `5000` |
| Auth Service           | `8081` |
| Restaurant Service     | `8082` |
| Menu Service           | `8083` |
| Order Service          | `8084` |
| Delivery Agent Service | `8085` |

### Option B — Run Individually with .NET CLI

Open a terminal for each service:

```bash
# Terminal 1 — API Gateway
cd API-Gateway/EatOClock.Gateway
dotnet run

# Terminal 2 — Auth Service
cd Services/Auth_Service
dotnet run

# Terminal 3 — Restaurant Service
cd Services/Restaurant_Service
dotnet run

# Terminal 4 — Menu Service
cd Services/Menu_Service
dotnet run

# Terminal 5 — Order Service
cd Services/Order_Service
dotnet run

# Terminal 6 — Delivery Agent Service
cd Services/DeliveryAgent_Service
dotnet run
```

### Option C — Visual Studio / Rider

Open `EatOClock.sln`, set multiple startup projects (all 5 services + gateway), and press **Run**.

---

## 🧪 How to Test

### Unit / Integration Tests

```bash
cd Tests/EatOClock.Tests
dotnet test --verbosity normal
```

### Swagger UI (per service, when running individually)

Each service exposes Swagger at:

```
http://localhost:{port}/swagger
```

Example: `http://localhost:8081/swagger` for Auth Service.

### Test via API Gateway (all routes unified)

Use the provided `.http` files in each service directory, or import into **Postman / Bruno**:

```
EatOClock/Services/Auth_Service/Auth-Service.http
EatOClock/Services/Restaurant_Service/Restaurant_Service.http
EatOClock/Services/DeliveryAgent_Service/DeliveryAgent_Service.http
EatOClock/API-Gateway/EatOClock.Gateway/EatOClock.Gateway.http
```

### Health Checks

```bash
curl http://localhost:8081/health   # Auth
curl http://localhost:8082/health   # Restaurant
curl http://localhost:8083/health   # Menu
curl http://localhost:8084/health   # Order
curl http://localhost:8085/health   # Delivery
```

---

## 🗄️ Database Schema

> All databases are PostgreSQL. Each service owns one isolated schema / database.

### Auth Database — `eatoclock_auth`

```sql
CREATE TABLE users (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    full_name     TEXT NOT NULL,
    email         TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    phone         TEXT,
    role          TEXT NOT NULL,          -- CUSTOMER | OWNER | AGENT | ADMIN
    provider      TEXT DEFAULT 'LOCAL',   -- LOCAL | GOOGLE | GITHUB
    is_active     BOOLEAN DEFAULT TRUE,
    profile_pic_url TEXT,
    created_at    TIMESTAMP DEFAULT NOW()
);
```

### Restaurant Database — `eatoclock_restaurants`

```sql
CREATE TABLE restaurants (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    owner_id              UUID NOT NULL,
    name                  TEXT NOT NULL,
    description           TEXT,
    cuisine               TEXT,
    address               TEXT,
    city                  TEXT,
    latitude              DOUBLE PRECISION,
    longitude             DOUBLE PRECISION,
    phone                 TEXT,
    avg_rating            FLOAT DEFAULT 0.0,
    is_open               BOOLEAN DEFAULT FALSE,
    is_approved           BOOLEAN DEFAULT FALSE,
    delivery_radius       FLOAT,
    min_order_amount      DECIMAL(10,2),
    estimated_delivery_min INT
);
```

### Menu Database — `eatoclock_menu`

```sql
CREATE TABLE menu_categories (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    restaurant_id UUID NOT NULL,
    name          TEXT NOT NULL,
    description   TEXT,
    image_url     TEXT,
    display_order INT DEFAULT 0
);

CREATE TABLE menu_items (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    restaurant_id    UUID NOT NULL,
    category_id      UUID REFERENCES menu_categories(id),
    name             TEXT NOT NULL,
    description      TEXT,
    price            DECIMAL(10,2) NOT NULL,
    discounted_price DECIMAL(10,2),
    image_url        TEXT,
    is_veg           BOOLEAN DEFAULT FALSE,
    is_available     BOOLEAN DEFAULT TRUE,
    rating           FLOAT DEFAULT 0.0,
    calories         INT,
    tags             TEXT
);
```

### Order Database — `eatoclock_orders`

```sql
CREATE TABLE orders (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id           UUID NOT NULL,
    restaurant_id         UUID NOT NULL,
    restaurant_name       TEXT,
    delivery_agent_id     UUID,
    total_amount          DECIMAL(10,2),
    discount              DECIMAL(10,2) DEFAULT 0,
    final_amount          DECIMAL(10,2),
    mode_of_payment       TEXT,           -- COD | ONLINE | WALLET
    status                TEXT NOT NULL,  -- PLACED | CONFIRMED | PREPARING | PICKED_UP | DELIVERED | CANCELLED
    order_date            TIMESTAMP DEFAULT NOW(),
    estimated_delivery    TIMESTAMP,
    delivery_address      TEXT,
    special_instructions  TEXT
);

CREATE TABLE order_items (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id     UUID REFERENCES orders(id),
    menu_item_id UUID NOT NULL,
    name         TEXT NOT NULL,
    price        DECIMAL(10,2) NOT NULL,
    quantity     INT NOT NULL,
    customization TEXT
);
```

### Delivery Database — `eatoclock_delivery`

```sql
CREATE TABLE delivery_agents (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         UUID NOT NULL,
    vehicle_type    TEXT,        -- BIKE | CYCLE | CAR
    vehicle_reg_no  TEXT,
    is_available    BOOLEAN DEFAULT FALSE,
    is_approved     BOOLEAN DEFAULT FALSE,
    current_lat     DOUBLE PRECISION,
    current_lng     DOUBLE PRECISION,
    avg_rating      FLOAT DEFAULT 0.0,
    total_deliveries INT DEFAULT 0
);

CREATE TABLE delivery_records (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id      UUID NOT NULL,
    agent_id      UUID REFERENCES delivery_agents(id),
    pickup_time   TIMESTAMP,
    delivery_time TIMESTAMP,
    rating        INT,
    notes         TEXT
);
```

---

## 🔒 Security Features

### JWT Authentication

- Short-lived access tokens (configurable, default 60 minutes)
- Refresh token support for seamless re-authentication
- Tokens carry `UserId`, `Email`, `Role` claims consumed by all downstream services
- The `Authentication` building block provides a one-liner `AddJwtBearerAuthentication()` extension that all services call at startup — consistent configuration, no duplication

### Role-Based Access Control (RBAC)

- Every sensitive endpoint is decorated with `[Authorize(Roles="...")]`
- Roles enforced: `Admin`, `Owner`, `Agent`, `Customer`
- The API Gateway validates JWT before the request ever reaches a service — invalid tokens are rejected at the edge

### Password Security

- Passwords are never stored in plaintext
- BCrypt hashing via ASP.NET Core's `PasswordHasher<T>`

### CORS

- Each service configures CORS policies in `Program.cs` restricted to known origins

### Admin-Gated Operations

- Restaurant approval, agent approval, and user management are locked behind `[Authorize(Roles="Admin")]`
- No self-approval paths exist in the codebase

---

## 🔐 API Security

| Layer                 | Mechanism                                                                                              |
| --------------------- | ------------------------------------------------------------------------------------------------------ |
| **Transport**         | HTTPS (TLS) enforced in production Docker config                                                       |
| **Authentication**    | JWT Bearer (validated at Gateway + per-service)                                                        |
| **Authorisation**     | `[Authorize(Roles)]` attributes on every protected controller action                                   |
| **Rate Limiting**     | Configured in YARP API Gateway to prevent abuse                                                        |
| **Input Validation**  | Data annotations + model state validation on all DTOs                                                  |
| **SQL Injection**     | EF Core parameterised queries — no raw SQL                                                             |
| **Secret Management** | JWT secrets and DB connection strings via `appsettings.json` / environment variables — never committed |

---

## ⚡ Performance Optimization

### Redis Caching

The `Caching` building block (`IRedisCache`) is wired up across services to cache:

- Frequently accessed restaurant listings
- Menu data (changes infrequently)
- Agent availability snapshots

Cache-aside pattern: check Redis → on miss, hit DB → write-back to Redis with TTL.

### EF Core Optimizations

- `AsNoTracking()` on all read-only queries to skip change-tracker overhead
- Projection via `.Select()` to fetch only needed columns
- Indexed columns: `email` (users), `restaurant_id` (menu_items), `customer_id` (orders), `status` (orders)

### Async Everywhere

All controller actions and service methods are `async Task<T>` — no blocking calls, full use of the ASP.NET Core thread pool.

### Pagination (Roadmap)

Large collection endpoints (orders history, restaurant lists) are planned for cursor-based pagination in the next milestone.

---

## 📨 Event-Driven Architecture

EatOClock includes an **`EventBus` building block** as a foundation for async, decoupled inter-service communication. Currently a stub (`Class1.cs`), it is designed to be backed by **RabbitMQ** or **Azure Service Bus** in production.

**Planned event flows:**

```
Order Service  ──[OrderPlaced]──►  Notification Service (email/SMS)
Order Service  ──[OrderPlaced]──►  Restaurant Service (new order alert)
Order Service  ──[OrderCancelled]──►  Delivery Service (release agent)
Auth Service   ──[UserDeactivated]──►  All services (revoke sessions)
```

**Why stub first?** Synchronous HTTP calls between services are simpler to debug in development. The EventBus interface is designed so the swap to a real message broker requires changing only the registration line in `Program.cs`, not any service logic.

---

## 📖 API Documentation

Each microservice ships with **Swagger / OpenAPI** enabled in development mode.

Access per service:

```
http://localhost:8081/swagger   → Auth Service
http://localhost:8082/swagger   → Restaurant Service
http://localhost:8083/swagger   → Menu Service
http://localhost:8084/swagger   → Order Service
http://localhost:8085/swagger   → Delivery Agent Service
```

All routes through the gateway:

```
http://localhost:5000/swagger   → Aggregated (if gateway aggregation is configured)
```

Swagger UI lets you:

- Browse all endpoints with request/response schemas
- Authenticate using the **Authorize** button (paste your JWT)
- Execute live requests directly from the browser

---

## 🐳 Docker Configuration

Each service has its own `Dockerfile`. The root `docker-compose.yml` (in `Menu_Service` directory, to be moved to root) orchestrates everything.

### Sample Dockerfile (per service)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Services/Auth_Service/Auth-Service.csproj"
RUN dotnet build "Services/Auth_Service/Auth-Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Services/Auth_Service/Auth-Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth-Service.dll"]
```

### docker-compose.yml (structure)

```yaml
version: "3.8"

services:
  api-gateway:
    build: ./API-Gateway/EatOClock.Gateway
    ports:
      - "5000:80"
    depends_on:
      - auth-service
      - restaurant-service
      - menu-service
      - order-service
      - delivery-service

  auth-service:
    build: ./Services/Auth_Service
    ports:
      - "8081:80"
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=eatoclock_auth;...
      - JwtSettings__Secret=${JWT_SECRET}

  restaurant-service:
    build: ./Services/Restaurant_Service
    ports:
      - "8082:80"
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=eatoclock_restaurants;...

  menu-service:
    build: ./Services/Menu_Service
    ports:
      - "8083:80"

  order-service:
    build: ./Services/Order_Service
    ports:
      - "8084:80"

  delivery-service:
    build: ./Services/DeliveryAgent_Service
    ports:
      - "8085:80"

  redis:
    image: redis:alpine
    ports:
      - "6379:6379"

  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - pgdata:/var/lib/postgresql/data

volumes:
  pgdata:
```

### Environment Variables

Create a `.env` file at the root (never commit this):

```env
JWT_SECRET=your-super-secret-key-minimum-32-characters
POSTGRES_PASSWORD=your-db-password
POSTGRES_HOST=localhost
```

---

## 🗺️ Roadmap

### Phase 1 — Core Services ✅ Complete

- [x] Auth Service (JWT, RBAC, profile management)
- [x] Restaurant Service (CRUD, geo-search, admin approval)
- [x] Menu Service (categories, items, availability toggle)
- [x] Order Service (lifecycle, assignment, cancel)
- [x] Delivery Agent Service (registration, location, SignalR)
- [x] API Gateway (YARP, routing, JWT validation)
- [x] Building Blocks (Auth, Cache, Logging, HealthChecks, EventBus stub)

### Phase 2 — Commerce Layer 🔜 In Progress

- [ ] **Cart Service** — single-restaurant cart, promo codes, quantity management
- [ ] **Payment / Wallet Service** — COD, wallet debit/credit, transaction history
- [ ] Payment gateway integration (Razorpay / Stripe)
- [ ] Wallet top-up and refund flows

### Phase 3 — Engagement Layer 📋 Planned

- [ ] **Review & Rating Service** — food rating, agent rating, review moderation
- [ ] **Notification Service** — email (MailKit), SMS (Twilio), in-app SignalR
- [ ] EventBus integration with RabbitMQ (replace stub)
- [ ] Order timeout automation (Hangfire background jobs)

### Phase 4 — Intelligence & Scale 🌟 Future

- [ ] Analytics dashboard API (revenue by period, peak hours, top items)
- [ ] Admin platform analytics endpoints
- [ ] Cursor-based pagination on all list endpoints
- [ ] Rate limiting per user (not just per IP)
- [ ] OpenTelemetry distributed tracing
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Kubernetes deployment manifests (Helm charts)
- [ ] API versioning (`/api/v2/...`)

---

<div align="center">

**Built with ❤️ using ASP.NET Core 8 · PostgreSQL · Redis · SignalR · Docker**

_EatOClock — Order Smarter. Eat Better. Delivered Faster._

</div>
