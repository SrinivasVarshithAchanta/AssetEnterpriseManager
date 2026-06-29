# Agile Notes

Supports planning structure for the resume claim about **Agile methodology / collaboration**. Wording is intentionally interview-safe.

---

## How to explain Agile in an interview without overclaiming

**Say this:**
> “I organized the project using Agile-style planning — short phases, user stories, and acceptance criteria. I tracked work in a simple board and used Git-style feature breakdowns. Where I collaborated, we reviewed each other's changes before merging.”

**Do not say this unless it is true:**
> “I worked on an enterprise Agile team with daily standups and sprint ceremonies at a company.”

If you built this mostly solo with learning support, say:
> “I applied Agile **principles** to my own development process — breaking work into sprints, writing user stories, and defining done criteria before moving to the next feature.”

---

## Sprint-style breakdown (4 phases)

### Phase 1 — Authentication and database design
- Entities, relationships, EF Core migrations
- Cookie auth with claims; password hashing
- Seed demo accounts (admin, manager, employee)
- **Done when:** all three roles log in and land on the correct page

### Phase 2 — Asset management
- Asset list with search, filter, pagination
- Create / edit / retire assets; category management
- **Done when:** admin can manage inventory end-to-end

### Phase 3 — Request and approval workflow
- Employee create/cancel requests
- Manager approve/reject (comment required on reject)
- Admin fulfil approved requests
- **Done when:** Pending → Approved → Fulfilled with asset assigned

### Phase 4 — Testing, optimization, documentation
- xUnit service tests; coverlet coverage
- Query optimization (indexes, `AsNoTracking`, server-side paging)
- Performance benchmark console app
- Interview and CV evidence docs
- **Done when:** tests pass, benchmark documented, lists stay responsive with seeded data

---

## User stories and acceptance criteria

**US-1 — Employee requests an asset**
- *As an* employee, *I want* to submit a request with category, priority, and reason *so that* I can get equipment.
- **AC:** Given valid input (reason ≥ 10 chars), a Pending request with unique `RequestNumber` appears in My Requests.

**US-2 — Manager rejects with reason**
- *As a* manager, *I want* to reject with a mandatory comment *so that* the employee understands why.
- **AC:** Reject without comment fails; with comment, status = Rejected and comment is stored.

**US-3 — Admin fulfils approved request**
- *As an* admin, *I want* to assign an available asset *so that* inventory stays accurate.
- **AC:** Only Approved requests; only Available non-retired assets; asset becomes Assigned; request becomes Fulfilled.

**US-4 — Fast lists at scale**
- *As a* user, *I want* paginated search *so that* lists stay fast with hundreds of records.
- **AC:** Users/Assets/Requests pages show one page at a time; filters run server-side (see `PERFORMANCE_RESULTS.md`).

**US-5 — Audit trail**
- *As an* admin, *I want* audit logs *so that* important actions are traceable.
- **AC:** Login, create, approve, reject, fulfil actions write to `AuditLogs`.

---

## Task breakdown (example board columns)

| To Do | In Progress | Done |
|-------|-------------|------|
| User entity + migration | Auth service | Login page |
| Asset CRUD | Request workflow | Asset list + filters |
| Approval UI | xUnit tests | Seed 500 users |
| Benchmarks | — | PERFORMANCE_RESULTS.md |

---

## Team collaboration template (fill in honestly)

If you worked with classmates or a mentor:

- **Who:** [names or “solo with mentor review”]
- **How we coordinated:** [e.g. WhatsApp / weekly call / pair review]
- **What I owned:** [e.g. backend services + EF Core]
- **What others owned:** [e.g. UI polish / test cases] — *skip if solo*
- **How we integrated:** [e.g. Git branches + merge after review]

If solo: state that clearly and emphasize self-managed Agile-style planning.
