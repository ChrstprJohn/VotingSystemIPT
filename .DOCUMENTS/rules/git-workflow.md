# Team Git Workflow & Branching Rules

To ensure team collaboration stays stable, follow these branch and contribution guidelines.

---

## 1. Branch Hierarchy

```
[main] (Production / Final Submission - Protected)
   ▲
   │ (Pull Request after testing)
[staging] (Integration Branch - Default Working Target)
   ▲
   │ (Pull Request from feature branch)
[feature/your-name-task-description] (Individual Developer Work)
```

- **`main`**: Protected branch. Direct pushes are **strictly forbidden**. Only tested code from `staging` is merged here.
- **`staging`**: The shared team integration branch. All feature branches must branch off and merge back into `staging`.
- **`feature/*`**: Your isolated development branches.

---

## 2. Daily Workflow Step-by-Step

### Step 1: Sync with `staging`
```bash
git checkout staging
git pull origin staging
```

### Step 2: Create a feature branch
```bash
git checkout -b feature/<your-name>-<short-description>
```
*Examples:*
- `feature/add-candidate-model`
- `feature/style-voting-cards`
- `bugfix/fix-login-redirect`

### Step 3: Commit your work
Make small, focused commits with clear messages:
```bash
git add .
git commit -m "Add responsive candidate vote cards with custom CSS"
```

### Step 4: Push and Open a Pull Request (PR)
```bash
git push -u origin feature/<your-name>-<short-description>
```
- Go to GitHub and open a Pull Request.
- **Base branch**: `staging`
- **Compare branch**: `feature/<your-name>-<short-description>`
- Wait for team review before merging.
