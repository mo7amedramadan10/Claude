import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';

class HomeAiBanner extends StatelessWidget {
  const HomeAiBanner({super.key, required this.onTap});

  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        margin: const EdgeInsets.fromLTRB(14, 14, 14, 0),
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 15),
        decoration: BoxDecoration(
          gradient: const LinearGradient(
            begin: Alignment.topRight,
            end: Alignment.bottomLeft,
            colors: [AppColors.navy, AppColors.navyLight],
          ),
          borderRadius: BorderRadius.circular(18),
        ),
        child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Container(
            width: 46,
            height: 46,
            decoration: BoxDecoration(
              color: AppColors.primary.withValues(alpha: 0.28),
              borderRadius: BorderRadius.circular(14),
            ),
            child: const Icon(TablerIcons.sparkles,
                size: 24, color: AppColors.aiSparkle),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('ابحث بالذكاء الاصطناعي',
                    style: AppTextStyles.titleLarge
                        .copyWith(color: Colors.white)),
                const SizedBox(height: 4),
                Text(
                  'صِف ما تريد بكلماتك وسنجد لك أفضل العروض القريبة.',
                  style: AppTextStyles.bodySmall.copyWith(
                      color: AppColors.navyMuted, height: 1.5),
                ),
                const SizedBox(height: 10),
                Container(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 14, vertical: 8),
                  decoration: BoxDecoration(
                    color: AppColors.primary,
                    borderRadius: BorderRadius.circular(11),
                  ),
                  child: Row(mainAxisSize: MainAxisSize.min, children: [
                    const Icon(TablerIcons.wand,
                        size: 14, color: Colors.white),
                    const SizedBox(width: 6),
                    Text('ابدأ البحث الذكي',
                        style: AppTextStyles.bodySmall.copyWith(
                            color: Colors.white,
                            fontWeight: FontWeight.w700)),
                  ]),
                ),
              ],
            ),
          ),
        ]),
      ),
    );
  }
}
