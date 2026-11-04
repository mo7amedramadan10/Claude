using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;

namespace ChatToDashboard.Api.Decks;

/// <summary>محرّك حتمي يحوّل تخطيطات وشرائح ملف PowerPoint إلى SVG بلا موديل ولا LibreOffice: الخلفية (لون/تدرّج/صورة)، الصور
/// (مع القص)، الأشكال الجاهزة والمخصّصة (مسارات)، التعبئة والحدود والشفافية، النصوص (في <c>foreignObject</c> حتى يلتفّ النص ويعمل RTL)،
/// المجموعات، والجداول. التأثيرات (ظل/ثلاثي الأبعاد) والرسوم البيانية الذكية تُتجاهل، والرسوم البيانية تُعلَّم كمنطقة.
/// الوحدات: EMU ÷ 9525 = بكسل عند 96dpi. الصور تُحال إلى عنوان يعطيه <see cref="MediaUrl"/> (data URL في الاختبارات، مسار API في المنصة).</summary>
public sealed class PptxSvgRenderer : IDisposable
{
    public const double Emu = 9525.0;
    private static readonly XNamespace A = "http://schemas.openxmlformats.org/drawingml/2006/main";
    private static readonly XNamespace P = "http://schemas.openxmlformats.org/presentationml/2006/main";
    private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    private readonly PresentationDocument _doc;
    private readonly PresentationPart _pres;
    public Func<ImagePart, string> MediaUrl { get; set; }
    public double Width { get; }
    public double Height { get; }
    public IReadOnlyList<SlideLayoutPart> Layouts { get; }
    public IReadOnlyList<SlidePart> Slides { get; }
    public IReadOnlyList<SlideMasterPart> Masters { get; }
    /// <summary>الصور التي لم يمكن عرضها (EMF/WMF/WDP) في آخر صفحة مُرسَلة.</summary>
    public List<string> Unsupported { get; } = new();

    public PptxSvgRenderer(byte[] bytes, Func<ImagePart, string>? mediaUrl = null)
    {
        _doc = PresentationDocument.Open(new MemoryStream(bytes), false);
        _pres = _doc.PresentationPart ?? throw new InvalidOperationException("ملف PowerPoint غير صالح.");
        var sz = _pres.Presentation.SlideSize;
        Width = (sz?.Cx?.Value ?? 12192000) / Emu; Height = (sz?.Cy?.Value ?? 6858000) / Emu;
        Masters = _pres.Presentation.SlideMasterIdList?.Elements<DocumentFormat.OpenXml.Presentation.SlideMasterId>().Select(m => (SlideMasterPart)_pres.GetPartById(m.RelationshipId!)).ToList() ?? new();
        Layouts = Masters.SelectMany(m => m.SlideMaster.SlideLayoutIdList?.Elements<DocumentFormat.OpenXml.Presentation.SlideLayoutId>().Select(l => (SlideLayoutPart)m.GetPartById(l.RelationshipId!)) ?? Enumerable.Empty<SlideLayoutPart>()).ToList();
        Slides = _pres.Presentation.SlideIdList?.Elements<DocumentFormat.OpenXml.Presentation.SlideId>().Select(s => (SlidePart)_pres.GetPartById(s.RelationshipId!)).ToList() ?? new();
        MediaUrl = mediaUrl ?? DataUrl;
    }

    public void Dispose() => _doc.Dispose();

    public static string DataUrl(ImagePart ip)
    {
        using var s = ip.GetStream(); using var ms = new MemoryStream(); s.CopyTo(ms);
        return $"data:{ip.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}";
    }

    public static string LayoutName(SlideLayoutPart l) => l.SlideLayout.CommonSlideData?.Name?.Value ?? "";
    public SlideMasterPart MasterOf(SlideLayoutPart l) => l.SlideMasterPart!;
    public int LayoutIndexOfSlide(SlidePart s) => Layouts.ToList().IndexOf(s.SlideLayoutPart!);

    public string RenderLayout(int index, bool showPrompts = true) { var l = Layouts[index]; return Render(Xml(l), Xml(MasterOf(l)), null, l, MasterOf(l), showPrompts); }
    public string RenderSlide(int index) { var s = Slides[index]; var l = s.SlideLayoutPart!; return Render(Xml(s), Xml(MasterOf(l)), Xml(l), l, MasterOf(l), false, s); }
    public string RenderMaster(int index) { var m = Masters[index]; return Render(Xml(m), Xml(m), null, null, m, true); }

    private static XElement Xml(OpenXmlPart part) { using var s = part.GetStream(); return XDocument.Load(s).Root!; }

    // ================= السياق =================
    private sealed record Ctx
    {
        public required Dictionary<string, string> Scheme;   // dk1/lt1/dk2/lt2/accent1..6/hlink/folHlink → hex
        public required Dictionary<string, string> ClrMap;   // bg1→lt1 …
        public string MajorFont = "Calibri", MinorFont = "Calibri";
        public required OpenXmlPart Part;                    // لحل r:embed
        public OpenXmlPart? LayoutPart; public OpenXmlPart? MasterPart;
        public XElement? Layout; public XElement? Master;
        public bool ShowPrompts; public bool IsLayoutOrMaster;
        public StringBuilder Defs = new(); public int DefId;
    }

    private string Render(XElement page, XElement master, XElement? layout, SlideLayoutPart? layoutPart, SlideMasterPart masterPart, bool showPrompts, SlidePart? slidePart = null)
    {
        Unsupported.Clear();
        var theme = masterPart.ThemePart is { } tp ? Xml(tp) : null;
        var ctx = new Ctx
        {
            Scheme = ThemeColors(theme), ClrMap = ClrMap(master), Part = (OpenXmlPart?)slidePart ?? (OpenXmlPart?)layoutPart ?? masterPart,
            LayoutPart = layoutPart, MasterPart = masterPart, Layout = layout, Master = master, ShowPrompts = showPrompts, IsLayoutOrMaster = slidePart is null,
        };
        if (theme is not null)
        {
            var fs = theme.Descendants(A + "fontScheme").FirstOrDefault();
            ctx.MajorFont = fs?.Element(A + "majorFont")?.Element(A + "latin")?.Attribute("typeface")?.Value ?? "Calibri";
            ctx.MinorFont = fs?.Element(A + "minorFont")?.Element(A + "latin")?.Attribute("typeface")?.Value ?? "Calibri";
        }
        var body = new StringBuilder();
        // الخلفية: الصفحة ← التخطيط ← الأساس
        var bg = page.Element(P + "cSld")?.Element(P + "bg") ?? layout?.Element(P + "cSld")?.Element(P + "bg") ?? master.Element(P + "cSld")?.Element(P + "bg");
        body.Append(Background(bg, ctx, bg is null ? masterPart : (bg.Parent?.Parent == page ? ctx.Part : bg.Parent?.Parent == layout ? (OpenXmlPart)layoutPart! : masterPart)));
        // أشكال الأساس ثم التخطيط (غير المحجوزة) ثم الصفحة
        var showMaster = page.Element(P + "cSld") is { } cs && (page.Attribute("showMasterSp")?.Value ?? "1") != "0";
        if (slidePart is not null || layoutPart is not null)
        {
            if (showMaster && (layout is null || (layout.Attribute("showMasterSp")?.Value ?? "1") != "0"))
                RenderTree(master.Element(P + "cSld")!.Element(P + "spTree")!, ctx with { Part = masterPart }, body, skipPlaceholders: true);
            if (layout is not null && slidePart is not null) RenderTree(layout.Element(P + "cSld")!.Element(P + "spTree")!, ctx with { Part = layoutPart! }, body, skipPlaceholders: true);
        }
        RenderTree(page.Element(P + "cSld")!.Element(P + "spTree")!, ctx, body, skipPlaceholders: false);
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" viewBox=\"0 0 {Width:0.##} {Height:0.##}\" width=\"{Width:0.##}\" height=\"{Height:0.##}\" font-family=\"{Esc(ctx.MinorFont)}, 'IBM Plex Sans Arabic', Arial, sans-serif\">");
        if (ctx.Defs.Length > 0) sb.Append("<defs>").Append(ctx.Defs).Append("</defs>");
        sb.Append(body).Append("</svg>");
        return sb.ToString();
    }

    private static Dictionary<string, string> ThemeColors(XElement? theme)
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["dk1"] = "000000", ["lt1"] = "FFFFFF", ["dk2"] = "44546A", ["lt2"] = "E7E6E6", ["accent1"] = "4472C4", ["accent2"] = "ED7D31", ["accent3"] = "A5A5A5", ["accent4"] = "FFC000", ["accent5"] = "5B9BD5", ["accent6"] = "70AD47", ["hlink"] = "0563C1", ["folHlink"] = "954F72" };
        var cs = theme?.Descendants(A + "clrScheme").FirstOrDefault(); if (cs is null) return d;
        foreach (var e in cs.Elements())
        {
            var c = e.Element(A + "srgbClr")?.Attribute("val")?.Value ?? e.Element(A + "sysClr")?.Attribute("lastClr")?.Value;
            if (c is not null) d[e.Name.LocalName] = c;
        }
        return d;
    }
    private static Dictionary<string, string> ClrMap(XElement master)
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["bg1"] = "lt1", ["tx1"] = "dk1", ["bg2"] = "lt2", ["tx2"] = "dk2" };
        var m = master.Element(P + "clrMap"); if (m is null) return d;
        foreach (var a in m.Attributes()) d[a.Name.LocalName] = a.Value;
        return d;
    }

    // ================= الألوان =================
    /// <summary>لون عنصر تعبئة/حد: (hex بلا #، alpha 0..1) أو null لبلا لون.</summary>
    private static (string Hex, double Alpha)? Color(XElement? colorParent, Ctx ctx)
    {
        if (colorParent is null) return null;
        XElement? c = colorParent.Element(A + "srgbClr") ?? colorParent.Element(A + "schemeClr") ?? colorParent.Element(A + "sysClr") ?? colorParent.Element(A + "prstClr");
        if (c is null) return null;
        string hex;
        if (c.Name.LocalName == "srgbClr") hex = c.Attribute("val")!.Value;
        else if (c.Name.LocalName == "sysClr") hex = c.Attribute("lastClr")?.Value ?? "000000";
        else if (c.Name.LocalName == "prstClr") hex = PresetColor(c.Attribute("val")?.Value ?? "black");
        else
        {
            var name = c.Attribute("val")!.Value;
            if (name is "phClr") hex = "808080";
            else { if (ctx.ClrMap.TryGetValue(name, out var mapped)) name = mapped; hex = ctx.Scheme.TryGetValue(name, out var h) ? h : "000000"; }
        }
        var (r, g, b) = (Convert.ToInt32(hex[..2], 16) / 255.0, Convert.ToInt32(hex[2..4], 16) / 255.0, Convert.ToInt32(hex[4..6], 16) / 255.0);
        double alpha = 1;
        foreach (var mod in c.Elements())
        {
            var v = double.TryParse(mod.Attribute("val")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var x) ? x / 100000.0 : 0;
            switch (mod.Name.LocalName)
            {
                case "alpha": alpha = v; break;
                case "lumMod": (r, g, b) = Hsl(r, g, b, l => l * v); break;
                case "lumOff": (r, g, b) = Hsl(r, g, b, l => l + v); break;
                case "tint": (r, g, b) = (1 - (1 - r) * v, 1 - (1 - g) * v, 1 - (1 - b) * v); break;
                case "shade": (r, g, b) = (r * v, g * v, b * v); break;
                case "satMod": (r, g, b) = HslS(r, g, b, s => s * v); break;
            }
        }
        return ($"{ToByte(r):X2}{ToByte(g):X2}{ToByte(b):X2}", alpha);
    }
    private static int ToByte(double v) => (int)Math.Round(Math.Clamp(v, 0, 1) * 255);
    private static (double, double, double) Hsl(double r, double g, double b, Func<double, double> f)
    {
        var (h, s, l) = RgbToHsl(r, g, b); return HslToRgb(h, s, Math.Clamp(f(l), 0, 1));
    }
    private static (double, double, double) HslS(double r, double g, double b, Func<double, double> f) { var (h, s, l) = RgbToHsl(r, g, b); return HslToRgb(h, Math.Clamp(f(s), 0, 1), l); }
    private static (double h, double s, double l) RgbToHsl(double r, double g, double b)
    {
        double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b)), l = (max + min) / 2, h = 0, s = 0;
        if (max != min)
        {
            var d = max - min; s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            h = max == r ? (g - b) / d + (g < b ? 6 : 0) : max == g ? (b - r) / d + 2 : (r - g) / d + 4; h /= 6;
        }
        return (h, s, l);
    }
    private static (double, double, double) HslToRgb(double h, double s, double l)
    {
        if (s == 0) return (l, l, l);
        double q = l < 0.5 ? l * (1 + s) : l + s - l * s, p = 2 * l - q;
        double F(double t) { if (t < 0) t += 1; if (t > 1) t -= 1; if (t < 1.0 / 6) return p + (q - p) * 6 * t; if (t < 0.5) return q; if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6; return p; }
        return (F(h + 1.0 / 3), F(h), F(h - 1.0 / 3));
    }
    private static string PresetColor(string name) => name switch { "white" => "FFFFFF", "black" => "000000", "red" => "FF0000", "blue" => "0000FF", "green" => "008000", "gray" or "grey" => "808080", "yellow" => "FFFF00", _ => "000000" };

    // ================= الخلفية =================
    private string Background(XElement? bg, Ctx ctx, OpenXmlPart partForMedia)
    {
        var sb = new StringBuilder();
        var fallback = Color(new XElement("x", new XElement(A + "schemeClr", new XAttribute("val", "bg1"))), ctx);
        var pr = bg?.Element(P + "bgPr");
        if (pr is null)
        {
            var bgRef = bg?.Element(P + "bgRef"); var c = bgRef is null ? fallback : Color(bgRef, ctx) ?? fallback;
            sb.Append(CultureInfo.InvariantCulture, $"<rect width=\"{Width:0.##}\" height=\"{Height:0.##}\" fill=\"#{c?.Hex ?? "FFFFFF"}\"/>");
            return sb.ToString();
        }
        var fill = FillAttr(pr, ctx with { Part = partForMedia }, 0, 0, Width, Height, out var extra);
        sb.Append(extra);
        sb.Append(CultureInfo.InvariantCulture, $"<rect width=\"{Width:0.##}\" height=\"{Height:0.##}\" {fill}/>");
        return sb.ToString();
    }

    /// <summary>سمة التعبئة لعنصر SVG من solidFill/gradFill/blipFill/noFill داخل <paramref name="props"/>؛ الصور تُعاد كعنصر منفصل في <paramref name="extra"/>.</summary>
    private string FillAttr(XElement props, Ctx ctx, double x, double y, double w, double h, out string extra, string? clipId = null)
    {
        extra = "";
        if (props.Element(A + "noFill") is not null) return "fill=\"none\"";
        if (props.Element(A + "solidFill") is { } sf) { var c = Color(sf, ctx); return c is null ? "fill=\"none\"" : $"fill=\"#{c.Value.Hex}\"{(c.Value.Alpha < 1 ? $" fill-opacity=\"{c.Value.Alpha.ToString("0.###", CultureInfo.InvariantCulture)}\"" : "")}"; }
        if (props.Element(A + "gradFill") is { } gf)
        {
            var stops = gf.Element(A + "gsLst")?.Elements(A + "gs").Select(gs => (pos: double.Parse(gs.Attribute("pos")?.Value ?? "0", CultureInfo.InvariantCulture) / 100000.0, c: Color(gs, ctx))).Where(t => t.c is not null).ToList() ?? new();
            if (stops.Count == 0) return "fill=\"none\"";
            var id = $"g{++ctx.DefId}"; var ang = double.Parse(gf.Element(A + "lin")?.Attribute("ang")?.Value ?? "5400000", CultureInfo.InvariantCulture) / 60000.0;
            var rad = ang * Math.PI / 180; var (x2, y2) = (0.5 + 0.5 * Math.Cos(rad), 0.5 + 0.5 * Math.Sin(rad)); var (x1, y1) = (1 - x2, 1 - y2);
            ctx.Defs.Append(CultureInfo.InvariantCulture, $"<linearGradient id=\"{id}\" x1=\"{x1:0.###}\" y1=\"{y1:0.###}\" x2=\"{x2:0.###}\" y2=\"{y2:0.###}\">");
            foreach (var s in stops) ctx.Defs.Append(CultureInfo.InvariantCulture, $"<stop offset=\"{s.pos:0.###}\" stop-color=\"#{s.c!.Value.Hex}\" stop-opacity=\"{s.c.Value.Alpha:0.###}\"/>");
            ctx.Defs.Append("</linearGradient>");
            return $"fill=\"url(#{id})\"";
        }
        if (props.Element(A + "blipFill") is { } bf)
        {
            var img = Image(bf, ctx, x, y, w, h, clipId);
            extra = img; return "fill=\"none\"";
        }
        return null!; // لا تعبئة صريحة: يقرّر المتصل (الأشكال: بلا تعبئة إن كانت من style، الخلفية: أبيض)
    }

    private string Image(XElement blipFill, Ctx ctx, double x, double y, double w, double h, string? clipId)
    {
        var rid = blipFill.Element(A + "blip")?.Attribute(R + "embed")?.Value; if (rid is null) return "";
        OpenXmlPart? part; try { part = ctx.Part.GetPartById(rid); } catch { return ""; }
        if (part is not ImagePart ip) return "";
        var ct = ip.ContentType.ToLowerInvariant();
        if (ct.Contains("emf") || ct.Contains("wmf") || ct.Contains("wdp") || ct.Contains("x-") && !ct.Contains("png"))
        {
            Unsupported.Add(Path.GetFileName(ip.Uri.ToString()));
            return $"<rect x=\"{F(x)}\" y=\"{F(y)}\" width=\"{F(w)}\" height=\"{F(h)}\" fill=\"none\" stroke=\"#C2413B\" stroke-dasharray=\"8 6\" stroke-width=\"2\"/><text x=\"{F(x + w / 2)}\" y=\"{F(y + h / 2)}\" font-size=\"{F(Math.Max(14, Math.Min(w, h) / 10))}\" fill=\"#C2413B\" text-anchor=\"middle\">صورة {Esc(ct.Split('/').Last().ToUpperInvariant())} غير مدعومة</text>";
        }
        var url = MediaUrl(ip);
        var alpha = blipFill.Element(A + "blip")?.Element(A + "alphaModFix")?.Attribute("amt")?.Value is { } am ? double.Parse(am, CultureInfo.InvariantCulture) / 100000.0 : 1;
        // القص srcRect بالنِسب: نعرض الصورة كاملة داخل svg متداخل بإطار عرض مقصوص
        var sr = blipFill.Element(A + "srcRect");
        double l = Pct(sr, "l"), t = Pct(sr, "t"), r = Pct(sr, "r"), b = Pct(sr, "b");
        var opacity = alpha < 1 ? $" opacity=\"{F(alpha)}\"" : "";
        var clip = clipId is null ? "" : $" clip-path=\"url(#{clipId})\"";
        if (l == 0 && t == 0 && r == 0 && b == 0)
            return $"<image x=\"{F(x)}\" y=\"{F(y)}\" width=\"{F(w)}\" height=\"{F(h)}\" preserveAspectRatio=\"none\" href=\"{url}\"{opacity}{clip}/>";
        double vw = 1000, vh = 1000 * (h / Math.Max(w, 1));
        double fullW = vw / Math.Max(1e-6, 1 - l - r), fullH = vh / Math.Max(1e-6, 1 - t - b);
        return $"<svg x=\"{F(x)}\" y=\"{F(y)}\" width=\"{F(w)}\" height=\"{F(h)}\" viewBox=\"0 0 {F(vw)} {F(vh)}\" preserveAspectRatio=\"none\"{opacity}{clip}><image x=\"{F(-l * fullW)}\" y=\"{F(-t * fullH)}\" width=\"{F(fullW)}\" height=\"{F(fullH)}\" preserveAspectRatio=\"none\" href=\"{url}\"/></svg>";
    }
    private static double Pct(XElement? e, string attr) => e?.Attribute(attr)?.Value is { } v ? double.Parse(v, CultureInfo.InvariantCulture) / 100000.0 : 0;

    // ================= الشجرة =================
    private readonly record struct Xf(double X, double Y, double W, double H, double Rot, bool FlipH, bool FlipV);

    private void RenderTree(XElement tree, Ctx ctx, StringBuilder sb, bool skipPlaceholders, Func<Xf, Xf>? map = null)
    {
        foreach (var el in tree.Elements())
        {
            switch (el.Name.LocalName)
            {
                case "sp": RenderShape(el, ctx, sb, skipPlaceholders, map); break;
                case "pic": RenderPic(el, ctx, sb, map); break;
                case "cxnSp": RenderShape(el, ctx, sb, skipPlaceholders, map); break;
                case "grpSp": RenderGroup(el, ctx, sb, skipPlaceholders, map); break;
                case "graphicFrame": RenderFrame(el, ctx, sb, skipPlaceholders, map); break;
            }
        }
    }

    private static bool Hidden(XElement el) => el.Descendants().FirstOrDefault(e => e.Name.LocalName == "cNvPr")?.Attribute("hidden")?.Value == "1";
    private static XElement? Ph(XElement el) => el.Element(P + "nvSpPr")?.Element(P + "nvPr")?.Element(P + "ph") ?? el.Element(P + "nvPicPr")?.Element(P + "nvPr")?.Element(P + "ph") ?? el.Element(P + "nvGraphicFramePr")?.Element(P + "nvPr")?.Element(P + "ph");

    private Xf? Transform(XElement? spPr, Func<Xf, Xf>? map)
    {
        var xf = spPr?.Element(A + "xfrm") ?? spPr?.Element(P + "xfrm"); var off = xf?.Element(A + "off"); var ext = xf?.Element(A + "ext");
        if (off is null || ext is null) return null;
        var t = new Xf(D(off, "x"), D(off, "y"), D(ext, "cx"), D(ext, "cy"), (xf!.Attribute("rot")?.Value is { } r ? double.Parse(r, CultureInfo.InvariantCulture) / 60000.0 : 0), xf.Attribute("flipH")?.Value == "1", xf.Attribute("flipV")?.Value == "1");
        return map is null ? t : map(t);
    }
    private static double D(XElement e, string a) => double.Parse(e.Attribute(a)?.Value ?? "0", CultureInfo.InvariantCulture) / Emu;
    private static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);
    private static string Esc(string s) => s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    /// <summary>موضع عنصر محجوز بلا xfrm: من التخطيط ثم الأساس بنفس idx أو النوع.</summary>
    private XElement? InheritedPlaceholder(XElement ph, XElement? container)
    {
        if (container is null) return null;
        var idx = ph.Attribute("idx")?.Value; var type = ph.Attribute("type")?.Value ?? "body";
        var all = container.Descendants(P + "ph").ToList();
        var match = (idx is not null ? all.FirstOrDefault(p => p.Attribute("idx")?.Value == idx) : null)
                    ?? all.FirstOrDefault(p => (p.Attribute("type")?.Value ?? "body") == type && (idx is null || p.Attribute("idx") is null || type != "body"))
                    ?? (type is "ctrTitle" ? all.FirstOrDefault(p => p.Attribute("type")?.Value == "title") : null);
        return match?.Parent?.Parent?.Parent; // ph → nvPr → nvSpPr → sp
    }

    private void RenderShape(XElement sp, Ctx ctx, StringBuilder sb, bool skipPlaceholders, Func<Xf, Xf>? map)
    {
        if (Hidden(sp)) return;
        var ph = Ph(sp);
        if (ph is not null && skipPlaceholders) return;
        var spPr = sp.Element(P + "spPr");
        var xf = Transform(spPr, map);
        XElement? inhLayout = null, inhMaster = null;
        if (ph is not null)
        {
            inhLayout = InheritedPlaceholder(ph, ctx.Layout); inhMaster = InheritedPlaceholder(ph, ctx.Master);
            xf ??= Transform(inhLayout?.Element(P + "spPr"), map) ?? Transform(inhMaster?.Element(P + "spPr"), map);
        }
        if (xf is null) return;
        var t = xf.Value;
        var geomEl = spPr?.Element(A + "custGeom") ?? spPr?.Element(A + "prstGeom") ?? inhLayout?.Element(P + "spPr")?.Element(A + "prstGeom");
        var prst = geomEl?.Name.LocalName == "prstGeom" ? geomEl.Attribute("prst")?.Value ?? "rect" : null;
        var isLine = prst is "line" or "straightConnector1" or "bentConnector2" or "bentConnector3" or "curvedConnector3";
        var group = new StringBuilder();
        var transform = TransformAttr(t);
        // التعبئة والحد (من الشكل، وإلا من style للمحجوز/الموصّل)
        var style = sp.Element(P + "style");
        string extra = "";
        string fill = spPr is null ? null! : FillAttr(spPr, ctx, 0, 0, t.W, t.H, out extra);
        if (fill is null) { var fr = style?.Element(A + "fillRef"); var c = fr is not null && fr.Attribute("idx")?.Value != "0" ? Color(fr, ctx) : null; fill = c is null || ph is not null ? "fill=\"none\"" : $"fill=\"#{c.Value.Hex}\""; extra = ""; }
        var stroke = StrokeAttr(spPr?.Element(A + "ln"), style, ctx, isLine);
        var clipId = $"c{++ctx.DefId}";
        if (prst is not null && prst != "rect" || geomEl?.Name.LocalName == "custGeom")
        {
            var path = geomEl!.Name.LocalName == "custGeom" ? CustomPath(geomEl, t.W, t.H) : PresetPath(prst!, t.W, t.H, geomEl);
            if (extra.Length > 0) { ctx.Defs.Append($"<clipPath id=\"{clipId}\"><path d=\"{path}\"/></clipPath>"); group.Append($"<g clip-path=\"url(#{clipId})\">").Append(extra).Append("</g>"); }
            group.Append($"<path d=\"{path}\" {fill} {stroke}/>");
        }
        else
        {
            if (extra.Length > 0) group.Append(extra);
            group.Append($"<rect width=\"{F(t.W)}\" height=\"{F(t.H)}\" {fill} {stroke}/>");
        }
        // النص
        var txBody = sp.Element(P + "txBody");
        if (txBody is not null)
        {
            var text = Text(txBody, t, ctx, ph, inhLayout, inhMaster);
            group.Append(text);
        }
        sb.Append($"<g{transform}>").Append(group).Append("</g>");
    }

    private static string TransformAttr(Xf t)
    {
        var parts = new List<string> { $"translate({F(t.X)} {F(t.Y)})" };
        if (t.Rot != 0) parts.Add($"rotate({F(t.Rot)} {F(t.W / 2)} {F(t.H / 2)})");
        if (t.FlipH || t.FlipV) parts.Add($"translate({F(t.FlipH ? t.W : 0)} {F(t.FlipV ? t.H : 0)}) scale({(t.FlipH ? -1 : 1)} {(t.FlipV ? -1 : 1)})");
        return $" transform=\"{string.Join(" ", parts)}\"";
    }

    private string StrokeAttr(XElement? ln, XElement? style, Ctx ctx, bool isLine)
    {
        if (ln?.Element(A + "noFill") is not null) return "stroke=\"none\"";
        (string Hex, double Alpha)? c = null; double w = 0.75 * 96 / 72;
        if (ln is not null)
        {
            if (ln.Attribute("w")?.Value is { } wv) w = double.Parse(wv, CultureInfo.InvariantCulture) / Emu;
            c = Color(ln.Element(A + "solidFill"), ctx);
            if (c is null && ln.Element(A + "solidFill") is null && style?.Element(A + "lnRef") is { } lr0 && lr0.Attribute("idx")?.Value != "0") c = Color(lr0, ctx);
        }
        else if (style?.Element(A + "lnRef") is { } lr && lr.Attribute("idx")?.Value != "0") c = Color(lr, ctx);
        if (c is null) return "stroke=\"none\"";
        var dash = ln?.Element(A + "prstDash")?.Attribute("val")?.Value;
        var da = dash is "dash" or "lgDash" ? $" stroke-dasharray=\"{F(w * 4)} {F(w * 3)}\"" : dash is "sysDash" or "dot" or "sysDot" ? $" stroke-dasharray=\"{F(w)} {F(w)}\"" : "";
        return $"stroke=\"#{c.Value.Hex}\" stroke-width=\"{F(w)}\"{(c.Value.Alpha < 1 ? $" stroke-opacity=\"{F(c.Value.Alpha)}\"" : "")}{da}";
    }

    // ================= الهندسة =================
    private static string PresetPath(string prst, double w, double h, XElement? geom)
    {
        double Adj(string name, double def) { var gd = geom?.Element(A + "avLst")?.Elements(A + "gd").FirstOrDefault(g => g.Attribute("name")?.Value == name); if (gd is null) return def; var f = gd.Attribute("fmla")?.Value ?? ""; var m = Regex.Match(f, @"val\s+(-?\d+)"); return m.Success ? double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) / 100000.0 : def; }
        var ss = Math.Min(w, h);
        switch (prst)
        {
            case "ellipse": return $"M{F(w / 2)} 0A{F(w / 2)} {F(h / 2)} 0 1 1 {F(w / 2)} {F(h)}A{F(w / 2)} {F(h / 2)} 0 1 1 {F(w / 2)} 0Z";
            case "roundRect": { var r = ss * Adj("adj", 0.16667); return $"M{F(r)} 0H{F(w - r)}A{F(r)} {F(r)} 0 0 1 {F(w)} {F(r)}V{F(h - r)}A{F(r)} {F(r)} 0 0 1 {F(w - r)} {F(h)}H{F(r)}A{F(r)} {F(r)} 0 0 1 0 {F(h - r)}V{F(r)}A{F(r)} {F(r)} 0 0 1 {F(r)} 0Z"; }
            case "round2SameRect": { var r = ss * Adj("adj1", 0.16667); return $"M{F(r)} 0H{F(w - r)}A{F(r)} {F(r)} 0 0 1 {F(w)} {F(r)}V{F(h)}H0V{F(r)}A{F(r)} {F(r)} 0 0 1 {F(r)} 0Z"; }
            case "line": case "straightConnector1": return $"M0 0L{F(w)} {F(h)}";
            case "bentConnector2": return $"M0 0H{F(w)}V{F(h)}";
            case "bentConnector3": return $"M0 0H{F(w / 2)}V{F(h)}H{F(w)}";
            case "triangle": return $"M{F(w / 2)} 0L{F(w)} {F(h)}H0Z";
            case "rtTriangle": return $"M0 0V{F(h)}H{F(w)}Z";
            case "diamond": return $"M{F(w / 2)} 0L{F(w)} {F(h / 2)}L{F(w / 2)} {F(h)}L0 {F(h / 2)}Z";
            case "rightArrow": { var a = h * (1 - Adj("adj1", 0.5)) / 2; var hd = ss * Adj("adj2", 0.5); return $"M0 {F(a)}H{F(w - hd)}V0L{F(w)} {F(h / 2)}L{F(w - hd)} {F(h)}V{F(h - a)}H0Z"; }
            case "leftArrow": { var a = h * (1 - Adj("adj1", 0.5)) / 2; var hd = ss * Adj("adj2", 0.5); return $"M{F(w)} {F(a)}H{F(hd)}V0L0 {F(h / 2)}L{F(hd)} {F(h)}V{F(h - a)}H{F(w)}Z"; }
            case "downArrow": { var a = w * (1 - Adj("adj1", 0.5)) / 2; var hd = ss * Adj("adj2", 0.5); return $"M{F(a)} 0H{F(w - a)}V{F(h - hd)}H{F(w)}L{F(w / 2)} {F(h)}L0 {F(h - hd)}H{F(a)}Z"; }
            case "upArrow": { var a = w * (1 - Adj("adj1", 0.5)) / 2; var hd = ss * Adj("adj2", 0.5); return $"M{F(a)} {F(h)}H{F(w - a)}V{F(hd)}H{F(w)}L{F(w / 2)} 0L0 {F(hd)}H{F(a)}Z"; }
            case "chevron": case "homePlate": { var d = ss * Adj("adj", 0.5); var o = prst == "chevron" ? d : 0; return $"M0 0H{F(w - d)}L{F(w)} {F(h / 2)}L{F(w - d)} {F(h)}H0L{F(o)} {F(h / 2)}Z"; }
            case "hexagon": { var d = ss * Adj("adj", 0.25); return $"M{F(d)} 0H{F(w - d)}L{F(w)} {F(h / 2)}L{F(w - d)} {F(h)}H{F(d)}L0 {F(h / 2)}Z"; }
            case "parallelogram": { var d = ss * Adj("adj", 0.25); return $"M{F(d)} 0H{F(w)}L{F(w - d)} {F(h)}H0Z"; }
            case "can": { var ry = h * Adj("adj", 0.25) / 2; return $"M0 {F(ry)}A{F(w / 2)} {F(ry)} 0 0 1 {F(w)} {F(ry)}V{F(h - ry)}A{F(w / 2)} {F(ry)} 0 0 1 0 {F(h - ry)}Z M0 {F(ry)}A{F(w / 2)} {F(ry)} 0 0 0 {F(w)} {F(ry)}"; }
            case "pie": case "arc": case "blockArc": case "donut": return $"M{F(w / 2)} 0A{F(w / 2)} {F(h / 2)} 0 1 1 {F(w / 2)} {F(h)}A{F(w / 2)} {F(h / 2)} 0 1 1 {F(w / 2)} 0Z";
            case "flowChartProcess": case "snip1Rect": case "snip2SameRect": case "plaque": case "bevel": default: return $"M0 0H{F(w)}V{F(h)}H0Z";
        }
    }

    private static string CustomPath(XElement custGeom, double w, double h)
    {
        var sb = new StringBuilder();
        foreach (var path in custGeom.Element(A + "pathLst")?.Elements(A + "path") ?? Enumerable.Empty<XElement>())
        {
            double pw = double.Parse(path.Attribute("w")?.Value ?? "0", CultureInfo.InvariantCulture), ph = double.Parse(path.Attribute("h")?.Value ?? "0", CultureInfo.InvariantCulture);
            double sx = pw > 0 ? w / pw : 1, sy = ph > 0 ? h / ph : 1;
            if (pw == 0 || ph == 0) { sx = w / Emu / Math.Max(1, w); sy = sx; } // مسار بلا أبعاد: وحدات EMU مباشرة
            string Pt(XElement pt) => $"{F(double.Parse(pt.Attribute("x")!.Value, CultureInfo.InvariantCulture) * (pw > 0 ? sx : 1 / Emu))} {F(double.Parse(pt.Attribute("y")!.Value, CultureInfo.InvariantCulture) * (ph > 0 ? sy : 1 / Emu))}";
            foreach (var cmd in path.Elements())
            {
                var pts = cmd.Elements(A + "pt").ToList();
                switch (cmd.Name.LocalName)
                {
                    case "moveTo": sb.Append('M').Append(Pt(pts[0])); break;
                    case "lnTo": sb.Append('L').Append(Pt(pts[0])); break;
                    case "cubicBezTo": sb.Append('C').Append(string.Join(" ", pts.Select(Pt))); break;
                    case "quadBezTo": sb.Append('Q').Append(string.Join(" ", pts.Select(Pt))); break;
                    case "arcTo":
                    {
                        double wr = double.Parse(cmd.Attribute("wR")!.Value, CultureInfo.InvariantCulture) * (pw > 0 ? sx : 1 / Emu), hr = double.Parse(cmd.Attribute("hR")!.Value, CultureInfo.InvariantCulture) * (ph > 0 ? sy : 1 / Emu);
                        double st = double.Parse(cmd.Attribute("stAng")!.Value, CultureInfo.InvariantCulture) / 60000.0, sw = double.Parse(cmd.Attribute("swAng")!.Value, CultureInfo.InvariantCulture) / 60000.0;
                        // نقطة النهاية من زاوية البداية + المسح، والقوس الكبير إن تجاوز 180
                        var (cx, cy) = (0.0, 0.0); // مركز القوس يُحسب من النقطة الحالية؛ تقريب: نرسم قوسًا SVG بنقطة نهاية محسوبة نسبيًا
                        var a0 = st * Math.PI / 180; var a1 = (st + sw) * Math.PI / 180;
                        var dx = wr * (Math.Cos(a1) - Math.Cos(a0)); var dy = hr * (Math.Sin(a1) - Math.Sin(a0));
                        sb.Append(CultureInfo.InvariantCulture, $"a{F(wr)} {F(hr)} 0 {(Math.Abs(sw) > 180 ? 1 : 0)} {(sw > 0 ? 1 : 0)} {F(dx)} {F(dy)}");
                        _ = (cx, cy); break;
                    }
                    case "close": sb.Append('Z'); break;
                }
            }
        }
        return sb.ToString();
    }

    // ================= الصور والمجموعات والإطارات =================
    private void RenderPic(XElement pic, Ctx ctx, StringBuilder sb, Func<Xf, Xf>? map)
    {
        if (Hidden(pic)) return;
        var spPr = pic.Element(P + "spPr"); var xf = Transform(spPr, map);
        if (xf is null && Ph(pic) is { } ph) xf = Transform(InheritedPlaceholder(ph, ctx.Layout)?.Element(P + "spPr"), map) ?? Transform(InheritedPlaceholder(ph, ctx.Master)?.Element(P + "spPr"), map);
        if (xf is null) return; var t = xf.Value;
        var bf = pic.Element(P + "blipFill"); if (bf is null) return;
        string? clip = null;
        var prst = spPr?.Element(A + "prstGeom")?.Attribute("prst")?.Value;
        if (prst is not null && prst != "rect") { clip = $"c{++ctx.DefId}"; ctx.Defs.Append($"<clipPath id=\"{clip}\"><path d=\"{PresetPath(prst, t.W, t.H, spPr?.Element(A + "prstGeom"))}\"/></clipPath>"); }
        else if (spPr?.Element(A + "custGeom") is { } cg) { clip = $"c{++ctx.DefId}"; ctx.Defs.Append($"<clipPath id=\"{clip}\"><path d=\"{CustomPath(cg, t.W, t.H)}\"/></clipPath>"); }
        var img = Image(bf, ctx, 0, 0, t.W, t.H, clip);
        var stroke = StrokeAttr(spPr?.Element(A + "ln"), null, ctx, false);
        sb.Append($"<g{TransformAttr(t)}>").Append(img);
        if (!stroke.StartsWith("stroke=\"none\"")) sb.Append($"<rect width=\"{F(t.W)}\" height=\"{F(t.H)}\" fill=\"none\" {stroke}/>");
        sb.Append("</g>");
    }

    private void RenderGroup(XElement grp, Ctx ctx, StringBuilder sb, bool skipPlaceholders, Func<Xf, Xf>? map)
    {
        if (Hidden(grp)) return;
        var xf = grp.Element(P + "grpSpPr")?.Element(A + "xfrm"); if (xf is null) return;
        var off = xf.Element(A + "off"); var ext = xf.Element(A + "ext"); var chOff = xf.Element(A + "chOff"); var chExt = xf.Element(A + "chExt");
        if (off is null || ext is null) return;
        var outer = new Xf(D(off, "x"), D(off, "y"), D(ext, "cx"), D(ext, "cy"), xf.Attribute("rot")?.Value is { } r ? double.Parse(r, CultureInfo.InvariantCulture) / 60000.0 : 0, xf.Attribute("flipH")?.Value == "1", xf.Attribute("flipV")?.Value == "1");
        if (map is not null) outer = map(outer);
        double cx = chOff is null ? outer.X : D(chOff, "x"), cy = chOff is null ? outer.Y : D(chOff, "y"), cw = chExt is null ? outer.W : D(chExt, "cx"), ch = chExt is null ? outer.H : D(chExt, "cy");
        double sx = cw > 0 ? outer.W / cw : 1, sy = ch > 0 ? outer.H / ch : 1;
        // الأبناء بإحداثيات الطفل: نحوّلها إلى إحداثيات المجموعة المحلية ثم نضع المجموعة بتحويلها
        Xf Inner(Xf c) => new((c.X - cx) * sx, (c.Y - cy) * sy, c.W * sx, c.H * sy, c.Rot, c.FlipH, c.FlipV);
        sb.Append($"<g{TransformAttr(outer with { X = outer.X, Y = outer.Y })}>");
        RenderTree(grp, ctx, sb, skipPlaceholders, Inner);
        sb.Append("</g>");
    }

    private void RenderFrame(XElement gf, Ctx ctx, StringBuilder sb, bool skipPlaceholders, Func<Xf, Xf>? map)
    {
        if (Hidden(gf)) return;
        if (Ph(gf) is not null && skipPlaceholders) return;
        var xf = gf.Element(P + "xfrm"); if (xf is null) return;
        var off = xf.Element(A + "off"); var ext = xf.Element(A + "ext"); if (off is null || ext is null) return;
        var t = new Xf(D(off, "x"), D(off, "y"), D(ext, "cx"), D(ext, "cy"), 0, false, false); if (map is not null) t = map(t);
        var data = gf.Element(A + "graphic")?.Element(A + "graphicData"); var uri = data?.Attribute("uri")?.Value ?? "";
        sb.Append($"<g{TransformAttr(t)}>");
        if (data?.Element(A + "tbl") is { } tbl) sb.Append(Table(tbl, t, ctx));
        else if (uri.Contains("chart")) sb.Append($"<rect width=\"{F(t.W)}\" height=\"{F(t.H)}\" fill=\"#F5F8FA\" stroke=\"#D7E2E9\" stroke-dasharray=\"10 8\" rx=\"8\"/><text x=\"{F(t.W / 2)}\" y=\"{F(t.H / 2)}\" text-anchor=\"middle\" font-size=\"{F(Math.Max(14, Math.Min(t.W, t.H) / 8))}\" fill=\"#5A6B77\">رسم بياني</text>");
        else if (uri.Contains("diagram")) sb.Append($"<rect width=\"{F(t.W)}\" height=\"{F(t.H)}\" fill=\"none\" stroke=\"#D7E2E9\" stroke-dasharray=\"10 8\" rx=\"8\"/>");
        sb.Append("</g>");
    }

    private string Table(XElement tbl, Xf t, Ctx ctx)
    {
        var cols = tbl.Element(A + "tblGrid")?.Elements(A + "gridCol").Select(c => D(c, "w")).ToList() ?? new();
        var rows = tbl.Elements(A + "tr").ToList(); if (rows.Count == 0 || cols.Count == 0) return "";
        var sb = new StringBuilder(); double y = 0;
        foreach (var tr in rows)
        {
            var rh = D(tr, "h"); double x = 0; var ci = 0;
            foreach (var tc in tr.Elements(A + "tc"))
            {
                var span = int.Parse(tc.Attribute("gridSpan")?.Value ?? "1"); var cw = cols.Skip(ci).Take(span).Sum();
                var tcPr = tc.Element(A + "tcPr");
                var fill = tcPr is null ? null : FillAttr(tcPr, ctx, x, y, cw, rh, out _);
                sb.Append($"<rect x=\"{F(x)}\" y=\"{F(y)}\" width=\"{F(cw)}\" height=\"{F(rh)}\" {fill ?? "fill=\"none\""} stroke=\"#BFC9D1\" stroke-width=\"1\"/>");
                if (tc.Element(A + "txBody") is { } tb) sb.Append(Text(tb, new Xf(x, y, cw, rh, 0, false, false), ctx, null, null, null, absolute: true));
                x += cw; ci += span;
            }
            y += rh;
        }
        return sb.ToString();
    }

    // ================= النص =================
    private string Text(XElement txBody, Xf t, Ctx ctx, XElement? ph, XElement? inhLayout, XElement? inhMaster, bool absolute = false)
    {
        var paras = txBody.Elements(A + "p").ToList();
        var hasText = paras.Any(p => p.Elements().Any(e => (e.Name.LocalName is "r" or "fld") && !string.IsNullOrEmpty(e.Element(A + "t")?.Value)));
        var isPrompt = false;
        if (!hasText)
        {
            if (ph is null || !ctx.ShowPrompts) return "";
            // على التخطيط: نص المطالبة (إن وُجد) باهتًا، وإلا نصًّا افتراضيًا بحسب النوع
            isPrompt = true;
        }
        var bodyPr = txBody.Element(A + "bodyPr") ?? inhLayout?.Element(P + "txBody")?.Element(A + "bodyPr") ?? inhMaster?.Element(P + "txBody")?.Element(A + "bodyPr");
        double lIns = Ins(bodyPr, "lIns", 91440), rIns = Ins(bodyPr, "rIns", 91440), tIns = Ins(bodyPr, "tIns", 45720), bIns = Ins(bodyPr, "bIns", 45720);
        var anchor = bodyPr?.Attribute("anchor")?.Value ?? "t";
        var fontScale = bodyPr?.Element(A + "normAutofit")?.Attribute("fontScale")?.Value is { } fsv ? double.Parse(fsv, CultureInfo.InvariantCulture) / 100000.0 : 1;
        var justify = anchor switch { "ctr" => "center", "b" => "flex-end", _ => "flex-start" };
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"<foreignObject x=\"{F(absolute ? t.X : 0)}\" y=\"{F(absolute ? t.Y : 0)}\" width=\"{F(t.W)}\" height=\"{F(t.H)}\"><div xmlns=\"http://www.w3.org/1999/xhtml\" style=\"box-sizing:border-box;width:{F(t.W)}px;height:{F(t.H)}px;padding:{F(tIns)}px {F(rIns)}px {F(bIns)}px {F(lIns)}px;display:flex;flex-direction:column;justify-content:{justify};overflow:hidden;{(isPrompt ? "opacity:.45;" : "")}\">");
        var lstStyle = txBody.Element(A + "lstStyle");
        var phType = ph?.Attribute("type")?.Value ?? (ph is null ? null : "body");
        if (isPrompt && !paras.Any(p => p.Elements(A + "r").Any()))
            paras = new List<XElement> { new(A + "p", new XElement(A + "r", new XElement(A + "t", phType is "title" or "ctrTitle" ? "عنوان الشريحة" : phType is "subTitle" ? "عنوان فرعي" : phType is "pic" ? "" : phType is "dt" or "ftr" or "sldNum" ? "" : "نص"))) };
        foreach (var p in paras)
        {
            var pPr = p.Element(A + "pPr"); var lvl = int.Parse(pPr?.Attribute("lvl")?.Value ?? "0");
            var algn = pPr?.Attribute("algn")?.Value ?? InheritPPr(lvl, "algn", lstStyle, inhLayout, inhMaster, ctx, phType);
            var rtl = (pPr?.Attribute("rtl")?.Value ?? InheritPPr(lvl, "rtl", lstStyle, inhLayout, inhMaster, ctx, phType)) == "1";
            var textAll = string.Concat(p.Elements().Where(e => e.Name.LocalName is "r" or "fld").Select(e => e.Element(A + "t")?.Value));
            if (!rtl && Regex.IsMatch(textAll, "[؀-ۿ]")) rtl = true;
            var ta = algn switch { "ctr" => "center", "r" => rtl ? "start" : "end", "l" => rtl ? "end" : "start", "just" => "justify", _ => rtl ? "start" : "start" };
            var bu = pPr?.Element(A + "buNone") is null && (pPr?.Element(A + "buChar") is not null || InheritBullet(lvl, lstStyle, inhLayout, inhMaster, ctx, phType));
            var marL = Ins(pPr, "marL", 0) + lvl * (Ins(pPr, "indent", 0) < 0 ? 0 : 0);
            var runs = p.Elements().Where(e => e.Name.LocalName is "r" or "fld" or "br").ToList();
            var line = new StringBuilder();
            if (bu && textAll.Trim().Length > 0) line.Append("<span style=\"opacity:.85\">• </span>");
            double maxSz = 0;
            foreach (var r in runs)
            {
                if (r.Name.LocalName == "br") { line.Append("<br/>"); continue; }
                var rPr = r.Element(A + "rPr"); var txt = r.Element(A + "t")?.Value ?? "";
                var sz = (rPr?.Attribute("sz")?.Value is { } s ? double.Parse(s, CultureInfo.InvariantCulture) : InheritSz(lvl, lstStyle, inhLayout, inhMaster, ctx, phType)) / 100.0 * fontScale;
                maxSz = Math.Max(maxSz, sz);
                var color = Color(rPr?.Element(A + "solidFill"), ctx) ?? InheritColor(lvl, lstStyle, inhLayout, inhMaster, ctx, phType) ?? ("000000", 1);
                var face = rPr?.Element(A + "latin")?.Attribute("typeface")?.Value ?? rPr?.Element(A + "cs")?.Attribute("typeface")?.Value ?? InheritFace(lvl, lstStyle, inhLayout, inhMaster, ctx, phType) ?? ctx.MinorFont;
                face = face.Replace("+mj-lt", ctx.MajorFont).Replace("+mn-lt", ctx.MinorFont).Replace("+mj-cs", ctx.MajorFont).Replace("+mn-cs", ctx.MinorFont).Replace("+mj-ea", ctx.MajorFont).Replace("+mn-ea", ctx.MinorFont);
                var bold = (rPr?.Attribute("b")?.Value ?? InheritPPr(lvl, "b", lstStyle, inhLayout, inhMaster, ctx, phType, rpr: true)) == "1";
                var italic = rPr?.Attribute("i")?.Value == "1";
                var alphaCss = color.Alpha < 1 ? $"opacity:{F(color.Alpha)};" : "";
                line.Append($"<span style=\"font-family:'{Esc(face)}','IBM Plex Sans Arabic',Arial,sans-serif;font-size:{F(sz * 96 / 72)}px;color:#{color.Hex};{(bold ? "font-weight:700;" : "")}{(italic ? "font-style:italic;" : "")}{alphaCss}\">{Esc(txt)}</span>");
            }
            var lnSpc = pPr?.Element(A + "lnSpc")?.Element(A + "spcPct")?.Attribute("val")?.Value is { } lsv ? double.Parse(lsv, CultureInfo.InvariantCulture) / 100000.0 : 1.0;
            var spcBef = pPr?.Element(A + "spcBef")?.Element(A + "spcPts")?.Attribute("val")?.Value is { } sb0 ? double.Parse(sb0, CultureInfo.InvariantCulture) / 100.0 * 96 / 72 : 0;
            var emptyH = runs.Count == 0 ? $"min-height:{F(Math.Max(10, InheritSz(lvl, lstStyle, inhLayout, inhMaster, ctx, phType) / 100.0 * 96 / 72 * 1.2))}px;" : "";
            sb.Append($"<p style=\"margin:{F(spcBef)}px 0 0;padding-inline-start:{F(marL)}px;text-align:{ta};direction:{(rtl ? "rtl" : "ltr")};line-height:{F(lnSpc * 1.2)};white-space:pre-wrap;word-wrap:break-word;{emptyH}\">{line}</p>");
        }
        sb.Append("</div></foreignObject>");
        return sb.ToString();
    }
    private static double Ins(XElement? e, string a, double defEmu) => e?.Attribute(a)?.Value is { } v ? double.Parse(v, CultureInfo.InvariantCulture) / Emu : defEmu / Emu;

    /// <summary>سلسلة وراثة خصائص النص: lstStyle الشكل ← المحجوز في التخطيط ← المحجوز في الأساس ← txStyles الأساس.</summary>
    private IEnumerable<XElement> LevelProps(int lvl, XElement? lstStyle, XElement? inhLayout, XElement? inhMaster, Ctx ctx, string? phType)
    {
        var name = A + $"lvl{lvl + 1}pPr";
        if (lstStyle?.Element(name) is { } a) yield return a;
        if (inhLayout?.Element(P + "txBody")?.Element(A + "lstStyle")?.Element(name) is { } b) yield return b;
        if (inhMaster?.Element(P + "txBody")?.Element(A + "lstStyle")?.Element(name) is { } c) yield return c;
        var styles = ctx.Master?.Element(P + "txStyles");
        var which = phType is "title" or "ctrTitle" ? "titleStyle" : phType is null ? "otherStyle" : "bodyStyle";
        if (styles?.Element(P + which)?.Element(name) is { } d) yield return d;
        if (lvl > 0 && styles?.Element(P + which)?.Element(A + "lvl1pPr") is { } e) yield return e;
    }
    private string? InheritPPr(int lvl, string attr, XElement? lstStyle, XElement? inhLayout, XElement? inhMaster, Ctx ctx, string? phType, bool rpr = false)
    {
        foreach (var lp in LevelProps(lvl, lstStyle, inhLayout, inhMaster, ctx, phType))
        {
            var v = rpr ? lp.Element(A + "defRPr")?.Attribute(attr)?.Value : lp.Attribute(attr)?.Value;
            if (v is not null) return v;
        }
        return null;
    }
    private double InheritSz(int lvl, XElement? lstStyle, XElement? inhLayout, XElement? inhMaster, Ctx ctx, string? phType)
    {
        var v = InheritPPr(lvl, "sz", lstStyle, inhLayout, inhMaster, ctx, phType, rpr: true);
        return v is not null ? double.Parse(v, CultureInfo.InvariantCulture) : 1800;
    }
    private (string Hex, double Alpha)? InheritColor(int lvl, XElement? lstStyle, XElement? inhLayout, XElement? inhMaster, Ctx ctx, string? phType)
    {
        foreach (var lp in LevelProps(lvl, lstStyle, inhLayout, inhMaster, ctx, phType))
            if (lp.Element(A + "defRPr")?.Element(A + "solidFill") is { } sf && Color(sf, ctx) is { } c) return c;
        return null;
    }
    private string? InheritFace(int lvl, XElement? lstStyle, XElement? inhLayout, XElement? inhMaster, Ctx ctx, string? phType)
    {
        foreach (var lp in LevelProps(lvl, lstStyle, inhLayout, inhMaster, ctx, phType))
            if (lp.Element(A + "defRPr")?.Element(A + "latin")?.Attribute("typeface")?.Value is { } f) return f;
        return null;
    }
    private bool InheritBullet(int lvl, XElement? lstStyle, XElement? inhLayout, XElement? inhMaster, Ctx ctx, string? phType)
    {
        foreach (var lp in LevelProps(lvl, lstStyle, inhLayout, inhMaster, ctx, phType))
        {
            if (lp.Element(A + "buNone") is not null) return false;
            if (lp.Element(A + "buChar") is not null || lp.Element(A + "buAutoNum") is not null) return true;
        }
        return false;
    }
}
