import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';

class SearchTopBar extends StatelessWidget {
  const SearchTopBar({
    super.key,
    required this.query,
    required this.onBack,
    required this.onClear,
  });

  final String query;
  final VoidCallback onBack;
  final VoidCallback onClear;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Row(children: [
      GestureDetector(
        onTap: onBack,
        child: Container(
          width: 38,
          height: 38,
          decoration: BoxDecoration(
            color: isDark ? AppColors.cardDark : AppColors.grey50,
            borderRadius: BorderRadius.circular(11),
          ),
          child: Icon(TablerIcons.arrow_right,
              size: 21, color: isDark ? Colors.white : AppColors.navy),
        ),
      ),
      const SizedBox(width: 10),
      Expanded(
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 13, vertical: 10),
          decoration: BoxDecoration(
            color: isDark ? AppColors.cardDark : AppColors.grey50,
            borderRadius: BorderRadius.circular(14),
            border: Border.all(color: AppColors.grey200, width: 1),
          ),
          child: Row(children: [
            const Icon(TablerIcons.search,
                size: 18, color: AppColors.grey600),
            const SizedBox(width: 9),
            Expanded(
              child: Text(
                query,
                style: AppTextStyles.titleMedium.copyWith(
                  fontSize: 13.5,
                  color: isDark ? Colors.white : AppColors.navy,
                ),
              ),
            ),
            GestureDetector(
              onTap: onClear,
              child: const Icon(TablerIcons.x,
                  size: 17, color: AppColors.grey400),
            ),
          ]),
        ),
      ),
    ]);
  }
}
