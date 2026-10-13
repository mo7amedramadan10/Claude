DB_LIST=[]
def _o(t,ti,sp=None,su=None,**o):
    d={'t':t,'ti':ti}
    if sp: d['sp']=sp
    if su: d['su']=su
    for k,v in o.items():
        if v is not None: d[k]=list(v) if isinstance(v,tuple) else v
    return d
def K(ti,v,u='',d=None,g=None,n=None,spk=0,hl=0,ic=None):
    o={'t':'kpi','ti':ti,'v':v}
    if u:o['u']=u
    if d is not None:o['d']=d
    if g is not None:o['g']=g
    if n:o['n']=n
    if spk:o['spk']=1
    if hl:o['hl']=1
    if ic:o['ic']=ic
    return o
def Y(ti,ly,cy,d,g=None,n=''):
    o={'t':'kpiyoy','ti':ti,'ly':ly,'cy':cy,'d':d,'n':n}
    if g is not None:o['g']=g
    return o
def H(ti,v,u='',it=(),sp=4): return {'t':'hero','ti':ti,'v':v,'u':u,'it':[list(x) for x in it],'sp':sp}
def W(t): return lambda ti,**o: _o(t,ti,**o)
L,A,CUM,B,G,ST,HB,RK,CMP,S100,DN,RG,GA,FN,HT,RISK,WF,DV,SC,BU,T,CO,INS,CB,NT=[W(x) for x in ['line','area','cum','bars','grouped','stacked','hbars','rank','compare','s100','donut','ring','gauges','funnel','heat','risk','waterfall','div','scatter','bubble','table','cohort','insights','combo','note']]
def P(n,*w): return {'n':n,'w':list(w)}
def DB(id,cat,name,en,desc,src,f,*pages,pop=0):
    DB_LIST.append({'id':'pl-'+id,'cat':cat,'name':name,'en':en,'desc':desc,'sources':list(src),'f':[[a,list(b)] for a,b in f],'p':list(pages),**({'popular':True} if pop else {})})
M='M12'
