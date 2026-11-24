using System.Text;
using ChatToDashboard.Api.Freeform;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>مرفقات محادثة تصميم حر: كتلة المرفقات في رسالة الموديل، استخراج النص، ونوع الصورة من التوقيع.</summary>
public class FreeformAttachmentTests
{
    [Fact]
    public void Build_ListsImagesDataAndDocs_AndRejectsForeignImageUrls()
    {
        var text = FreeformAttachmentText.Build(new List<FreeformAttachment>
        {
            new() { Kind = "image", Name = "شعار.png", Url = "/api/freeform/assets/0123456789abcdef01234567", Width = 200, Height = 80 },
            new() { Kind = "image", Name = "خارجي", Url = "https://evil.example/x.png" },
            new() { Kind = "data", Name = "مبيعات.xlsx", Table = "proj_1.sales" },
            new() { Kind = "doc", Name = "تقرير.pdf", Text = "الإيرادات 5 ملايين" },
        });
        Assert.Contains("/api/freeform/assets/0123456789abcdef01234567", text);
        Assert.Contains("(200×80)", text);
        Assert.DoesNotContain("evil.example", text);
        Assert.Contains("proj_1.sales", text);
        Assert.Contains("الإيرادات 5 ملايين", text);
        Assert.Equal("", FreeformAttachmentText.Build(null));
        Assert.Equal("", FreeformAttachmentText.Build(new List<FreeformAttachment> { new() { Kind = "doc", Name = "x", Text = " " } }));
    }

    [Fact]
    public void Build_TruncatesAndCapsTotalLength()
    {
        var big = new string('أ', 40_000);
        var text = FreeformAttachmentText.Build(new List<FreeformAttachment> { new() { Kind = "doc", Name = "1", Text = big }, new() { Kind = "doc", Name = "2", Text = big }, new() { Kind = "doc", Name = "3", Text = big } });
        Assert.Contains("(اقتُطع)", text);
        Assert.Contains("«3»: (أُهمل", text);
        Assert.True(text.Length < 70_000);
    }

    [Fact]
    public void ExtractText_TxtHtmlAndDocx()
    {
        Assert.Equal("سطر أول", FreeformAttachmentText.ExtractText("a.txt", Encoding.UTF8.GetBytes("سطر أول\n")));
        var html = FreeformAttachmentText.ExtractText("p.html", Encoding.UTF8.GetBytes("<html><body><h1>الأهداف</h1><p>رفع رضا العملاء إلى 90٪</p></body></html>"));
        Assert.Contains("رفع رضا العملاء", html);

        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
        {
            var main = doc.AddMainDocumentPart();
            main.Document = new W.Document(new W.Body(
                new W.Paragraph(new W.Run(new W.Text("خطة الربع الثالث"))),
                new W.Table(new W.TableRow(new W.TableCell(new W.Paragraph(new W.Run(new W.Text("البند")))), new W.TableCell(new W.Paragraph(new W.Run(new W.Text("القيمة"))))))));
        }
        var docx = FreeformAttachmentText.ExtractText("plan.docx", ms.ToArray());
        Assert.Contains("خطة الربع الثالث", docx);
        Assert.Contains("البند | القيمة", docx);
    }

    [Fact]
    public void ImageType_BySignatureOnly()
    {
        Assert.Equal("image/png", FreeformAttachmentText.ImageType(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0 }));
        Assert.Equal("image/jpeg", FreeformAttachmentText.ImageType(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }));
        Assert.Null(FreeformAttachmentText.ImageType(Encoding.UTF8.GetBytes("<svg onload=alert(1)>")));
        Assert.Null(FreeformAttachmentText.ImageType(Encoding.UTF8.GetBytes("plain")));
    }
}
