# Recall — Interview Prep

A daily interview-practice workspace with C# algorithms, system design, CS fundamentals, beginner-friendly SQL, linked study guides, and private progress. The original 121 coding and 36 design card IDs are retained.

**Study:** https://kseniabeth.github.io/interview-prep-anki/

## Today: a small daily plan

The app opens on **Today**, with at most three actions: coding, system design, and behavioral practice. A new user starts with no completed work. Existing reviews and practice are never reset.

- The first session includes two original C# diagnostic prompts, a small URL-shortener design exercise, and a generic cross-team story prompt. Suggested time: 75 + 30 + 15 minutes.
- Open a prompt, make an attempt, then log actual minutes, partial/completed work, help used, and an optional short gap/next step. Opening an exercise does not count as completion. Incomplete attempts are useful.
- Later sessions rotate a small practice plan, advancing each track only after practice explicitly marked completed on a previous day. Partial attempts resume the same step; older logs without an outcome prompt a review instead of silently advancing. The initial diagnostic is never repeated by the recurring cycle. Missed days create no catch-up queue. Suggestions are timeboxes, not requirements.
- Each track has one log per local calendar day. Use Update log to record the total minutes and change a partial attempt to completed; repeated saves cannot create duplicate credit. A form opened before midnight stays attached to that original day. Undo removes a session without changing card reviews. Recent sessions and per-track totals appear in Progress.
- Card review fits inside the practice blocks. Keep it to about 10–15 minutes; recall counts and practice logs are not assessments of interview readiness.

Today is a generic helper, not an adaptive assessment or automatic feed from chat. Personalized daily-brief assignments do not sync into this plan; follow the more specific assignment in your brief when provided.

Keep confidential work details out of notes. Session notes are stored with your private progress, never in this public repository. Guest storage is local to this browser; account storage uses the existing Supabase sync and conflict handling. No notification service, analytics, or new account is introduced.

## Study

Open the GitHub Pages site. No installation is needed. The original single-file offline edition can still be used separately, with backups to transfer its progress.

- Reveal answers and rate your recall with Again, Hard, Good, or Easy.
- Study scheduled cards or use free practice without changing review dates.
- Choose **Coding**, **System Design**, **CS Fundamentals**, or **SQL**. Coding keeps its original collections; the new sections have their own stable card IDs and review schedules.
- Every answer has **Read study guide**, including cards found through Browse. A guide gives the contract or core idea, worked example, pitfalls, and linked sources. Implementation cards land on the exact C# code. Guide reading does not rate a card or complete practice.
- Use **Study guides** to search explanations and examples within a section. SQL starts with rows, columns, and a first SELECT before joins, grouping, and performance.
- Browser Back returns to the previous card and its revealed state. Direct guide URLs also work; shared links use `#guide/<guide-id>/<section-id>`. Section libraries use `#guides/cs` and `#guides/sql`.
- Search the card library and undo the last rating.
- Export and restore progress backups from the Progress view.

Keyboard shortcuts: **Space** reveals an answer, **1–4** rate it, and **Ctrl/Cmd + Z** undoes the last rating.

## Progress and scheduling

Click **Sign in**, enter your email, and open the magic link in your email. Signed-in reviews, due dates, and practice sessions save automatically to Supabase and load on another device when you sign in with the same email. The app uses the same Supabase email-login service as the character project, with separate account-owned study storage.

Guest progress remains on the current device. Signing in does not silently copy it into an account. To transfer it, open **Progress → Save guest progress to account**, or restore an exported backup while signed in. Sign-out returns to guest progress and clears the account data from the displayed app.

Account progress is protected by database row-level security: each user can read and write only their own row. Browser storage keeps an account-specific recovery copy for failed or offline saves. A visible status reports pending, saved, or failed sync. Writes check the loaded revision; if another device saved first, the app keeps your unsynced copy and offers a backup and the option to load the newer account version.

This app uses a simple spaced repetition scheduler, not Anki's FSRS scheduler, and does not sync with Anki. For a new card, Again schedules it in 1 minute, Hard in 10 minutes, Good in 1 day, and Easy in 4 days. Subsequent ratings adjust the interval according to the displayed button labels.

## GitHub Pages

In the repository's **Settings → Pages**, select **Deploy from a branch**, then **main** and **/ (root)**. The site uses `index.html` directly; `.nojekyll` disables Jekyll processing.

The app is still a static, offline-friendly page. Styles and generated payloads are in `index.html`; `app.js` handles the interface, guide routes, and card scheduler. `study-store.js` handles the optional practice-session extension. The editable generic first-session exercises are in `first-session.json`, and the sourced design deck is in `system-design.json`. Run `node scripts/update-design-deck.cjs` after editing either JSON file. New guide/deck sources live under `content/`; run `node scripts/build-study-content.cjs` to validate every card-to-guide link and rebuild the embedded payload. The legacy coding source is retained separately; audited corrections are applied by ID from `content/coding-corrections.json`, never by reordering the deck. `cloud.js` handles login and cloud progress; `sync-store.js` handles recovery and save conflicts. The pinned Supabase browser SDK is vendored locally. Authentication and progress requests go to Supabase; the app uses no analytics.

## Supabase setup

1. Apply `supabase/001_recall_progress.sql` in the character project's Supabase SQL editor.
2. Add `https://kseniabeth.github.io/interview-prep-anki/` to **Authentication → URL Configuration → Redirect URLs**. Preserve the character app's Site URL and existing redirects.
3. Keep the email provider and email magic-link templates enabled. Existing custom SMTP settings are reused.
4. `config.js` contains only the public project URL and publishable/anon key. Never put a service-role key, database password, or Supabase management token in the browser code or repository.

No character tables, profiles, or saved characters are changed by this app's migration.

The current project uses Supabase's built-in email delivery. It sends links only to project-team email addresses and has a low sending limit. To support other users, configure an SMTP provider in Supabase's Authentication → Emails → SMTP Settings. See [Supabase SMTP documentation](https://supabase.com/docs/guides/auth/auth-smtp).

## Verification

Run `node --test tests/*.test.cjs` for the app/DOM-model tests and account-sync tests. Coverage includes old-backup migration, stable card IDs, zero-start state, duplicate log/rating prevention, session undo, section filtering, free practice, export/restore (including cancellation and invalid files), account transitions, offline recovery, and revision conflicts. The GitHub verification workflow also compiles and runs the exact C# files embedded in the guides, executes SQL fixtures, and runs isolated Chromium smoke tests at desktop and mobile widths. Browser tests use a mock authentication provider, so they never send sign-in emails or change a real account. These checks do not prove live email delivery or a real multi-device signed-in round trip. `supabase/verify_rls.sql` checks real database owner access, denial to a different account, and rejection of stale writes; its test changes are rolled back. Real email delivery also depends on the Supabase project's SMTP configuration and limits.



## Backup compatibility and verification limits

The existing version-1 `cards` and `log` fields and all 121 legacy `card-0` through `card-120` identities are preserved. `sessions` is an optional extension; old backups load with an empty session list. Design cards use stable `sd-...` IDs. New backups include both card sections and all practice sessions. The existing JSON database column accepts this optional field; no database migration or permission change is required.

Use the current app to restore new backups. Older app versions do not know the new design IDs and may reject these backups or discard the extra session data on import. Export a backup before replacing any current progress. Do not alternate current and obsolete app versions for account editing.

Live magic-link delivery and multi-device account behavior still depend on the existing Supabase configuration. App logic is tested with simulated DOM and remote-store adapters. Browser smoke checks use an isolated mock account at 1440, 393, and 320 CSS-pixel widths. A live signed-in round trip remains a separate manual check; do not describe mock-provider checks as proof of email delivery or production account access.


## Coverage and reference examples

The expandable coverage map in Study guides distinguishes recall content from practical interview skill. Detailed topic/card matrices are in `content/coding-coverage.json` and `content/backend-coverage.json`; `content/coding-corrections.json` records each legacy clarification or correctness repair. It covers a bounded set of coding patterns, backend system design, CS fundamentals, and beginner-to-intermediate SQL. It does not claim to cover every employer's interview. Behavioral preparation remains practice-based through Today: build real evidence, explain decisions aloud, and practice follow-up questions rather than memorize invented stories.

C# reference files and a deterministic assertion harness are in `examples/csharp/` (target: .NET 8 / C# 12). Guide sections with a `codeFile` embed the exact file bytes; a test rejects drift between teaching material and the compiled code. SQL examples and fixtures are in `examples/sql/`; their README states dialect and portability limits.

Local verification:

```sh
node scripts/update-design-deck.cjs
node scripts/build-study-content.cjs
node --test tests/*.test.cjs
dotnet run --project examples/csharp/ReferenceExamples.csproj --configuration Release
python examples/cs-fundamentals/run_examples.py
python -m unittest discover -s examples/sql -p 'test*.py'
python -m pip install playwright==1.62.0
python -m playwright install chromium
python tests/browser_smoke.py
```

Browser screenshots use a temporary directory unless `RECALL_QA_OUTPUT` is set. All test accounts and fixtures are synthetic. No private job criteria, contacts, resumes, outreach, account tokens, or populated career trackers belong in this public repository.
