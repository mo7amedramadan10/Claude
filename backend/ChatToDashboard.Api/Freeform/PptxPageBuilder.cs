using System.Globalization;
using System.Net;
using System.Text;
using ChatToDashboard.Api.Decks;

namespace ChatToDashboard.Api.Freeform;

/// <summary>
/// يحوّل ملف PowerPoint إلى صفحة شرائح HTML واحدة مستقلة <b>كما هو بلا موديل</b>: كل شريحة تُرسم SVG بالمحرّك الحتمي
/// <see cref="PptxSvgRenderer"/> (الخلفيات والأشكال والصور والنصوص في مواضعها الأصلية، والصور مضمّنة data URL فلا تعتمد
/// على الخادم) داخل <c>&lt;section class="slide"&gt;</c> بتنقّل لوحة مفاتيح/أزرار/لمس وشريط تقدّم و<c>window.goToSlide</c>
/// (يستخدمه شريط المصغّرات) وطباعة شريحة لكل صفحة. النتيجة تصميم من نوع «شرائح» يُعدَّل نصيًا أو يُنشر.
/// </summary>
public static class PptxPageBuilder
{
    public sealed record Result(string Html, int SlideCount, IReadOnlyList<string> Unsupported, double Width, double Height);

    public static Result Build(byte[] pptx, string title)
    {
        using var r = new PptxSvgRenderer(pptx, PptxSvgRenderer.DataUrl);
        if (r.Slides.Count == 0) throw new InvalidOperationException("الملف بلا شرائح.");
        var sb = new StringBuilder();
        var ratio = (r.Height / r.Width).ToString("0.####", CultureInfo.InvariantCulture);
        var safeTitle = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(title) ? "عرض تقديمي" : title);
        sb.Append("<!doctype html>\n<html lang=\"ar\" dir=\"rtl\">\n<head>\n<meta charset=\"utf-8\">\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n");
        sb.Append("<title>").Append(safeTitle).Append("</title>\n");
        sb.Append("<!-- صفحة شرائح مولَّدة من ملف PowerPoint كما هو: كل شريحة SVG بالإحداثيات الأصلية -->\n");
        sb.Append("<style>\n");
        sb.Append(":root{--ar:").Append(ratio).Append(";}\n");
        sb.Append("html,body{margin:0;height:100%;background:#1c2128;color:#e6e9ee;font-family:system-ui,-apple-system,\"Segoe UI\",sans-serif;overflow:hidden}\n");
        // --jeem-strip-h يعلنه شريط المصغّرات (jeem-slides.js) فتُحجز له مساحة أسفل الصفحة
        sb.Append(".deck{position:relative;height:calc(100% - var(--jeem-strip-h,0px));display:flex;flex-direction:column}\n");
        sb.Append(".stage{flex:1;min-height:0;display:flex;align-items:center;justify-content:center;padding:16px}\n");
        sb.Append(".slide{display:none;width:min(100%,calc((100vh - 96px - var(--jeem-strip-h,0px)) / var(--ar)));aspect-ratio:1 / var(--ar);background:#fff;box-shadow:0 12px 40px rgba(0,0,0,.45);border-radius:6px;overflow:hidden}\n");
        sb.Append(".slide.active{display:block}\n.slide svg{display:block;width:100%;height:100%}\n");
        sb.Append(".bar{display:flex;align-items:center;justify-content:center;gap:12px;padding:10px 16px;background:#11151b}\n");
        sb.Append(".bar button{background:#2a323c;color:#e6e9ee;border:1px solid #3a4452;border-radius:10px;padding:8px 14px;font:inherit;font-size:14px;cursor:pointer}\n.bar button:hover{background:#364050}\n");
        sb.Append(".pos{min-width:64px;text-align:center;font-variant-numeric:tabular-nums}\n");
        sb.Append(".prog{position:absolute;top:0;right:0;height:3px;background:#2A78D6;width:0;transition:width .25s}\n");
        sb.Append("@media (prefers-reduced-motion: reduce){.prog{transition:none}}\n");
        sb.Append("@media print{html,body{overflow:visible;background:#fff}.bar,.prog{display:none}.stage{display:block;padding:0}.slide{display:block!important;width:100%;box-shadow:none;border-radius:0;page-break-after:always;break-after:page}}\n");
        sb.Append("</style>\n</head>\n<body>\n<div class=\"deck\"><div class=\"prog\" id=\"prog\"></div><div class=\"stage\" id=\"stage\">\n");
        for (var i = 0; i < r.Slides.Count; i++)
        {
            var svg = r.RenderSlide(i);
            sb.Append("<section class=\"slide").Append(i == 0 ? " active" : "").Append("\" data-slide=\"").Append(i + 1).Append("\" aria-label=\"شريحة ").Append(i + 1).Append("\">\n");
            sb.Append(svg).Append("\n</section>\n");
        }
        sb.Append("</div>\n<div class=\"bar\"><button type=\"button\" data-prev aria-label=\"السابق\">→ السابق</button><span class=\"pos\" id=\"pos\"></span><button type=\"button\" data-next aria-label=\"التالي\">التالي ←</button><button type=\"button\" data-print>طباعة / PDF</button></div></div>\n");
        sb.Append("<script>\n(function(){var slides=[].slice.call(document.querySelectorAll('.slide')),i=0;function go(n){i=Math.max(0,Math.min(slides.length-1,n));slides.forEach(function(s,k){s.classList.toggle('active',k===i)});document.getElementById('pos').textContent=(i+1)+' / '+slides.length;document.getElementById('prog').style.width=((i+1)/slides.length*100)+'%';try{history.replaceState(null,'','#'+(i+1))}catch(e){}document.dispatchEvent(new CustomEvent('jeem:slide',{detail:{index:i}}))}\n");
        sb.Append("window.goToSlide=go;document.querySelector('[data-prev]').onclick=function(){go(i-1)};document.querySelector('[data-next]').onclick=function(){go(i+1)};document.querySelector('[data-print]').onclick=function(){window.print()};\n");
        sb.Append("document.addEventListener('keydown',function(e){if(e.key==='ArrowLeft'||e.key==='PageDown'||e.key===' ')go(i+1);if(e.key==='ArrowRight'||e.key==='PageUp')go(i-1);if(e.key==='Home')go(0);if(e.key==='End')go(slides.length-1)});\n");
        sb.Append("var tx=null;document.addEventListener('touchstart',function(e){tx=e.touches[0].clientX},{passive:true});document.addEventListener('touchend',function(e){if(tx===null)return;var dx=e.changedTouches[0].clientX-tx;tx=null;if(Math.abs(dx)>40)go(dx>0?i+1:i-1)});\n");
        sb.Append("var h=parseInt((location.hash||'').slice(1),10);go(isNaN(h)?0:h-1)})();\n</script>\n</body>\n</html>\n");
        return new Result(sb.ToString(), r.Slides.Count, r.Unsupported.ToList(), r.Width, r.Height);
    }
}
