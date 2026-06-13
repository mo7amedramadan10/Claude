import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../auth/providers/auth_provider.dart';
import '../data/account_mock_data.dart';
import 'widgets/account_header.dart';
import 'widgets/account_menu_group.dart';
import 'widgets/account_stats_card.dart';

class AccountScreen extends ConsumerWidget {
  const AccountScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final isLoggedIn = ref.watch(authProvider).isLoggedIn;

    if (!isLoggedIn) {
      return _GuestView(isDark: isDark);
    }

    Future<void> handleLogout() async {
      await ref.read(authProvider.notifier).signOut();
      if (context.mounted) context.go(AppRoutes.home);
    }

    return Scaffold(
      backgroundColor:
          isDark ? AppColors.backgroundDark : AppColors.backgroundLight,
      body: SingleChildScrollView(
        padding: EdgeInsets.zero,
        child: Column(
          children: [
            Container(
              color: AppColors.navy,
              child: const SafeArea(
                bottom: false,
                child: AccountHeader(),
              ),
            ),
            Transform.translate(
              offset: const Offset(0, -30),
              child: Column(
                children: [
                  const AccountStatsCard(),
                  Padding(
                    padding: const EdgeInsets.fromLTRB(16, 18, 16, 0),
                    child: Column(
                      children: [
                        for (final group in AccountMockData.groups)
                          AccountMenuGroupCard(
                            group: group,
                            onDangerTap: handleLogout,
                          ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// شاشة الضيف — تظهر عند الضغط على "حسابي" بدون تسجيل دخول
class _GuestView extends StatelessWidget {
  const _GuestView({required this.isDark});

  final bool isDark;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor:
          isDark ? AppColors.backgroundDark : AppColors.backgroundLight,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 28),
          child: Column(
            children: [
              const Spacer(flex: 2),

              // ── أيقونة ───────────────────────────────────────────────
              Container(
                width: 90,
                height: 90,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  color: AppColors.primary.withValues(alpha: 0.1),
                ),
                child: const Icon(
                  TablerIcons.user_circle,
                  size: 48,
                  color: AppColors.primary,
                ),
              ),
              const SizedBox(height: 24),

              // ── نص ترحيبي ─────────────────────────────────────────────
              Text(
                'مرحباً بك في بيكيا',
                textAlign: TextAlign.center,
                style: AppTextStyles.headlineLarge.copyWith(fontSize: 22),
              ),
              const SizedBox(height: 10),
              Text(
                'سجّل دخولك للوصول إلى حسابك،\nإعلاناتك، ورسائلك.',
                textAlign: TextAlign.center,
                style: AppTextStyles.bodyLarge.copyWith(
                  color: AppColors.grey500,
                  height: 1.6,
                ),
              ),

              const Spacer(flex: 2),

              // ── زرار تسجيل الدخول ─────────────────────────────────────
              SizedBox(
                width: double.infinity,
                height: 52,
                child: FilledButton(
                  onPressed: () => context.push(AppRoutes.login),
                  style: FilledButton.styleFrom(
                    backgroundColor: AppColors.primary,
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14)),
                  ),
                  child: Text(
                    'تسجيل الدخول',
                    style: AppTextStyles.headlineMedium
                        .copyWith(color: Colors.white, fontSize: 15.5),
                  ),
                ),
              ),
              const SizedBox(height: 12),

              // ── زرار إنشاء حساب ──────────────────────────────────────
              SizedBox(
                width: double.infinity,
                height: 52,
                child: OutlinedButton(
                  onPressed: () => context.push(AppRoutes.register),
                  style: OutlinedButton.styleFrom(
                    foregroundColor: AppColors.primary,
                    side: const BorderSide(color: AppColors.primary),
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14)),
                  ),
                  child: Text(
                    'إنشاء حساب جديد',
                    style: AppTextStyles.headlineMedium
                        .copyWith(color: AppColors.primary, fontSize: 15.5),
                  ),
                ),
              ),

              const Spacer(),
            ],
          ),
        ),
      ),
    );
  }
}
