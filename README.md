# Recall — Interview Prep

A flashcard app with 121 C# and algorithm interview preparation cards, email magic-link login, and private account progress.

**Study:** https://kseniabeth.github.io/interview-prep-anki/

## Study

Open the GitHub Pages site. No installation is needed. The original single-file offline edition can still be used separately, with backups to transfer its progress.

- Reveal answers and rate your recall with Again, Hard, Good, or Easy.
- Study scheduled cards or use free practice without changing review dates.
- Filter by complexity, pattern triggers, C# templates, C# APIs, or common gotchas.
- Search the card library and undo the last rating.
- Export and restore progress backups from the Progress view.

Keyboard shortcuts: **Space** reveals an answer, **1–4** rate it, and **Ctrl/Cmd + Z** undoes the last rating.

## Progress and scheduling

Click **Sign in**, enter your email, and open the magic link in your email. Signed-in reviews and due dates save automatically to Supabase and load on another device when you sign in with the same email. The app uses the same Supabase email-login service as the character project, with separate account-owned study storage.

Guest progress remains on the current device. Signing in does not silently copy it into an account. To transfer it, open **Progress → Save guest progress to account**, or restore an exported backup while signed in. Sign-out returns to guest progress and clears the account data from the displayed app.

Account progress is protected by database row-level security: each user can read and write only their own row. Browser storage keeps an account-specific recovery copy for failed or offline saves. A visible status reports pending, saved, or failed sync. Writes check the loaded revision; if another device saved first, the app keeps your unsynced copy and offers a backup and the option to load the newer account version.

This app uses a simple spaced repetition scheduler, not Anki's FSRS scheduler, and does not sync with Anki. For a new card, Again schedules it in 1 minute, Hard in 10 minutes, Good in 1 day, and Easy in 4 days. Subsequent ratings adjust the interval according to the displayed button labels.

## GitHub Pages

In the repository's **Settings → Pages**, select **Deploy from a branch**, then **main** and **/ (root)**. The site uses `index.html` directly; `.nojekyll` disables Jekyll processing.

The card data, styling, and review logic are embedded in `index.html`. `cloud.js` handles login and cloud progress; `sync-store.js` handles recovery and save conflicts. The pinned Supabase browser SDK is vendored locally. Authentication and progress requests go to Supabase; the app uses no analytics.

## Supabase setup

1. Apply `supabase/001_recall_progress.sql` in the character project's Supabase SQL editor.
2. Add `https://kseniabeth.github.io/interview-prep-anki/` to **Authentication → URL Configuration → Redirect URLs**. Preserve the character app's Site URL and existing redirects.
3. Keep the email provider and email magic-link templates enabled. Existing custom SMTP settings are reused.
4. `config.js` contains only the public project URL and publishable/anon key. Never put a service-role key, database password, or Supabase management token in the browser code or repository.

No character tables, profiles, or saved characters are changed by this app's migration.

The current project uses Supabase's built-in email delivery. It sends links only to project-team email addresses and has a low sending limit. To support other users, configure an SMTP provider in Supabase's Authentication → Emails → SMTP Settings. See [Supabase SMTP documentation](https://supabase.com/docs/guides/auth/auth-smtp).

## Verification

Run `node tests/sync.test.cjs` for account isolation, concurrent edits, revision conflicts, offline recovery, retries, and stale-session races. `supabase/verify_rls.sql` checks real database owner access, denial to a different account, and rejection of stale writes; its test changes are rolled back. Real email delivery also depends on the Supabase project's SMTP configuration and limits.
