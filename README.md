# E-Commerce Order System — Monolith → Microservices

A course project that evolves an e-commerce order system from a monolith into
production-style microservices, phase by phase.

- **Phase 1 (done):** a single .NET 8 WebAPI monolith + one SQL Server database.
- **Phase 2 (done):** split into **4 microservices** with **database-per-service**
  and **polyglot persistence**.
- **Phase 3 (current):** add an **API Gateway (YARP)**, a **BFF**, and **load
  balancing** (2 ProductCatalog replicas behind Nginx). The gateway is the only
  exposed entry point.

> Phase 3 still uses synchronous HTTP between services. No message broker, saga,
> cache-aside, or monitoring yet — those are later phases.

---

## Phase 3 — API Gateway, BFF & Load Balancing (current)

**Everything is reached through the API Gateway at `http://localhost:8080`.** The
individual services are no longer exposed to the host.

### Gateway routes

| Through the gateway | Goes to |
|---|---|
| `http://localhost:8080/catalog/api/products` | ProductCatalogService (via Nginx LB → 2 replicas) |
| `http://localhost:8080/inventory/api/inventory/{productId}` | InventoryService |
| `http://localhost:8080/orders/api/orders` | OrderService |
| `http://localhost:8080/notifications/api/notifications` | NotificationService |
| `http://localhost:8080/bff/api/order-details/{orderId}` | WebBffService (BFF) |

### Gateway vs BFF

- **Gateway (YARP):** single entry point + generic, domain-agnostic **routing**
  (and future edge concerns like rate limiting). One path prefix → one service.
- **BFF (WebBffService):** **client-specific aggregation** — `order-details`
  combines an order (OrderService) with each item's product (ProductCatalogService)
  into one response. This domain logic belongs in the BFF, not the gateway.

### Run it

```bash
./publish-all.sh            # or  .\publish-all.ps1
docker compose up --build   # starts gateway + bff + nginx LB + 4 services + 4 DBs
```

`docker compose up` honors `deploy.replicas: 2` for ProductCatalogService. To use
more replicas: `docker compose up -d --scale productcatalog=3`.

### Test through the gateway (end-to-end)

```bash
# 1) create a product (note the returned id)
curl -X POST http://localhost:8080/catalog/api/products -H "Content-Type: application/json" \
  -d '{"name":"Mechanical Keyboard","price":75.00,"category":"Accessories","isActive":true,"attributes":{"switch":"blue"}}'

# 2) set inventory (use the id)
curl -X PUT http://localhost:8080/inventory/api/inventory/<ID> -H "Content-Type: application/json" \
  -d '{"quantityAvailable":20,"quantityReserved":0}'

# 3) place an order (use the id) -> note the order id
curl -X POST http://localhost:8080/orders/api/orders -H "Content-Type: application/json" \
  -d '{"customerEmail":"buyer@example.com","items":[{"productId":"<ID>","quantity":2}]}'

# 4) BFF aggregated order details (order + live product data)
curl http://localhost:8080/bff/api/order-details/<ORDER_ID>
```

### Prove load balancing

```bash
# Repeated calls alternate between the two replica container ids:
for i in $(seq 1 10); do curl -s http://localhost:8080/catalog/api/products/instance; echo; done

# Resilience: kill one replica, requests still succeed from the other:
docker stop project-ai-productcatalog-1
curl -s http://localhost:8080/catalog/api/products/instance
docker start project-ai-productcatalog-1
```

### Phase 3 checkpoint checklist

- [ ] Gateway runs and is reachable at http://localhost:8080/health
- [ ] Client can access all APIs through the gateway (catalog/inventory/orders/notifications)
- [ ] Internal services still run (and are no longer exposed directly to the host)
- [ ] BFF aggregates order + product data at `/bff/api/order-details/{id}`
- [ ] ProductCatalogService runs 2+ replicas
- [ ] Load-balancing proof works (alternating `instanceId` / `X-Instance-Id`)
- [ ] `docker compose up` runs everything from the root
- [ ] README and architecture docs are updated

---

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

> **⚠️ Temporary build workaround.** NuGet restore fails *inside* Docker on this
> machine (`NU1301`), so we publish each service **on the host** first and the
> Docker images only run the published output. Once Docker can reach NuGet again,
> the Dockerfiles can return to normal multi-stage builds.
>
> After **any code change**: `./publish-all.sh && docker compose up -d --build --force-recreate`.
> To stop: `docker compose down` (add `-v` to delete the data volumes).

---

## Phase 2 — Microservices (preserved, now behind the gateway)

The four services and their databases from Phase 2 are unchanged. **In Phase 3
their direct host ports are no longer exposed** — reach them through the gateway
(see the Phase 3 section above).

| Service | Responsibility | Database | Family | Gateway prefix |
|---|---|---|---|---|
| ProductCatalogService | products: create/list/get/update | **MongoDB** | document | `/catalog` |
| InventoryService | stock: get/update/reserve/release | **PostgreSQL** | relational | `/inventory` |
| OrderService | orders: place/list/get + orchestration | **SQL Server** | relational | `/orders` |
| NotificationService | record/"send" notifications | **Redis** | key-value | `/notifications` |

Each service owns its own database and **never** accesses another service's
database — the only cross-service access is HTTP. Database design rationale is in
the ADRs in [docs/adr/](docs/adr); architecture details in
[docs/microservices-architecture.md](docs/microservices-architecture.md).

Order placement (now also reachable via the gateway): OrderService validates each
product against ProductCatalogService, reserves stock via InventoryService,
persists a Confirmed/Rejected order, and records a notification via
NotificationService.

### Ports summary

| Component | URL / Port | Exposed to host? |
|---|---|---|
| **API Gateway** | http://localhost:8080 | **Yes (only app entry)** |
| ProductCatalogService (×2), Inventory, Order, Notification, BFF, catalog-lb | — | No (internal) |
| MongoDB / PostgreSQL / SQL Server / Redis | 27017 / 5432 / 1433 / 6379 | Yes (dev convenience) |

---

## Phase 1 — Monolith (baseline, preserved)

The original monolith lives in [src/ECommerce.Monolith.Api/](src/ECommerce.Monolith.Api)
and its compose file is [docker-compose.phase1.yml](docker-compose.phase1.yml).
It is kept for "before vs. after" comparison. To run it on its own:

```bash
dotnet publish ./src/ECommerce.Monolith.Api/ECommerce.Monolith.Api.csproj -c Release -o ./src/ECommerce.Monolith.Api/publish
docker compose -f docker-compose.phase1.yml up --build
```

Monolith docs: [docs/monolith-architecture.md](docs/monolith-architecture.md).

---

## Repository structure

```
docker-compose.yml            # Phase 3: gateway + bff + nginx LB + 4 services + 4 DBs
docker-compose.phase1.yml     # Phase 1 monolith (preserved)
publish-all.sh / .ps1         # publish all services before compose
infra/
  catalog-lb/nginx.conf       # Nginx load balancer for catalog replicas
src/
  ECommerce.Monolith.Api/     # Phase 1 baseline
  ProductCatalogService/      # MongoDB (runs as 2 replicas in Phase 3)
  InventoryService/           # PostgreSQL
  OrderService/               # SQL Server + HTTP clients
  NotificationService/        # Redis
  WebBffService/              # Phase 3 BFF (aggregation, no DB)
  ApiGateway/                 # Phase 3 YARP gateway (single entry point)
docs/
  monolith-architecture.md
  microservices-architecture.md   # includes the Phase 3 section + diagram
  adr/                        # one ADR per database choice
```
