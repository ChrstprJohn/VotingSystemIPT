# VotingSystem

ASP.NET Core MVC voting system targeting .NET 10.

## Prerequisites
- .NET 10 SDK
- Visual Studio Community 2026 (recommended) or the `dotnet` CLI

## Quick start (Git workflow)
1. Clone the repo
   - git clone <repository-url>
   - cd VotingSystem

2. Ensure you are on `staging`
   - git fetch --all
   - git branch -a
   - git checkout staging
   - git pull origin staging

3. Create a feature branch off `staging`
   - git checkout -b feature/<your-name>-short-description
   - Make changes, then:
     - git add .
     - git commit -m "Short descriptive message"

4. Push and open a Pull Request
   - git push -u origin feature/<your-name>-short-description
   - Open a PR on GitHub with:
     - Base branch: `staging`
     - Compare: your feature branch
   - IMPORTANT: Do NOT push to or target `main`. All work must go through `staging`.

## Branch naming examples
- `feature/add-voter-form`
- `bugfix/fix-election-list`
- `hotfix/seed-data-patch`

## Run locally
- CLI:
  - dotnet restore
  - dotnet build
  - dotnet run
  - Or for hot-reload during development: `dotnet watch`
- Or open `VotingSystem.slnx` in Visual Studio 2026 and run.

## Notes
- `main` is protected — do not modify directly.
- Keep PRs focused and include testing steps.