# Recall — Interview Prep

A self-contained HTML flashcard app with 121 C# and algorithm interview preparation cards.

## Study

Open the GitHub Pages site or download `index.html` and open it in a browser. No installation, server, or dependencies are needed.

- Reveal answers and rate your recall with Again, Hard, Good, or Easy.
- Study scheduled cards or use free practice without changing review dates.
- Filter by complexity, pattern triggers, C# templates, C# APIs, or common gotchas.
- Search the card library and undo the last rating.
- Export and restore progress backups from the Progress view.

Keyboard shortcuts: **Space** reveals an answer, **1–4** rate it, and **Ctrl/Cmd + Z** undoes the last rating.

## Progress and scheduling

Progress saves in browser local storage on the current device. It does not sync between browsers, devices, the offline file, and the hosted site. Use Export progress and Restore progress to transfer it.

This app uses a simple spaced repetition scheduler, not Anki's FSRS scheduler, and does not sync with Anki. For a new card, Again schedules it in 1 minute, Hard in 10 minutes, Good in 1 day, and Easy in 4 days. Subsequent ratings adjust the interval according to the displayed button labels.

## GitHub Pages

In the repository's **Settings → Pages**, select **Deploy from a branch**, then **main** and **/ (root)**. The site uses `index.html` directly; `.nojekyll` disables Jekyll processing.

The card data, styling, and application logic are embedded in `index.html`. The app makes no external requests and uses no analytics.
