import 'package:flutter/material.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../models/listing_detail_model.dart';

class DetailSpecsCard extends StatelessWidget {
  const DetailSpecsCard({super.key, required this.specs});

  final List<SpecItem> specs;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 14),
      decoration: BoxDecoration(
        color: isDark ? AppColors.cardDark : const Color(0xFFF7F8FA),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: isDark ? AppColors.navyLight : AppColors.grey300,
          width: 1,
        ),
      ),
      child: IntrinsicHeight(
        child: Row(
          children: List.generate(specs.length * 2 - 1, (i) {
            if (i.isOdd) {
              return const VerticalDivider(
                width: 1,
                thickness: 1,
                color: Color(0xFFE6EAEF),
              );
            }
            final spec = specs[i ~/ 2];
            return Expanded(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(spec.icon, size: 22, color: AppColors.primary),
                  const SizedBox(height: 6),
                  Text(spec.value,
                      style: AppTextStyles.titleMedium.copyWith(
                        fontWeight: FontWeight.w700,
                        color: isDark ? Colors.white : AppColors.navy,
                      ),
                      textAlign: TextAlign.center),
                  const SizedBox(height: 4),
                  Text(spec.label,
                      style: AppTextStyles.labelLarge
                          .copyWith(color: AppColors.grey600),
                      textAlign: TextAlign.center),
                ],
              ),
            );
          }),
        ),
      ),
    );
  }
}
