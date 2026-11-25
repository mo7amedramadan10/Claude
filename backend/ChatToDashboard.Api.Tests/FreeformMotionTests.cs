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
}
