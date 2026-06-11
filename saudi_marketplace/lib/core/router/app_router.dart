import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../shared/widgets/placeholder_screen.dart';
import 'app_routes.dart';
import 'shell_scaffold.dart';
import '../../features/home/presentation/home_screen.dart';
import '../../features/categories/presentation/categories_screen.dart';
import '../../features/chat/presentation/chat_screen.dart';
import '../../features/product_details/presentation/listing_detail_screen.dart';
import '../../features/search/presentation/search_screen.dart';

part 'app_router.g.dart';

// Shell navigator keys — each tab keeps its own navigation stack
final _rootKey = GlobalKey<NavigatorState>(debugLabel: 'root');
final _homeKey = GlobalKey<NavigatorState>(debugLabel: 'home');
final _categoriesKey = GlobalKey<NavigatorState>(debugLabel: 'categories');
final _favoritesKey = GlobalKey<NavigatorState>(debugLabel: 'favorites');
final _profileKey = GlobalKey<NavigatorState>(debugLabel: 'profile');

@riverpod
GoRouter appRouter(Ref ref) {
  return GoRouter(
    navigatorKey: _rootKey,
    initialLocation: AppRoutes.home,
    debugLogDiagnostics: true,

    routes: [
      // ─── Auth routes (full-screen, outside shell) ─────────────────
      GoRoute(
        path: AppRoutes.login,
        builder: (_, __) => const PlaceholderScreen(title: 'تسجيل الدخول'),
      ),
      GoRoute(
        path: AppRoutes.register,
        builder: (_, __) => const PlaceholderScreen(title: 'إنشاء حساب'),
      ),
      GoRoute(
        path: AppRoutes.otpVerify,
        builder: (_, __) => const PlaceholderScreen(title: 'تحقق OTP'),
      ),

      // ─── Shell (bottom nav tabs) ──────────────────────────────────
      StatefulShellRoute.indexedStack(
        builder: (_, __, navigationShell) =>
            ShellScaffold(navigationShell: navigationShell),
        branches: [
          StatefulShellBranch(
            navigatorKey: _homeKey,
            routes: [
              GoRoute(
                path: AppRoutes.home,
                builder: (_, __) => const HomeScreen(),
              ),
            ],
          ),
          StatefulShellBranch(
            navigatorKey: _categoriesKey,
            routes: [
              GoRoute(
                path: AppRoutes.categories,
                builder: (_, __) => const CategoriesScreen(),
              ),
            ],
          ),
          StatefulShellBranch(
            navigatorKey: _favoritesKey,
            routes: [
              GoRoute(
                path: AppRoutes.favorites,
                builder: (_, __) =>
                    const PlaceholderScreen(title: 'المحفوظات'),
              ),
            ],
          ),
          StatefulShellBranch(
            navigatorKey: _profileKey,
            routes: [
              GoRoute(
                path: AppRoutes.profile,
                builder: (_, __) =>
                    const PlaceholderScreen(title: 'حسابي'),
              ),
            ],
          ),
        ],
      ),

      // ─── Full-screen routes (outside shell) ───────────────────────
      GoRoute(
        path: AppRoutes.productDetails,
        builder: (_, state) => ListingDetailScreen(
          listingId: state.pathParameters['id']!,
        ),
      ),
      GoRoute(
        path: AppRoutes.search,
        builder: (_, __) => const SearchScreen(),
      ),
      GoRoute(
        path: AppRoutes.sellItem,
        builder: (_, __) => const PlaceholderScreen(title: 'أضف إعلاناً'),
      ),
      GoRoute(
        path: AppRoutes.chatList,
        builder: (_, __) => const PlaceholderScreen(title: 'المحادثات'),
      ),
      GoRoute(
        path: AppRoutes.chatDetail,
        builder: (_, state) => ChatScreen(
          chatId: state.pathParameters['id']!,
        ),
      ),
      GoRoute(
        path: AppRoutes.notifications,
        builder: (_, __) => const PlaceholderScreen(title: 'الإشعارات'),
      ),
    ],

    errorBuilder: (context, state) => Scaffold(
      body: Center(child: Text('خطأ: ${state.error}')),
    ),
  );
}
