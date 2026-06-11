import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:shimmer/shimmer.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../../../core/utils/number_formatter.dart';

class DetailGallery extends StatelessWidget {
  const DetailGallery({
    super.key,
    required this.images,
    required this.activeIndex,
    required this.isFavorite,
    required this.onImageSelect,
    required this.onBack,
    required this.onFavoriteTap,
    this.onShareTap,
  });

  final List<String> images;
  final int activeIndex;
  final bool isFavorite;
  final ValueChanged<int> onImageSelect;
  final VoidCallback onBack;
  final VoidCallback onFavoriteTap;
  final VoidCallback? onShareTap;

  @override
  Widget build(BuildContext context) {
    return Column(children: [
      _Hero(
        imageUrl: images[activeIndex],
        counter:
            '${NumberFormatter.toArabicDigits(activeIndex + 1)} / ${NumberFormatter.toArabicDigits(images.length)}',
        isFavorite: isFavorite,
        onBack: onBack,
        onFavoriteTap: onFavoriteTap,
        onShareTap: onShareTap,
      ),
      _ThumbnailStrip(
        images: images,
        activeIndex: activeIndex,
        onSelect: onImageSelect,
      ),
    ]);
  }
}

class _Hero extends StatelessWidget {
  const _Hero({
    required this.imageUrl,
    required this.counter,
    required this.isFavorite,
    required this.onBack,
    required this.onFavoriteTap,
    this.onShareTap,
  });

  final String imageUrl;
  final String counter;
  final bool isFavorite;
  final VoidCallback onBack;
  final VoidCallback onFavoriteTap;
  final VoidCallback? onShareTap;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 264,
      child: Stack(fit: StackFit.expand, children: [
        CachedNetworkImage(
          imageUrl: imageUrl,
          fit: BoxFit.cover,
          placeholder: (_, __) => Shimmer.fromColors(
            baseColor: AppColors.grey100,
            highlightColor: AppColors.grey50,
            child: Container(color: AppColors.grey100),
          ),
          errorWidget: (_, __, ___) => Container(
            color: AppColors.grey100,
            child:
                const Icon(TablerIcons.photo_off, color: AppColors.grey400),
          ),
        ),
        // Navy gradient scrim — darker at top and bottom edges
        const DecoratedBox(
          decoration: BoxDecoration(
            gradient: LinearGradient(
              begin: Alignment.topCenter,
              end: Alignment.bottomCenter,
              stops: [0, 0.28, 0.7, 1],
              colors: [
                Color(0x520D1F3C),
                Color(0x000D1F3C),
                Color(0x000D1F3C),
                Color(0x2E0D1F3C),
              ],
            ),
          ),
        ),
        Positioned(
          top: 12,
          right: 14,
          left: 14,
          child: Row(children: [
            _CircleButton(
              icon: TablerIcons.arrow_right,
              onTap: onBack,
            ),
            const Spacer(),
            _CircleButton(
              icon: TablerIcons.share_2,
              iconSize: 19,
              onTap: onShareTap,
            ),
            const SizedBox(width: 9),
            _CircleButton(
              icon: isFavorite ? TablerIcons.heart_filled : TablerIcons.heart,
              iconSize: 19,
              iconColor: isFavorite ? AppColors.error : AppColors.navy,
              onTap: onFavoriteTap,
            ),
          ]),
        ),
        Positioned(
          bottom: 12,
          left: 14,
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 11, vertical: 4),
            decoration: BoxDecoration(
              color: AppColors.navy.withValues(alpha: 0.72),
              borderRadius: BorderRadius.circular(20),
            ),
            child: Row(mainAxisSize: MainAxisSize.min, children: [
              const Icon(TablerIcons.photo, size: 14, color: Colors.white),
              const SizedBox(width: 5),
              Text(counter,
                  style: AppTextStyles.bodySmall.copyWith(
                      color: Colors.white, fontWeight: FontWeight.w600)),
            ]),
          ),
        ),
      ]),
    );
  }
}

class _CircleButton extends StatelessWidget {
  const _CircleButton({
    required this.icon,
    this.onTap,
    this.iconSize = 21,
    this.iconColor = AppColors.navy,
  });

  final IconData icon;
  final VoidCallback? onTap;
  final double iconSize;
  final Color iconColor;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 38,
        height: 38,
        decoration: BoxDecoration(
          color: Colors.white.withValues(alpha: 0.92),
          shape: BoxShape.circle,
          boxShadow: const [
            BoxShadow(
              color: Color(0x1F000000),
              blurRadius: 8,
              offset: Offset(0, 2),
            ),
          ],
        ),
        child: Icon(icon, size: iconSize, color: iconColor),
      ),
    );
  }
}

class _ThumbnailStrip extends StatelessWidget {
  const _ThumbnailStrip({
    required this.images,
    required this.activeIndex,
    required this.onSelect,
  });

  final List<String> images;
  final int activeIndex;
  final ValueChanged<int> onSelect;

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      padding: const EdgeInsets.fromLTRB(16, 12, 16, 4),
      child: Row(
        children: List.generate(images.length, (i) {
          final isActive = i == activeIndex;
          return Padding(
            padding: const EdgeInsets.only(left: 8),
            child: GestureDetector(
              onTap: () => onSelect(i),
              child: Opacity(
                opacity: isActive ? 1 : 0.8,
                child: Container(
                  width: 66,
                  height: 50,
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(11),
                    border: Border.all(
                      color:
                          isActive ? AppColors.primary : AppColors.grey100,
                      width: isActive ? 2.5 : 1.5,
                    ),
                  ),
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(8),
                    child: CachedNetworkImage(
                      imageUrl: images[i],
                      fit: BoxFit.cover,
                      placeholder: (_, __) =>
                          Container(color: AppColors.grey100),
                      errorWidget: (_, __, ___) =>
                          Container(color: AppColors.grey100),
                    ),
                  ),
                ),
              ),
            ),
          );
        }),
      ),
    );
  }
}
