import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../models/saved_item.dart';

class SavedCard extends StatelessWidget {
  const SavedCard({
    super.key,
    required this.item,
    required this.onTap,
    required this.onRemove,
  });

  final SavedItem item;
  final VoidCallback onTap;
  final VoidCallback onRemove;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GestureDetector(
      onTap: onTap,
      child: Container(
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
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // ─── Image with overlays ──────────────────────────────
            SizedBox(
              height: 120,
              child: Stack(
                children: [
                  Positioned.fill(
                    child: CachedNetworkImage(
                      imageUrl: item.imageUrl,
                      fit: BoxFit.cover,
                      placeholder: (_, __) =>
                          Container(color: AppColors.grey100),
                      errorWidget: (_, __, ___) =>
                          Container(color: AppColors.grey100),
                    ),
                  ),
                  // Heart remove button — top-left (physical)
                  Positioned(
                    top: 8,
                    left: 8,
                    child: GestureDetector(
                      onTap: onRemove,
                      behavior: HitTestBehavior.opaque,
                      child: Container(
                        width: 30,
                        height: 30,
                        decoration: BoxDecoration(
                          color: Colors.white.withOpacity(0.94),
                          shape: BoxShape.circle,
                          boxShadow: [
                            BoxShadow(
                              color: Colors.black.withOpacity(0.10),
                              blurRadius: 6,
                              offset: const Offset(0, 2),
                            ),
                          ],
                        ),
                        child: const Icon(
                          TablerIcons.heart_filled,
                          size: 15,
                          color: AppColors.error,
                        ),
                      ),
                    ),
                  ),
                  // Verified badge — top-right (physical)
                  if (item.isVerified)
                    Positioned(
                      top: 8,
                      right: 8,
                      child: Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 7, vertical: 3),
                        decoration: BoxDecoration(
                          color: AppColors.primaryDark,
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            const Icon(
                              TablerIcons.rosette_discount_check,
                              size: 11,
                              color: Colors.white,
                            ),
                            const SizedBox(width: 3),
                            Text(
                              'موثق',
                              style: AppTextStyles.labelSmall.copyWith(
                                fontSize: 10,
                                fontWeight: FontWeight.w700,
                                color: Colors.white,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                ],
              ),
            ),

            // ─── Content ──────────────────────────────────────────
            Padding(
              padding: const EdgeInsets.fromLTRB(10, 10, 10, 11),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    item.title,
                    style: AppTextStyles.titleMedium.copyWith(
                      fontSize: 13,
                      fontWeight: FontWeight.w700,
                      color: isDark ? Colors.white : AppColors.navy,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 5),
                  RichText(
                    text: TextSpan(
                      children: [
                        TextSpan(
                          text: item.price,
                          style: AppTextStyles.titleLarge.copyWith(
                            fontSize: 15,
                            fontWeight: FontWeight.w700,
                            color: AppColors.primaryDark,
                          ),
                        ),
                        TextSpan(
                          text: ' ريال',
                          style: AppTextStyles.labelLarge.copyWith(
                            fontSize: 11,
                            fontWeight: FontWeight.w600,
                            color: AppColors.primaryDark,
                          ),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 7),
                  Row(
                    children: [
                      const Icon(TablerIcons.map_pin,
                          size: 12, color: AppColors.grey600),
                      const SizedBox(width: 3),
                      Text(
                        item.city,
                        style: AppTextStyles.labelLarge.copyWith(
                          fontSize: 11,
                          color: AppColors.grey600,
                        ),
                      ),
                      const Spacer(),
                      Text(
                        item.postedAt,
                        style: AppTextStyles.labelLarge.copyWith(
                          fontSize: 11,
                          color: AppColors.grey600,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
