# 🏁 Final Project — From Monolith to Production-Grade Microservices

## Goal

Take a working monolithic API and evolve it, step by step, into a distributed, production-style system that demonstrates everything we covered in this course: containers, microservices, caching, async messaging, saga, API gateway, BFF, load balancing, polyglot persistence, and monitoring.

> **💡 Important — you are NOT limited to the technologies we used in class.**
> Everywhere this document names a tool (RabbitMQ, Redis, Ocelot, Nginx, MongoDB, Serilog...), you may substitute an equivalent technology that was **not** taught in class — for example **Kafka instead of RabbitMQ**, **YARP instead of Ocelot**, **Traefik or HAProxy instead of Nginx**, **DynamoDB-local or CouchDB instead of MongoDB**, **OpenTelemetry + Grafana instead of ELK**.
> Using a technology we didn't cover is **encouraged** — but you must be able to **explain why you chose it** and how it compares to the class alternative.

---

## The Domain

You will build (or refactor) an **e-commerce order system**. You may start from:

- **Option A:** An existing monolithic API you already have (yours or open-source), or
- **Option B:** A minimal monolith you build yourself in Phase 1.

Core capabilities the final system must support:

- Browse products
- Place an order
- Reserve inventory
- Notify the customer when the order is confirmed or rejected

---

## Phase 1 — The Monolith Baseline (mandatory)

**Task 1.1** — Create (or adopt) a single .NET 8 WebAPI containing Orders, Products, and Inventory logic, backed by **one** relational database.

**Task 1.2** — Write a `docker-compose.yml` that runs the API + its database with a single command.

**Task 1.3** — Document the monolith: one diagram, list of endpoints, and 3 problems you expect this architecture to have at scale.

> 💡 *Hint: keep the monolith intentionally simple — it exists so you can compare "before vs. after". Don't gold-plate it.*

**✔ Checkpoint:** `docker compose up` → you can create a product, place an order, and see inventory decrease.

---

## Phase 2 — Split into Microservices (mandatory)

**Task 2.1** — Split the monolith into at least **4 services**:

- `OrderService`
- `ProductCatalogService`
- `InventoryService`
- `NotificationService`

**Task 2.2** — Apply **database-per-service**. Each service owns its data — no service touches another service's database.

**Task 2.3 — Polyglot persistence (NoSQL).** Choose the right database family per service and justify it:

- `ProductCatalogService` → a **document database** (e.g., MongoDB). Why does a catalog with varying attributes per category fit the document model?
- `OrderService` → stays **relational**. Why does money demand ACID?
- At least one more NoSQL decision of your choice (key-value, wide-column, graph, search...).

**Task 2.4** — Write a short **Architecture Decision Record (ADR)** per database choice, using the vocabulary from the Databases lesson: ACID, BASE, CAP, consistency model.

> 💡 *Hint: notice that Redis (Phase 4) is already a NoSQL key-value store — you'll be using two NoSQL families without even trying.*

> **💡 Reminder: any equivalent database technology is allowed** — Cosmos DB, RavenDB, Cassandra, Neo4j... as long as your ADR justifies the family choice.

**✔ Checkpoint:** all services run in docker-compose, each with its own data store, and an order can still be placed end-to-end (synchronous HTTP between services is fine *for now*).

---

## Phase 3 — Gateway, BFF & Load Balancing (mandatory)

**Task 3.1** — Add an **API Gateway** (Ocelot or YARP — **or any other gateway technology**). All client traffic enters through the gateway; services are no longer exposed directly.

**Task 3.2** — Add a **BFF** for a web client that aggregates data from at least two services in a single endpoint (e.g., "order details" = order data + product data).

**Task 3.3** — Run **2+ replicas** of one service (suggested: `ProductCatalogService`) behind a **load balancer** (Nginx — **or Traefik / HAProxy / built-in Docker load balancing**). Prove the load balancing works (hint: return the container ID in a response header and call the endpoint repeatedly).

> 💡 *Hint: think about what belongs in the gateway (routing, rate limiting, auth) vs. what belongs in the BFF (aggregation, client-specific shaping). Be ready to defend the boundary.*

**✔ Checkpoint:** the client talks only to the gateway; killing one catalog replica doesn't break the system.

---

## Phase 4 — Async Messaging, Saga & Caching (mandatory)

**Task 4.1** — Replace the synchronous order flow with **asynchronous messaging** using RabbitMQ — **or Kafka, or Azure Service Bus, or NATS**. If you choose a technology not taught in class, include a half-page comparison: *why this instead of RabbitMQ?*

**Task 4.2** — Implement an **Order Saga** (choreography-based):

1. `OrderService` publishes `OrderPlaced`
2. `InventoryService` reserves stock → publishes `InventoryReserved` or `InventoryRejected`
3. `OrderService` confirms or **compensates** (cancels the order, releases reservation)
4. `NotificationService` informs the customer of the final state

**Task 4.3** — Demonstrate the **failure path**: place an order for an out-of-stock product and show the compensation happening (logs/screenshots).

**Task 4.4** — Add **Redis** (or another distributed cache) to `ProductCatalogService` reads using the **cache-aside** pattern. Show cache hit vs. miss in your logs, and decide on an invalidation strategy when a product is updated.

> 💡 *Hint: at-least-once delivery means your consumers may receive the same message twice. What makes a consumer idempotent?*

**✔ Checkpoint:** happy path and compensation path both work end-to-end through the broker; repeated catalog reads hit the cache.

---

## Phase 5 — Monitoring & Observability (mandatory, small)

**Task 5.1** — Structured logging in every service (Serilog — **or any structured logging stack**), aggregated to one place (Seq, ELK, Loki...).

**Task 5.2** — A `/health` endpoint per service, wired into docker-compose healthchecks.

**Task 5.3** — **Correlation ID**: a single order's journey must be traceable across all services and the broker with one ID. Show one full saga traced in the logs.

> 💡 *Hint: the correlation ID has to survive the trip through the message broker, not just HTTP headers.*

**✔ Checkpoint:** given an order ID, you can show its complete story across all services from the log aggregator.

---

## 🌟 Bonus Phases (choose any — extra credit)

- **Kafka deep-dive:** if you used RabbitMQ, add one Kafka-based flow (or vice versa) and compare them in writing.
- **Orchestration Saga:** re-implement the saga with a central orchestrator and compare to choreography.
- **Search family:** add Elasticsearch for product search (and note it doubles as your log store).
- **Graph family:** add Neo4j (or another graph DB) for "customers also bought" recommendations.
- **Resilience patterns:** Polly-based retry + circuit breaker on inter-service calls; demonstrate the circuit opening.
- **Grafana dashboard:** metrics (request rate, error rate, queue depth) visualized.

### 🌟 Bonus Phase — CI/CD Pipeline (recommended bonus, up to +5%)

**Task B.1** — Create a pipeline (GitHub Actions — **or GitLab CI, Azure DevOps, Jenkins...**) that triggers on every push/PR and **builds all services**.

**Task B.2** — Run your **unit tests** in the pipeline and fail the build on a failing test. (Hint: this is a great place to reuse the AI-generated xUnit tests technique from the AI lab.)

**Task B.3** — Build a **Docker image per service** in the pipeline and tag it with the commit SHA.

**Task B.4** — *Stretch:* push the images to a registry (Docker Hub / GitHub Container Registry) and add a **smoke-test job**: run `docker compose up`, wait for all `/health` endpoints to return healthy, then tear down.

> 💡 *Hint: do you really need to rebuild every service on every push? Can your pipeline detect which folders changed?*

> **💡 As always — any CI/CD technology is allowed**, but explain your choice in the architecture document.

**✔ Checkpoint:** a green pipeline badge in your `README.md`, and a failing test visibly blocks the merge.

---

## 📦 Deliverables

1. **Git repository** — all services, `docker-compose.yml`, and a root `README.md` with one-command startup instructions.
2. **Architecture document** (2–4 pages): final diagram, the ADRs from Task 2.4, and your messaging-technology comparison if you went off-script.
3. **Demo evidence**: logs/screenshots of (a) the saga happy path, (b) the compensation path, (c) cache hit/miss, (d) one fully-traced correlation ID.
4. **Short presentation** (10 min): walk through your architecture and defend **two decisions** — one database choice and one technology substitution (or why you stayed with the class stack).

---

## 🧮 Grading Rubric

| Component | Weight |
|---|---|
| Phase 1 — Monolith + Docker Compose | 10% |
| Phase 2 — Microservices split + polyglot persistence + ADRs | 25% |
| Phase 3 — Gateway, BFF, Load Balancer | 15% |
| Phase 4 — Messaging, Saga, Caching | 25% |
| Phase 5 — Monitoring & Correlation | 10% |
| Architecture document & presentation | 15% |
| Bonus phases | up to +10% |
| CI/CD bonus phase | up to +5% (on top of other bonuses) |

---

## ⚠️ Rules

- **No copying full solutions.** AI assistants are allowed as helpers (as practiced in class) — but you must understand and be able to explain every line you submit.
- Every service must run via the single root `docker-compose.yml`.
- **Technology substitutions are welcome and rewarded — undocumented ones are not.** Every deviation from the class stack needs a written justification.

Good luck — this is the project that turns the course from lessons into an architecture you can defend. 🚀
