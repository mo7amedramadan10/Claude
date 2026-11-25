using ChatToDashboard.Api.Freeform;
using Xunit;

namespace ChatToDashboard.Api.Tests;

/// <summary>صفحات «حركة» بعقد محرّك المنصة jeem-motion.js: الحقن عند النشر، وتعليمات الموديل.</summary>
public class FreeformMotionTests
{
    private const string Page = "<html><body><div data-jeem-motion><section class=\"jm-scene\" data-slide>أ</section><section class=\"jm-scene\" data-slide>ب</section></div></body></html>";

    [Fact]
    public void MotionKit_InjectedOnce_OnlyForMotionPages()
    {
        var once = FreeformDataBlock.EnsureMotionKit(Page);
        Assert.Contains("<script src=\"/js/jeem-motion.js\"></script></body>", once);
        Assert.Equal(once, FreeformDataBlock.EnsureMotionKit(once));
        var plain = "<html><body><p>نص</p></body></html>";
        Assert.Equal(plain, FreeformDataBlock.EnsureMotionKit(plain));
        // مشاهد بلا سمة المسرح تكفي
        Assert.Contains("jeem-motion.js", FreeformDataBlock.EnsureMotionKit("<body><section class='jm-scene'>x</section></body>"));
    }

    [Fact]
    public void MotionInstructions_UsePlatformEngine_NotAllScenesAtOnce()
    {
        var k = FreeformBuilder.KindInstructions("motion");
        Assert.Contains("data-jeem-motion", k);
        Assert.Contains("jm-scene", k);
        Assert.Contains("data-in", k);
        Assert.DoesNotContain("اعرض الحالة النهائية", k);   // كانت تجعل الموديل يُظهر كل المشاهد معًا مع تقليل الحركة
        // المشاهد تعمل مع «الشريحة الحالية» في نطاق التعديل
        Assert.Equal(2, FreeformScope.FindSlides(Page).Count);
    }

    private static string Scene(string body, int i = 1) => $"<section class=\"jm-scene\" data-slide data-duration=\"5\" data-title=\"م{i}\"><h2 data-in=\"up\">{body}</h2></section>";
    private static string Stage(params string[] scenes) => "<html><head><style>.jm-scene{display:flex}</style></head><body><div data-jeem-motion>" + string.Join("", scenes) + "</div></body></html>";

    [Fact]
    public void MotionCheck_AcceptsContractPage()
    {
        Assert.Empty(FreeformMotionCheck.Validate(Stage(Scene("المشكلة"), Scene("الثمن", 2), Scene("الحل", 3))));
    }

    [Fact]
    public void MotionCheck_FlagsLegacyAndBadPages()
    {
        // الصفحة القديمة: مشاهد بمحرّك خاص بلا عقد
        var legacy = "<html><body><div class='scene'>أ</div><div class='scene'>ب</div><button>تشغيل</button><script>setInterval(()=>{},100)</script></body></html>";
        Assert.Contains("عقد محرّك الحركة", Assert.Single(FreeformMotionCheck.Validate(legacy)));
        Assert.Contains(FreeformMotionCheck.Validate(Stage(Scene("وحيد"))), p => p.Contains("مشهد فقط"));
        var longText = string.Join(" ", Enumerable.Repeat("كلمة", 60));
        Assert.Contains(FreeformMotionCheck.Validate(Stage(Scene("أ"), Scene(longText, 2))), p => p.Contains("المشهد 2"));
        var withEngine = Stage(Scene("أ"), Scene("ب", 2)).Replace("</body>", "<style>@keyframes x{}</style><script>requestAnimationFrame(f)</script><button>إيقاف</button></body>");
        var probs = FreeformMotionCheck.Validate(withEngine);
        Assert.Equal(3, probs.Count);
        // سكربتات المنصة وكتلة البيانات لا تُحسب
        var withKits = Stage(Scene("أ"), Scene("ب", 2)).Replace("</body>", "<script src=\"/js/jeem-motion.js\"></script><script type=\"application/json\" id=\"jeem-data\">{}</script></body>");
        Assert.Empty(FreeformMotionCheck.Validate(withKits));
        Assert.Contains("رُفضت آليًا", FreeformMotionCheck.RetryMessage("<html></html>", probs));
    }
}
