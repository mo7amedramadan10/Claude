import 'package:flutter/material.dart';

import '../../../core/theme/app_colors.dart';
import '../data/account_mock_data.dart';
import 'widgets/account_header.dart';
import 'widgets/account_menu_group.dart';
import 'widgets/account_stats_card.dart';

class AccountScreen extends StatelessWidget {
  const AccountScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      // Navy hero extends behind the status bar
      backgroundColor: Theme.of(context).brightness == Brightness.dark
          ? AppColors.backgroundDark
          : AppColors.backgroundLight,
      body: SingleChildScrollView(
        padding: EdgeInsets.zero,
        child: Column(
          children: [
            // SafeArea inside the gradient so the navy fills the notch
            Container(
              color: AppColors.navy,
              child: const SafeArea(
                bottom: false,
                child: AccountHeader(),
              ),
            ),
            // Stats card overlaps the hero by 30px (hero has 46px
            // bottom padding reserving the space)
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
                          AccountMenuGroupCard(group: group),
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
