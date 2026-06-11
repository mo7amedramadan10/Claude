import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../../../core/utils/number_formatter.dart';
import '../../../home/models/seller_model.dart';

class DetailSellerCard extends StatelessWidget {
  const DetailSellerCard({super.key, required this.seller, this.onTap});

  final SellerModel seller;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: isDark ? AppColors.cardDark : AppColors.surfaceLight,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: isDark ? AppColors.navyLight : AppColors.grey300,
            width: 1,
          ),
        ),
        child: Row(children: [
          Container(
            width: 52,
            height: 52,
            decoration: BoxDecoration(
              shape: BoxShape.circle,
              border: Border.all(color: AppColors.primary, width: 2.5),
            ),
            child: ClipOval(
              child: CachedNetworkImage(
                imageUrl: seller.avatarUrl,
                fit: BoxFit.cover,
                placeholder: (_, __) =>
                    Container(color: AppColors.grey100),
                errorWidget: (_, __, ___) => Container(
                  color: AppColors.primaryLight,
                  child: const Icon(TablerIcons.user,
                      size: 24, color: AppColors.primary),
                ),
              ),
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(seller.name,
                    style: AppTextStyles.titleLarge.copyWith(
                      fontWeight: FontWeight.w700,
                      color: isDark ? Colors.white : AppColors.navy,
                    )),
                const SizedBox(height: 3),
                Row(children: [
                  const Icon(TablerIcons.star_filled,
                      size: 13, color: AppColors.warning),
                  const SizedBox(width: 3),
                  Text('${seller.rating}',
                      style: AppTextStyles.labelLarge
                          .copyWith(color: AppColors.grey600)),
                  const SizedBox(width: 10),
                  Text(
                    '${NumberFormatter.toArabicDigits(seller.completedDeals)} صفقة مكتملة',
                    style: AppTextStyles.labelLarge
                        .copyWith(color: AppColors.grey600),
                  ),
                ]),
                const SizedBox(height: 6),
                Container(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 8, vertical: 3),
                  decoration: BoxDecoration(
                    color: AppColors.primaryLight,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Row(mainAxisSize: MainAxisSize.min, children: [
                    const Icon(TablerIcons.shield_check_filled,
                        size: 12, color: AppColors.primaryDark),
                    const SizedBox(width: 4),
                    Text('موثّق عبر نفاذ وطني',
                        style: AppTextStyles.labelSmall.copyWith(
                            color: AppColors.primaryDark,
                            fontWeight: FontWeight.w700)),
                  ]),
                ),
              ],
            ),
          ),
          const Icon(TablerIcons.chevron_left,
              size: 22, color: AppColors.grey400),
        ]),
      ),
    );
  }
}
