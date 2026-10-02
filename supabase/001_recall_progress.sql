-- Isolated from the character app's tables. Apply in the same Supabase project.
begin;
create table if not exists public.recall_progress (
  user_id uuid primary key references auth.users(id) on delete cascade,
  progress jsonb not null,
  revision bigint not null default 1 check (revision > 0),
  updated_at timestamptz not null default now(),
  constraint recall_progress_shape check (
    jsonb_typeof(progress) = 'object' and
    progress ?& array['version', 'cards', 'log'] and
    progress ->> 'version' = '1' and
    jsonb_typeof(progress -> 'cards') = 'object' and
    jsonb_typeof(progress -> 'log') = 'array' and
    octet_length(progress::text) <= 10000000
  )
);
alter table public.recall_progress enable row level security;
revoke all on public.recall_progress from anon;
grant select, insert, update on public.recall_progress to authenticated;
drop policy if exists recall_read_own on public.recall_progress;
create policy recall_read_own on public.recall_progress
  for select to authenticated using ((select auth.uid()) = user_id);
drop policy if exists recall_insert_own on public.recall_progress;
create policy recall_insert_own on public.recall_progress
  for insert to authenticated with check ((select auth.uid()) = user_id);
drop policy if exists recall_update_own on public.recall_progress;
create policy recall_update_own on public.recall_progress
  for update to authenticated using ((select auth.uid()) = user_id)
  with check ((select auth.uid()) = user_id);
commit;
