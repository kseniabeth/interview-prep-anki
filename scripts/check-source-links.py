"""Check public learning-resource URLs. Block release on confirmed 404/410 only.
403/429/timeouts are recorded as unverified, never treated as proof of a good link.
"""
import argparse, concurrent.futures, json, socket
from datetime import datetime, timezone
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.parse import urldefrag, urlsplit
from urllib.request import Request, urlopen
ROOT=Path(__file__).resolve().parents[1]

def check(url):
    entry={'url':url}
    # These URLs come from checked-in, public reference metadata only.
    request=Request(url,headers={'User-Agent':'Recall-study-link-check/1.0'},method='HEAD')
    try:
        with urlopen(request,timeout=12) as response:
            entry.update(status=response.status,final_url=response.url,result='reachable')
    except HTTPError as error:
        if error.code in (405,501):
            try:
                with urlopen(Request(url,headers={'User-Agent':'Recall-study-link-check/1.0'}),timeout=12) as response:
                    response.read(512);entry.update(status=response.status,final_url=response.url,result='reachable')
            except HTTPError as second:entry.update(status=second.code,result='missing' if second.code in (404,410) else 'unverified')
            except (URLError,TimeoutError,socket.timeout) as second:entry.update(result='unverified',error=str(second))
        else:entry.update(status=error.code,result='missing' if error.code in (404,410) else 'unverified')
    except (URLError,TimeoutError,socket.timeout) as error:entry.update(result='unverified',error=str(error))
    return entry

def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--output',type=Path);args=parser.parse_args()
    content=json.loads((ROOT/'content/study-content.json').read_text())
    sources=[s['url'] for g in content['guides'] for s in g['sources']]+[c['source']['url'] for c in json.loads((ROOT/'system-design.json').read_text())]
    urls=sorted({urldefrag(url)[0] for url in sources})
    assert all(urlsplit(url).scheme=='https' and urlsplit(url).hostname for url in urls)
    with concurrent.futures.ThreadPoolExecutor(max_workers=6) as pool:results=list(pool.map(check,urls))
    report={'checked_at':datetime.now(timezone.utc).isoformat(),'checked':len(results),'reachable':sum(r['result']=='reachable' for r in results),'missing':sum(r['result']=='missing' for r in results),'unverified':sum(r['result']=='unverified' for r in results),'results':results}
    if args.output:args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_text(json.dumps(report,indent=2)+'\n')
    print(json.dumps(report,indent=2))
    raise SystemExit(1 if report['missing'] else 0)
if __name__=='__main__':main()
