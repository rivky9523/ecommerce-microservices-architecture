# ECommerce Monolith — Phase 1 Baseline

A simple **.NET 8 WebAPI monolith** for an e-commerce order system, backed by
**one SQL Server** relational database. It contains Products, Inventory and
Orders logic in a single deployable. This is **Phase 1** of a course project
that will later evolve into production-style microservices.

> Phase 1 is intentionally simple — no auth, no messaging, no caching, no
> microservices. It is the baseline we will improve and measure against.

## What it does

- Create / browse / update products
- Set and read inventory per product
- Place an order, which:
  - checks every requested product exists and is active
  - checks there is enough available inventory
  - if yes → decreases available stock (reserves it) and creates a **Confirmed** order
  - if no → returns a clear validation error (HTTP 422), nothing is saved

## Tech stack

.NET 8 WebAPI · Entity Framework Core · SQL Server · Docker · Docker Compose · Swagger

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (includes Docker Compose)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) — temporarily
  required because we publish the app **on the host** (see note below).

> **⚠️ Temporary build workaround (Phase 1).** On this machine, NuGet restore
> fails *inside* Docker (`NU1301: unable to load the service index for
> api.nuget.org` — a network/proxy/certificate issue, not a code issue). To
> work around it we publish the app locally first, and the Docker image just
> runs the published output. Once Docker can reach NuGet again, the Dockerfile
> can return to a normal multi-stage build that compiles inside the container.

## How to run

From the repository root, run these **two** commands:

```bash
# 1) Publish the app locally (restores/builds on the host, where NuGet works)
dotnet publish ./src/ECommerce.Monolith.Api/ECommerce.Monolith.Api.csproj -c Release -o ./src/ECommerce.Monolith.Api/publish

# 2) Build the image from that output and start everything
docker compose up --build
```

This builds the API image and starts two containers:

- `ecommerce-sqlserver` — SQL Server (port `1433`)
- `ecommerce-api` — the WebAPI (port `8080`)

The API waits for the database to be healthy, then auto-creates the schema on
startup. First run takes a minute or two while images download and SQL Server
initializes.

To stop: press `Ctrl+C`, then optionally `docker compose down` (add `-v` to
also delete the database volume).

## How to test with Swagger

1. Open **http://localhost:8080/** in your browser (Swagger UI is served at the root).
2. Use the endpoints below in order to see the full flow.

### Suggested test flow

1. **POST `/api/products`** — create a product:
   ```json
   {
     "name": "Wireless Mouse",
     "description": "Ergonomic 2.4GHz mouse",
     "price": 29.90,
     "category": "Accessories",
     "isActive": true
   }
   ```
   Note the returned `id` (e.g. `1`).

2. **GET `/api/products`** — confirm the product is listed.

3. **PUT `/api/inventory/1`** — set stock for product `1`:
   ```json
   { "quantityAvailable": 10, "quantityReserved": 0 }
   ```

4. **GET `/api/inventory/1`** — confirm `quantityAvailable` is `10`.

5. **POST `/api/orders`** — place an order:
   ```json
   {
     "customerEmail": "buyer@example.com",
     "items": [ { "productId": 1, "quantity": 3 } ]
   }
   ```
   You should get a **Confirmed** order with `totalAmount` = `89.70`.

6. **GET `/api/inventory/1`** — confirm stock decreased: `quantityAvailable` is
   now `7` and `quantityReserved` is `3`.

7. **POST `/api/orders`** with `quantity: 999` — should fail with HTTP **422**
   and a clear "Insufficient inventory" message (nothing is saved).

## Main endpoints

| Area | Method | Route |
|------|--------|-------|
| Products  | POST | `/api/products` |
| Products  | GET  | `/api/products` |
| Products  | GET  | `/api/products/{id}` |
| Products  | PUT  | `/api/products/{id}` |
| Inventory | GET  | `/api/inventory/{productId}` |
| Inventory | PUT  | `/api/inventory/{productId}` |
| Orders    | POST | `/api/orders` |
| Orders    | GET  | `/api/orders` |
| Orders    | GET  | `/api/orders/{id}` |
| Health    | GET  | `/health` |

## Project structure

```
src/ECommerce.Monolith.Api/
  Controllers/   # thin HTTP endpoints
  Services/      # business logic (Products, Inventory, Orders)
  Data/          # EF Core AppDbContext
  Entities/      # database entities
  DTOs/          # request/response shapes
  Program.cs     # startup, DI, Swagger, DB init
docs/
  monolith-architecture.md   # diagram, endpoints, scaling problems
docker-compose.yml           # API + SQL Server
```

## Phase 1 checkpoint checklist

- [ ] `docker compose up --build` starts both containers
- [ ] API is reachable and Swagger loads at http://localhost:8080/
- [ ] Database runs and schema is created automatically
- [ ] A product can be created (POST `/api/products`)
- [ ] Products can be browsed (GET `/api/products`)
- [ ] Inventory can be set/updated (PUT `/api/inventory/{productId}`)
- [ ] An order can be placed and is **Confirmed** (POST `/api/orders`)
- [ ] Inventory decreases after the order
- [ ] An over-quantity order is rejected with a clear error (HTTP 422)
- [ ] `README.md` exists
- [ ] `docs/monolith-architecture.md` exists
