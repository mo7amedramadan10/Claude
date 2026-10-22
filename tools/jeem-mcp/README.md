# جسر جيم لـ Claude Desktop (`jeem-mcp`)

خادم MCP محلي صغير يشتغل على جهازك داخل شبكة الشركة، فيقدر Claude Desktop ينشر العروض التقديمية
(صفحات HTML) على منصة جيم مباشرة ويرجّع لك الرابط العام — بدون ما تخرج المنصة على الإنترنت.

## المتطلبات
- Node.js 20 أو أحدث على جهازك.
- تطبيق Claude Desktop (الأدوات المحلية لا تعمل من المتصفح).
- مفتاح وصول شخصي من المنصة: شاشة «العروض التقديمية» (القائمة الجانبية ← المنظمة) → زر «🔑 ربط Claude» → «إنشاء مفتاح». المفتاح
  يُعرض مرة واحدة فقط.

## التركيب
1. انسخ هذا المجلد إلى جهازك (مثلًا `C:\jeem-mcp`) ثم داخله:
   ```
   npm install
   ```
2. افتح ملف إعداد Claude Desktop:
   - Windows: `%APPDATA%\Claude\claude_desktop_config.json`
   - macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`

   وأضف (أو ادمج) هذا المقطع، مع عنوان المنصة الداخلي ومفتاحك:
   ```json
   {
     "mcpServers": {
       "jeem": {
         "command": "node",
         "args": ["C:\\jeem-mcp\\index.js"],
         "env": {
           "JEEM_URL": "http://jeem.internal.company:5000",
           "JEEM_TOKEN": "jeem_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
         }
       }
     }
   }
   ```
3. أعد تشغيل Claude Desktop. ستظهر أدوات «jeem» في قائمة الأدوات.

## الاستخدام
### الصفحات الكبيرة: مرّر مسار الملف بدل النص
`publish_presentation` و`replace_presentation` يقبلان `filePath` (مسار ملف `.html` على جهازك، مطلق أو يبدأ
بـ `~`) بدل `html`. الجسر يقرأ الملف بنفسه ويرسله، فلا يعيد Claude كتابة صفحة بحجم ميغابايت في كل تحديث:
«عدّل الملف `~/Downloads/site/index.html` ثم استبدل العرض `q3-results` منه». الحد 15 ميغابايت.

في أي محادثة: «اعمل عرضًا عن نتائج الربع الثالث وانشره على جيم» — Claude يبني الصفحة، ينادي
`publish_presentation`، ويعطيك الرابط `…/p/xxxxxxxx`. أدوات أخرى: `replace_presentation` (نسخة أحدث على
نفس الرابط)، `list_presentations`، `revoke_presentation`، و`whoami` للتحقق من الاتصال.

## الأمان
- المفتاح يرث صلاحيات حسابك فقط، ويُلغى من نفس الشاشة في المنصة في أي وقت.
- الصفحات المنشورة تُعرض على المنصة داخل إطار معزول (`sandbox`) فلا تصل إلى بيانات النظام.
- لا تضع المفتاح في أي مستودع أو رسالة؛ ملف الإعداد على جهازك فقط.
