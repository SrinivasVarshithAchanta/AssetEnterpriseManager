# Git / GitHub Evidence

## Current status

**This folder is NOT currently a Git repository.**  
Verified: no `.git` directory exists at:

`C:\Users\s_var\OneDrive\Desktop\Personal projects\EnterpriseAssetManager`

There is **no commit history** in this folder yet. Do not claim an existing GitHub repo unless you create and push one.

## What already exists

| Item | Location | Purpose |
|------|----------|---------|
| `.gitignore` | solution root | Excludes `bin/`, `obj/`, `TestResults/`, `CoverageReport/`, `.vs/`, etc. |
| Suggested workflow | `README.md` → “Suggested Git Workflow” | Branch names + commit checkpoints |
| SDLC notes | `README.md`, `TEST_PLAN.md` | Manual verification checklist |

## Suggested Git workflow

Branches:
- `main` — stable, runnable app
- `feature/authentication`
- `feature/asset-management`
- `feature/request-approval`
- `feature/testing`
- `feature/performance-benchmarks`

Suggested commit checkpoints (create these as you go — do not invent past commits):
1. Initial working Enterprise Asset Manager project
2. Add database models and migrations
3. Add authentication and role-based access
4. Add asset management and categories
5. Add request and approval workflow
6. Add xUnit tests and coverage tooling
7. Add performance benchmarks and documentation
8. UI polish and interview docs

## Initialize Git (exact commands)

```cmd
cd "C:\Users\s_var\OneDrive\Desktop\Personal projects\EnterpriseAssetManager"
git init
git add .
git commit -m "Initial working Enterprise Asset Manager project"
git branch -M main
```

## Push to GitHub (after creating an empty repo on github.com)

```cmd
git remote add origin https://github.com/YOUR_USERNAME/EnterpriseAssetManager.git
git push -u origin main
```

Replace `YOUR_USERNAME` and repo name with your actual GitHub details.

## Interview-safe wording

**Strong (after you push):**
> “The project is version-controlled with Git, uses a `.gitignore` for .NET, and I pushed it to GitHub with feature-style commits for auth, assets, requests, and tests.”

**Before you push:**
> “The project is ready for Git — `.gitignore` and a suggested branch workflow are documented; I initialize the repo locally before sharing on GitHub.”

**Do not claim:**
> “We used GitHub throughout development” — unless you actually have commits and a remote.
