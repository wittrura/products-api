# Products API
Simple backend powered by .NET with Entity Framework, consumed by a modern Angular (v21) frontend

## Quick Start
Prerequisites
- .NET SDK 10.0+
- Node.js 18+
- npm
- Angular CLI if you want it globally - optional, but useful

### Backend .NET API
```
cd src/Api
dotnet restore
dotnet run
```

This will start the API at `http://localhost:5062`, and you can curl to verify:
```
❯ curl http://localhost:5062/health
{"status":"ok"}⏎
```

### Frontend Angular
```
cd client/storefront
npm install
npm start
```

This will start the Angular app at `http://localhost:4200`


## Architecture
The intent is to follow a clean, layered architecture

Within the .NET API:
- API - top-level HTTP experience
- Application - DTO's / validation / business rules, putting domain concepts into practical application logic
- Domain - focused on core domain entities and concepts
- Infrastructure - persistence concerns, anything that could need to be flexible (different databases, logging systems, etc.)

### Database schema
Categories: ID, Name, Description, IsActive

Products: ID, Name, Description, Price, StockQuantity, CreatedDate, IsActive, CategoryId (FK)

Mental model - Category has many Products, and each Product has one Category, so this is defined as a one-to-many relationship. This is reflected in the entity mappings

### Technology choices
- .NET Core - minimal API style, based on this being a small app
- EF Core - easy mapping of entities
- SQLite - zero-setup local testing and file-based persistence, fine given this app is meant as a dev experiment
- Angular 21 - almost mean as a learning exercise, compared to earlier versions. Makes use of new conventions in structuring services and components, dependency injection, and Signals for state management


## Design Descisions
### Single Responsibility & Dependency Inversion
For single responsibility, components are scoped to handle only the bare minimum data necessary to perform its basic function. In the application, this means that validation is a separate concern from routing, so the different `Validators` are separated from the API.

The API structure and dependency graph shows the application of dependency inversion. Our Domain has no knowledge of Entity, which means we end up with lightweight and fully isolated POCO's. Then we make use of DTO's in the Application to remove any coupling between our application models and the Domain entities.

The result is:
- Domain - no dependencies
- Application - depends on Domain
- Infrastructure - depends on Domain
- API - depends on Application and Infrastructure

Core models are decoupled from persistence. And we have less-stable layers depending on more stable - domain may only change a little or none, while infrastructure can change and changes separately from the domain's needs (aka: "save costs by using a different DB", not "products have completely changed as a concept")

### EF Core Approach & Query Optimization
Projection to DTO's lets us select only the data we need before shaping it directly into a DTO. We alternatively could have used `Include`, but that would mean loading full entity graphs for both Products and Categories in our use case. Projection avoids over-fetching and also prevents N+1 issues since we get only what we need and we get it by making use of our defined relationship between Products and Categories.

`AsNoTracking` is used for read-only ops to improve performance and memory usage by disabling change tracking

Server-side aggregation for analytics endpoints

### Complex Endpoint Choice
Analytics endpoint was chosen since it felt like a good way to practice and demonstrate thinking about query design and performance. The goal was to execute this as a single query by making use of all the collections functions that are available.

### Repository Pattern Decision
Intentioanally not used - EF Core already gives us everything we need out of the box, and adding a repository here would mostly just be creating a new class and interface that acts as a pass-through down to the DB component.

### Index Strategy
Indexes were chosen based on access patterns for our two entities:
- primary keys (implicit)
- composite index on `CategoryId` and `IsActive` for product filtering, based on soft-deletion

## What I Would Do With More Time

### Features
- Authentication and authorization
- Better global error handling, something like `UseExceptionHandler`, `Results.Problem`
- Search endpoint
- Pagination and filtering on product lists
- Management UI - adding categories, products

### Refactoring
- Controller API
- MediatR / CQRS
- Repository pattern - if queries start getting reused, business workflows come up outside the API layer, testing, background jobs or message handlers

### Production considerations
- "real" database, option for Postgres which can be run locally for testing via Docker - production parity, concurrency, indexing, partial indexes, advanced types, query planner
- Docker Compose - for building and deploying locally and across different environments, depends more on what the team's infrastructure looks like, but even if only used locally by devs, it's a nice way to quickly spin up an application instance for testing
- Data is seeded on startup, along with migrations - this would need to be separated out, likely via CI for production and lower environments


## Assumptions & Trade-offs
- SQLite - only meant for local development, and this actually unturned a few "interestings" in how SQLite
- soft-delete scalability - need to figure out things like retention policy - 30/90/365 days to purge, event and audit logs, archive table / partitioning (Postgres), database views vs application-level control, triggers (hidden behavior)
- frontend is very barebones, and I assumed that the team would be able to use this newer version of Angular - it is very new (Nov 2025)
- no respository pattern means short-term readability is prioritized over longer-term abstraction and flexibility