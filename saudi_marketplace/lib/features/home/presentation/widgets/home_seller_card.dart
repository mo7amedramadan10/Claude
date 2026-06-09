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
        width: 130,
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 12),
        decoration: BoxDecoration(
          color: isDark ? AppColors.cardDark : AppColors.surfaceLight,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: isDark ? AppColors.navyLight : AppColors.grey100,
            width: 0.5,
          ),
        ),
        child: Column(children: [
          _Avatar(url: seller.avatarUrl),
          const SizedBox(height: 7),
          Text(seller.name,
              style: AppTextStyles.titleSmall,
              textAlign: TextAlign.center,
              maxLines: 1,
              overflow: TextOverflow.ellipsis),
          const SizedBox(height: 3),
          Row(mainAxisAlignment: MainAxisAlignment.center, children: [
            Text('★★★★★',
                style: AppTextStyles.labelSmall
                    .copyWith(color: AppColors.warning)),
            const SizedBox(width: 3),
            Text(seller.rating.toString(), style: AppTextStyles.labelSmall),
          ]),
          const SizedBox(height: 3),
          Text('${seller.completedDeals} صفقة مكتملة',
              style: AppTextStyles.labelSmall, textAlign: TextAlign.center),
          if (seller.isNafadhVerified) ...[
            const SizedBox(height: 6),
            _NafadhBadge(),
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
        width: 50, height: 50,
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
              child: const Icon(TablerIcons.user, size: 24, color: AppColors.primary),
            ),
          ),
        ),
      );
}

class _NafadhBadge extends StatelessWidget {
  @override
  Widget build(BuildContext context) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 3),
        decoration: BoxDecoration(
          color: AppColors.primaryLight,
          borderRadius: BorderRadius.circular(20),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          const Icon(TablerIcons.id, size: 11, color: AppColors.primary),
          const SizedBox(width: 3),
          Text('نفاذ وطني',
              style: AppTextStyles.labelSmall.copyWith(
                  color: AppColors.primary,
                  fontWeight: FontWeight.w600,
                  fontSize: 9)),
        ]),
      );
}
