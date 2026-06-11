import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';

class ChatHeader extends StatelessWidget {
  const ChatHeader({
    super.key,
    required this.sellerName,
    required this.avatarUrl,
    required this.onBack,
    this.isOnline = true,
    this.onCallTap,
  });

  final String sellerName;
  final String avatarUrl;
  final VoidCallback onBack;
  final bool isOnline;
  final VoidCallback? onCallTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Container(
      decoration: BoxDecoration(
        color: isDark ? AppColors.surfaceDark : AppColors.surfaceLight,
        border: const Border(
          bottom: BorderSide(color: AppColors.grey100, width: 1),
        ),
      ),
      padding: const EdgeInsets.fromLTRB(14, 6, 14, 10),
      child: Row(children: [
        GestureDetector(
          onTap: onBack,
          child: SizedBox(
            width: 36,
            height: 36,
            child: Icon(TablerIcons.arrow_right,
                size: 22, color: isDark ? Colors.white : AppColors.navy),
          ),
        ),
        const SizedBox(width: 10),
        Stack(children: [
          Container(
            width: 42,
            height: 42,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              border: Border.all(color: AppColors.primary, width: 2),
            ),
            child: ClipOval(
              child: CachedNetworkImage(
                imageUrl: avatarUrl,
                fit: BoxFit.cover,
                placeholder: (_, __) =>
                    Container(color: AppColors.grey100),
                errorWidget: (_, __, ___) => Container(
                  color: AppColors.primaryLight,
                  child: const Icon(TablerIcons.user,
                      size: 20, color: AppColors.primary),
                ),
              ),
            ),
          ),
          if (isOnline)
            Positioned(
              bottom: 0,
              left: 0,
              child: Container(
                width: 12,
                height: 12,
                decoration: BoxDecoration(
                  color: AppColors.online,
                  shape: BoxShape.circle,
                  border: Border.all(color: Colors.white, width: 2),
                ),
              ),
            ),
        ]),
        const SizedBox(width: 10),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(children: [
                Flexible(
                  child: Text(sellerName,
                      style: AppTextStyles.headlineMedium.copyWith(
                        color: isDark ? Colors.white : AppColors.navy,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis),
                ),
                const SizedBox(width: 5),
                const Icon(TablerIcons.rosette_discount_check_filled,
                    size: 15, color: AppColors.primary),
              ]),
              if (isOnline)
                Text('متصل الآن',
                    style: AppTextStyles.labelLarge.copyWith(
                        fontSize: 11.5,
                        color: const Color(0xFF22A152),
                        fontWeight: FontWeight.w600)),
            ],
          ),
        ),
        GestureDetector(
          onTap: onCallTap,
          child: Container(
            width: 38,
            height: 38,
            decoration: BoxDecoration(
              color: isDark ? AppColors.cardDark : AppColors.grey50,
              shape: BoxShape.circle,
            ),
            child: const Icon(TablerIcons.phone,
                size: 19, color: AppColors.primaryDark),
          ),
        ),
      ]),
    );
  }
}
