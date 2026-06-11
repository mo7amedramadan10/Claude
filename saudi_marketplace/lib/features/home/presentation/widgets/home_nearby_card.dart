import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:shimmer/shimmer.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../../../core/utils/number_formatter.dart';
import '../../models/listing_model.dart';

class HomeNearbyCard extends StatelessWidget {
  const HomeNearbyCard({super.key, required this.listing, this.onTap});

  final ListingModel listing;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 160,
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
            ClipRRect(
              borderRadius:
                  const BorderRadius.vertical(top: Radius.circular(15)),
              child: CachedNetworkImage(
                imageUrl: listing.imageUrl,
                width: double.infinity,
                height: 100,
                fit: BoxFit.cover,
                placeholder: (_, __) => Shimmer.fromColors(
                  baseColor: AppColors.grey100,
                  highlightColor: AppColors.grey50,
                  child: Container(height: 100, color: AppColors.grey100),
                ),
                errorWidget: (_, __, ___) => Container(
                  height: 100,
                  color: AppColors.grey100,
                  child: const Icon(TablerIcons.photo_off,
                      color: AppColors.grey400),
                ),
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(10, 10, 10, 11),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(listing.title,
                      style: AppTextStyles.titleSmall.copyWith(
                        fontWeight: FontWeight.w700,
                        color: isDark ? Colors.white : AppColors.navy,
                      ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis),
                  const SizedBox(height: 4),
                  Text.rich(TextSpan(
                    text: NumberFormatter.formatPrice(listing.price),
                    style: AppTextStyles.price
                        .copyWith(color: AppColors.primaryDark),
                    children: [
                      TextSpan(
                        text: ' ريال',
                        style: AppTextStyles.labelLarge.copyWith(
                            color: AppColors.primaryDark,
                            fontWeight: FontWeight.w600),
                      ),
                    ],
                  )),
                  if (listing.distanceKm != null) ...[
                    const SizedBox(height: 5),
                    Row(children: [
                      const Icon(TablerIcons.route,
                          size: 12, color: AppColors.grey600),
                      const SizedBox(width: 4),
                      Text('يبعد ${listing.distanceKm} كم',
                          style: AppTextStyles.labelLarge
                              .copyWith(color: AppColors.grey600)),
                    ]),
                  ],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
