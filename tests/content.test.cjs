'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'..'),html=fs.readFileSync(path.join(root,'index.html'),'utf8');
const payload=id=>JSON.parse(html.match(new RegExp('<script id="'+id+'" type="application/json">([\\s\\S]*?)</script>'))[1]);
const content=payload('studyContentData');
const all=[...payload('deckData').map((c,i)=>({...c,id:'card-'+i,section:'coding'})),...payload('designDeckData').map(c=>({...c,section:'design'})),...content.cards];
test('legacy card identities are retained; only declared correctness fixes change content',()=>{
 assert.equal(payload('deckData').length,121);assert.equal(payload('designDeckData').length,36);
 const original=JSON.parse(fs.readFileSync(path.join(root,'content/legacy-coding.json'),'utf8')),corrections=JSON.parse(fs.readFileSync(path.join(root,'content/coding-corrections.json'),'utf8'));
 assert.equal(crypto.createHash('sha256').update(JSON.stringify(original)).digest('hex'),'fc07424be0e8ed58f30bef9b6bfa0301d01349ac5442b1796fdb36f3c6425f2e');
 const expected=original.map(c=>({...c}));for(const correction of corrections){assert(correction.reason);const i=Number(correction.id.replace('card-',''));assert(i>=0&&i<121);for(const key of ['front','back'])if(correction[key]!==undefined)expected[i][key]=correction[key];}assert.deepEqual(payload('deckData'),expected);
 assert.equal(crypto.createHash('sha256').update(JSON.stringify(payload('designDeckData'))).digest('hex'),'6af32400aab83e20ae86ed0ca441557c1f010c019c52ebd87e2803737103afa9');
});
test('every card has a valid relevant guide and every specified anchor exists',()=>{
 assert(content.guides.length>0,'Study guide content must be built before release');
 assert.equal(new Set(all.map(c=>c.id)).size,all.length);
 assert.equal(new Set(content.guides.map(g=>g.id)).size,content.guides.length);
 for(const c of all){const link=content.cardGuides[c.id];assert(link,`${c.id}: missing guide map`);const g=content.guides.find(g=>g.id===link.guideId);assert(g,`${c.id}: unknown guide`);assert.equal(g.section,c.section,`${c.id}: wrong section`);if(link.anchor)assert(g.sections.some(s=>s.id===link.anchor),`${c.id}: unknown anchor`);if(c.tags.split(/\s+/)[0]==='template'){assert(link.anchor,`${c.id}: implementation needs an exact anchor`);const part=g.sections.find(s=>s.id===link.anchor);assert(part.code&&part.language==='csharp',`${c.id}: implementation anchor lacks C#`);}}
 assert.equal(Object.keys(content.cardGuides).length,all.length);
 for(const g of content.guides){assert(g.summary&&g.sections.length>=2,`${g.id}: thin guide`);assert(g.sources?.length,`${g.id}: no sources`);assert(g.sources.every(s=>s.title&&s.url.startsWith('https://')),`${g.id}: insecure or untitled source`);assert.equal(new Set(g.sections.map(s=>s.id)).size,g.sections.length);}
});
test('new section cards use stable namespaced IDs and useful groups',()=>{
 for(const c of content.cards){assert(['coding','cs','sql'].includes(c.section));assert(c.id.startsWith(c.section==='coding'?'coding-':c.section+'-')||c.section==='coding'&&c.id.startsWith('alg-'),`${c.id}: stable namespace required`);assert(c.front&&c.back&&c.tags);}
 assert(content.cards.some(c=>c.section==='cs'));assert(content.cards.some(c=>c.section==='sql'));
});
test('generated study payload matches the editable source and compiled C# files',()=>{
 assert.deepEqual(content,JSON.parse(fs.readFileSync(path.join(root,'content/study-content.json'),'utf8')));
 let checked=0;for(const g of content.guides)for(const s of g.sections)if(s.codeFile){assert(!s.codeFile.includes('..'));assert.equal(s.code,fs.readFileSync(path.join(root,s.codeFile),'utf8'),`${g.id}/${s.id} must exactly match executable source`);checked++;}
 assert(checked>=20,'The reference implementations must be linked to actual compiled source files');
});
test('stack lessons begin with foundations and retain existing card/deep-link targets',()=>{
 const guide=content.guides.find(g=>g.id==='coding-stacks');
 assert.equal(guide.sections[0].id,'start');
 const sectionIds=guide.sections.map(s=>s.id);
 for(const id of ['operations','choose','brackets','brackets-code','check-basics','next-greater','next-greater-walkthrough','csharp','next-greater-reasoning','next-greater-check'])assert(sectionIds.includes(id));
 assert(sectionIds.indexOf('check-basics')<sectionIds.indexOf('next-greater'));
 assert.match(guide.sections.find(s=>s.id==='next-greater').title,/Optional/);
 assert.equal(guide.sections.find(s=>s.id==='operations').codeFile,'examples/csharp/StackBasics.cs');
 assert.equal(guide.sections.find(s=>s.id==='brackets-code').codeFile,'examples/csharp/StackBrackets.cs');
 assert.equal(guide.sections.find(s=>s.id==='csharp').codeFile,'examples/csharp/StackNextGreater.cs');
 const api=content.guides.find(g=>g.id==='coding-csharp-heaps-queues').sections.find(s=>s.id==='stack');
 assert.equal(api.codeFile,'examples/csharp/StackBasics.cs');
 for(const [id,anchor] of [['card-22','brackets'],['card-20','next-greater'],['card-75','csharp']])assert.deepEqual(content.cardGuides[id],{guideId:'coding-stacks',anchor});
 assert.deepEqual(content.cardGuides['card-89'],{guideId:'coding-csharp-heaps-queues',anchor:'stack'});
});
