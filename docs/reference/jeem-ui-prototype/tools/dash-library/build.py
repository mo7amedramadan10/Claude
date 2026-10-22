import json,sys
import os; sys.path.insert(0,os.path.dirname(os.path.abspath(__file__)))
import d_hr,d_fin,d_sales,d_proc,d_ops,d_rest,dsl
L=dsl.DB_LIST
# لوحات مستوحاة من معرض Databox (157 لوحة بعد دمج المتكرر) — المصدر: d_databox.json، المواصفة: DATABOX_SPEC.md
_here=os.path.dirname(os.path.abspath(__file__))
for d in json.load(open(os.path.join(_here,'d_databox.json'))):
    d=dict(d); d.setdefault('f',[]); d['ref']={'by':'Databox','items':d['ref']}; L.append(d)
TINT={'hr':'var(--brand-teal)','finance':'#0E6399','sales':'','procurement':'var(--brand-purple)','supply':'#0E8FB8','ops':'var(--brand-gray)','hse':'var(--brand-teal)','projects':'var(--brand-purple)','retail':'','saas':'#0E8FB8','banking':'#0E6399','health':'var(--brand-teal)','gov':'var(--brand-purple)','hospitality':'#0E8FB8','other':'var(--brand-gray)','marketing':'','ads':'#0E8FB8','social':'var(--brand-purple)','cx':'var(--brand-teal)','dev':'var(--brand-gray)','retail':''}
POP={'db-web-traffic-kpis','db-search-ads-overview','db-saas-mrr-growth','db-ig-account-overview','db-helpdesk-tickets','pl-hr-command-center','pl-cfo-income-statement','pl-budget-vs-actual','pl-procurement-hub','pl-project-portfolio','pl-sales-pipeline'}
for d in L:
    n=len(d['p'])
    d['tint']=TINT.get(d['cat'],'')
    if d['id'] in POP: d['popular']=True
    fl='، '.join(a for a,_ in d['f'])
    d['prompt']=(f"ابنِ لوحة «{d['name']}» من {{{{sources}}}} للفترة {{{{period}}}}"+(f"، وتتكون من {n} صفحات" if n>1 else '')+" بالعناصر التالية:\n{{widgets}}\n"
      f"- الهدف: {d['desc']}\n- اعرض كل مؤشر مع مقارنته بالفترة السابقة، والمبالغ بعملة {{{{currency}}}}.\n"+(f"- أضف فلاتر أعلى اللوحة: {fl}.\n" if fl else '')+"- إذا لم يتوفر عمود مطلوب في المصادر، اسأل المستخدم بدل الافتراض.")
out='/* جيم — مكتبة لوحات المتابعة الجاهزة (مولّد آليًا: لوحات Power BI المرجعية + لوحات معرض Databox)\n   كل لوحة: { id, cat, name, en, desc, sources, f: الفلاتر, p: الصفحات [{ n, w: العناصر }], prompt }\n   أنواع العناصر وخياراتها موثقة في assets/js/dash-render.js */\nwindow.JEEM_DASH_LIBRARY='+json.dumps(L,ensure_ascii=False,separators=(',',':'))+';\n'
open(os.path.join(os.path.dirname(os.path.abspath(__file__)),'..','..','assets','js','dash-library.js'),'w').write(out)
print(len(L),len(out.encode())//1024,'KB')
