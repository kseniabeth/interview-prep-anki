-- Run in the Supabase SQL editor after the migration. All test writes roll back.
begin;
do $$
declare
  owner_id uuid;
  affected_rows integer;
  denied boolean := false;
  payload jsonb := '{"version":1,"cards":{},"log":[]}'::jsonb;
begin
  select id into owner_id from auth.users order by created_at limit 1;
  if owner_id is null then raise exception 'A signed-in account is required for this test'; end if;
  insert into public.recall_progress(user_id,progress,revision) values(owner_id,payload,1)
    on conflict(user_id) do update set progress=excluded.progress,revision=1;
  perform set_config('request.jwt.claims',jsonb_build_object('sub',owner_id,'role','authenticated')::text,true);
  execute 'set local role authenticated';
  select count(*) into affected_rows from public.recall_progress where user_id=owner_id and progress=payload;
  if affected_rows <> 1 then raise exception 'Owner cannot reload progress'; end if;
  update public.recall_progress set revision=2 where user_id=owner_id and revision=1;
  get diagnostics affected_rows = row_count;
  if affected_rows <> 1 then raise exception 'Owner cannot save progress'; end if;
  update public.recall_progress set revision=3 where user_id=owner_id and revision=1;
  get diagnostics affected_rows = row_count;
  if affected_rows <> 0 then raise exception 'Stale save was not rejected'; end if;
  perform set_config('request.jwt.claims','{"sub":"00000000-0000-0000-0000-000000000001","role":"authenticated"}',true);
  select count(*) into affected_rows from public.recall_progress where user_id=owner_id;
  if affected_rows <> 0 then raise exception 'Another account can read owner progress'; end if;
  update public.recall_progress set revision=99 where user_id=owner_id;
  get diagnostics affected_rows = row_count;
  if affected_rows <> 0 then raise exception 'Another account can update owner progress'; end if;
  begin
    insert into public.recall_progress(user_id,progress,revision) values(owner_id,payload,1);
  exception when insufficient_privilege then denied := true;
  end;
  if not denied then raise exception 'Another account can insert for owner'; end if;
  execute 'reset role';
end;
$$;
select true as owner_save_reload_verified, true as other_account_denied, true as stale_save_rejected;
rollback;
