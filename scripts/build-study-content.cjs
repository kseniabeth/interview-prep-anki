'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const root=path.resolve(__dirname,'..');
const read=p=>JSON.parse(fs.readFileSync(path.join(root,p),'utf8'));
const coding=read('content/coding-guides.json'),foundations=read('content/foundations.json'),newCoding=read('content/new-coding-cards.json');
const extra=Array.isArray(newCoding)?newCoding:newCoding.cards;
const content={schemaVersion:1,cards:[...extra,...foundations.cards],guides:[...coding.guides,...foundations.guides],cardGuides:{...coding.cardGuides,...foundations.cardGuides},groups:{...coding.groups,...foundations.groups},coverage:[...(coding.coverage||[]),...(foundations.coverage||[])]};
const codingAudit=read('content/coding-coverage.json');
content.coverage.unshift({section:'coding',topic:'Coding scope and practice limits',status:'practice-required',scope:codingAudit.scope,practice:codingAudit.practicePolicy,gaps:codingAudit.deferred,notes:codingAudit.formatPolicy},...coding.guides.map(g=>({section:'coding',topic:g.title,status:'covered',priority:g.priority||'core',guideId:g.id})));
const file=path.join(root,'index.html');let html=fs.readFileSync(file,'utf8');
const payload=id=>JSON.parse(html.match(new RegExp('<script id="'+id+'" type="application/json">([\\s\\S]*?)</script>'))[1]);
const original=read('content/legacy-coding.json'),corrections=read('content/coding-corrections.json');
const corrected=original.map(c=>({...c})),correctedIds=new Set();
for(const correction of corrections){const i=Number(correction.id.replace('card-',''));assert(Number.isInteger(i)&&i>=0&&i<121&&correction.id==='card-'+i&&!correctedIds.has(correction.id),'Invalid correction ID');assert(correction.reason,'Correction needs an explanation');correctedIds.add(correction.id);for(const key of ['front','back'])if(correction[key]!==undefined)corrected[i][key]=correction[key];}
content.corrections=corrections;
if(corrections.length)content.coverage.push({section:'coding',topic:'Audited legacy corrections',status:corrections.length+' answers clarified or corrected; all original IDs retained',scope:'Review history, due dates, and card identity are unchanged.',covered:corrections.map(c=>c.id+': '+c.reason)});
const legacy=corrected.map((c,i)=>({...c,id:'card-'+i,section:'coding'}));
html=html.replace(/<script id="deckData" type="application\/json">[\s\S]*?<\/script>/,()=>'<script id="deckData" type="application/json">'+JSON.stringify(corrected,null,2).replace(/</g,'\\u003c')+'</script>');
assert.equal(legacy.length,121,'Do not change legacy card identities');
const design=read('system-design.json');assert.equal(design.length,36);
const all=[...legacy,...design,...content.cards],ids=new Set(),guideIds=new Set();
for(const g of content.guides){assert(/^[a-z][a-z0-9-]*$/.test(g.id),`Invalid guide ID: ${g.id}`);assert(!guideIds.has(g.id),`Duplicate guide ID: ${g.id}`);guideIds.add(g.id);assert(['coding','design','cs','sql'].includes(g.section));assert(g.title&&g.summary&&g.sections.length>=2,`Guide incomplete: ${g.id}`);const anchors=new Set();for(const s of g.sections){assert(s.id&&s.title&&!anchors.has(s.id),`Invalid section: ${g.id}`);assert(/^[a-z][a-z0-9-]*$/.test(s.id));anchors.add(s.id);for(const key of ['paragraphs','bullets'])if(s[key]!==undefined)assert(Array.isArray(s[key])&&s[key].every(x=>typeof x==='string'),`Invalid ${key}: ${g.id}/${s.id}`);for(const key of ['code','example'])if(s[key]!==undefined)assert(typeof s[key]==='string',`Invalid ${key}: ${g.id}/${s.id}`);assert((s.paragraphs?.length||s.bullets?.length||s.code||s.example),`Empty section: ${g.id}/${s.id}`);}assert(g.sources?.length,`Missing sources: ${g.id}`);for(const source of g.sources)assert(source.title&&/^https:\/\//.test(source.url),`Invalid source: ${g.id}`);}
for(const c of all){assert(c.id&&!ids.has(c.id),`Duplicate card: ${c.id}`);ids.add(c.id);const link=content.cardGuides[c.id]||{guideId:c.guideId,anchor:c.guideAnchor||c.anchor};assert(guideIds.has(link.guideId),`Card missing guide: ${c.id}`);const g=content.guides.find(g=>g.id===link.guideId);const anchor=link.anchor||link.guideAnchor;assert(!anchor||g.sections.some(s=>s.id===anchor),`Card guide anchor missing: ${c.id}`);content.cardGuides[c.id]={guideId:link.guideId,...(anchor?{anchor}: {})};}
for(const id of Object.keys(content.cardGuides))assert(ids.has(id),`Guide map points to unknown card: ${id}`);
const data='<script id="studyContentData" type="application/json">'+JSON.stringify(content,null,2).replace(/</g,'\\u003c')+'</script>';
const pattern=/<script id="studyContentData" type="application\/json">[\s\S]*?<\/script>/;assert(pattern.test(html),'Missing content script');html=html.replace(pattern,()=>data);fs.writeFileSync(file,html);fs.writeFileSync(path.join(root,'content/study-content.json'),JSON.stringify(content,null,2)+'\n');
console.log(`${all.length} cards linked to ${content.guides.length} guides; ${content.cards.length} new cards. Legacy IDs intact.`);
