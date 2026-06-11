// ──────────────────────────────────────────────────────────────
// بعد إنشاء مشروع Supabase، ضع القيم من:
// Settings → API → Project URL  +  anon public key
//
// ملاحظة: طالما القيم placeholders، يعمل التطبيق تلقائياً
// على البيانات التجريبية (mock data) دون أي إعداد.
// ──────────────────────────────────────────────────────────────
abstract final class Env {
  static const String supabaseUrl  = 'https://YOUR_PROJECT_ID.supabase.co';
  static const String supabaseAnon = 'YOUR_ANON_PUBLIC_KEY';

  /// true فقط بعد تعبئة قيم المشروع الحقيقية
  static bool get isConfigured =>
      !supabaseUrl.contains('YOUR_PROJECT_ID') &&
      !supabaseAnon.contains('YOUR_ANON');
}
