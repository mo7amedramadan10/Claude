import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:go_router/go_router.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../models/account_menu_item.dart';

class AccountMenuGroupCard extends StatelessWidget {
  const AccountMenuGroupCard({super.key, required this.group});

  final AccountMenuGroup group;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.fromLTRB(4, 0, 4, 9),
          child: Text(
            group.title,
            style: AppTextStyles.titleMedium.copyWith(
              fontSize: 13,
              fontWeight: FontWeight.w700,
              color: AppColors.grey600,
            ),
          ),
        ),
        Container(
          decoration: BoxDecoration(
            color: isDark ? AppColors.cardDark : Colors.white,
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color: isDark ? AppColors.navyLight : AppColors.grey100,
              width: 1,
            ),
          ),
          clipBehavior: Clip.hardEdge,
          child: Column(
            children: [
              for (var i = 0; i < group.items.length; i++)
                _MenuRow(
                  item: group.items[i],
                  isLast: i == group.items.length - 1,
                ),
            ],
          ),
        ),
        const SizedBox(height: 20),
      ],
    );
  }
}

class _MenuRow extends StatelessWidget {
  const _MenuRow({required this.item, required this.isLast});

  final AccountMenuItem item;
  final bool isLast;

  // Icon tile tint palettes (background, foreground)
  static const _tints = {
    MenuTint.green: (Color(0xFFF0F7F4), AppColors.primary),
    MenuTint.navy: (Color(0xFFEEF1F6), AppColors.navyLight),
    MenuTint.amber: (Color(0xFFFDF4E3), AppColors.noticeIcon),
    MenuTint.red: (Color(0xFFFCEEEF), AppColors.error),
  };

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final (tileBg, tileFg) = _tints[item.tint]!;

    return InkWell(
      onTap: item.route == null
          ? () {}
          : item.isTabRoute
              ? () => context.go(item.route!)
              : () => context.push(item.route!),
      child: Container(
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          border: isLast
              ? null
              : Border(
                  bottom: BorderSide(
                    color: isDark
                        ? AppColors.navyLight
                        : const Color(0xFFF1F3F6),
                    width: 1,
                  ),
                ),
        ),
        child: Row(
          children: [
            Container(
              width: 36,
              height: 36,
              decoration: BoxDecoration(
                color: tileBg,
                borderRadius: BorderRadius.circular(11),
              ),
              child: Icon(item.icon, size: 20, color: tileFg),
            ),
            const SizedBox(width: 13),
            Expanded(
              child: Text(
                item.label,
                style: AppTextStyles.titleLarge.copyWith(
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                  color: item.isDanger
                      ? AppColors.error
                      : (isDark ? Colors.white : AppColors.navy),
                ),
              ),
            ),
            if (item.value != null) ...[
              Text(
                item.value!,
                style: AppTextStyles.labelLarge.copyWith(
                  fontSize: 12.5,
                  fontWeight: FontWeight.w600,
                  color: AppColors.grey600,
                ),
              ),
              const SizedBox(width: 8),
            ],
            if (!item.isDanger)
              const Icon(TablerIcons.chevron_left,
                  size: 20, color: Color(0xFFC2C9D3)),
          ],
        ),
      ),
    );
  }
}
