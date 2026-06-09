import 'package:flutter/material.dart';

import '../../theme/app_colors.dart';
import '../../theme/app_text_styles.dart';

class SectionHeader extends StatelessWidget {
  const SectionHeader({
    super.key,
    required this.title,
    this.onViewAll,
  });

  final Widget title;
  final VoidCallback? onViewAll;

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.center,
      children: [
        DefaultTextStyle(
          style: AppTextStyles.headlineMedium.copyWith(
            color: Theme.of(context).colorScheme.onSurface,
          ),
          child: title,
        ),
        const Spacer(),
        if (onViewAll != null)
          GestureDetector(
            onTap: onViewAll,
            child: Text(
              'عرض الكل ←',
              style: AppTextStyles.bodySmall.copyWith(
                color: AppColors.primary,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
      ],
    );
  }
}
