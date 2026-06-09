import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:shimmer/shimmer.dart';

import '../../theme/app_colors.dart';
import '../../theme/app_text_styles.dart';
import '../../utils/number_formatter.dart';
import '../../../features/home/models/listing_model.dart';

class ListingCard extends StatelessWidget {
  const ListingCard({
    super.key,
    required this.listing,
    required this.onFavoriteTap,
    this.onTap,
    this.width = 155,
  });

  final ListingModel listing;
  final VoidCallback onFavoriteTap;
  final VoidCallback? onTap;
  final double width;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: width,
        decoration: BoxDecoration(
          color: isDark ? AppColors.cardDark : AppColors.surfaceLight,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: isDark ? AppColors.navyLight : AppColors.grey100,
            width: 0.5,
          ),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _CardImage(listing: listing, onFavoriteTap: onFavoriteTap),
            _CardBody(listing: listing),
          ],
        ),
      ),
    );
  }
}

class _CardImage extends StatelessWidget {
  const _CardImage({required this.listing, required this.onFavoriteTap});

  final ListingModel listing;
  final VoidCallback onFavoriteTap;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 110,
      child: Stack(
        children: [
          ClipRRect(
            borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
            child: CachedNetworkImage(
              imageUrl: listing.imageUrl,
              width: double.infinity,
              height: 110,
              fit: BoxFit.cover,
              placeholder: (_, __) => Shimmer.fromColors(
                baseColor: AppColors.grey100,
                highlightColor: AppColors.grey50,
                child: Container(color: AppColors.grey100),
              ),
              errorWidget: (_, __, ___) => Container(
                color: AppColors.grey100,
                child: const Icon(TablerIcons.photo_off, color: AppColors.grey400),
              ),
            ),
          ),
          Positioned(
            top: 7, left: 7,
            child: GestureDetector(
              onTap: onFavoriteTap,
              child: Container(
                width: 28, height: 28,
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.88),
                  shape: BoxShape.circle,
                ),
                child: Icon(
                  listing.isFavorite ? TablerIcons.heart_filled : TablerIcons.heart,
                  size: 14,
                  color: listing.isFavorite ? AppColors.error : AppColors.grey500,
                ),
              ),
            ),
          ),
          if (listing.isVerified || listing.isAiRecommended)
            Positioned(
              top: 7, right: 7,
              child: listing.isAiRecommended
                  ? const _AiBadge()
                  : const _VerifiedBadge(),
            ),
        ],
      ),
    );
  }
}

class _VerifiedBadge extends StatelessWidget {
  const _VerifiedBadge();
  @override
  Widget build(BuildContext context) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
        decoration: BoxDecoration(color: AppColors.primary, borderRadius: BorderRadius.circular(20)),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          const Icon(TablerIcons.shield_check, size: 10, color: Colors.white),
          const SizedBox(width: 3),
          Text('موثق', style: AppTextStyles.labelSmall.copyWith(color: Colors.white, fontSize: 9)),
        ]),
      );
}

class _AiBadge extends StatelessWidget {
  const _AiBadge();
  @override
  Widget build(BuildContext context) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 2),
        decoration: BoxDecoration(
          color: AppColors.ai.withValues(alpha: 0.85),
          borderRadius: BorderRadius.circular(20),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          const Icon(TablerIcons.sparkles, size: 9, color: Colors.white),
          const SizedBox(width: 3),
          Text('AI', style: AppTextStyles.labelSmall.copyWith(color: Colors.white, fontSize: 9)),
        ]),
      );
}

class _CardBody extends StatelessWidget {
  const _CardBody({required this.listing});
  final ListingModel listing;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(9),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(listing.title, style: AppTextStyles.titleSmall, maxLines: 1, overflow: TextOverflow.ellipsis),
          const SizedBox(height: 4),
          Text.rich(TextSpan(
            text: NumberFormatter.formatPrice(listing.price),
            style: AppTextStyles.price,
            children: [TextSpan(text: ' ريال', style: AppTextStyles.labelSmall.copyWith(color: AppColors.primary))],
          )),
          const SizedBox(height: 5),
          Row(children: [
            const Icon(TablerIcons.map_pin, size: 11, color: AppColors.grey500),
            const SizedBox(width: 3),
            Text(listing.city, style: AppTextStyles.labelSmall),
            const Spacer(),
            if (listing.rating != null) ...[
              const Icon(TablerIcons.star_filled, size: 11, color: AppColors.warning),
              const SizedBox(width: 2),
              Text(listing.rating!.toString(), style: AppTextStyles.labelSmall),
            ],
          ]),
          if (listing.postedAt.isNotEmpty) ...[
            const SizedBox(height: 4),
            Text(listing.postedAt,
                style: AppTextStyles.labelSmall.copyWith(color: AppColors.grey400, fontSize: 9)),
          ],
        ],
      ),
    );
  }
}
