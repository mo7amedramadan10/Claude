import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/theme/app_colors.dart';
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

    Future<void> handleLogout() async {
      await ref.read(authProvider.notifier).signOut();
      if (context.mounted) context.go(AppRoutes.login);
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
