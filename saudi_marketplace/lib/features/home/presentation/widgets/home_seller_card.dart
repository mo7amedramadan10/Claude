import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:shimmer/shimmer.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../models/seller_model.dart';

class HomeSellerCard extends StatelessWidget {
  const HomeSellerCard({super.key, required this.seller, this.onTap});

  final SellerModel seller;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 134,
        padding: const EdgeInsets.symmetric(horizontal: 11, vertical: 14),
        decoration: BoxDecoration(
          color: isDark ? AppColors.cardDark : AppColors.surfaceLight,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: isDark ? AppColors.navyLight : AppColors.grey100,
            width: 1,
          ),
        ),
        child: Column(children: [
          _Avatar(url: seller.avatarUrl),
          const SizedBox(height: 8),
          Text(seller.name,
              style: AppTextStyles.titleSmall.copyWith(
                fontWeight: FontWeight.w700,
                color: isDark ? Colors.white : AppColors.navy,
              ),
              textAlign: TextAlign.center,
              maxLines: 1,
              overflow: TextOverflow.ellipsis),
          const SizedBox(height: 4),
          Row(mainAxisAlignment: MainAxisAlignment.center, children: [
            const Icon(TablerIcons.star_filled,
                size: 12, color: AppColors.warning),
            const SizedBox(width: 3),
            Text(
              '${seller.rating} · ${seller.completedDeals} صفقة',
              style:
                  AppTextStyles.labelLarge.copyWith(color: AppColors.grey600),
            ),
          ]),
          if (seller.isNafadhVerified) ...[
            const SizedBox(height: 7),
            const _NafadhBadge(),
          ],
        ]),
      ),
    );
  }
}

class _Avatar extends StatelessWidget {
  const _Avatar({required this.url});

  final String url;

  @override
  Widget build(BuildContext context) => Container(
        width: 52,
        height: 52,
        decoration: BoxDecoration(
          shape: BoxShape.circle,
          border: Border.all(color: AppColors.primary, width: 2.5),
        ),
        child: ClipOval(
          child: CachedNetworkImage(
            imageUrl: url,
            fit: BoxFit.cover,
            placeholder: (_, __) => Shimmer.fromColors(
              baseColor: AppColors.grey100,
              highlightColor: AppColors.grey50,
              child: Container(color: AppColors.grey100),
            ),
            errorWidget: (_, __, ___) => Container(
              color: AppColors.primaryLight,
              child: const Icon(TablerIcons.user,
                  size: 24, color: AppColors.primary),
            ),
          ),
        ),
      );
}

class _NafadhBadge extends StatelessWidget {
  const _NafadhBadge();

  @override
  Widget build(BuildContext context) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 4),
        decoration: BoxDecoration(
          color: AppColors.primaryLight,
          borderRadius: BorderRadius.circular(20),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          const Icon(TablerIcons.shield_check_filled,
              size: 12, color: AppColors.primaryDark),
          const SizedBox(width: 4),
          Text('نفاذ وطني',
              style: AppTextStyles.labelSmall.copyWith(
                  color: AppColors.primaryDark, fontWeight: FontWeight.w700)),
        ]),
      );
}
