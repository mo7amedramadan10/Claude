#!/usr/bin/env python3
"""Validate a batch file of Jeem dashboards extracted from Databox. Usage: python3 validate.py out/B1.json"""
import json,sys,re
CATS={'marketing','ads','social','sales','retail','finance','saas','cx','dev','projects'}
DIMS={'M12','W13','Q4','Y5','Y11','DAY5','DAY7','HR8','DEPT','CITY','REG','NAT','GRADE','AGE','TEN','LVL','CH','SUP','PCAT','PERSON','PRJ','SITE','WH','STAT',
      'TRF','DEVC','SOC','CNTRY','PAGE','KW','CMPN','LS','DST','PLAN','AGEM','GEN','PROD','AGENT'}
KINDS={'','sar','pct','d','x','s','r','n'}
TCOL={'l','p','dt','id','dp','st','sp','bar','n','sar','pct','d','r','s'}
SPANS={2,3,4,5,6,7,8,9,10,12}
T={ # type: (required keys, allowed keys)
 'kpi':({'ti','v'},{'u','d','g','n','spk','hl','ic'}),
 'kpiyoy':({'ti','ly','cy','d'},{'g','n'}),
 'hero':({'ti','v'},{'u','it','sp'}),
 'line':({'ti'},{'x','b','k','s','ly','tg','vol','tr','su','sp'}),
 'area':({'ti'},{'x','b','k','s','ly','tg','vol','tr','su','sp'}),
 'cum':({'ti'},{'x','b','k','s','ly','tg','su','sp'}),
 'bars':({'ti','x'},{'b','k','su','sp'}),
 'grouped':({'ti','x','s'},{'b','k','su','sp'}),
 'stacked':({'ti','x','s'},{'b','k','su','sp'}),
 'combo':({'ti','x','s'},{'b','k','lb','lk','su','sp'}),
 'hbars':({'ti','x'},{'b','k','u','tg','ns','su','sp'}),
 'rank':({'ti','x'},{'b','k','u','su','sp'}),
 'compare':({'ti','x'},{'b','k','inv','su','sp'}),
 's100':({'ti','x','s'},{'su','sp'}),
 'donut':({'ti','x'},{'tot','tl','b','k','su','sp'}),
 'ring':({'ti','v'},{'lab','it','tg','vl','vu','su','sp'}),
 'gauges':({'ti','x'},{'tg','su','sp'}),
 'funnel':({'ti','x','b'},{'k','su','sp'}),
 'heat':({'ti','rows','cols'},{'b','k','num','su','sp'}),
 'waterfall':({'ti','x','b'},{'k','sg','su','sp'}),
 'div':({'ti','x'},{'b','k','inv','su','sp'}),
 'scatter':({'ti'},{'s','xl','yl','n','su','sp'}),
 'table':({'ti','c'},{'rows','n','idp','sts','tot','su','sp'}),
 'cohort':({'ti'},{'rows','cols','su','sp'}),
 'insights':({'ti','it'},{'su','sp'}),
 'note':({'ti','tx'},{'su','sp'}),
}
AR=re.compile(r'[؀-ۿ]')
def chkdim(v,path,err,maxn=None):
    if isinstance(v,str):
        k,_,n=v.partition(':')
        if k not in DIMS: err.append(f'{path}: unknown dim "{v}"'); return 0
        return int(n) if n else 99
    if isinstance(v,list):
        if not v or not all(isinstance(x,str) and x for x in v): err.append(f'{path}: dim list must be non-empty strings'); return 0
        if maxn and len(v)>maxn: err.append(f'{path}: too many categories ({len(v)}>{maxn})')
        return len(v)
    err.append(f'{path}: bad dim type'); return 0
def main(fn):
    err=[]; warn=[]
    data=json.load(open(fn))
    if not isinstance(data,list): print('ERROR: top level must be a list'); sys.exit(1)
    ids=set()
    for i,d in enumerate(data):
        P=f'[{i}:{d.get("id","?")}]'
        for k in ('id','cat','name','en','desc','sources','ref','p'):
            if k not in d: err.append(f'{P} missing {k}')
        extra=set(d)-{'id','cat','name','en','desc','sources','ref','f','p'}
        if extra: err.append(f'{P} unknown keys {extra}')
        if not str(d.get('id','')).startswith('db-') or not re.fullmatch(r'db-[a-z0-9-]+',str(d.get('id',''))): err.append(f'{P} id must match db-[a-z0-9-]+')
        if d.get('id') in ids: err.append(f'{P} duplicate id')
        ids.add(d.get('id'))
        if d.get('cat') not in CATS: err.append(f'{P} cat must be one of {sorted(CATS)}')
        for k in ('name','desc'):
            if not AR.search(str(d.get(k,''))): err.append(f'{P} {k} must be Arabic')
        if re.search(r'hubspot|semrush|ahrefs|databox|profitwell|chartmogul|quickbooks|xero|shopify|woocommerce|bigcommerce|pipedrive|salesforce|pardot|marketo|klaviyo|mailchimp|sendgrid|intercom|freshdesk|helpscout|drift|outreach|copper|sharpspring|intrix|appfigures|accuranker|stackadapt|callrail|infusionsoft|freshbooks|harvest|surveymonkey|campaign monitor|activecampaign',(d.get('name','')+' '+d.get('en','')).lower()):
            err.append(f'{P} name/en must not contain vendor tool names (keep them in sources/ref only)')
        if not isinstance(d.get('sources'),list) or not d.get('sources'): err.append(f'{P} sources must be non-empty list')
        refs=d.get('ref',[])
        if not isinstance(refs,list) or not refs or not all(isinstance(r,dict) and r.get('slug') and r.get('title') for r in refs): err.append(f'{P} ref must be list of {{title,slug}}')
        for fi,f in enumerate(d.get('f',[])):
            if not (isinstance(f,list) and len(f)==2 and isinstance(f[0],str) and isinstance(f[1],list) and 2<=len(f[1])<=6): err.append(f'{P} f[{fi}] must be [label,[2..6 options]]')
        if len(d.get('f',[]))>3: err.append(f'{P} max 3 filters')
        pages=d.get('p',[])
        if not 1<=len(pages)<=4: err.append(f'{P} 1–4 pages')
        tw=0
        for pi,pg in enumerate(pages):
            Q=f'{P}.p[{pi}]'
            if not AR.search(pg.get('n','')): err.append(f'{Q} page name n must be Arabic')
            ws=pg.get('w',[]); tw+=len(ws)
            if len(ws)<3: err.append(f'{Q} needs >=3 widgets')
            nk=0;run=0
            for wi,w in enumerate(ws):
                R=f'{Q}.w[{wi}]'
                t=w.get('t')
                if t not in T: err.append(f'{R} unknown type {t}'); continue
                req,opt=T[t]
                miss=req-set(w); 
                if miss: err.append(f'{R} ({t}) missing {miss}')
                bad=set(w)-req-opt-{'t'}
                if bad: err.append(f'{R} ({t}) unknown keys {bad}')
                if not AR.search(str(w.get('ti',''))): err.append(f'{R} title ti must be Arabic')
                if t in('kpi','kpiyoy'):
                    run+=1
                    if run>8: err.append(f'{R} more than 8 KPIs in a row (split with a chart)')
                    if t=='kpi' and not isinstance(w.get('v'),str): err.append(f'{R} kpi v must be a string')
                    if 'd' in w and not isinstance(w['d'],str): err.append(f'{R} kpi d must be a string like "+4.2%"')
                    if 'g' in w and not isinstance(w['g'],bool): err.append(f'{R} g must be true/false')
                else: run=0
                if 'sp' in w and w['sp'] not in SPANS: err.append(f'{R} sp must be in {sorted(SPANS)}')
                if 'k' in w and w['k'] not in KINDS: err.append(f'{R} k must be in {sorted(KINDS)}')
                if 'lk' in w and w['lk'] not in KINDS: err.append(f'{R} lk invalid')
                for key in ('b','lb','tg'):
                    if key in w and not isinstance(w[key],(int,float)): err.append(f'{R} {key} must be a number')
                if 'x' in w:
                    mx={'donut':5,'bars':13,'funnel':7,'gauges':6,'waterfall':9,'combo':13,'grouped':12,'stacked':13}.get(t,12)
                    n=chkdim(w['x'],R+'.x',err,mx)
                    if t=='donut' and isinstance(w['x'],str) and n>5: warn.append(f'{R} donut over 5 → folded into أخرى')
                if 'rows' in w: chkdim(w['rows'],R+'.rows',err,12)
                if t=='heat': chkdim(w['cols'],R+'.cols',err,12)
                if 's' in w:
                    if not (isinstance(w['s'],list) and 1<=len(w['s'])<=4 and all(isinstance(x,str) for x in w['s'])): err.append(f'{R} s must be 1–4 series names')
                    elif t in('grouped','stacked','s100','combo') and len(w['s'])<2: err.append(f'{R} {t} needs >=2 series')
                if t=='table':
                    c=w.get('c',[])
                    if not (isinstance(c,list) and 2<=len(c)<=8): err.append(f'{R} table c must have 2–8 columns')
                    for ci,col in enumerate(c):
                        if not(isinstance(col,list) and len(col)==2): err.append(f'{R} c[{ci}] must be [header,kind]'); continue
                        k=col[1]
                        if not(k in TCOL or (k.startswith('@') and k[1:].split(':')[0] in DIMS)): err.append(f'{R} c[{ci}] unknown column kind {k}')
                    if c and c[0][1]=='l' and 'rows' not in w: err.append(f'{R} table with "l" first column needs rows')
                if t=='ring' and not isinstance(w.get('v'),(int,float)): err.append(f'{R} ring v must be a number 0–100')
                if t in('insights',) and not all(isinstance(x,list) and 2<=len(x)<=4 for x in w.get('it',[])): err.append(f'{R} insights it = [[head,text,value?,tone?]]')
                if t=='hero' and 'it' in w and not all(isinstance(x,list) and len(x)==2 for x in w['it']): err.append(f'{R} hero it = [[label,value]]')
                if t=='note' and not isinstance(w.get('tx'),list): err.append(f'{R} note tx must be list of strings')
            if not any(w.get('t') in('kpi','kpiyoy','hero') for w in ws): warn.append(f'{Q} no KPI on page')
        if tw<5: err.append(f'{P} too few widgets ({tw})')
    for e in err: print('ERROR',e)
    for w in warn: print('warn ',w)
    print(f'{len(data)} dashboards, {len(err)} errors, {len(warn)} warnings')
    sys.exit(1 if err else 0)
main(sys.argv[1])
