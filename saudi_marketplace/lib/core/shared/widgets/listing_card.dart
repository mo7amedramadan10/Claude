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
    this.width = 160,
    this.imageHeight = 114,
  });

  final ListingModel listing;
  final VoidCallback onFavoriteTap;
  final VoidCallback? onTap;

  /// Pass null to let the parent (e.g. a grid) control the width.
  final double? width;
  final double imageHeight;

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
            width: 1,
          ),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _CardImage(
              listing: listing,
              onFavoriteTap: onFavoriteTap,
              height: imageHeight,
            ),
            _CardBody(listing: listing),
          ],
        ),
      ),
    );
  }
}

class _CardImage extends StatelessWidget {
  const _CardImage({
    required this.listing,
    required this.onFavoriteTap,
    required this.height,
  });

  final ListingModel listing;
  final VoidCallback onFavoriteTap;
  final double height;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: height,
      child: Stack(
        children: [
          ClipRRect(
            borderRadius:
                const BorderRadius.vertical(top: Radius.circular(15)),
            child: CachedNetworkImage(
              imageUrl: listing.imageUrl,
              width: double.infinity,
              height: height,
              fit: BoxFit.cover,
              placeholder: (_, __) => Shimmer.fromColors(
                baseColor: AppColors.grey100,
                highlightColor: AppColors.grey50,
                child: Container(color: AppColors.grey100),
              ),
              errorWidget: (_, __, ___) => Container(
                color: AppColors.grey100,
                child: const Icon(TablerIcons.photo_off,
                    color: AppColors.grey400),
              ),
            ),
          ),
          Positioned(
            top: 8,
            left: 8,
            child: GestureDetector(
              onTap: onFavoriteTap,
              child: Container(
                width: 28,
                height: 28,
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.92),
                  shape: BoxShape.circle,
                ),
                child: Icon(
                  listing.isFavorite
                      ? TablerIcons.heart_filled
                      : TablerIcons.heart,
                  size: 14,
                  color: listing.isFavorite
                      ? AppColors.error
                      : AppColors.grey600,
                ),
              ),
            ),
          ),
          if (listing.isVerified)
            const Positioned(top: 8, right: 8, child: _VerifiedBadge()),
        ],
      ),
    );
  }
}

class _VerifiedBadge extends StatelessWidget {
  const _VerifiedBadge();

  @override
  Widget build(BuildContext context) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 3),
        decoration: BoxDecoration(
          color: AppColors.primaryDark,
          borderRadius: BorderRadius.circular(20),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          const Icon(TablerIcons.rosette_discount_check,
              size: 11, color: Colors.white),
          const SizedBox(width: 3),
          Text('موثق',
              style: AppTextStyles.labelSmall
                  .copyWith(color: Colors.white, fontWeight: FontWeight.w700)),
        ]),
      );
}

class _CardBody extends StatelessWidget {
  const _CardBody({required this.listing});

  final ListingModel listing;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Padding(
      padding: const EdgeInsets.fromLTRB(10, 10, 10, 11),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            listing.title,
            style: AppTextStyles.titleMedium.copyWith(
              fontWeight: FontWeight.w700,
              color: isDark ? Colors.white : AppColors.navy,
            ),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),
          const SizedBox(height: 5),
          Text.rich(TextSpan(
            text: NumberFormatter.formatPrice(listing.price),
            style: AppTextStyles.price
                .copyWith(fontSize: 15, color: AppColors.primaryDark),
            children: [
              TextSpan(
                text: ' ريال',
                style: AppTextStyles.labelLarge.copyWith(
                    color: AppColors.primaryDark,
                    fontWeight: FontWeight.w600),
              ),
            ],
          )),
          const SizedBox(height: 7),
          Row(children: [
            const Icon(TablerIcons.map_pin,
                size: 12, color: AppColors.grey600),
            const SizedBox(width: 3),
            Text(listing.city,
                style: AppTextStyles.labelLarge
                    .copyWith(color: AppColors.grey600)),
            const Spacer(),
            Text(listing.postedAt,
                style: AppTextStyles.labelLarge
                    .copyWith(color: AppColors.grey600)),
          ]),
        ],
      ),
    );
  }
}
