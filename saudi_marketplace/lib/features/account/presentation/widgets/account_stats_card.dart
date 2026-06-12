import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../models/account_menu_item.dart';

class AccountStatsCard extends StatelessWidget {
  const AccountStatsCard({super.key});

  static const _stats = [
    AccountStat(value: '٠', label: 'إعلاناتي'),
    AccountStat(value: '٠', label: 'صفقة مكتملة'),
    AccountStat(value: '—', label: 'تقييمي', showStar: true),
  ];

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 16),
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 16),
      decoration: BoxDecoration(
        color: isDark ? AppColors.cardDark : Colors.white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(
          color: isDark ? AppColors.navyLight : AppColors.grey100,
          width: 1,
        ),
        boxShadow: [
          BoxShadow(
            color: AppColors.navy.withOpacity(0.08),
            blurRadius: 24,
            offset: const Offset(0, 8),
          ),
        ],
      ),
      child: IntrinsicHeight(
        child: Row(
          children: [
            for (var i = 0; i < _stats.length; i++) ...[
              if (i > 0)
                const VerticalDivider(
                    width: 1, thickness: 1, color: AppColors.grey300),
              Expanded(
                  child: _StatColumn(stat: _stats[i], isDark: isDark)),
            ],
          ],
        ),
      ),
    );
  }
}

class _StatColumn extends StatelessWidget {
  const _StatColumn({required this.stat, required this.isDark});

  final AccountStat stat;
  final bool isDark;

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              stat.value,
              style: AppTextStyles.headlineLarge.copyWith(
                fontSize: 19,
                color: isDark ? Colors.white : AppColors.navy,
              ),
            ),
            if (stat.showStar && stat.value != '—') ...[
              const SizedBox(width: 4),
              const Icon(TablerIcons.star_filled,
                  size: 15, color: AppColors.warning),
            ],
          ],
        ),
        const SizedBox(height: 4),
        Text(
          stat.label,
          style: AppTextStyles.labelLarge.copyWith(
            fontSize: 11.5,
            fontWeight: FontWeight.w600,
            color: AppColors.grey600,
          ),
        ),
      ],
    );
  }
}
