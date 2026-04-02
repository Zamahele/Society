# Society Management System

A web-based membership and funeral benefit management system built with ASP.NET Core MVC.

## Purpose

This system helps a funeral society manage its members, monthly contributions, and death claims. When a covered member or their registered dependant passes away, the main member submits a claim. Upon approval the society pays out R15,000 cash and R15,000 in grocery vouchers (R30,000 total).

---

## Roles

| Role | Responsibilities |
|---|---|
| **Member** | Self-register online, add dependants, view payment history, submit death claims |
| **Clerk** | Confirm payments, review submitted claims, manage member records |
| **Admin** | Everything a clerk can do, plus approve/reject claims, record payouts, create clerk accounts |

---

## Member Registration Flow

```
Member registers online using ID Number
            |
            v
System generates MembershipNumber (e.g. SOC-0001)
            |
            v
Member adds dependants (up to 10)
            |
            v
Member pays R150 joining fee via EFT
using MembershipNumber as payment reference
            |
            v
Clerk confirms payment in system
            |
            v
Status: ACTIVE
            |
            v
Member pays R150 per month (EFT, same reference)
Clerk confirms each month
```

---

## Death Claim Flow

```
Covered person passes away (main member or registered dependant)
            |
            v
Main member logs in and submits claim
Uploads death certificate (stored in database)
            |
            v
System validates eligibility:
  - Member status is Active?
  - 6 months waiting period from activation date met?
  - No monthly payment overdue more than 30 days?
            |
        Pass / Fail
            |
     Status: Submitted
            |
     Clerk reviews -> Status: UnderReview
            |
     Admin decision
      |           |
  Rejected      Approved
      |           |
  Notify      Admin records R15,000 cash payout
  member      Admin records R15,000 grocery voucher
                  |
           Status: FullyPaid
```

---

## Business Rules

| Rule | Detail |
|---|---|
| Membership number | Format `SOC-0001`, auto-incremented, padded to 4 digits |
| Max dependants | 10 per membership |
| Joining fee | R150, once-off, paid before activation |
| Monthly fee | R150 per month, ongoing |
| Grace period | 30 days before a missed payment suspends the membership |
| Waiting period | Member must be active for 6 months before submitting a claim |
| Claim payout | R15,000 cash + R15,000 grocery voucher = R30,000 total |
| Payout recipient | Main member (their registered bank account) |
| Duplicate claims | System prevents more than one claim per deceased person |
| Eligibility check | Performed at claim submission time |
| Login | Members use ID Number as username and create their own password |

---

## Member Status Values

| Status | Meaning |
|---|---|
| `Pending` | Registered, joining fee not yet confirmed |
| `Active` | Joining fee confirmed, payments up to date |
| `Suspended` | Monthly payment overdue more than 30 days |
| `Cancelled` | Membership terminated |

---

## Claim Status Values

| Status | Meaning |
|---|---|
| `Submitted` | Claim submitted by member, awaiting clerk review |
| `UnderReview` | Clerk is reviewing the claim |
| `Approved` | Admin approved, payout to be processed |
| `Rejected` | Admin rejected, reason recorded |
| `PartiallyPaid` | Cash paid, voucher pending (or vice versa) |
| `FullyPaid` | Both cash and voucher paid out |

---

## Data Model

### ApplicationUser (extends ASP.NET Identity)
```
IDNumber          - used as username for login
FullName
Phone
Address
DateOfBirth
BankAccountName
BankAccountNumber
BankName
```

### Membership
```
Id
MembershipNumber  - SOC-0001
UserId            - FK to ApplicationUser
Status
DateIssued
DateActivated
```

### MemberDependant
```
Id
MembershipId      - FK to Membership
FullName
IDNumber
DateOfBirth
Relationship      - Spouse | Child | Parent | Sibling | Other
DateAdded
```

### JoiningFeePayment
```
Id
MembershipId      - FK
Amount            - 150.00
PaymentDate
PaymentReference  - EFT reference provided by member
Status            - Pending | Confirmed
ConfirmedByClerkId
ConfirmedDate
```

### MonthlyPayment
```
Id
MembershipId      - FK
ForMonth          - e.g. 2026-04
Amount            - 150.00
PaymentDate
PaymentReference
Status            - Pending | Confirmed | Missed
ConfirmedByClerkId
ConfirmedDate
```

### DeathClaim
```
Id
MembershipId          - FK
DeceasedType          - MainMember | Dependant
DependantId           - FK nullable
DeceasedFullName
DateOfDeath
DeathCertificateData  - byte[] stored in SQL Server
DeathCertificateFileName
ClaimDate
ClaimStatus
CashAmount            - 15000.00
CashPaidDate
VoucherAmount         - 15000.00
VoucherReference
VoucherPaidDate
RejectionReason
ProcessedByAdminId
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| UI | Razor Pages + Bootstrap 5 |
| ORM | Entity Framework Core |
| Database | SQL Server |
| Authentication | ASP.NET Identity (ID Number as username) |
| File storage | `varbinary(max)` column in SQL Server — no file system required |

---

## Project Structure

```
SocietyApp/
├── Controllers/
│   ├── AccountController.cs
│   ├── MembersController.cs
│   ├── DependantsController.cs
│   ├── PaymentsController.cs
│   ├── ClaimsController.cs
│   └── AdminController.cs
├── Models/
│   ├── ApplicationUser.cs
│   ├── Membership.cs
│   ├── MemberDependant.cs
│   ├── JoiningFeePayment.cs
│   ├── MonthlyPayment.cs
│   └── DeathClaim.cs
├── Data/
│   └── AppDbContext.cs
├── Services/
│   ├── MembershipService.cs
│   ├── PaymentService.cs
│   └── ClaimService.cs
├── ViewModels/
│   ├── RegisterViewModel.cs
│   ├── AddDependantViewModel.cs
│   ├── ConfirmPaymentViewModel.cs
│   └── SubmitClaimViewModel.cs
└── Views/
    ├── Account/
    ├── Members/
    ├── Payments/
    ├── Claims/
    └── Admin/
```

---

## Getting Started (Development)

### Prerequisites
- .NET 8 SDK
- SQL Server or SQL Server Express (LocalDB works for development)
- Visual Studio 2022 or VS Code

### Setup
```bash
git clone https://github.com/Zamahele/Society.git
cd Society/SocietyApp
dotnet restore
# Update connection string in appsettings.json
dotnet ef database update
dotnet run
```

### Seed Data
On first run the system seeds:
- One Admin account
- One Clerk account
- Default membership fee amounts (R150 joining, R150 monthly)

---

## Development Roadmap

- [x] System design and documentation
- [ ] Project scaffold + Identity setup
- [ ] Models and database migrations
- [ ] Member registration and dependant management
- [ ] Payment confirmation (joining fee + monthly)
- [ ] Death claim submission and file upload
- [ ] Clerk review workflow
- [ ] Admin approval and payout recording
- [ ] Admin dashboard (member stats, pending claims, monthly collection)
