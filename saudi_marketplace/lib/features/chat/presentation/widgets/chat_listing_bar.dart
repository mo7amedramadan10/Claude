import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';

class ChatListingBar extends StatelessWidget {
  const ChatListingBar({
    super.key,
    required this.title,
    required this.price,
    required this.imageUrl,
    required this.onTap,
  });

  final String title;
  final String price;
  final String imageUrl;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        decoration: BoxDecoration(
          color: isDark ? AppColors.surfaceDark : AppColors.surfaceLight,
          border: const Border(
            bottom: BorderSide(color: AppColors.grey100, width: 1),
          ),
        ),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 9),
        child: Row(children: [
          ClipRRect(
            borderRadius: BorderRadius.circular(11),
            child: CachedNetworkImage(
              imageUrl: imageUrl,
              width: 46,
              height: 46,
              fit: BoxFit.cover,
              placeholder: (_, __) => Container(
                  width: 46, height: 46, color: AppColors.grey100),
              errorWidget: (_, __, ___) => Container(
                  width: 46, height: 46, color: AppColors.grey100),
            ),
          ),
          const SizedBox(width: 11),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title,
                    style: AppTextStyles.titleMedium.copyWith(
                      fontWeight: FontWeight.w700,
                      color: isDark ? Colors.white : AppColors.navy,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis),
                const SizedBox(height: 2),
                Text('$price ريال',
                    style: AppTextStyles.titleMedium.copyWith(
                        fontWeight: FontWeight.w700,
                        color: AppColors.primaryDark)),
              ],
            ),
          ),
          Row(children: [
            Text('عرض الإعلان',
                style: AppTextStyles.labelLarge.copyWith(
                    fontSize: 11.5,
                    color: AppColors.primaryDark,
                    fontWeight: FontWeight.w700)),
            const Icon(TablerIcons.chevron_left,
                size: 15, color: AppColors.primaryDark),
          ]),
        ]),
      ),
    );
  }
}
