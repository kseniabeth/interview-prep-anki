'use strict';
const codingDeck=JSON.parse(document.getElementById('deckData').textContent).map((c,i)=>({...c,id:'card-'+i,section:'coding',group:c.tags.split(/\s+/)[0]}));
const designDeck=JSON.parse(document.getElementById('designDeckData').textContent).map(c=>({...c,section:'design',group:c.tags.split(/\s+/)[0]}));
const studyContent=JSON.parse(document.getElementById('studyContentData').textContent);
const extraDeck=studyContent.cards.map(c=>({...c,group:c.tags.split(/\s+/)[0]}));
const deck=[...codingDeck,...designDeck,...extraDeck].map(c=>({...c,...(studyContent.cardGuides[c.id]||{}),guideAnchor:c.guideAnchor||studyContent.cardGuides[c.id]?.anchor||c.anchor})),study=window.RecallStudy;
const guides=studyContent.guides,guideById=new Map(guides.map(g=>[g.id,g]));
const firstSession=JSON.parse(document.getElementById('firstSessionData').textContent);
const KEY='recall-interview-prep-v1',DAY=86400000,MINUTE=60000;
const sectionNames={coding:'Coding',design:'System Design',cs:'CS Fundamentals',sql:'SQL'};
const names={complexity:'Complexity',trigger:'Pattern triggers',template:'C# templates',api:'C# APIs',gotcha:'Common gotchas','sd-concept':'Concepts','sd-tool':'Tools','sd-scenario':'Decisions & failures','cs-runtime':'Runtime & memory','cs-concurrency':'Concurrency','cs-networking':'Networks & HTTP','cs-engineering':'Engineering practice','cs-security':'Security','sql-basics':'Start here','sql-queries':'Queries & joins','sql-design':'Data modeling','sql-performance':'Performance',...studyContent.groups};
for(const c of deck)if(!names[c.group])names[c.group]=c.group.replace(/-/g,' ');
const $=id=>document.getElementById(id);let view='today',section='coding',topic='all',mode='scheduled',current=null,revealed=false,practiceQueue=[],undoStack=[];
function fresh(){return study.fresh();}
let state=fresh();
function valid(s){return s&&s.version===1&&s.cards&&typeof s.cards==='object'&&!Array.isArray(s.cards)&&Array.isArray(s.log)&&Object.entries(s.cards).every(([id,c])=>deck.some(d=>d.id===id)&&c&&Number.isFinite(c.due)&&c.due>=0&&Number.isFinite(c.interval)&&c.interval>=0&&Number.isInteger(c.reps)&&c.reps>=0&&Number.isInteger(c.lapses)&&c.lapses>=0)&&study.validSessions(s.sessions)&&s.log.every(l=>l&&deck.some(d=>d.id===l.id)&&Number.isFinite(l.at)&&l.at>=0&&['again','hard','good','easy'].includes(l.rating));}
function notice(s){$('notice').textContent=s;$('notice').classList.remove('hidden');}
try{const raw=localStorage.getItem(KEY);if(raw){const saved=JSON.parse(raw);if(!valid(saved))throw Error('Invalid data');state=study.normalize(saved);}}catch(e){notice('Saved progress could not be loaded. You can study now or restore a progress backup.');}
function persist(){try{if(window.recallCloud?.save(state))return;localStorage.setItem(KEY,JSON.stringify(state));}catch(e){notice('Your progress could not be saved. Export a backup from Progress before closing this page.');}}
function canChange(){if(window.recallCloud&&!window.recallCloud.canEdit()){toast('Account progress is loading or needs attention. Check Progress before reviewing.');return false;}return true;}
const dayKey=study.dayKey;
function text(s){const el=document.createElement('textarea');el.innerHTML=s.replace(/</g,'&lt;').replace(/>/g,'&gt;');return el.value;}
function filtered(){return deck.filter(c=>c.section===section&&(topic==='all'||c.group===topic));}
function eligible(){const now=Date.now();return filtered().filter(c=>!state.cards[c.id]||state.cards[c.id].due<=now).sort((a,b)=>{const sa=state.cards[a.id],sb=state.cards[b.id];return (sa?0:1)-(sb?0:1)||(sa?.due||0)-(sb?.due||0);});}
function streak(){const dates=new Set([...state.log.map(l=>dayKey(l.at)),...state.sessions.map(s=>s.date)]);let d=new Date(),n=0;if(!dates.has(dayKey(d.getTime())))d.setDate(d.getDate()-1);while(dates.has(dayKey(d.getTime()))){n++;d.setDate(d.getDate()-1);}return n;}
function stats(){const now=Date.now(),cards=filtered();$('statNew').textContent=cards.filter(c=>!state.cards[c.id]).length;$('statDue').textContent=cards.filter(c=>state.cards[c.id]?.due<=now).length;$('statToday').textContent=state.log.filter(l=>dayKey(l.at)===dayKey(now)&&cards.some(c=>c.id===l.id)).length;const s=streak();$('statStreak').textContent=s+' '+(s===1?'day':'days');}
function buildTopics(){const items=[['all','All '+sectionNames[section].toLowerCase()+' cards'],...Object.entries(names).filter(([id])=>deck.some(c=>c.section===section&&c.group===id))];$('topics').replaceChildren();$('mobileTopic').replaceChildren();for(const [id,name]of items){const count=deck.filter(c=>c.section===section&&(id==='all'||c.group===id)).length;const b=document.createElement('button');b.dataset.topic=id;b.classList.toggle('active',id===topic);const span=document.createElement('span');span.textContent=name;const n=document.createElement('span');n.textContent=count;b.append(span,n);b.onclick=()=>setTopic(id);$('topics').append(b);const o=document.createElement('option');o.value=id;o.textContent=name+' ('+count+')';$('mobileTopic').append(o);}$('mobileTopic').value=topic;}
function setTopic(id){if(view==='guides'){leaveGuideRoute();view='review';}if(view==='today')view='review';topic=id;current=null;revealed=false;practiceQueue=[];buildTopics();render();}
function setSection(id){if(!sectionNames[id])return;const wasGuides=view==='guides';if(wasGuides){selectedGuide=null;replaceHash('#guides/'+id);section=id;topic='all';buildTopics();render();return;}section=id;topic='all';current=null;revealed=false;practiceQueue=[];if(view==='today')view='review';buildTopics();render();}
function tags(c){const row=document.createElement('div');row.className='tags';for(const t of c.tags.split(/\s+/)){const e=document.createElement('span');e.className='tag';e.textContent=t;row.append(e);}return row;}
function guideUrl(c){return '#guide/'+encodeURIComponent(c.guideId)+(c.guideAnchor?'/'+encodeURIComponent(c.guideAnchor):'');}
function answerNode(c){const el=document.createElement(c.group==='template'||c.back.includes('\n')?'pre':'div');el.className='answer-text'+(el.tagName==='PRE'?' code':'');el.textContent=text(c.back);const wrapper=document.createElement('div');wrapper.append(el);if(c.source){const link=document.createElement('a');link.className='source-link';link.textContent=c.source.title;link.href=c.source.url;link.target='_blank';link.rel='noopener noreferrer';wrapper.append(link);}const guide=document.createElement('a');guide.className='guide-link';guide.textContent='Read study guide →';guide.href=guideUrl(c);guide.setAttribute('aria-label','Read study guide: '+(guideById.get(c.guideId)?.title||text(c.front)));guide.onclick=e=>{if(e.ctrlKey||e.metaKey||e.shiftKey||e.altKey)return;e.preventDefault();openGuide(c.guideId,c.guideAnchor);};wrapper.append(guide);return wrapper;}
function shuffled(a){a=[...a];for(let i=a.length-1;i>0;i--){const j=Math.floor(Math.random()*(i+1));[a[i],a[j]]=[a[j],a[i]];}return a;}
function choose(){if(mode==='practice'){if(!practiceQueue.length)practiceQueue=shuffled(filtered());current=practiceQueue.shift()||null;}else current=eligible()[0]||null;revealed=false;}
function intervalFor(r,c=current){const old=state.cards[c.id]?.interval||0;if(r==='again')return MINUTE;if(r==='hard')return old>=DAY?Math.max(DAY,Math.round(old*1.2)):10*MINUTE;if(r==='good')return old>=DAY?Math.round(old*2.5):DAY;return old>=DAY?Math.round(old*3.2):4*DAY;}
function duration(ms){if(ms<DAY)return Math.round(ms/MINUTE)+' min';return Math.round(ms/DAY)+' '+(Math.round(ms/DAY)===1?'day':'days');}
function renderCard(){if(!current)choose();$('practiceBanner').classList.toggle('hidden',mode!=='practice');$('scheduled').classList.toggle('selected',mode==='scheduled');$('practice').classList.toggle('selected',mode==='practice');$('undo').disabled=!undoStack.length;$('queueLabel').textContent=mode==='practice'?(practiceQueue.length+1)+' cards in this round':eligible().length+' cards ready';const root=$('card');root.replaceChildren();if(!current){root.classList.add('empty');const icon=document.createElement('div');icon.className='symbol';icon.textContent='✓';const h=document.createElement('h2');h.textContent='You’re caught up.';const p=document.createElement('p');p.className='small';const future=filtered().map(c=>state.cards[c.id]?.due).filter(t=>t>Date.now());p.textContent=future.length?'Next review: '+new Date(Math.min(...future)).toLocaleString()+'. Cards return here when they are due.':'No cards in this collection.';const b=document.createElement('button');b.className='primary';b.textContent='Practice this collection';b.onclick=()=>setMode('practice');root.append(icon,h,p,b);$('queueLabel').textContent='0 cards ready';return;}root.classList.remove('empty');const header=document.createElement('div');header.className='card-header';const status=document.createElement('span');status.className='card-state';const s=state.cards[current.id];status.textContent=(s?'Review':'New card')+' · '+(filtered().indexOf(current)+1)+' / '+filtered().length;header.append(tags(current),status);const q=document.createElement('div');q.className='question';q.textContent=text(current.front);root.append(header,q);if(revealed){const a=document.createElement('div');a.className='answer';const label=document.createElement('div');label.className='answer-label';label.textContent='Answer';a.append(label,answerNode(current));root.append(a);const buttons=document.createElement('div');buttons.className='rating-grid';['again','hard','good','easy'].forEach((r,i)=>{const b=document.createElement('button');b.textContent=r[0].toUpperCase()+r.slice(1);const small=document.createElement('span');small.textContent=mode==='practice'?'Key '+(i+1):duration(intervalFor(r));b.append(small);b.onclick=()=>rate(r);buttons.append(b);});root.append(buttons);}else{const b=document.createElement('button');b.className='primary reveal';b.textContent='Show answer';b.onclick=reveal;root.append(b);}}
function reveal(){if(current&&!revealed){revealed=true;renderCard();}}
function rate(r){if(!current||!revealed||(mode==='scheduled'&&!canChange()))return;const c=current;if(mode==='scheduled'){undoStack.push({id:c.id,previous:state.cards[c.id]?{...state.cards[c.id]}:null,logLength:state.log.length});const old=state.cards[c.id]||{reps:0,lapses:0};const interval=intervalFor(r);state.cards[c.id]={due:Date.now()+interval,interval,reps:old.reps+1,lapses:old.lapses+(r==='again'?1:0)};state.log.push({id:c.id,at:Date.now(),rating:r});persist();}else{if(r==='again')practiceQueue.push(c);if(!practiceQueue.length){current=null;revealed=false;renderPracticeComplete();return;}}current=null;revealed=false;render();}
function renderPracticeComplete(){stats();$('card').classList.add('empty');$('card').replaceChildren();const h=document.createElement('h2');h.textContent='Practice round complete.';const p=document.createElement('p');p.className='small';p.textContent='Your scheduled reviews haven’t changed.';const b=document.createElement('button');b.className='primary';b.textContent='Practice another round';b.onclick=()=>{choose();render();};$('card').append(h,p,b);$('queueLabel').textContent='Round complete';}
function undo(){if(!canChange())return;const action=undoStack.pop();if(!action)return;if(action.previous)state.cards[action.id]=action.previous;else delete state.cards[action.id];state.log.length=action.logLength;mode='scheduled';current=deck.find(c=>c.id===action.id);section=current.section;topic='all';buildTopics();revealed=true;persist();render();toast('Last rating undone.');}
function renderBrowse(){const query=$('search').value.trim().toLowerCase();const matches=filtered().filter(c=>text(c.front+' '+c.back+' '+c.tags).toLowerCase().includes(query));$('resultsLabel').textContent=matches.length+' '+(matches.length===1?'card':'cards');$('list').replaceChildren();if(!matches.length){const p=document.createElement('p');p.className='small';p.textContent='No matching cards. Try another search or collection.';$('list').append(p);}for(const c of matches){const d=document.createElement('details');const s=document.createElement('summary');s.textContent=text(c.front);d.append(s,tags(c),answerNode(c));$('list').append(d);}}
function renderProgress(){$('progressRows').replaceChildren();for(const [id,name]of Object.entries(names).filter(([id])=>deck.some(c=>c.section===section&&c.group===id))){const cards=deck.filter(c=>c.section===section&&c.group===id),started=cards.filter(c=>state.cards[c.id]).length;const row=document.createElement('div');row.className='progress-row';const label=document.createElement('div');label.className='row-label';const a=document.createElement('span');a.textContent=name;const b=document.createElement('span');b.textContent=started+' / '+cards.length;label.append(a,b);const bar=document.createElement('div');bar.className='bar';const fill=document.createElement('div');fill.style.width=started/cards.length*100+'%';bar.append(fill);row.append(label,bar);$('progressRows').append(row);}$('backupInfo').textContent=state.log.length+' total reviews · '+Object.keys(state.cards).length+' cards started · '+state.sessions.length+' practice sessions';renderSessionHistory();}
function render(){stats();for(const v of ['today','review','browse','progress','guides'])$(v+'View').classList.toggle('hidden',view!==v);document.querySelector('.stats').hidden=view==='today'||view==='guides';$('mobileTopic').classList.toggle('hidden',view==='today'||view==='guides');document.querySelectorAll('[data-view]').forEach(b=>{b.classList.toggle('active',b.dataset.view===view);b.setAttribute('aria-current',b.dataset.view===view?'page':'false');});document.querySelectorAll('[data-section]').forEach(b=>{b.classList.toggle('active',b.dataset.section===section);b.setAttribute('aria-pressed',String(b.dataset.section===section));});$('sectionLabel').textContent='Interview Prep / '+(view==='today'?'Today':sectionNames[section]);$('deckCount').textContent=deck.length+' cards · '+guides.length+' guides';$('pageTitle').textContent={today:'Your next practice session.',review:'A little practice, every day.',browse:'Your card library.',progress:'Keep the momentum.',guides:selectedGuide?selectedGuide.title:'Study one idea at a time.'}[view];$('subtitle').textContent={today:'A short plan, with your progress in one place.',review:'Recall the approach before you reveal the answer.',browse:'Find a concept. Open a card to see the answer.',progress:'Practice sessions and card recall are different kinds of progress.',guides:selectedGuide?selectedGuide.summary:'Short explanations, worked examples, and reference code. Start with the gap from your last attempt.'}[view];if(view==='today')renderToday();if(view==='review')renderCard();if(view==='browse')renderBrowse();if(view==='progress')renderProgress();if(view==='guides')renderGuides();}
function setMode(m){mode=m;current=null;revealed=false;practiceQueue=[];render();}
let toastTimer;function toast(msg){$('toast').textContent=msg;$('toast').classList.remove('hidden');clearTimeout(toastTimer);toastTimer=setTimeout(()=>$('toast').classList.add('hidden'),3000);}
const trackLabels={coding:'Coding',design:'System design',behavioral:'Behavioral'};
let activeTask=null,lastTodayDate=null;
function node(tag,content,className){const el=document.createElement(tag);if(content!==undefined)el.textContent=content;if(className)el.className=className;return el;}

function dailyTasks(){
  return study.today(state).map(task=>{
    if(task.completedCount===0){
      const exercise=firstSession.actions.find(a=>(a.track==='system-design'?'design':a.track)===task.track);
      task={...task,title:exercise.title,minutes:exercise.minutes,instructions:{coding:'Try two original C# prompts before looking at hints. Keep incomplete work and record what slowed you down.',design:'Sketch a small URL shortener, then explain what happens during a data-store outage.',behavioral:'Choose one real cross-team example. Give a short answer aloud, then practice follow-up questions.'}[task.track],exercise};
    }
    if(task.continuation==='partial')return {...task,title:'Continue: '+task.title,instructions:'Resume your last partial attempt before starting anything new. '+task.instructions};
    if(task.continuation==='review')return {...task,title:'Review: '+task.title,instructions:'Your earlier log did not record completion. Review where you stopped and continue as needed. '+task.instructions};
    return task;
  });
}
function openExercise(task){
  const exercise=task.exercise;$('exerciseTitle').textContent=exercise.title;$('exerciseContent').replaceChildren();
  $('exerciseContent').append(node('p',exercise.instructions||exercise.prompt,'small'));
  const stepNodes=[];
  for(const step of exercise.steps){
    const block=node('section',undefined,'exercise-step');block.append(node('h3',step.title+' · '+step.minutes+' min'),node('p',step.prompt||'','small'));
    if(step.signature)block.append(node('pre',step.signature));
    if(step.constraints){const list=node('ul',undefined,'small');step.constraints.forEach(x=>list.append(node('li',x)));block.append(list);}
    if(step.examples)block.append(node('pre',step.examples.map(x=>JSON.stringify(x.input)+' → '+JSON.stringify(x.output)).join('\n')));
    if(step.completionEvidence)block.append(node('p',step.completionEvidence,'small'));
    if(step.revealAfter){block.hidden=true;const previous=stepNodes.find(x=>x.id===step.revealAfter);const next=node('button','Finished this attempt · show next prompt');next.onclick=()=>{block.hidden=false;next.disabled=true;block.scrollIntoView({block:'start'});};previous?.block.append(next);}
    stepNodes.push({id:step.id,block});$('exerciseContent').append(block);
  }
  const rubric=node('details',undefined,'rubric');rubric.append(node('summary','After your attempt: self-review criteria'),node('p','Use these questions after attempting the exercise. With no evidence, the result is unassessed. Opening this prompt does not log practice.','small'));
  const list=node('ul',undefined,'small');for(const item of firstSession.reviewRubrics[{coding:'coding',design:'systemDesign',behavioral:'behavioral'}[task.track]])list.append(node('li',item.dimension+': '+item.criterion));rubric.append(list);$('exerciseContent').append(rubric);
  const log=node('button',task.completed?'Update my log':'Log my attempt','primary');log.style.marginTop='20px';log.onclick=()=>{$('exerciseDialog').close();openSession(task);};$('exerciseContent').append(log);$('exerciseDialog').showModal();
}
$('closeExercise').onclick=()=>$('exerciseDialog').close();

function renderToday(){
  lastTodayDate=dayKey(Date.now());
  const tasks=dailyTasks(),done=tasks.filter(t=>t.completed).length;
  $('todayCount').textContent=done+' of 3 logged';$('todayTasks').replaceChildren();
  for(const task of tasks){
    const article=node('article',undefined,'today-task'+(task.completed?.outcome==='completed'?' done':''));article.dataset.track=task.track;
    const content=node('div');content.append(node('div',trackLabels[task.track]+' · '+task.minutes+' min suggested','task-meta'),node('h3',task.title),node('p',task.instructions,'small'));article.append(content);
    if(task.completed)article.append(node('span',task.completed.minutes+' min · '+(task.completed.outcome==='completed'?'completed':task.completed.outcome==='partial'?'partial':'logged'),'done-badge'));
    const buttons=node('div',undefined,'task-buttons'),log=node('button',task.completed?'Update log':'Log this practice',task.completed?'':'primary');log.onclick=()=>openSession(task);buttons.append(log);if(task.exercise){const exercise=node('button','Open practice prompt');exercise.onclick=()=>openExercise(task);buttons.prepend(exercise);}
    if(task.track==='coding'||task.track==='design'){
      const cards=node('button','Review '+(task.track==='coding'?'coding':'design')+' cards');cards.onclick=()=>{view='review';setSection(task.track);};buttons.append(cards);
    }
    if(task.track==='design'){
      const guide=node('a','Delivery framework ↗');guide.href='https://www.hellointerview.com/learn/system-design/in-a-hurry/delivery';guide.target='_blank';guide.rel='noopener noreferrer';buttons.append(guide);
    }
    if(task.completed){const undoButton=node('button','Undo log');undoButton.onclick=()=>undoSession(task.completed.id);buttons.append(undoButton);}
    article.append(buttons);$('todayTasks').append(article);
  }
}
function openSession(task){
  if(!canChange())return;
  const existing=state.sessions.find(s=>s.taskKey===task.taskKey)||null;
  activeTask={...task,completed:existing};$('sessionForm').reset();$('sessionTitle').textContent=(existing?'Update ':'Log ')+trackLabels[task.track].toLowerCase()+' practice';$('sessionTopic').value=existing?.topic||task.title;$('sessionMinutes').value=existing?.minutes||task.minutes;$('sessionOutcome').value=existing?.outcome||'partial';$('sessionAssistance').value=existing?.assistance||(task.track==='behavioral'?'not-applicable':'none');$('sessionNote').value=existing?.note||'';$('sessionError').textContent='';$('saveSession').textContent=existing?'Update practice session':'Save practice session';$('saveSession').disabled=false;$('sessionDialog').showModal();$('sessionTopic').focus();
}
function undoSession(id){
  if(!canChange()||!state.sessions.some(s=>s.id===id))return;
  state=study.undo(state,id);persist();render();toast('Practice log removed. Card reviews are unchanged.');
}
function renderSessionHistory(){
  const sessions=state.sessions;$('sessionSummary').textContent=sessions.length?`${sessions.length} practice sessions · ${sessions.reduce((sum,s)=>sum+s.minutes,0)} minutes logged. Attempts are valuable even when unfinished.`:'No practice logged yet. Start with one small attempt in Today.';
  $('trackProgress').replaceChildren();for(const track of study.tracks){const entries=sessions.filter(s=>s.track===track),el=node('div',undefined,'track-total');el.append(node('span',trackLabels[track],'small'),node('strong',entries.length+' sessions'),node('span',entries.reduce((sum,s)=>sum+s.minutes,0)+' minutes','small'));$('trackProgress').append(el);}
  $('sessionHistory').replaceChildren();if(!sessions.length)$('sessionHistory').append(node('p','Your recent practice will appear here.','small'));
  for(const s of [...sessions].sort((a,b)=>b.at-a.at).slice(0,15)){
    const row=node('div',undefined,'session-row'),content=node('div');content.append(node('strong',s.topic),node('p',s.date+' · '+trackLabels[s.track]+' · '+s.minutes+' min · '+(s.outcome==='completed'?'planned practice completed':s.outcome==='partial'?'partial attempt':'attempt logged')+' · assistance: '+s.assistance.replace('not-applicable','not applicable'),'small'));if(s.note)content.append(node('p',s.note,'small'));const remove=node('button','Undo log');remove.onclick=()=>undoSession(s.id);row.append(content,remove);$('sessionHistory').append(row);
  }
}
$('closeSession').onclick=()=>$('sessionDialog').close();
$('sessionDialog').addEventListener('close',()=>{if(!$('sessionDialog').open){activeTask=null;$('sessionForm').reset();}});
$('sessionForm').onsubmit=e=>{
  e.preventDefault();if(!activeTask||!canChange()||!$('sessionForm').reportValidity())return;
  const task=activeTask,topicValue=$('sessionTopic').value.trim();if(!topicValue){$('sessionError').textContent='Add a prompt or topic.';return;}
  const entry={id:task.completed?.id||globalThis.crypto?.randomUUID?.()||'session-'+Date.now()+'-'+Math.random().toString(36).slice(2),taskKey:task.taskKey,date:task.date,track:task.track,at:task.completed?.at||Date.now(),minutes:Number($('sessionMinutes').value),topic:topicValue,assistance:$('sessionAssistance').value,outcome:$('sessionOutcome').value,note:$('sessionNote').value.trim()};
  try{state=study.record(state,entry);$('saveSession').disabled=true;activeTask=null;persist();$('sessionDialog').close();render();toast(task.completed?'Practice log updated.':'Practice session logged.');}catch{$('sessionError').textContent='Check the session details and try again.';}
};

let selectedGuide=null,selectedGuideAnchor='',guideReturn=null,guideError='';
function currentHash(){return window.location?.hash||'';}
function replaceHash(hash){if(window.history?.replaceState)window.history.replaceState(null,'',hash||(window.location.pathname+window.location.search));}
function navigateHash(hash){if(window.history?.pushState){if(currentHash()!==hash)window.history.pushState(null,'',hash);}else if(window.location)window.location.hash=hash;readGuideRoute();render();}
function rememberGuideReturn(){if(view!=='guides'&&!guideReturn)guideReturn={view,section,topic,current,revealed,scrollY:window.scrollY||0};}
function restoreGuideReturn(){if(guideReturn){({view,section,topic,current,revealed}=guideReturn);const y=guideReturn.scrollY;guideReturn=null;buildTopics();if(window.requestAnimationFrame)window.requestAnimationFrame(()=>window.scrollTo?.(0,y));}else{view='today';}selectedGuide=null;selectedGuideAnchor='';guideError='';}
function leaveGuideRoute(){if(view==='guides'){current=null;revealed=false;practiceQueue=[];}if(currentHash().startsWith('#guide'))replaceHash('');selectedGuide=null;selectedGuideAnchor='';guideReturn=null;guideError='';}
function openGuide(id,anchor){rememberGuideReturn();navigateHash('#guide/'+encodeURIComponent(id)+(anchor?'/'+encodeURIComponent(anchor):''));}
function openGuideLibrary(){rememberGuideReturn();navigateHash('#guides/'+section);}
function readGuideRoute(initial=false){
  const hash=currentHash();
  if(hash==='#guides'||hash.startsWith('#guides/')){view='guides';selectedGuide=null;selectedGuideAnchor='';guideError='';const requested=hash.split('/')[1];if(requested){if(sectionNames[requested])section=requested;else guideError='That section was not found. Choose a section from the menu.';}topic='all';buildTopics();return;}
  if(hash.startsWith('#guide/')){
    if(!initial)rememberGuideReturn();view='guides';guideError='';
    try{const parts=hash.slice(7).split('/').map(decodeURIComponent);selectedGuide=guideById.get(parts[0])||null;selectedGuideAnchor=parts[1]||'';if(!selectedGuide)guideError='This guide could not be found. Choose a guide below.';else if(selectedGuideAnchor&&!selectedGuide.sections.some(s=>s.id===selectedGuideAnchor)){selectedGuideAnchor='';guideError='That section was not found. The full guide is shown below.';}}
    catch{selectedGuide=null;selectedGuideAnchor='';guideError='This guide address is invalid. Choose a guide below.';}
    if(selectedGuide){section=selectedGuide.section;topic='all';buildTopics();}
  }else if(view==='guides'){restoreGuideReturn();}
}
function returnFromGuide(){const targetSection=selectedGuide?.section||section;replaceHash('');if(guideReturn)restoreGuideReturn();else{selectedGuide=null;view='browse';section=targetSection;topic='all';buildTopics();}render();}
function guideSearchText(g){return [g.title,g.summary,...g.sections.flatMap(s=>[s.title,...(s.paragraphs||[]),...(s.bullets||[]),s.code||'',s.example||''])].join(' ').toLowerCase();}
function renderCoverage(){
  const root=$('coverageContent');root.replaceChildren();
  root.append(node('p','This is a bounded interview-prep map, not a guarantee that every employer or interview question is covered. Cards build recall; timed implementation, design discussion, SQL exercises, and spoken stories build interview skill.','small'));
  const rows=(studyContent.coverage||[]).filter(item=>!item.section||item.section===section||item.section==='interview');
  function coverageRow(item){const row=node('div',undefined,'coverage-row');row.append(node('h3',item.topic||item.area||item.title||'Coverage'),node('p',(item.status==='covered'?'Content provided':item.status||item.level||'')+(item.priority?' · '+item.priority:''),'small'));for(const key of ['scope','covered','practice','gaps','next','notes']){const value=item[key];if(value)row.append(node('p',(Array.isArray(value)?value.join('; '):value),'small'));}if(item.guideId&&guideById.has(item.guideId)){const a=node('a','Open this topic');a.href='#guide/'+encodeURIComponent(item.guideId);a.onclick=e=>{if(e.ctrlKey||e.metaKey||e.shiftKey||e.altKey)return;e.preventDefault();openGuide(item.guideId);};row.append(a);}return row;}
  const inventory=rows.filter(item=>item.status==='covered'||item.status==='guide + practice');
  rows.filter(item=>!inventory.includes(item)).forEach(item=>root.append(coverageRow(item)));
  if(inventory.length){const detail=node('details');detail.append(node('summary','Topic inventory: '+inventory.length+' '+sectionNames[section].toLowerCase()+' entries'));inventory.forEach(item=>detail.append(coverageRow(item)));root.append(detail);}
}
function renderGuides(){
  $('guideLibrary').classList.toggle('hidden',Boolean(selectedGuide));$('guideDetail').classList.toggle('hidden',!selectedGuide);
  if(!selectedGuide){
    const query=$('guideSearch').value.trim().toLowerCase();const matches=guides.filter(g=>g.section===section&&guideSearchText(g).includes(query));if(section==='design')matches.sort((a,b)=>Number(deck.some(c=>c.guideId===a.id))-Number(deck.some(c=>c.guideId===b.id)));$('guideResultsLabel').textContent=(guideError?guideError+' ':'')+matches.length+' '+sectionNames[section].toLowerCase()+' guides';$('guideList').replaceChildren();
    for(const g of matches){const link=node('a',undefined,'guide-tile');link.href='#guide/'+encodeURIComponent(g.id);link.append(node('span',sectionNames[g.section],'task-meta'),node('h2',g.title),node('p',g.summary,'small'),node('p',deck.some(c=>c.guideId===g.id)?deck.filter(c=>c.guideId===g.id).length+' linked cards':'Practice primer','guide-count'));link.onclick=e=>{if(e.ctrlKey||e.metaKey||e.shiftKey||e.altKey)return;e.preventDefault();openGuide(g.id);};$('guideList').append(link);}
    if(!matches.length)$('guideList').append(node('p','No guides match this search. Try a shorter term or another section.','small'));renderCoverage();return;
  }
  const g=selectedGuide,root=$('guideDetail');root.replaceChildren();
  const actions=node('div',undefined,'guide-actions'),back=node('button',guideReturn?.view==='review'?'← Back to card':guideReturn?.view==='browse'?'← Back to cards':guideReturn?.view==='today'?'← Back to Today':guideReturn?.view==='progress'?'← Back to Progress':'← Browse cards');back.onclick=returnFromGuide;const all=node('a','All '+sectionNames[g.section].toLowerCase()+' guides');all.href='#guides/'+g.section;all.onclick=e=>{if(e.ctrlKey||e.metaKey||e.shiftKey||e.altKey)return;e.preventDefault();openGuideLibrary();};actions.append(back,all);root.append(actions);
  if(guideError)root.append(node('p',guideError,'notice'));
  const toc=node('nav',undefined,'guide-toc');toc.setAttribute('aria-label','In this guide');for(const part of g.sections){const a=node('a',part.title);a.href='#guide/'+encodeURIComponent(g.id)+'/'+encodeURIComponent(part.id);a.onclick=e=>{if(e.ctrlKey||e.metaKey||e.shiftKey||e.altKey)return;e.preventDefault();openGuide(g.id,part.id);};toc.append(a);}root.append(toc);
  for(const part of g.sections){const block=node('section',undefined,'guide-part');block.id='guide-part-'+part.id;block.append(node('h2',part.title));for(const paragraph of part.paragraphs||[])block.append(node('p',paragraph));if(part.bullets?.length){const list=node('ul');for(const bullet of part.bullets)list.append(node('li',bullet));block.append(list);}if(part.example)block.append(node('p',part.example));if(part.code){block.append(node('div',part.language==='csharp'?'C# reference implementation':part.language==='sql'?'SQL example':part.language||'Example','code-label'));const pre=node('pre'),code=node('code',part.code);pre.append(code);block.append(pre);}root.append(block);}
  const sources=node('footer',undefined,'guide-sources');sources.append(node('h2','Sources & further reading'));const list=node('ul');for(const source of g.sources){const item=node('li'),a=node('a',source.title);a.href=source.url;a.target='_blank';a.rel='noopener noreferrer';item.append(a);list.append(item);}sources.append(list,node('p','Read, then close the guide and try to explain or implement the idea without looking. Opening a guide does not record a review or a completed session.','small'));root.append(sources);
  if(window.requestAnimationFrame)window.requestAnimationFrame(()=>{if(selectedGuideAnchor)$('guide-part-'+selectedGuideAnchor)?.scrollIntoView({block:'start'});else window.scrollTo?.(0,0);});
}
$('guideSearch').oninput=renderGuides;
window.addEventListener?.('hashchange',()=>{readGuideRoute();render();});

document.querySelectorAll('[data-view]').forEach(b=>b.onclick=()=>{if(b.dataset.view==='guides'){openGuideLibrary();return;}leaveGuideRoute();view=b.dataset.view;render();});document.querySelectorAll('[data-section]').forEach(b=>b.onclick=()=>setSection(b.dataset.section));$('mobileTopic').onchange=e=>setTopic(e.target.value);$('scheduled').onclick=()=>setMode('scheduled');$('practice').onclick=()=>setMode('practice');$('undo').onclick=undo;$('search').oninput=renderBrowse;
$('export').onclick=()=>{const blob=new Blob([JSON.stringify({...state,deck:'Interview Prep',exportedAt:new Date().toISOString()},null,2)],{type:'application/json'});const url=URL.createObjectURL(blob);const a=document.createElement('a');a.href=url;a.download='recall-progress-'+dayKey(Date.now())+'.json';a.click();setTimeout(()=>URL.revokeObjectURL(url),1000);toast('Progress backup exported.');};
$('import').onclick=()=>$('importFile').click();$('importFile').onchange=async e=>{const file=e.target.files[0];if(!file)return;try{if(file.size>10000000)throw Error('Too large');const incoming=JSON.parse(await file.text());if(!valid(incoming))throw Error('Invalid backup');if(!canChange())return;if(!confirm('Replace your current study progress with this backup? Export your current progress first if you want to keep it.'))return;state=study.normalize(incoming);undoStack=[];current=null;revealed=false;persist();render();toast('Progress restored.');}catch(err){toast('Could not restore this file. Choose a valid Recall progress backup.');}finally{e.target.value='';}};
document.addEventListener('keydown',e=>{if($('accountDialog').open||$('sessionDialog').open||$('exerciseDialog').open||['INPUT','TEXTAREA','SELECT'].includes(document.activeElement.tagName)||view!=='review')return;if((e.ctrlKey||e.metaKey)&&e.key.toLowerCase()==='z'){e.preventDefault();undo();return;}if(e.ctrlKey||e.metaKey||e.altKey||e.repeat)return;if(e.code==='Space'){e.preventDefault();reveal();}if(['1','2','3','4'].includes(e.key)){e.preventDefault();rate(['again','hard','good','easy'][Number(e.key)-1]);}});
setInterval(()=>{if(view==='review'&&mode==='scheduled'&&!current)render();else if(view==='today'&&lastTodayDate!==dayKey(Date.now())&&!$('exerciseDialog').open&&!$('sessionDialog').open&&!$('accountDialog').open)renderToday();else stats();},15000);
buildTopics();readGuideRoute(true);render();
window.recallApp={
 empty:fresh,valid,
 guest(){try{const s=JSON.parse(localStorage.getItem(KEY)||'null');return valid(s)?study.normalize(s):fresh();}catch{return fresh();}},
 replace(s){if(!valid(s))throw Error('Invalid progress');state=study.normalize(s);$('exerciseDialog').close();$('sessionDialog').close();activeTask=null;current=null;revealed=false;undoStack=[];practiceQueue=[];render();}
};
