# DevExtreme (الرسوم فقط)

`dx.viz.jeem.js` حزمة مبنية من `devextreme@26.1.5` تضم مكوّنات الرسوم (Chart، PieChart، Funnel،
Bullet، Sparkline، التصدير، الثيمات) فقط — ~920KB بدل `dx.all.js` (4.7MB). لإعادة بنائها:

```
cd build && npm install && npm run build
```

الترخيص لكل مطوّر (انظر js.devexpress.com/Licensing). المفتاح لا يُكتب في المستودع:
`dotnet user-secrets set "DevExtreme:LicenseKey" "<المفتاح>"` أو متغيّر البيئة
`DevExtreme__LicenseKey`، ويحقنه الخادم في الصفحة عبر `/api/ui-config.js`. بلا مفتاح تعمل
النسخة التجريبية (30 يومًا) بتنبيه ظاهر.
