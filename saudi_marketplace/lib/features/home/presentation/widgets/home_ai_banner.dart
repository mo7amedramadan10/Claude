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
        margin: const EdgeInsets.fromLTRB(14, 12, 14, 0),
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          gradient: const LinearGradient(
            begin: Alignment.topRight,
            end: Alignment.bottomLeft,
            colors: [AppColors.navy, AppColors.navyLight],
          ),
          borderRadius: BorderRadius.circular(16),
        ),
        child: Row(children: [
          _AiOrb(),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('ماذا تبحث عنه اليوم؟',
                    style: AppTextStyles.titleMedium.copyWith(color: Colors.white)),
                const SizedBox(height: 3),
                Text(
                  'صف ما تريد وسيساعدك الذكاء الاصطناعي في العثور على أفضل العروض.',
                  style: AppTextStyles.labelSmall.copyWith(
                    color: const Color(0xFFA8C0D6),
                    height: 1.4,
                  ),
                ),
                const SizedBox(height: 8),
                _StartButton(),
              ],
            ),
          ),
        ]),
      ),
    );
  }
}

class _AiOrb extends StatelessWidget {
  @override
  Widget build(BuildContext context) => Container(
        width: 44, height: 44,
        decoration: BoxDecoration(
          color: AppColors.primary.withValues(alpha: 0.25),
          shape: BoxShape.circle,
        ),
        child: const Icon(TablerIcons.sparkles, size: 22, color: AppColors.primary),
      );
}

class _StartButton extends StatelessWidget {
  @override
  Widget build(BuildContext context) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 11, vertical: 7),
        decoration: BoxDecoration(
          color: AppColors.primary,
          borderRadius: BorderRadius.circular(10),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          const Text('✦', style: TextStyle(color: Colors.white, fontSize: 10)),
          const SizedBox(width: 4),
          Text('ابدأ البحث الذكي',
              style: AppTextStyles.labelSmall
                  .copyWith(color: Colors.white, fontWeight: FontWeight.w600)),
        ]),
      );
}
