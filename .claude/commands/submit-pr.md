Submit the current work as a pull request: $ARGUMENTS

## IMPORTANT: User confirmation gate

**This command must ONLY be run after the user has explicitly confirmed the changes are ready.** Before invoking `/submit-pr`, Claude must have:
1. Told the user what changed and asked them to test it
2. Waited for the user to confirm the feature works in their browser/IDE
3. Received an explicit go-ahead (e.g. "looks good", "ship it", "submit the PR")

If the user has NOT confirmed, **stop immediately** and ask them to test first. Playwright testing by Claude is not a substitute for user verification.

## Steps

Follow these steps in order. Do not skip any.

### 1. Understand what changed

- Run `git diff main...HEAD` and `git status` to get the full picture of what has been modified
- Read changed files if needed to understand the intent behind the changes
- Do NOT proceed until you have a clear understanding of what the work does and why

### 2. Sync with main

- Run `git fetch origin`
- Check if main has moved ahead: `git log HEAD..origin/main --oneline`
- If it has, rebase onto the latest: `git rebase origin/main`
- Resolve any conflicts carefully — never discard changes without understanding them

### 3. Create or switch to a feature branch

- Check the current branch: `git branch --show-current`
- If already on `main` or a stale branch, create a new one with a short, descriptive kebab-case name that reflects the work (e.g. `add-seafoam-theme`, `fix-auth-redirect`, `refactor-file-tree`)
- Branch name must NOT be `main`, `master`, or `dev`

### 4. Pre-flight safety checks

Before staging anything, run these checks. If ANY fail, **stop and tell the user** — do NOT proceed.

- **No secrets**: Ensure no `.env`, `appsettings.json`, credentials, or API keys are being staged.
- **No Playwright screenshots**: Delete any `.png` or `.jpeg` files in the repo root that were created by Playwright MCP during review. Do NOT delete files under `wwwroot/` — those are real assets.
- **Build passes**: Run `dotnet build Project.sln` and confirm 0 errors.

### 5. Documentation review

Before staging, check whether any project documentation needs updating to reflect the changes in this PR. Review each of the following files and update them if they are stale or incomplete:

- **`CLAUDE.md`** — project dependency graph, DI section, architecture sections, configuration section. If you added/removed projects, changed DI wiring, modified auth flow, or altered how config works, update the relevant sections.
- **`README.md`** (if it exists) — project description, setup instructions, prerequisites. If the PR changes how to build, run, or configure the app, update accordingly.
- **`TODO.md`** — mark completed items, update status of in-progress items, add new items if the PR creates follow-up work.
- **Agent files (`.claude/agents/*.md`)** — if the PR changes project structure, DI patterns, file locations, or conventions that agents reference, update the affected agent files so they don't give stale guidance.

Only make changes that are directly necessitated by the code changes in this PR. Do not speculatively rewrite documentation.

### 6. Stage changes

- Review all unstaged changes with `git diff`
- Stage files individually by name — do NOT use `git add -A` or `git add .` unless you have verified there are no unintended files (no `.env`, no build artifacts, no large binaries)
- Confirm staged changes with `git diff --staged`

### 7. Commit

- Write a commit message that explains **why** the change was made, not just what changed
- Use the imperative mood ("add seafoam theme" not "added seafoam theme")
- Keep the subject line under 72 characters
- If there are multiple logical changes, consider splitting into multiple commits
- Always include the co-author trailer:
  ```
  Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>
  ```

### 8. Push the branch

- Push with tracking: `git push -u origin <branch-name>`
- Do NOT force push unless the user explicitly asks

### 9. Open a pull request

Use `gh pr create` with a well-structured body. The PR description should include:

- **Summary**: 2–4 bullet points describing what changed and why — focus on intent, not a list of files
- **Changes**: a concise breakdown of the meaningful modifications
- **Test plan**: specific things to verify manually or via tests
- **Screenshots or notes** (if UI changes): call out what to look for

Use this format:

```
gh pr create --title "<short title>" --body "$(cat <<'EOF'
## Summary
- <why this work was done>
- <what problem it solves or what it improves>

## Changes
- <component or area>: <what changed>
- <component or area>: <what changed>

## Test plan
- [ ] <specific thing to verify>
- [ ] <specific thing to verify>

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

### 10. Confirm and report

- Print the PR URL to the user
- Summarise what was submitted in plain language
