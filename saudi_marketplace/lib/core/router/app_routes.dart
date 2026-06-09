abstract final class AppRoutes {
  // ─── Root ────────────────────────────────────────────────────────
  static const splash = '/';
  static const home = '/home';

  // ─── Auth ────────────────────────────────────────────────────────
  static const login = '/login';
  static const register = '/register';
  static const otpVerify = '/otp-verify';

  // ─── Shell tabs ──────────────────────────────────────────────────
  static const categories = '/categories';
  static const favorites = '/favorites';
  static const profile = '/profile';

  // ─── Product ─────────────────────────────────────────────────────
  static const productDetails = '/product/:id';
  static String productDetailsPath(String id) => '/product/$id';

  // ─── Search ──────────────────────────────────────────────────────
  static const search = '/search';

  // ─── Sell ────────────────────────────────────────────────────────
  static const sellItem = '/sell';

  // ─── Chat ────────────────────────────────────────────────────────
  static const chatList = '/chat';
  static const chatDetail = '/chat/:id';
  static String chatDetailPath(String id) => '/chat/$id';

  // ─── Notifications ───────────────────────────────────────────────
  static const notifications = '/notifications';
}
