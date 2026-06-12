// ──────────────────────────────────────────────────────────────
// جلسة تجريبية في الذاكرة — تُستخدم فقط قبل تهيئة Supabase
// (Env.isConfigured == false). تُفقد عند إغلاق التطبيق.
// ──────────────────────────────────────────────────────────────
abstract final class MockSession {
  static String? userId;
  static String? username;
  static String? displayName;

  static bool get isLoggedIn => userId != null;

  static void start({
    required String id,
    required String name,
    String? display,
  }) {
    userId = id;
    username = name;
    displayName = display ?? name;
  }

  static void clear() {
    userId = null;
    username = null;
    displayName = null;
  }
}
