(function(){
  'use strict';
  const app=window.recallApp,config=window.RECALL_CONFIG;
  const $=id=>document.getElementById(id);
  let user=null,client=null,authReady=false,timer=null,cooldown=0,cooldownTimer=null,transitionId=0;
  const messages={guest:'Guest · saved in this browser',loading:'Loading your saved progress…',pending:'Changes waiting to sync',saving:'Saving to your account…',saved:'Saved to your account',error:'Not synced · your changes stay on this device',conflict:'Another device has newer progress', 'cache-error':'Device backup unavailable · export a backup'};
  let sync;
  function refresh(status){
    $('syncStatus').textContent=messages[status]||messages.guest;
    $('accountButton').textContent=user?'Account':'Sign in';
    $('accountEmail').textContent=user?.email||'Sign in to save progress across devices.';
    $('loginForm').classList.toggle('hidden',Boolean(user));
    $('accountSignedIn').classList.toggle('hidden',!user);
    $('accountSaveDetails').textContent=user?'Your reviews and due dates are saved privately to your account.':'Guest progress stays in this browser. Sign in to save across devices.';
    $('retrySync').classList.toggle('hidden',!user||!['error','pending','cache-error'].includes(status));
    $('conflictActions').classList.toggle('hidden',status!=='conflict');
    const guest=app.guest();
    $('saveGuest').classList.toggle('hidden',!user||!Object.keys(guest.cards).length);
    $('saveGuest').disabled=!sync?.canEdit();
    $('signOut').disabled=status==='loading';
    $('accountState').textContent=user?(messages[status]||''):'';
  }
  function authMessage(message){$('authMessage').textContent=message;}
  function schedule(){clearTimeout(timer);timer=setTimeout(()=>sync.flush(),450);}
  const remote={
    async load(id){const {data,error}=await client.from('recall_progress').select('progress,revision').eq('user_id',id).maybeSingle();if(error)throw error;return data;},
    async save(id,progress,revision){
      const row={user_id:id,progress,revision:revision+1,updated_at:new Date().toISOString()};
      const query=revision===0?client.from('recall_progress').insert(row):client.from('recall_progress').update(row).eq('user_id',id).eq('revision',revision);
      const {data,error}=await query.select('revision').maybeSingle();
      if(error){if(error.code==='23505')throw Object.assign(Error('Save conflict'),{conflict:true});throw error;}
      if(!data)throw Object.assign(Error('Save conflict'),{conflict:true});
      return data.revision;
    }
  };
  sync=new window.RecallSync({remote,storage:localStorage,valid:app.valid,onState:app.replace,onStatus:refresh});
  window.recallCloud={
    canEdit(){return authReady&&(!user||sync.canEdit());},
    save(progress){if(!user)return false;sync.change(progress);schedule();return true;},
    isSignedIn(){return Boolean(user);}
  };
  async function transition(next){
    authReady=true;
    if(next?.id===user?.id&&Boolean(next)===Boolean(user)){refresh(sync.status);return;}
    const epoch=++transitionId;clearTimeout(timer);user=next;sync.close();
    // Clear the prior account's UI immediately; never carry its progress into another account.
    app.replace(next?app.empty():app.guest());
    if(next)await sync.open(next.id);
    if(epoch!==transitionId)return;refresh(sync.status);
  }
  $('accountButton').onclick=()=>{$('accountDialog').showModal();if(!user)$('loginEmail').focus();};
  $('closeAccount').onclick=()=>$('accountDialog').close();
  $('loginForm').onsubmit=async event=>{
    event.preventDefault();if(!client){authMessage('Sign-in is unavailable. Reload the page and try again.');return;}
    if(cooldown>Date.now())return;
    const email=$('loginEmail').value.trim();if(!$('loginEmail').checkValidity()){$('loginEmail').reportValidity();return;}
    const button=$('sendMagicLink');button.disabled=true;authMessage('Sending your sign-in link…');
    try{const {error}=await client.auth.signInWithOtp({email,options:{emailRedirectTo:config.redirectUrl,shouldCreateUser:true}});if(error)throw error;
      authMessage('Check your email for a sign-in link. Open it to return here and save your progress.');
      cooldown=Date.now()+60000;clearInterval(cooldownTimer);cooldownTimer=setInterval(()=>{const seconds=Math.ceil((cooldown-Date.now())/1000);if(seconds<=0){clearInterval(cooldownTimer);button.disabled=false;button.textContent='Send magic link';}else button.textContent='Resend in '+seconds+'s';},1000);
    }catch(error){button.disabled=false;authMessage(error.code==='email_address_not_authorized'?'This email service currently accepts only project-team addresses. The app owner needs to configure email delivery for other users.':error.status===429?'Too many requests. Wait a minute, then try again.':'Could not send the link. Please try again in a minute.');}
  };
  $('signOut').onclick=async()=>{
    if(!client)return;
    $('signOut').disabled=true;
    try{await sync.flush();if(sync.context?.dirty&&!confirm('Some changes haven’t synced. A recovery copy remains on this device. Sign out anyway?'))return;
      const {error}=await client.auth.signOut({scope:'local'});if(error)throw error;await transition(null);authMessage('Signed out. Your guest progress is shown.');
    }catch{authMessage('Could not sign out. Please try again.');}finally{$('signOut').disabled=false;}
  };
  $('retrySync').onclick=()=>sync.retry();
  $('useCloudProgress').onclick=async()=>{if(!confirm('Load the newer account progress? Export a backup first if you want to keep this device’s unsynced changes.'))return;await sync.useCloud();};
  $('saveGuest').onclick=()=>{
    if(!user||!sync.canEdit())return;
    if(!confirm('Replace your account’s current progress with this browser’s guest progress? Export a backup first if you want to keep the account version.'))return;
    const guest=app.guest();app.replace(guest);sync.change(guest);schedule();
  };
  $('conflictBackup').onclick=()=>$('export').click();
  window.addEventListener('online',()=>{if(user)sync.retry();});
  document.addEventListener('visibilitychange',()=>{if(document.hidden&&user)sync.flush();});
  window.addEventListener('beforeunload',e=>{if(user&&sync.context?.dirty){e.preventDefault();e.returnValue='';}});
  refresh('guest');
  if(!config?.url||!config?.anonKey||!window.supabase){authReady=true;authMessage('Sign-in is unavailable. You can continue as a guest.');return;}
  try{
    const errors=new URLSearchParams(location.hash.slice(1));
    if(errors.has('error')||errors.has('error_code')){history.replaceState(null,'',location.pathname);authMessage('This sign-in link has expired or is invalid. Request a new one.');$('accountDialog').showModal();}
    client=window.supabase.createClient(config.url,config.anonKey,{auth:{storageKey:'recall-auth-v1',flowType:'implicit',detectSessionInUrl:true,persistSession:true,autoRefreshToken:true}});
    client.auth.onAuthStateChange((event,session)=>{if(['INITIAL_SESSION','SIGNED_IN','SIGNED_OUT','TOKEN_REFRESHED'].includes(event))setTimeout(()=>{transition(session?.user||null).catch(()=>authMessage('Could not load your account. Please retry.'));},0);});
    client.auth.getSession().then(({data,error})=>{if(error)throw error;return transition(data.session?.user||null);}).catch(()=>{authReady=true;authMessage('Could not restore sign-in. Try signing in again.');});
  }catch{authReady=true;authMessage('Sign-in is unavailable. You can continue as a guest.');}
})();
