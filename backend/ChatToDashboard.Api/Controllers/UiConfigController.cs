using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatToDashboard.Api.Controllers;

/// <summary>
/// إعدادات واجهة تُحقن في الصفحة وقت التشغيل كسكربت صغير (قبل تحميل مكتبات الرسم) بدل كتابتها
/// في ملفات ثابتة: حاليًا مفتاح ترخيص DevExtreme من <c>DevExtreme:LicenseKey</c> (user-secrets أو
/// متغيّر بيئة). المفتاح ليس سرًا تشغيليًا (يُرسل للمتصفح بطبيعته) لكنه يبقى خارج المستودع.
/// </summary>
[ApiController]
[Route("api/ui-config.js")]
public class UiConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public UiConfigController(IConfiguration configuration) => _configuration = configuration;

    // الصفحة تحمّل هذا السكربت قبل تسجيل الدخول (index.html يُفتح مجهولًا ثم يسجّل المستخدم داخله بلا
    // إعادة تحميل)، فلا يخضع لسياسة المصادقة العامة — ولا يحمل شيئًا سريًا.
    [AllowAnonymous]
    [HttpGet]
    [ResponseCache(NoStore = true)]
    public IActionResult Get()
    {
        var key = _configuration["DevExtreme:LicenseKey"]?.Trim() ?? "";
        var js = "window.JEEM_UI_CONFIG = " + System.Text.Json.JsonSerializer.Serialize(new { devExtremeLicenseKey = key }) + ";";
        return Content(js, "application/javascript; charset=utf-8");
    }
}
