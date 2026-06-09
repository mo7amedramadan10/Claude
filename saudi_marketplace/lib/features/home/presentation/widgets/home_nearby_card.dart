import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:shimmer/shimmer.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../../../core/utils/number_formatter.dart';
import '../../models/listing_model.dart';

class HomeNearbyCard extends StatelessWidget {
  const HomeNearbyCard({
    super.key,
    required this.listing,
    this.onTap,
  });

  final ListingModel listing;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 155,
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
            ClipRRect(
              borderRadius:
                  const BorderRadius.vertical(top: Radius.circular(16)),
              child: CachedNetworkImage(
                imageUrl: listing.imageUrl,
                width: double.infinity,
                height: 95,
                fit: BoxFit.cover,
                placeholder: (_, __) => Shimmer.fromColors(
                  baseColor: AppColors.grey100,
                  highlightColor: AppColors.grey50,
                  child: Container(height: 95, color: AppColors.grey100),
                ),
                errorWidget: (_, __, ___) => Container(
                  height: 95,
                  color: AppColors.grey100,
                  child: const Icon(TablerIcons.photo_off, color: AppColors.grey400),
                ),
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(9),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(listing.title,
                      style: AppTextStyles.titleSmall,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis),
                  const SizedBox(height: 3),
                  Text(
                    '${NumberFormatter.formatPrice(listing.price)} ريال',
                    style: AppTextStyles.price.copyWith(fontSize: 13),
                  ),
                  if (listing.distanceKm != null) ...[
                    const SizedBox(height: 3),
                    Row(children: [
                      const Icon(TablerIcons.map_pin,
                          size: 10, color: AppColors.grey500),
                      const SizedBox(width: 3),
                      Text('${listing.distanceKm} كم بعيد',
                          style: AppTextStyles.labelSmall),
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
