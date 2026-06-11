import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/utils/number_formatter.dart';
import '../models/listing_detail_model.dart';
import '../providers/listing_detail_provider.dart';
import 'widgets/detail_bottom_bar.dart';
import 'widgets/detail_error_view.dart';
import 'widgets/detail_gallery.dart';
import 'widgets/detail_seller_card.dart';
import 'widgets/detail_specs_card.dart';

class ListingDetailScreen extends ConsumerWidget {
  const ListingDetailScreen({super.key, required this.listingId});

  final String listingId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(listingDetailProvider(listingId));
    final notifier = ref.read(listingDetailProvider(listingId).notifier);
    final detail = state.detail;
    final isDark = Theme.of(context).brightness == Brightness.dark;

    if (state.isLoading) {
      return Scaffold(
        backgroundColor: isDark ? AppColors.backgroundDark : Colors.white,
        body: const Center(
          child: CircularProgressIndicator(color: AppColors.primary),
        ),
      );
    }

    if (state.errorMessage != null) {
      return Scaffold(
        backgroundColor: isDark ? AppColors.backgroundDark : Colors.white,
        body: DetailErrorView(
          message: state.errorMessage!,
          onRetry: notifier.load,
          onBack: () => context.pop(),
        ),
      );
    }

    return Scaffold(
      backgroundColor: isDark ? AppColors.backgroundDark : Colors.white,
      body: SafeArea(
        bottom: false,
        child: ListView(
          padding: const EdgeInsets.only(bottom: 20),
          children: [
            DetailGallery(
              images: detail.gallery,
              activeIndex: state.activeImageIndex,
              isFavorite: state.isFavorite,
              onImageSelect: notifier.selectImage,
              onBack: () => context.pop(),
              onFavoriteTap: notifier.toggleFavorite,
            ),
            _TitleBlock(detail: detail),
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 18, 16, 0),
              child: DetailSpecsCard(specs: detail.specs),
            ),
            _Section(title: 'الوصف', child: _Description(detail: detail)),
            _Section(title: 'الموقع', child: _MapPlaceholder(detail: detail)),
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 20, 16, 0),
              child: DetailSellerCard(seller: detail.seller),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 14, 16, 0),
              child: _SafetyNotice(text: detail.safetyNotice),
            ),
          ],
        ),
      ),
      bottomNavigationBar: DetailBottomBar(
        onChatTap: () => context.push(AppRoutes.chatDetailPath(detail.id)),
      ),
    );
  }
}

class _TitleBlock extends StatelessWidget {
  const _TitleBlock({required this.detail});

  final ListingDetailModel detail;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Padding(
      padding: const EdgeInsets.fromLTRB(18, 10, 18, 0),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Text(detail.title,
            style: AppTextStyles.displayMedium.copyWith(
              fontSize: 20,
              height: 1.35,
              color: isDark ? Colors.white : AppColors.navy,
            )),
        const SizedBox(height: 6),
        Row(children: [
          const Icon(TablerIcons.map_pin, size: 14, color: AppColors.grey600),
          const SizedBox(width: 4),
          Text(detail.location,
              style: AppTextStyles.bodySmall.copyWith(
                  fontSize: 12.5, color: AppColors.grey600)),
          const SizedBox(width: 12),
          const Icon(TablerIcons.clock, size: 14, color: AppColors.grey600),
          const SizedBox(width: 4),
          Text(detail.postedAt,
              style: AppTextStyles.bodySmall.copyWith(
                  fontSize: 12.5, color: AppColors.grey600)),
        ]),
        const SizedBox(height: 12),
        Text.rich(TextSpan(
          text: NumberFormatter.formatPrice(detail.price),
          style: AppTextStyles.priceLarge
              .copyWith(fontSize: 26, color: AppColors.primaryDark),
          children: [
            TextSpan(
              text: ' ريال',
              style: AppTextStyles.titleLarge
                  .copyWith(color: AppColors.primaryDark),
            ),
          ],
        )),
        if (detail.isVerified) ...[
          const SizedBox(height: 12),
          Container(
            padding:
                const EdgeInsets.symmetric(horizontal: 11, vertical: 6),
            decoration: BoxDecoration(
              color: AppColors.primaryLight,
              borderRadius: BorderRadius.circular(10),
            ),
            child: Row(mainAxisSize: MainAxisSize.min, children: [
              const Icon(TablerIcons.rosette_discount_check,
                  size: 15, color: AppColors.primaryDark),
              const SizedBox(width: 6),
              Text(detail.verifiedNote,
                  style: AppTextStyles.bodySmall.copyWith(
                      color: AppColors.primaryDark,
                      fontWeight: FontWeight.w700)),
            ]),
          ),
        ],
      ]),
    );
  }
}

class _Section extends StatelessWidget {
  const _Section({required this.title, required this.child});

  final String title;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Padding(
      padding: const EdgeInsets.fromLTRB(18, 20, 18, 0),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Text(title,
            style: AppTextStyles.headlineMedium.copyWith(
                color: isDark ? Colors.white : AppColors.navy)),
        const SizedBox(height: 10),
        child,
      ]),
    );
  }
}

class _Description extends StatelessWidget {
  const _Description({required this.detail});

  final ListingDetailModel detail;

  @override
  Widget build(BuildContext context) {
    return Text(
      detail.description,
      style: AppTextStyles.bodyMedium.copyWith(
        fontSize: 13.5,
        height: 1.75,
        color: AppColors.grey700,
      ),
    );
  }
}

class _MapPlaceholder extends StatelessWidget {
  const _MapPlaceholder({required this.detail});

  final ListingDetailModel detail;

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 120,
      decoration: BoxDecoration(
        color: const Color(0xFFEDF1ED),
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppColors.grey300, width: 1),
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(13),
        child: CustomPaint(
          painter: _MapGridPainter(),
          child: Center(
            child: Column(mainAxisSize: MainAxisSize.min, children: [
              const Icon(TablerIcons.map_pin_filled,
                  size: 30, color: AppColors.error),
              const SizedBox(height: 4),
              Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.85),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Text(detail.district,
                    style: AppTextStyles.labelLarge.copyWith(
                        color: AppColors.primaryDark,
                        fontWeight: FontWeight.w600)),
              ),
            ]),
          ),
        ),
      ),
    );
  }
}

class _MapGridPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = AppColors.primary.withValues(alpha: 0.08)
      ..strokeWidth = 1;
    for (double y = 0; y < size.height; y += 26) {
      canvas.drawLine(Offset(0, y), Offset(size.width, y), paint);
    }
    for (double x = 0; x < size.width; x += 26) {
      canvas.drawLine(Offset(x, 0), Offset(x, size.height), paint);
    }
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}

class _SafetyNotice extends StatelessWidget {
  const _SafetyNotice({required this.text});

  final String text;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 13, vertical: 12),
      decoration: BoxDecoration(
        color: AppColors.noticeBg,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppColors.noticeBorder, width: 1),
      ),
      child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
        const Padding(
          padding: EdgeInsets.only(top: 1),
          child: Icon(TablerIcons.shield_half_filled,
              size: 20, color: AppColors.noticeIcon),
        ),
        const SizedBox(width: 10),
        Expanded(
          child: Text(text,
              style: AppTextStyles.bodySmall.copyWith(
                  color: AppColors.noticeText, height: 1.6)),
        ),
      ]),
    );
  }
}
