using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace ChatToDashboard.Api.Export;

/// <summary>
/// Builds a .pptx from scratch — a title slide (question + summary) followed by one slide
/// per group of up to <see cref="WidgetsPerSlide"/> widgets: 2 side by side on top and the
/// 3rd spanning the full width below for a full group of 3, or however many fit evenly for a
/// trailing group of 1 or 2 — see CreateWidgetGroupSlide. No template file: the theme/master/
/// layout below are the minimal boilerplate every valid OOXML presentation needs, written
/// once here. KPI values and table widgets become native PowerPoint text/tables (so they
/// stay editable); bar, line and pie widgets become a picture, since the browser already
/// rendered them as SVG and hands over a PNG snapshot of exactly what's on screen.
/// </summary>
public static class PptxBuilder
{
    private const long SlideWidth = 12192000L;   // 13.333in, 16:9
    private const long SlideHeight = 6858000L;   // 7.5in
    private const long Margin = 457200L;         // 0.5in
    private const long TitleTop = 274638L;
    private const long TitleHeight = 800100L;
    private const long ContentTop = TitleTop + TitleHeight + 137160L;
    private static long ContentWidth => SlideWidth - 2 * Margin;
    private static long ContentHeight => SlideHeight - ContentTop - Margin;

    /// <summary>How many widgets share one slide — see CreateWidgetGroupSlide.</summary>
    private const int WidgetsPerSlide = 3;
    private const long ColumnGap = 182880L;   // 0.2in between side-by-side widgets
    private const long RowGap = 137160L;      // 0.15in between the top and bottom rows of a 3-widget slide
    private const long ColumnTitleHeight = 400050L;
    private const long TitleToBodyGap = 45720L;   // 0.05in
    private const long SourceHeight = 228600L;   // 0.25in, reserved uniformly so bodies stay aligned

    public static byte[] Build(PptxExportRequest request)
    {
        using var stream = new MemoryStream();
        using (var doc = PresentationDocument.Create(stream, PresentationDocumentType.Presentation))
        {
            var presentationPart = doc.AddPresentationPart();
            presentationPart.Presentation = new P.Presentation();

            var slideMasterPart = CreateSlideMaster(presentationPart);
            var slideLayoutPart = slideMasterPart.SlideLayoutParts.First();

            var slideParts = new List<SlidePart>
            {
                CreateTitleSlide(presentationPart, slideLayoutPart, request.Title, request.Summary),
            };
            foreach (var group in request.Widgets.Chunk(WidgetsPerSlide))
                slideParts.Add(CreateWidgetGroupSlide(presentationPart, slideLayoutPart, group));

            presentationPart.Presentation.Append(new P.SlideMasterIdList(new P.SlideMasterId
            {
                Id = 2147483648U,
                RelationshipId = presentationPart.GetIdOfPart(slideMasterPart),
            }));

            var slideIdList = new P.SlideIdList();
            uint slideId = 256;
            foreach (var part in slideParts)
            {
                slideIdList.Append(new P.SlideId { Id = slideId++, RelationshipId = presentationPart.GetIdOfPart(part) });
            }
            presentationPart.Presentation.Append(slideIdList);

            presentationPart.Presentation.Append(new P.SlideSize { Cx = (int)SlideWidth, Cy = (int)SlideHeight, Type = P.SlideSizeValues.Screen16x9 });
            presentationPart.Presentation.Append(new P.NotesSize { Cx = 6858000, Cy = 9144000 });
            presentationPart.Presentation.Save();
        }
        return stream.ToArray();
    }

    // ---------- theme / master / layout (fixed boilerplate) ----------

    private static SlideMasterPart CreateSlideMaster(PresentationPart presentationPart)
    {
        var slideMasterPart = presentationPart.AddNewPart<SlideMasterPart>();
        var themePart = slideMasterPart.AddNewPart<ThemePart>();
        themePart.FeedData(GenerateStream(ThemeXml));

        var slideLayoutPart = slideMasterPart.AddNewPart<SlideLayoutPart>();
        slideLayoutPart.AddPart(slideMasterPart);
        slideLayoutPart.FeedData(GenerateStream(SlideLayoutXml));

        var layoutRelId = slideMasterPart.GetIdOfPart(slideLayoutPart);
        slideMasterPart.FeedData(GenerateStream(string.Format(SlideMasterXmlTemplate, layoutRelId)));

        return slideMasterPart;
    }

    private static MemoryStream GenerateStream(string xml) => new(System.Text.Encoding.UTF8.GetBytes(xml));

    private const string ThemeXml = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <a:theme xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main" name="Chat to Dashboard">
          <a:themeElements>
            <a:clrScheme name="Office">
              <a:dk1><a:sysClr val="windowText" lastClr="0B0B0B"/></a:dk1>
              <a:lt1><a:sysClr val="window" lastClr="FFFFFF"/></a:lt1>
              <a:dk2><a:srgbClr val="52514E"/></a:dk2>
              <a:lt2><a:srgbClr val="F9F9F7"/></a:lt2>
              <a:accent1><a:srgbClr val="2A78D6"/></a:accent1>
              <a:accent2><a:srgbClr val="EB6834"/></a:accent2>
              <a:accent3><a:srgbClr val="1BAF7A"/></a:accent3>
              <a:accent4><a:srgbClr val="EDA100"/></a:accent4>
              <a:accent5><a:srgbClr val="E87BA4"/></a:accent5>
              <a:accent6><a:srgbClr val="008300"/></a:accent6>
              <a:hlink><a:srgbClr val="2A78D6"/></a:hlink>
              <a:folHlink><a:srgbClr val="52514E"/></a:folHlink>
            </a:clrScheme>
            <a:fontScheme name="Office">
              <a:majorFont><a:latin typeface="Calibri"/><a:ea typeface=""/><a:cs typeface="Arial"/></a:majorFont>
              <a:minorFont><a:latin typeface="Calibri"/><a:ea typeface=""/><a:cs typeface="Arial"/></a:minorFont>
            </a:fontScheme>
            <a:fmtScheme name="Office">
              <a:fillStyleLst>
                <a:solidFill><a:schemeClr val="phClr"/></a:solidFill>
                <a:solidFill><a:schemeClr val="phClr"/></a:solidFill>
                <a:solidFill><a:schemeClr val="phClr"/></a:solidFill>
              </a:fillStyleLst>
              <a:lnStyleLst>
                <a:ln><a:solidFill><a:schemeClr val="phClr"/></a:solidFill></a:ln>
                <a:ln><a:solidFill><a:schemeClr val="phClr"/></a:solidFill></a:ln>
                <a:ln><a:solidFill><a:schemeClr val="phClr"/></a:solidFill></a:ln>
              </a:lnStyleLst>
              <a:effectStyleLst>
                <a:effectStyle><a:effectLst/></a:effectStyle>
                <a:effectStyle><a:effectLst/></a:effectStyle>
                <a:effectStyle><a:effectLst/></a:effectStyle>
              </a:effectStyleLst>
              <a:bgFillStyleLst>
                <a:solidFill><a:schemeClr val="phClr"/></a:solidFill>
                <a:solidFill><a:schemeClr val="phClr"/></a:solidFill>
                <a:solidFill><a:schemeClr val="phClr"/></a:solidFill>
              </a:bgFillStyleLst>
            </a:fmtScheme>
          </a:themeElements>
        </a:theme>
        """;

    private const string SlideLayoutXml = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <p:sldLayout xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                     xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                     xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main"
                     type="blank" preserve="1">
          <p:cSld name="Blank">
            <p:spTree>
              <p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr>
              <p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr>
            </p:spTree>
          </p:cSld>
          <p:clrMapOvr><a:masterClrMapping/></p:clrMapOvr>
        </p:sldLayout>
        """;

    private const string SlideMasterXmlTemplate = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <p:sldMaster xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
                     xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                     xmlns:p="http://schemas.openxmlformats.org/presentationml/2006/main">
          <p:cSld>
            <p:bg><p:bgRef idx="1001"><a:schemeClr val="bg1"/></p:bgRef></p:bg>
            <p:spTree>
              <p:nvGrpSpPr><p:cNvPr id="1" name=""/><p:cNvGrpSpPr/><p:nvPr/></p:nvGrpSpPr>
              <p:grpSpPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="0" cy="0"/><a:chOff x="0" y="0"/><a:chExt cx="0" cy="0"/></a:xfrm></p:grpSpPr>
            </p:spTree>
          </p:cSld>
          <p:clrMap bg1="lt1" tx1="dk1" bg2="lt2" tx2="dk2" accent1="accent1" accent2="accent2"
                    accent3="accent3" accent4="accent4" accent5="accent5" accent6="accent6"
                    hlink="hlink" folHlink="folHlink"/>
          <p:sldLayoutIdLst>
            <p:sldLayoutId id="2147483649" r:id="{0}"/>
          </p:sldLayoutIdLst>
          <p:txStyles>
            <p:titleStyle><a:lvl1pPr><a:defRPr sz="4400"/></a:lvl1pPr></p:titleStyle>
            <p:bodyStyle><a:lvl1pPr><a:defRPr sz="2800"/></a:lvl1pPr></p:bodyStyle>
            <p:otherStyle><a:lvl1pPr><a:defRPr sz="1800"/></a:lvl1pPr></p:otherStyle>
          </p:txStyles>
        </p:sldMaster>
        """;

    // ---------- slides ----------

    private static SlidePart NewSlidePart(PresentationPart presentationPart, SlideLayoutPart layoutPart)
    {
        var slidePart = presentationPart.AddNewPart<SlidePart>();
        slidePart.AddPart(layoutPart);
        slidePart.Slide = new P.Slide(
            new P.CommonSlideData(new P.ShapeTree(
                new P.NonVisualGroupShapeProperties(
                    new P.NonVisualDrawingProperties { Id = 1, Name = "" },
                    new P.NonVisualGroupShapeDrawingProperties(),
                    new P.ApplicationNonVisualDrawingProperties()),
                new P.GroupShapeProperties(new A.TransformGroup(
                    new A.Offset { X = 0, Y = 0 },
                    new A.Extents { Cx = 0, Cy = 0 },
                    new A.ChildOffset { X = 0, Y = 0 },
                    new A.ChildExtents { Cx = 0, Cy = 0 })))),
            new P.ColorMapOverride(new A.MasterColorMapping()));
        return slidePart;
    }

    private static P.Shape TextBox(
        uint id, string text, long x, long y, long cx, long cy,
        int sizePt, bool bold = false, string colorHex = "0B0B0B", A.TextAlignmentTypeValues? align = null)
    {
        var paragraphProperties = new A.ParagraphProperties
        {
            Alignment = align ?? A.TextAlignmentTypeValues.Right, RightToLeft = BooleanValue.FromBoolean(true),
        };
        var runProperties = new A.RunProperties
        {
            Language = "ar-SA",
            FontSize = sizePt * 100,
            Bold = bold,
        };
        runProperties.Append(new A.SolidFill(new A.RgbColorModelHex { Val = colorHex }));

        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties { Id = id, Name = $"TextBox {id}" },
                new P.NonVisualShapeDrawingProperties(new A.ShapeLocks()),
                new P.ApplicationNonVisualDrawingProperties()),
            new P.ShapeProperties(
                new A.Transform2D(new A.Offset { X = x, Y = y }, new A.Extents { Cx = cx, Cy = cy }),
                new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }),
            new P.TextBody(
                new A.BodyProperties { Wrap = A.TextWrappingValues.Square, Anchor = A.TextAnchoringTypeValues.Top },
                new A.ListStyle(),
                new A.Paragraph(paragraphProperties, new A.Run(runProperties, new A.Text(text ?? "")))));
    }

    private static SlidePart CreateTitleSlide(
        PresentationPart presentationPart, SlideLayoutPart layoutPart, string title, string summary)
    {
        var slidePart = NewSlidePart(presentationPart, layoutPart);
        var tree = slidePart.Slide!.CommonSlideData!.ShapeTree!;
        tree.Append(TextBox(2, string.IsNullOrWhiteSpace(title) ? "لوحة معلومات" : title,
            Margin, TitleTop, ContentWidth, TitleHeight, 32, bold: true, colorHex: "2A78D6"));
        tree.Append(TextBox(3, summary ?? "", Margin, ContentTop, ContentWidth, ContentHeight, 20));
        return slidePart;
    }

    /// <summary>One slide holding up to <see cref="WidgetsPerSlide"/> widgets. A 3-widget group
    /// gets 2 side by side on top and the 3rd spanning the full width below — 3 equal columns
    /// packed every chart/table into a cramped third of the slide (titles wrapping across
    /// several lines, bars squeezed down to a few pixels wide), reported live as unreadable.
    /// Two widgets get 2 even side-by-side columns at full height; a single trailing widget
    /// gets the whole slide, same as before.</summary>
    private static SlidePart CreateWidgetGroupSlide(
        PresentationPart presentationPart, SlideLayoutPart layoutPart, IReadOnlyList<PptxWidgetInput> widgets)
    {
        var slidePart = NewSlidePart(presentationPart, layoutPart);
        var tree = slidePart.Slide!.CommonSlideData!.ShapeTree!;
        uint nextId = 2;

        switch (widgets.Count)
        {
            case 1:
                AppendWidgetInRect(slidePart, tree, ref nextId, widgets[0], Margin, ContentTop, ContentWidth, ContentHeight);
                break;

            case 2:
            {
                var halfWidth = (ContentWidth - ColumnGap) / 2;
                AppendWidgetInRect(slidePart, tree, ref nextId, widgets[0], Margin, ContentTop, halfWidth, ContentHeight);
                AppendWidgetInRect(slidePart, tree, ref nextId, widgets[1], Margin + halfWidth + ColumnGap, ContentTop, halfWidth, ContentHeight);
                break;
            }

            default: // 3 — two on top, one full-width below
            {
                var halfWidth = (ContentWidth - ColumnGap) / 2;
                var topHeight = (ContentHeight - RowGap) / 2;
                var bottomTop = ContentTop + topHeight + RowGap;
                var bottomHeight = ContentHeight - topHeight - RowGap;

                AppendWidgetInRect(slidePart, tree, ref nextId, widgets[0], Margin, ContentTop, halfWidth, topHeight);
                AppendWidgetInRect(slidePart, tree, ref nextId, widgets[1], Margin + halfWidth + ColumnGap, ContentTop, halfWidth, topHeight);
                AppendWidgetInRect(slidePart, tree, ref nextId, widgets[2], Margin, bottomTop, ContentWidth, bottomHeight);
                break;
            }
        }

        return slidePart;
    }

    /// <summary>Renders one widget's title, body and (optional) source note into the given
    /// rect — shared by every cell of CreateWidgetGroupSlide's layout so a full-slide, half-
    /// slide or third-of-a-row-and-half-column cell all lay out the same way, just scaled to
    /// whatever rect they're given.</summary>
    private static void AppendWidgetInRect(
        SlidePart slidePart, P.ShapeTree tree, ref uint nextId, PptxWidgetInput widget, long x, long y, long width, long height)
    {
        tree.Append(TextBox(nextId++, widget.Title, x, y, width, ColumnTitleHeight, 16, bold: true, colorHex: "2A78D6"));

        var bodyTop = y + ColumnTitleHeight + TitleToBodyGap;
        var bodyHeight = height - ColumnTitleHeight - TitleToBodyGap - SourceHeight;
        AppendWidgetBody(slidePart, tree, ref nextId, widget, x, bodyTop, width, bodyHeight);

        if (!string.IsNullOrWhiteSpace(widget.Source))
            tree.Append(TextBox(nextId++, widget.Source, x, y + height - SourceHeight + TitleToBodyGap, width, SourceHeight,
                9, colorHex: "898781"));
    }

    /// <summary>Renders one widget's body (everything but its title/source, which the caller
    /// places) into the given rect — shared by every cell of AppendWidgetInRect.</summary>
    private static void AppendWidgetBody(
        SlidePart slidePart, P.ShapeTree tree, ref uint nextId, PptxWidgetInput widget, long x, long y, long width, long height)
    {
        switch ((widget.Type ?? "").ToLowerInvariant())
        {
            case "kpi":
                tree.Append(TextBox(nextId++, widget.Value ?? "—", x, y, width, height / 2,
                    44, bold: true, align: A.TextAlignmentTypeValues.Center));
                if (!string.IsNullOrWhiteSpace(widget.Label))
                    tree.Append(TextBox(nextId++, widget.Label, x, y + height / 2, width, height / 2,
                        18, colorHex: "52514E", align: A.TextAlignmentTypeValues.Center));
                break;

            case "table":
                tree.Append(BuildTable(nextId++, widget, x, y, width, height));
                break;

            default: // bar, line, pie — a snapshot of the SVG the browser already rendered
                var imageBytes = TryDecodeImage(widget.Image);
                if (imageBytes is not null)
                    tree.Append(BuildPicture(slidePart, nextId++, imageBytes, x, y, width, height));
                else
                    tree.Append(TextBox(nextId++, "تعذّر تضمين الرسم البياني.", x, y, width, height, 14,
                        colorHex: "898781", align: A.TextAlignmentTypeValues.Center));
                break;
        }
    }

    private static P.GraphicFrame BuildTable(uint id, PptxWidgetInput widget, long x, long y, long cx, long cy)
    {
        var columns = widget.Columns is { Count: > 0 } ? widget.Columns : new List<string> { "" };
        const int maxRows = 18; // keep every row readable on one slide; note the rest below
        var rows = widget.Rows ?? new List<List<string>>();
        var shown = rows.Take(maxRows).ToList();

        var table = new A.Table(new A.TableProperties { FirstRow = true, BandRow = true });
        var grid = new A.TableGrid();
        var colWidth = cx / Math.Max(columns.Count, 1);
        foreach (var _ in columns) grid.Append(new A.GridColumn { Width = colWidth });
        table.Append(grid);

        var rowHeight = Math.Max(cy / Math.Max(shown.Count + 1, 2), 228600); // >= ~0.25in
        table.Append(BuildTableRow(columns, rowHeight, header: true));
        foreach (var row in shown)
            table.Append(BuildTableRow(row, rowHeight, header: false));
        if (rows.Count > maxRows)
            table.Append(BuildTableRow(new List<string> { $"+ {rows.Count - maxRows} صف إضافي…" }
                .Concat(Enumerable.Repeat("", Math.Max(columns.Count - 1, 0))).ToList(), rowHeight, header: false));

        var graphicFrame = new P.GraphicFrame(
            new P.NonVisualGraphicFrameProperties(
                new P.NonVisualDrawingProperties { Id = id, Name = $"Table {id}" },
                new P.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks()),
                new P.ApplicationNonVisualDrawingProperties()),
            new P.Transform(new A.Offset { X = x, Y = y }, new A.Extents { Cx = cx, Cy = cy }),
            new A.Graphic(new A.GraphicData(table) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/table" }));
        return graphicFrame;
    }

    private static A.TableRow BuildTableRow(List<string> cells, long height, bool header)
    {
        var row = new A.TableRow { Height = height };
        foreach (var cellText in cells)
        {
            var paragraphProperties = new A.ParagraphProperties
            {
                Alignment = A.TextAlignmentTypeValues.Right, RightToLeft = BooleanValue.FromBoolean(true),
            };
            var runProperties = new A.RunProperties { Language = "ar-SA", FontSize = 1200, Bold = header };
            var cell = new A.TableCell(
                new A.TextBody(
                    new A.BodyProperties(), new A.ListStyle(),
                    new A.Paragraph(paragraphProperties, new A.Run(runProperties, new A.Text(cellText ?? "")))),
                new A.TableCellProperties());
            row.Append(cell);
        }
        return row;
    }

    private static P.Picture BuildPicture(SlidePart slidePart, uint id, byte[] pngBytes, long x, long y, long maxCx, long maxCy)
    {
        var imagePart = slidePart.AddImagePart(ImagePartType.Png);
        using (var imgStream = new MemoryStream(pngBytes)) imagePart.FeedData(imgStream);
        var relId = slidePart.GetIdOfPart(imagePart);

        var (cx, cy) = FitWithin(GetPngDimensions(pngBytes), maxCx, maxCy);
        var offsetX = x + (maxCx - cx) / 2;
        var offsetY = y + (maxCy - cy) / 2;

        return new P.Picture(
            new P.NonVisualPictureProperties(
                new P.NonVisualDrawingProperties { Id = id, Name = $"Picture {id}" },
                new P.NonVisualPictureDrawingProperties(new A.PictureLocks { NoChangeAspect = true }),
                new P.ApplicationNonVisualDrawingProperties()),
            new P.BlipFill(new A.Blip { Embed = relId }, new A.Stretch(new A.FillRectangle())),
            new P.ShapeProperties(
                new A.Transform2D(new A.Offset { X = offsetX, Y = offsetY }, new A.Extents { Cx = cx, Cy = cy }),
                new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }));
    }

    private static (long Cx, long Cy) FitWithin((int Width, int Height)? pixelSize, long maxCx, long maxCy)
    {
        if (pixelSize is not { Width: > 0, Height: > 0 } size) return (maxCx, maxCy);
        var scale = Math.Min((double)maxCx / size.Width, (double)maxCy / size.Height);
        return ((long)(size.Width * scale), (long)(size.Height * scale));
    }

    /// <summary>Reads width/height straight out of the PNG's IHDR chunk — no imaging library needed.</summary>
    private static (int Width, int Height)? GetPngDimensions(byte[] png)
    {
        if (png.Length < 24) return null;
        // Bytes 0-7: PNG signature. Bytes 16-19 / 20-23: width/height, big-endian, in the IHDR chunk.
        int Width() => (png[16] << 24) | (png[17] << 16) | (png[18] << 8) | png[19];
        int Height() => (png[20] << 24) | (png[21] << 16) | (png[22] << 8) | png[23];
        return (Width(), Height());
    }

    private static byte[]? TryDecodeImage(string? dataUrlOrBase64)
    {
        if (string.IsNullOrWhiteSpace(dataUrlOrBase64)) return null;
        var comma = dataUrlOrBase64.IndexOf(',');
        var base64 = dataUrlOrBase64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma > -1
            ? dataUrlOrBase64[(comma + 1)..]
            : dataUrlOrBase64;
        try { return Convert.FromBase64String(base64); }
        catch (FormatException) { return null; }
    }
}
