# UI Knowledge Base

Reference for the SocietyApp Razor UI layer. Captures layouts, theme, page inventory,
role-based navigation, and the reusable patterns the views share.

For business logic see `SYSTEM_FLOW.md`. For project layering see `ARCHITECTURE.md`.

---

## 1. Layouts

`Views/_ViewStart.cshtml` sets `Layout = "_Layout"` for every view by default. The landing
page opts out and sets `Layout = "_LayoutLanding"`.

| Layout | File | Used by | Purpose |
|---|---|---|---|
| App shell | `Views/Shared/_Layout.cshtml` | All authenticated and most public views | Role-aware navbar, TempData alerts, reusable confirm modal, footer pulled from `PublicSiteSettings` |
| Landing shell | `Views/Shared/_LayoutLanding.cshtml` | `Home/Index.cshtml` only | Sticky dark navbar with hash-anchor section links, marketing footer with registration details, loads `landing.css` |

Both layouts query `AppDbContext.PublicSiteSettings` for org name, contact phones and
email. Hardcoded fallbacks (e.g. "Give Compassion NPC", Ulundi address) apply when no row
exists.

### Layout responsibilities

`_Layout.cshtml`:
- Brand link is role-aware: Admin/Clerk -> Admin Dashboard, Member -> Members Dashboard, anonymous -> Home.
- Renders `TempData["Success"]` and `TempData["Error"]` as dismissible Bootstrap alerts above `@RenderBody()`.
- Hosts `#confirmModal` (used by the global confirm script in `site.js`).
- Loads jQuery, Bootstrap bundle, `site.js`, and renders the optional `Scripts` section.

`_LayoutLanding.cshtml`:
- No auth-aware logic; only Login + "Join Now" CTAs.
- Loads `landing.css` in addition to `site.css`.

---

## 2. Theme

`wwwroot/css/site.css` defines the palette as CSS variables on `:root`:

| Variable | Value | Role |
|---|---|---|
| `--navy` | `#0f3460` | Primary brand, navbar, card headers, table headers |
| `--navy-dark` | `#0a2444` | Hover state for primary buttons |
| `--gold` | `#c9a84c` | Navbar brand text, focus rings, `.btn-gold` |
| `--gold-light` | `#f0c96a` | Brand hover, nav-link hover |
| `--bg-page` | `#f4f5f7` | Page background |

Card colour classes used on the admin dashboard tiles: `.card-navy`, `.card-navy2`,
`.card-gold`, `.card-slate`, `.card-teal`, `.card-rust`, `.card-plum`, `.card-olive`.

Auth pages (`Login`, `Register`, `ForgotPassword`, `SecurityQuestions`,
`ResetPasswordConfirm`) wrap content in `.auth-page` for the gradient background and
shadowed card.

Bootstrap 5 (local), bootstrap-icons (CDN), jQuery + jquery-validation-unobtrusive
(local) are the only client-side libraries.

---

## 3. Sitemap by role

### Public (`_LayoutLanding`)

- `Home/Index` - hero, How It Works (4 steps), Benefits, Fees, Banking, Committee, Contact. Pulls all dynamic copy from `PublicSiteSettings`.

### Anonymous (`_Layout`)

- `Account/Login` - ID Number + password.
- `Account/Register` - 3-step wizard: Personal -> Nominee (optional, can skip) -> Password. SA ID auto-fills date of birth.
- `Account/ForgotPassword` -> `Account/SecurityQuestions` -> `Account/ResetPasswordConfirm`.

### Member (`_Layout`)

- `Members/Dashboard` - the member's primary hub. Two-column layout:
  - Left: membership card with status-dependent alerts, edit-profile collapse, banking link, waiting-period progress bar, claim-eligibility badge, Nominee card with inline form.
  - Right: Dependants card (inline Add/Edit collapses + responsive table), Payment History (inline Submit Payment collapse, type switcher Joining/Monthly), Claims card (inline Submit Claim collapse, gated on eligibility).
- `Members/BankingDetails` - bank dropdown + account name/number.
- `Members/EditProfile`, `Members/AddDependant`, `Members/EditDependant`, `Members/Dependants` - older standalone pages; mostly superseded by inline collapses on the Dashboard.
- `Claims/MyClaims` - list of the member's own claims.
- `Claims/Submit` - standalone claim form.
- `Claims/NotEligible` - explains failed eligibility reasons.

### Admin / Clerk (`_Layout`)

- `Admin/Dashboard` - 7 coloured stat tiles linking to detail pages: Total / Active / Suspended Members, Pending (split into Awaiting Approval + Awaiting Payment), Claims (Pending Review + Total), Payment Confirmations (Joining Fees + Monthly).
- `Admin/Members` - all memberships. Desktop table + mobile card list. Inline Approve button for pending members.
- `Admin/MemberDetails` - per-member admin console:
  - Yellow "Admin Actions" strip: Approve / Reactivate / Deactivate / Cancel / Add Dependant / Delete Member Data (requires re-typing the ID number).
  - Left column: Member Info (with edit collapse), Nominee, Dependants list with edit/remove.
  - Right column: Monthly Payment History, Reset Password (collapse), Claims list with link to Details.
- `Admin/CreateMember` - office-walk-in registration. Banking details optional, temporary password required, DOB auto-fills.
- `Admin/AddDependantForMember`, `Admin/EditDependantForMember` - clerk/admin variants of the member-side forms.
- `Admin/PublicContent` - two-column form: enterprise + banking + contact settings on the left, committee CRUD on the right.
- `Admin/Clerks`, `Admin/CreateClerk` - Admin-only clerk account management. Activate / Deactivate uses `LockoutEnd`.
- `Payments/PendingJoiningFees`, `Payments/PendingMonthly` - confirmation queues. Same shape: responsive table + mobile cards, proof thumbnails (image lightbox) or PDF link, Confirm / Delete actions.
- `Payments/SubmitJoiningFee`, `Payments/SubmitMonthly` - older standalone payment-submission flows.
- `Claims/Index` - all claims across the system, responsive table + mobile cards.
- `Claims/Details` - full claim record. Role-gated controls: Clerk/Admin can move to Under Review; Admin can Approve / Reject (with reason) and record Cash / Voucher payouts.

### Errors

- `Shared/Error.cshtml` - generic error page with Request ID display, links back to Home and Login.

---

## 4. Reusable patterns

### Status badge colour mapping (duplicated across views)

`MembershipStatus`:

| Status | Class | Display label |
|---|---|---|
| `Active` | `bg-success` | Active |
| `Pending` | `bg-warning text-dark` | Pending |
| `PendingPayment` | `bg-info text-dark` | Pending Payment (space inserted) |
| `Suspended` | `bg-danger` | Suspended |
| `Cancelled` | `bg-dark` | Cancelled |

`ClaimStatus`:

| Status | Class |
|---|---|
| `FullyPaid` | `bg-success` |
| `Approved` | `bg-primary` |
| `Rejected` | `bg-danger` |
| `PartiallyPaid` | `bg-info text-dark` |
| anything else | `bg-warning text-dark` |

These `switch` expressions are inlined in `Members/Dashboard`, `Admin/Members`,
`Admin/MemberDetails`, `Claims/Index`, `Claims/Details`, `Claims/MyClaims`. Single point
of truth would be a tag helper or partial - currently it is not centralised.

### Global confirm modal

`_Layout.cshtml` renders `#confirmModal`. `wwwroot/js/site.js` intercepts:
- any `<button type="submit" data-confirm="...">` click - shows the modal with that
  message, suppresses the form submit until OK is clicked, then submits.
- any `<form data-confirm="...">` submit - same path.

Used wherever an action is destructive, e.g.:
- Remove dependant / nominee
- Delete pending payment, joining fee, monthly payment
- Cancel membership, Delete Member Data
- Deactivate / Delete clerk
- Remove committee member

To add a confirm prompt, just put `data-confirm="message"` on the submit button or form.

### Responsive list pages

Pattern repeated across `Admin/Members`, `Claims/Index`, `Payments/PendingJoiningFees`,
`Payments/PendingMonthly`:

```razor
<div class="d-none d-md-block">
    <table class="table table-hover"> ... full columns ... </table>
</div>
<div class="d-md-none row g-3">
    @foreach (...) { <div class="col-12"><div class="card shadow-sm"> ... </div></div> }
</div>
```

Desktop gets a wide table, narrow screens get stacked cards.

### Proof of payment lightbox

`Payments/PendingJoiningFees` and `Payments/PendingMonthly` render proof thumbnails:
- Images use a thumb with `class="proof-thumb"` and `data-full` / `data-label`. A small
  script in the view's `Scripts` section opens `#proofModal` and swaps the `src` / title.
- PDFs render as a button linking to `JoiningFeeProof` / `MonthlyProof` controller
  actions in a new tab.

### Inline-collapse forms on Dashboard

`Members/Dashboard.cshtml` heavily uses Bootstrap `.collapse` to keep forms on a single
page:
- `#editProfileForm`
- `#nomineeForm`
- `#addDependantForm`, `#editDependantForm` (with `openEditDependant(btn)` JS to
  populate fields from `data-*` attributes on the row's edit button)
- `#submitPaymentForm` (with a Joining/Monthly switcher that shows/hides the For Month
  field)
- `#submitClaimForm` (with a switcher for Member vs Dependant deceased)

`Admin/MemberDetails.cshtml` uses the same pattern for `#editProfileForm` and
`#resetPasswordForm`. `Admin/PublicContent.cshtml` uses `.collapse` for each committee
member's inline edit row.

The older standalone pages (`Members/AddDependant`, `EditDependant`, `EditProfile`,
`Payments/SubmitJoiningFee`, `Payments/SubmitMonthly`) still exist and remain reachable
through certain controller flows, but the Dashboard largely supersedes them.

### SA ID -> Date of Birth auto-fill

Repeated in `Register`, `Admin/CreateMember`, `Members/Dashboard` (add and edit forms),
`Members/AddDependant`, `Members/EditDependant`:

```js
function parseDobFromSaId(id) {
    id = id.replace(/\D/g, '');
    if (id.length < 6) return '';
    var yy = parseInt(id.substring(0, 2), 10);
    var mm = id.substring(2, 4);
    var dd = id.substring(4, 6);
    var currentYY = new Date().getFullYear() % 100;
    var yyyy = yy <= currentYY ? 2000 + yy : 1900 + yy;
    return yyyy + '-' + mm + '-' + dd;
}
```

Wired as `input` listener on the ID Number field, writing into a `readonly` DOB input.

### Proof file upload UX

`Members/Dashboard.cshtml` submit-payment form uses
`<input type="file" accept="image/*,.pdf" capture="environment">` plus an image preview
via `FileReader` to support mobile camera capture for proof of payment.

---

## 5. ViewModel reference

| ViewModel | Used by |
|---|---|
| `LoginViewModel` | `Account/Login` |
| `RegisterViewModel` | `Account/Register` |
| `ForgotPasswordViewModel` | `Account/ForgotPassword` |
| `SecurityQuestionsViewModel` | `Account/SecurityQuestions` |
| `ResetPasswordConfirmViewModel` | `Account/ResetPasswordConfirm` |
| `AddDependantViewModel` | `Members/AddDependant`, `Admin/AddDependantForMember` |
| `EditDependantViewModel` | `Members/EditDependant`, `Admin/EditDependantForMember` |
| `EditProfileViewModel` | `Members/EditProfile` |
| `UpdateBankingDetailsViewModel` | `Members/BankingDetails` |
| `SubmitJoiningFeeViewModel` | `Payments/SubmitJoiningFee` |
| `SubmitMonthlyPaymentViewModel` | `Payments/SubmitMonthly` |
| `SubmitClaimViewModel` | `Claims/Submit` |
| `CreateMemberViewModel` | `Admin/CreateMember` |
| `CreateClerkViewModel` | `Admin/CreateClerk` |
| `PublicContentAdminViewModel` | `Admin/PublicContent` |
| `Membership` (entity, not VM) | `Members/Dashboard`, `Admin/MemberDetails` |
| `IEnumerable<...>` | List views (Members, Claims, Dependants, PendingJoiningFees, PendingMonthly, MyClaims, Clerks) |
| `ErrorViewModel` | `Shared/Error` |

`ViewBag` is used to push supplementary data into views where the model is a list or a
single entity, e.g. `ViewBag.Eligibility`, `ViewBag.WaitingMonthsElapsed`,
`ViewBag.HasPendingJoiningFee`, `ViewBag.JoiningFeePayments`, `ViewBag.MonthlyPayments`,
`ViewBag.Claims`, `ViewBag.Dependants`, `ViewBag.CanAdd`, `ViewBag.MemberName`,
`ViewBag.MembershipNumber`, etc.

---

## 6. Convention quick-reference

- All forms use POST + anti-forgery tokens.
- Tag helpers (`asp-controller`, `asp-action`, `asp-for`, `asp-validation-for`,
  `asp-items`) registered globally via `Views/_ViewImports.cshtml`.
- Date formatting: `dd MMM yyyy` for display, `yyyy-MM-dd` for `type="date"` inputs.
- Money formatting: `R@(amount)` (e.g. `R150`, `R15000`).
- Destructive submit buttons use `btn-danger` or `btn-outline-danger` plus `data-confirm`.
- Primary action buttons inside cards use `btn-dark`; primary CTAs on auth/marketing
  pages use `btn-primary` (which the theme maps to navy).
- Card headers default to `bg-dark text-white` for dark panels or to the navy theme via
  the global `.card-header` rule.
- Bootstrap utility classes do most of the spacing; bespoke CSS is limited to the theme,
  the landing page, and the auth pages.

---

## 7. Known duplications / smells

Not blockers, but worth knowing when planning changes:

1. Status -> badge `switch` is inlined in at least six views. A `IHtmlHelper` extension
   or a `_StatusBadge.cshtml` partial would centralise it.
2. SA ID -> DOB JS is duplicated in five views. Could move to `site.js` as a small
   delegated handler on `[data-id-to-dob]` inputs.
3. Banking details block (Capitec / 1054981680 / 450105) is hardcoded in
   `Members/Dashboard.cshtml` and `Payments/SubmitMonthly.cshtml` but driven by
   `PublicSiteSettings` in `Home/Index.cshtml` and `_LayoutLanding.cshtml`. Single source
   would be the settings table.
4. Older standalone pages (`Members/AddDependant`, `EditDependant`, `EditProfile`,
   `Payments/SubmitJoiningFee`, `Payments/SubmitMonthly`, `Members/Dependants`) duplicate
   functionality already inlined on `Members/Dashboard`. Audit which routes still need
   them before deleting.
5. `Home/Privacy.cshtml` is a placeholder.
