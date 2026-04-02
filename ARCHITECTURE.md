# Architecture Decision Record — Society Management System

## Decision

Use **MVC + Service Layer** architecture in a single project.  
Services are backed by interfaces to keep the system **unit testable** without adding unnecessary complexity.

---

## Architecture Pattern: MVC + Service Layer

### Why Not Onion / Clean Architecture

| Concern | Decision |
|---|---|
| Team size | Small, likely one maintainer |
| Domain complexity | Straightforward — membership, payments, claims |
| Speed of delivery | Single project is faster to scaffold and navigate |
| Testability | Achieved through service interfaces, not layer separation |

Onion Architecture (Domain / Application / Infrastructure / Web projects) is reserved for:
- Large teams working on separate layers simultaneously
- Complex domains requiring isolated business logic
- Systems that need to swap databases or external services
- Long-term enterprise products

None of those apply here.

---

## Request Flow

```
HTTP Request
      |
      v
Controller          - Thin. Receives request, calls service, passes ViewModel to View.
      |
      v
IService            - Interface. Controller depends on this, never on the class directly.
      |
      v
Service             - All business logic lives here. Eligibility checks, ID generation,
      |               status transitions, validation rules.
      v
AppDbContext        - EF Core. Direct data access. No repository pattern.
      |
      v
SQL Server
```

---

## Project Structure

```
SocietyApp/                            ← Main project
│
├── Controllers/
│   ├── AccountController.cs           ← Login, register, logout
│   ├── MembersController.cs           ← Member registration, dependants
│   ├── PaymentsController.cs          ← Joining fee + monthly payments
│   ├── ClaimsController.cs            ← Death claim submission + tracking
│   └── AdminController.cs             ← Admin and clerk management
│
├── Models/                            ← EF Core entities (DB tables)
│   ├── ApplicationUser.cs             ← Extends IdentityUser
│   ├── Membership.cs
│   ├── MemberDependant.cs
│   ├── JoiningFeePayment.cs
│   ├── MonthlyPayment.cs
│   └── DeathClaim.cs
│
├── Data/
│   └── AppDbContext.cs                ← DbContext, relationships, seeding
│
├── Services/
│   ├── Interfaces/
│   │   ├── IMembershipService.cs      ← Contract for membership operations
│   │   ├── IPaymentService.cs         ← Contract for payment operations
│   │   └── IClaimService.cs           ← Contract for claim operations
│   ├── MembershipService.cs           ← MembershipNumber generation, status transitions
│   ├── PaymentService.cs              ← Payment confirmation, grace period logic
│   └── ClaimService.cs                ← Eligibility checks, claim state management
│
├── ViewModels/                        ← What views receive and send (never raw models)
│   ├── RegisterViewModel.cs
│   ├── AddDependantViewModel.cs
│   ├── ConfirmPaymentViewModel.cs
│   └── SubmitClaimViewModel.cs
│
└── Views/                             ← Razor views, no business logic
    ├── Account/
    ├── Members/
    ├── Payments/
    ├── Claims/
    └── Admin/

SocietyApp.Tests/                      ← Separate test project
├── Services/
│   ├── MembershipServiceTests.cs
│   ├── PaymentServiceTests.cs
│   └── ClaimServiceTests.cs
└── Controllers/
    ├── MembersControllerTests.cs
    └── ClaimsControllerTests.cs
```

---

## Service Interface Rule

Every service has a matching interface. Controllers are registered against the interface, never the concrete class.

```csharp
// Correct — controller depends on interface
public class ClaimsController : Controller
{
    private readonly IClaimService _claimService;

    public ClaimsController(IClaimService claimService)
    {
        _claimService = claimService;
    }
}

// Registered in Program.cs
builder.Services.AddScoped<IClaimService, ClaimService>();
```

This means in tests we can mock `IClaimService` without touching the database.

---

## Test Project Stack

| Tool | Purpose |
|---|---|
| `xUnit` | Test framework |
| `Moq` | Mock interfaces (IClaimService, IPaymentService, etc.) |
| `FluentAssertions` | Readable, expressive assertions |
| `EF Core InMemory` | In-memory database for service-level tests without SQL Server |

---

## What We Test

| Layer | What | How |
|---|---|---|
| Services | Business rules — eligibility, grace period, waiting period, ID generation | EF Core InMemory + xUnit |
| Controllers | Correct ViewModel returned, correct redirect on success/fail | Moq interfaces + xUnit |
| We do NOT test | EF Core queries, Razor views, Identity framework | Covered by framework itself |

---

## Dependency Injection Registration (Program.cs)

```csharp
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IClaimService, ClaimService>();
```

---

## Key Rules

- Controllers must be thin — no business logic inside a controller
- Business logic lives in services only
- Views receive ViewModels — never raw EF Core models
- Services receive and return plain objects or primitives — never HttpContext or ViewData
- No static classes for business logic
- No repository pattern — DbContext is injected directly into services
