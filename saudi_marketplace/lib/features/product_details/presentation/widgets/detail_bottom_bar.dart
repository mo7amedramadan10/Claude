import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';

class DetailBottomBar extends StatelessWidget {
  const DetailBottomBar({
    super.key,
    required this.onChatTap,
    this.onCallTap,
  });

  final VoidCallback onChatTap;
  final VoidCallback? onCallTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Container(
      decoration: BoxDecoration(
        color: isDark ? AppColors.surfaceDark : AppColors.surfaceLight,
        border: const Border(
          top: BorderSide(color: AppColors.grey100, width: 1),
        ),
        boxShadow: const [
          BoxShadow(
            color: Color(0x0F0D1F3C),
            blurRadius: 20,
            offset: Offset(0, -6),
          ),
        ],
      ),
      padding: const EdgeInsets.fromLTRB(16, 12, 16, 12),
      child: SafeArea(
        top: false,
        child: Row(children: [
          GestureDetector(
            onTap: onCallTap,
            child: Container(
              width: 50,
              height: 50,
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(14),
                border: Border.all(color: AppColors.primary, width: 1.5),
              ),
              child: const Icon(TablerIcons.phone,
                  size: 23, color: AppColors.primaryDark),
            ),
          ),
          const SizedBox(width: 10),
          Expanded(
            child: GestureDetector(
              onTap: onChatTap,
              child: Container(
                height: 50,
                decoration: BoxDecoration(
                  color: AppColors.primary,
                  borderRadius: BorderRadius.circular(14),
                  boxShadow: const [
                    BoxShadow(
                      color: Color(0x521A7A4A),
                      blurRadius: 16,
                      offset: Offset(0, 6),
                    ),
                  ],
                ),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    const Icon(TablerIcons.message_circle,
                        size: 21, color: Colors.white),
                    const SizedBox(width: 8),
                    Text('تواصل مع البائع',
                        style: AppTextStyles.headlineMedium
                            .copyWith(color: Colors.white)),
                  ],
                ),
              ),
            ),
          ),
        ]),
      ),
    );
  }
}
