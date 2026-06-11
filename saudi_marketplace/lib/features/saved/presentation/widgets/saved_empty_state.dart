import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';

class SavedEmptyState extends StatelessWidget {
  const SavedEmptyState({super.key, required this.onBrowse});

  final VoidCallback onBrowse;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 96,
              height: 96,
              decoration: const BoxDecoration(
                color: AppColors.primaryTint,
                shape: BoxShape.circle,
              ),
              child: const Icon(
                TablerIcons.heart,
                size: 46,
                color: AppColors.primary,
              ),
            ),
            const SizedBox(height: 18),
            Text(
              'لا توجد إعلانات محفوظة',
              style: AppTextStyles.titleLarge.copyWith(
                fontSize: 17,
                fontWeight: FontWeight.w700,
                color: AppColors.navy,
              ),
            ),
            const SizedBox(height: 7),
            Text(
              'احفظ الإعلانات التي تهمك بالضغط على رمز القلب، وستظهر هنا للرجوع إليها لاحقاً.',
              textAlign: TextAlign.center,
              style: AppTextStyles.bodySmall.copyWith(
                fontSize: 13,
                color: AppColors.grey600,
                height: 1.6,
              ),
            ),
            const SizedBox(height: 22),
            ElevatedButton(
              onPressed: onBrowse,
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primary,
                foregroundColor: Colors.white,
                padding:
                    const EdgeInsets.symmetric(horizontal: 26, vertical: 12),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(13),
                ),
                elevation: 6,
                shadowColor: AppColors.primary.withOpacity(0.30),
              ),
              child: Text(
                'تصفّح الإعلانات',
                style: AppTextStyles.labelLarge.copyWith(
                  fontSize: 14,
                  fontWeight: FontWeight.w700,
                  color: Colors.white,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
