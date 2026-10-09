"""Browser smoke tests against a local, isolated study app. No real auth requests."""
import json, os, threading, tempfile
from functools import partial
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from pathlib import Path
from playwright.sync_api import sync_playwright

ROOT=Path(__file__).resolve().parents[1]
OUT=Path(os.environ.get('RECALL_QA_OUTPUT', tempfile.mkdtemp(prefix='recall-qa-')))
OUT.mkdir(parents=True, exist_ok=True)
class QuietHandler(SimpleHTTPRequestHandler):
    def log_message(self, *args): pass
server=ThreadingHTTPServer(('127.0.0.1',0),partial(QuietHandler,directory=str(ROOT)))
threading.Thread(target=server.serve_forever,daemon=True).start()
BASE=f'http://127.0.0.1:{server.server_port}/'
MOCK_AUTH=r'''
window.__testRemote={row:null,listener:null,requests:0};
window.supabase={createClient(){return {
  auth:{onAuthStateChange(fn){window.__testRemote.listener=fn;},async getSession(){return {data:{session:null},error:null};},async signInWithOtp(){window.__testRemote.requests++;return {error:null};},async signOut(){window.__testRemote.listener('SIGNED_OUT',null);return {error:null};}},
  from(){let write=null,rev=null;const q={select(){return q;},eq(k,v){if(k==='revision')rev=v;return q;},insert(row){write=row;return q;},update(row){write=row;return q;},async maybeSingle(){if(write){if(rev!==null&&window.__testRemote.row?.revision!==rev)return {data:null,error:null};window.__testRemote.row=JSON.parse(JSON.stringify(write));return {data:{revision:write.revision},error:null};}return {data:window.__testRemote.row,error:null};}};return q;}
};}};
'''
OLD={'version':1,'cards':{'card-7':{'due':1999999999999,'interval':86400000,'reps':2,'lapses':1}},'log':[{'id':'card-7','at':1000000,'rating':'good'}]}

def snap_state(page):
    return page.evaluate('JSON.parse(JSON.stringify(state))')
def no_overflow(page):
    assert not page.evaluate('document.documentElement.scrollWidth > innerWidth'), 'Horizontal page overflow'
def wait_for_guide_position(page, anchor=None):
    # Rendering scrolls on requestAnimationFrame. Wait for that scroll before
    # checking the viewport or capturing a mobile screenshot.
    page.wait_for_function('''anchor => anchor
        ? Math.abs(document.getElementById('guide-part-'+anchor).getBoundingClientRect().top - 20) < 2
        : window.scrollY === 0''', arg=anchor)
    page.evaluate('new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))')

def exercise(page,label):
    errors=[]
    page.on('pageerror',lambda e:errors.append(str(e)))
    page.route('**/vendor/supabase-2.98.0.js', lambda route:route.fulfill(status=200,content_type='application/javascript',body=MOCK_AUTH))
    page.add_init_script("localStorage.setItem('recall-interview-prep-v1',"+json.dumps(json.dumps(OLD))+");")
    page.goto(BASE);page.wait_for_function('window.recallCloud && window.recallCloud.canEdit()')
    assert page.locator('.today-task').count()==3
    assert snap_state(page)['cards']==OLD['cards']
    no_overflow(page);page.screenshot(path=str(OUT/f'{label}-today.png'),full_page=True)
    page.get_by_role('button',name='Coding',exact=True).click()
    page.get_by_role('button',name='Show answer',exact=True).click()
    original_id=page.evaluate('current.id');before=snap_state(page)
    link=page.locator('#card .guide-link');assert link.get_attribute('href').startswith('#guide/')
    link.click();page.wait_for_function("view==='guides' && selectedGuide")
    assert page.locator('#guideDetail').is_visible()
    page.keyboard.press('1');page.keyboard.press('Space');assert snap_state(page)==before
    no_overflow(page);page.screenshot(path=str(OUT/f'{label}-guide.png'),full_page=True)
    page.go_back();page.wait_for_function("view==='review'");assert page.evaluate('current.id')==original_id;assert page.locator('#card .answer').is_visible()
    assert snap_state(page)==before
    for section in ['CS Fundamentals','SQL']:
        page.get_by_role('button',name=section,exact=True).click()
        page.get_by_role('button',name='Show answer',exact=True).click()
        card_id=page.evaluate('current.id');page.locator('#card .guide-link').click();assert snap_state(page)==before
        page.get_by_role('button',name='← Back to card',exact=True).click()
        page.locator('.rating-grid button').nth(2).click();assert snap_state(page)['cards'][card_id]['reps']==1
        page.get_by_role('button',name='Undo last rating',exact=True).click();assert snap_state(page)==before
    page.get_by_role('button',name='Study guides',exact=True).click()
    page.locator('#guideSearch').fill('SELECT');page.keyboard.press('1');assert snap_state(page)==before
    assert page.locator('#guideSearch').input_value()=='SELECT1', 'Typing must edit the search, not rate a card'
    page.locator('#guideSearch').fill('SELECT')
    assert page.locator('#guideList .guide-tile').count()>0
    page.locator('#guideList .guide-tile').first.click();deep=page.url;page.reload();page.wait_for_function("view==='guides' && selectedGuide");assert page.url==deep
    no_overflow(page);page.screenshot(path=str(OUT/f'{label}-sql-guide.png'),full_page=True)
    # The stack root and long-lived card links must show the rewritten lesson,
    # including on narrow screens, without recording study progress.
    page.goto(BASE+'#guide/coding-stacks')
    page.wait_for_function("view==='guides' && selectedGuide?.id==='coding-stacks'")
    assert page.locator('#guideDetail .guide-part').first.get_attribute('id')=='guide-part-start'
    assert 'pile of plates' in page.locator('#guide-part-start').inner_text()
    assert 'Push(10)' in page.locator('#guide-part-operations code').inner_text()
    assert page.locator('#guide-part-next-greater h2').inner_text().startswith('Optional')
    assert snap_state(page)==before
    wait_for_guide_position(page)
    no_overflow(page);page.screenshot(path=str(OUT/f'{label}-stack-start.png'))
    page.locator('.guide-toc a[href="#guide/coding-stacks/brackets"]').click()
    page.wait_for_function("selectedGuideAnchor==='brackets'")
    wait_for_guide_position(page,'brackets')
    assert 'most recent opener' in page.locator('#guide-part-brackets h2').inner_text()
    page.go_back();page.wait_for_function("selectedGuideAnchor===''");wait_for_guide_position(page)
    page.go_forward();page.wait_for_function("selectedGuideAnchor==='brackets'");wait_for_guide_position(page,'brackets')
    for anchor in ['brackets','next-greater','csharp']:
        page.goto(BASE+'#guide/coding-stacks/'+anchor)
        page.wait_for_function('anchor => selectedGuideAnchor===anchor',arg=anchor)
        wait_for_guide_position(page,anchor)
        page.reload();page.wait_for_function("view==='guides' && selectedGuide?.id==='coding-stacks'")
        assert page.evaluate('selectedGuideAnchor')==anchor
        assert page.locator('#guide-part-'+anchor).is_visible()
        wait_for_guide_position(page,anchor)
        no_overflow(page)
        if anchor=='csharp':page.screenshot(path=str(OUT/f'{label}-stack-next-greater-code.png'))
        assert snap_state(page)==before
    assert 'class StackNextGreater' in page.locator('#guide-part-csharp code').inner_text()
    page.goto(BASE+'#guide/coding-csharp-heaps-queues/stack')
    page.wait_for_function("selectedGuide?.id==='coding-csharp-heaps-queues' && selectedGuideAnchor==='stack'")
    assert 'class StackBasics' in page.locator('#guide-part-stack code').inner_text()
    wait_for_guide_position(page,'stack')
    no_overflow(page);assert snap_state(page)==before
    # Space may scroll the page; test rating isolation after layout checks.
    page.keyboard.press('1');page.keyboard.press('Space');assert snap_state(page)==before
    page.get_by_role('button',name='Sign in',exact=True).click();page.locator('#loginEmail').fill('study-test@example.invalid');page.locator('#sendMagicLink').click();page.wait_for_function("document.querySelector('#authMessage').textContent.includes('Check your email')");assert page.evaluate('__testRemote.requests')==1
    page.locator('#closeAccount').click()
    page.evaluate("__testRemote.listener('SIGNED_IN',{user:{id:'test-account',email:'study-test@example.invalid'}})")
    page.wait_for_function('window.recallCloud.isSignedIn() && window.recallCloud.canEdit()')
    assert snap_state(page)['cards']=={},'Guest data silently migrated'
    page.get_by_role('button',name='SQL',exact=True).click();page.get_by_role('button',name='Study',exact=True).click();page.get_by_role('button',name='Show answer',exact=True).click();new_id=page.evaluate('current.id');page.locator('.rating-grid button').nth(2).click()
    page.wait_for_function('__testRemote.row?.progress?.cards && Object.keys(__testRemote.row.progress.cards).length===1')
    assert page.evaluate('__testRemote.row.progress.cards')[new_id]['reps']==1
    page.get_by_role('button',name='Account',exact=True).click();page.get_by_role('button',name='Sign out',exact=True).click();page.wait_for_function('!window.recallCloud.isSignedIn()');assert snap_state(page)['cards']==OLD['cards']
    page.locator('#closeAccount').click()
    page.get_by_role('button',name='Progress',exact=True).click()
    with page.expect_download() as download_info:page.locator('#export').click()
    download=download_info.value;backup=json.loads(Path(download.path()).read_text());assert backup['cards']==OLD['cards'];assert backup['sessions']==[]
    restored={'version':1,'cards':{**OLD['cards'],new_id:{'due':1999999999999,'interval':86400000,'reps':1,'lapses':0}},'log':OLD['log'],'sessions':[]}
    page.once('dialog',lambda dialog:dialog.dismiss())
    page.locator('#importFile').set_input_files({'name':'progress.json','mimeType':'application/json','buffer':json.dumps(restored).encode()})
    page.wait_for_function("document.querySelector('#importFile').value === ''");assert snap_state(page)['cards']==OLD['cards']
    page.once('dialog',lambda dialog:dialog.accept())
    page.locator('#importFile').set_input_files({'name':'progress.json','mimeType':'application/json','buffer':json.dumps(restored).encode()})
    page.wait_for_function("document.querySelector('#importFile').value === ''");assert snap_state(page)['cards']==restored['cards']
    assert not errors,errors
    return {'viewport':label,'checks':'Today (3 actions), all section links, exact card/back, guide keyboard isolation, new-card ratings/undo, deep-link reload, stack foundations and all existing stack anchors, stack Back/Forward, search, auth UI with mock provider, cloud new-card save, sign-out guest isolation, export, cancelled/repeated backup restore, no page overflow','errors':errors}

try:
    with sync_playwright() as p:
        options={'headless':True}
        if os.environ.get('CHROMIUM_PATH'):options['executable_path']=os.environ['CHROMIUM_PATH']
        browser=p.chromium.launch(**options)
        reports=[]
        for width,height,label in [(1440,1000,'desktop'),(393,852,'mobile'),(320,740,'small-mobile')]:
            context=browser.new_context(viewport={'width':width,'height':height},is_mobile=width<500,has_touch=width<500)
            reports.append(exercise(context.new_page(),label));context.close()
        browser.close()
    (OUT/'browser-report.json').write_text(json.dumps(reports,indent=2))
    print(json.dumps(reports,indent=2));print('Screenshots:',OUT)
finally:
    server.shutdown()
