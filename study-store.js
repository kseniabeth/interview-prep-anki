/* Optional v1 extension: old card schedules/logs keep their original IDs and values. */
(function(root){
  'use strict';
  const tracks=['coding','design','behavioral'];
  const clone=value=>JSON.parse(JSON.stringify(value));
  const dayKey=t=>{const d=new Date(t);return d.getFullYear()+'-'+String(d.getMonth()+1).padStart(2,'0')+'-'+String(d.getDate()).padStart(2,'0');};
  const plans={
    coding:[
      ['Establish a coding baseline','In C#, try one unseen easy problem for 20 minutes, then one unseen medium for 30. Use the last 10 minutes to record where you got stuck. Avoid hints during these diagnostic attempts. An incomplete attempt counts.'],
      ['Practice one useful pattern','Choose one problem that addresses yesterday’s gap. Restate it, attempt a solution in C#, test edge cases, then close the reference and explain the approach. Record any hint or solution used.'],
      ['Re-solve, then change a condition','Re-solve a previous problem without notes. Try a related variant with one changed condition. Explain why your approach works and test a boundary case.'],
      ['Try an unfamiliar mixed problem','Choose a problem without looking at its pattern tag. Explain the input, output and a simple correct approach. Implement and test in C#, then name the biggest remaining gap.'],
      ['Check what transferred','Attempt an unseen variant. Explain correctness, complexity and tests aloud. Use the final 10 minutes to choose the one coding gap to work on next.']
    ],
    design:[
      ['Build a first design','Read the delivery framework for 10 minutes. Spend 25 minutes sketching a URL shortener before reading an answer. Write down one unclear decision. If this design is familiar, choose a new prompt.'],
      ['Defend one design decision','Review up to 10 minutes of design cards. Revisit your first sketch and explain one storage or caching choice. State when the alternative would be better.'],
      ['Design for one failure','Review up to 10 minutes of cards. Change one requirement in your design: a write times out, a consumer retries, or one key becomes hot. Explain the failure and your recovery plan.'],
      ['Start a cold design','Pick an unfamiliar system. Clarify requirements, sketch a working baseline, then explain the most important bottleneck. Attempt it before opening a reference.'],
      ['Redraw and explain tradeoffs','Redraw a design without notes. Defend two decisions and answer one “what if?” failure question. Choose the one concept to revisit next.']
    ],
    behavioral:[
      ['Tell one true story','Choose a real example of a difficult technical decision. Make a short outline: context, your actions, result and learning. Say it aloud in 2–3 minutes. Keep confidential details out of the log.'],
      ['Make your contribution clear','Retell yesterday’s example. Answer: what did I personally do, what alternative did I reject, and why? Use only results you can support.'],
      ['Practice a collaboration story','Choose a real disagreement or collaboration example. Give a concise answer, then answer two follow-up questions about your actions and what you learned.'],
      ['Answer an unfamiliar prompt','Pick a new behavioral question and select a relevant real example. Practice a short initial answer, then probe the decision and outcome.'],
      ['Review one answer aloud','Retell a story without a script. Check relevance, individual contribution, evidence and learning. Choose one improvement for the next practice session.']
    ]
  };
  function validSessions(sessions){
    if(sessions===undefined)return true;
    if(!Array.isArray(sessions))return false;
    const keys=new Set(),ids=new Set();
    return sessions.every(s=>{
      if(!s||typeof s.id!=='string'||!s.id||s.id.length>100||ids.has(s.id)||!tracks.includes(s.track)||!/^\d{4}-\d{2}-\d{2}$/.test(s.date)||s.taskKey!==s.date+':'+s.track||keys.has(s.taskKey)||!Number.isFinite(s.at)||s.at<0||!Number.isInteger(s.minutes)||s.minutes<1||s.minutes>480||!['none','hint','solution','not-applicable'].includes(s.assistance)||(s.outcome!==undefined&&!['partial','completed'].includes(s.outcome))||typeof s.topic!=='string'||s.topic.length<1||s.topic.length>200||typeof s.note!=='string'||s.note.length>1000)return false;
      keys.add(s.taskKey);ids.add(s.id);return true;
    });
  }
  function fresh(){return {version:1,cards:{},log:[],sessions:[]};}
  function normalize(s){return {...clone(s),sessions:clone(s.sessions||[])};}
  function today(s,at=Date.now()){
    const date=dayKey(at);
    return tracks.map(track=>{
      const prior=(s.sessions||[]).filter(x=>x.track===track&&x.date<date).sort((a,b)=>b.date.localeCompare(a.date)||b.at-a.at);
      const completedCount=prior.filter(x=>x.outcome==='completed').length;
      // The initial diagnostic is one-time. Later practice repeats without relabeling it a baseline.
      const planIndex=completedCount===0?0:1+(completedCount-1)%(plans[track].length-1);
      const [title,instructions]=plans[track][planIndex];
      const lastSession=prior[0]||null;
      const continuation=lastSession?(lastSession.outcome==='partial'?'partial':lastSession.outcome!=='completed'?'review':null):null;
      return {track,title,instructions,completedCount,planIndex,continuation,lastSession,minutes:{coding:60,design:40,behavioral:20}[track],taskKey:date+':'+track,date,completed:(s.sessions||[]).find(x=>x.taskKey===date+':'+track)||null};
    });
  }
  function record(s,entry){
    const next=normalize(s);
    const index=next.sessions.findIndex(x=>x.taskKey===entry.taskKey);
    if(index>=0){
      // Editing today's existing log is explicit; a second newly generated ID cannot double-count it.
      if(next.sessions[index].id!==entry.id)return next;
      next.sessions[index]=clone(entry);
    }else next.sessions.push(clone(entry));
    if(!validSessions(next.sessions))throw Error('Invalid study session');
    return next;
  }
  function undo(s,id){const next=normalize(s);next.sessions=next.sessions.filter(x=>x.id!==id);return next;}
  const api={tracks,dayKey,fresh,normalize,validSessions,today,record,undo};
  root.RecallStudy=api;if(typeof module!=='undefined')module.exports=api;
})(typeof window!=='undefined'?window:globalThis);
