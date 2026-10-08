// Keep the offline-friendly HTML payload identical to its editable JSON source.
const fs=require('node:fs'),path=require('node:path');
const root=path.resolve(__dirname,'..');
const cards=JSON.parse(fs.readFileSync(path.join(root,'system-design.json'),'utf8'));
const payload='<script id="designDeckData" type="application/json">'+JSON.stringify(cards,null,2).replace(/</g,'\\u003c')+'</script>';
const file=path.join(root,'index.html');let html=fs.readFileSync(file,'utf8');
const pattern=/<script id="designDeckData" type="application\/json">[\s\S]*?<\/script>/;
html=pattern.test(html)?html.replace(pattern,()=>payload):html.replace('<script src="study-store.js">',payload+'\n<script src="study-store.js">');
const first=JSON.parse(fs.readFileSync(path.join(root,'first-session.json'),'utf8'));
const firstPayload='<script id="firstSessionData" type="application/json">'+JSON.stringify(first,null,2).replace(/</g,'\\u003c')+'</script>';
const firstPattern=/<script id="firstSessionData" type="application\/json">[\s\S]*?<\/script>/;
html=firstPattern.test(html)?html.replace(firstPattern,()=>firstPayload):html.replace('<script src="study-store.js">',firstPayload+'\n<script src="study-store.js">');
fs.writeFileSync(file,html);
console.log('Embedded '+cards.length+' system-design cards.');
