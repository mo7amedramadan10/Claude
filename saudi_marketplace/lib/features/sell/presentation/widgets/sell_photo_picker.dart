import 'dart:io';

import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:image_picker/image_picker.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../../../core/utils/number_formatter.dart';
import '../../models/sell_models.dart';
import '../../providers/sell_provider.dart';

class SellPhotoPicker extends StatelessWidget {
  const SellPhotoPicker({
    super.key,
    required this.existingImages,
    required this.newImages,
    required this.onAdd,
    required this.onRemoveExisting,
    required this.onRemoveNew,
  });

  final List<ExistingImage> existingImages;
  final List<XFile> newImages;
  final void Function(List<XFile>) onAdd;
  final void Function(int) onRemoveExisting;
  final void Function(int) onRemoveNew;

  int get _total => existingImages.length + newImages.length;

  Future<void> _pick() async {
    final picked = await ImagePicker().pickMultiImage(
      imageQuality: 82,
      maxWidth: 1600,
    );
    if (picked.isNotEmpty) onAdd(picked);
  }

  @override
  Widget build(BuildContext context) {
    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Row(children: [
        Text('الصور',
            style: AppTextStyles.titleMedium
                .copyWith(color: AppColors.grey700, fontSize: 13.5)),
        const SizedBox(width: 6),
        Text(
          '(${NumberFormatter.toArabicDigits(_total)}/'
          '${NumberFormatter.toArabicDigits(maxListingImages)})',
          style: AppTextStyles.labelLarge
              .copyWith(fontSize: 11.5, color: AppColors.grey400),
        ),
      ]),
      const SizedBox(height: 10),
      SizedBox(
        height: 96,
        child: ListView(
          scrollDirection: Axis.horizontal,
          children: [
            if (_total < maxListingImages) _AddTile(onTap: _pick),
            // ── صور موجودة مسبقاً ──────────────────────────────────
            for (var i = 0; i < existingImages.length; i++) ...[
              const SizedBox(width: 10),
              _NetworkTile(
                url: existingImages[i].publicUrl,
                isPrimary: i == 0 && newImages.isEmpty,
                onRemove: () => onRemoveExisting(i),
              ),
            ],
            // ── صور جديدة من الجهاز ────────────────────────────────
            for (var i = 0; i < newImages.length; i++) ...[
              const SizedBox(width: 10),
              _LocalTile(
                file: newImages[i],
                isPrimary: existingImages.isEmpty && i == 0,
                onRemove: () => onRemoveNew(i),
              ),
            ],
          ],
        ),
      ),
      const SizedBox(height: 6),
      Text(
        'الصورة الأولى هي الرئيسية في نتائج البحث',
        style: AppTextStyles.labelLarge
            .copyWith(fontSize: 11, color: AppColors.grey400),
      ),
    ]);
  }
}

// ── Add tile ──────────────────────────────────────────────────────────────
class _AddTile extends StatelessWidget {
  const _AddTile({required this.onTap});
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 96,
        height: 96,
        decoration: BoxDecoration(
          color: AppColors.primaryLight,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: AppColors.primary, width: 1.2),
        ),
        child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
          const Icon(TablerIcons.camera_plus,
              size: 26, color: AppColors.primaryDark),
          const SizedBox(height: 4),
          Text('أضف صوراً',
              style: AppTextStyles.labelLarge.copyWith(
                  fontSize: 11,
                  color: AppColors.primaryDark,
                  fontWeight: FontWeight.w700)),
        ]),
      ),
    );
  }
}

// ── Shared tile scaffold ──────────────────────────────────────────────────
class _TileScaffold extends StatelessWidget {
  const _TileScaffold({
    required this.child,
    required this.isPrimary,
    required this.onRemove,
  });

  final Widget child;
  final bool isPrimary;
  final VoidCallback onRemove;

  @override
  Widget build(BuildContext context) {
    return Stack(children: [
      ClipRRect(
        borderRadius: BorderRadius.circular(14),
        child: child,
      ),
      PositionedDirectional(
        top: 5,
        start: 5,
        child: GestureDetector(
          onTap: onRemove,
          child: Container(
            width: 22,
            height: 22,
            decoration: BoxDecoration(
              color: Colors.black.withValues(alpha: 0.55),
              shape: BoxShape.circle,
            ),
            child: const Icon(TablerIcons.x, size: 13, color: Colors.white),
          ),
        ),
      ),
      if (isPrimary)
        PositionedDirectional(
          bottom: 5,
          start: 5,
          child: Container(
            padding:
                const EdgeInsets.symmetric(horizontal: 7, vertical: 2),
            decoration: BoxDecoration(
              color: AppColors.primary,
              borderRadius: BorderRadius.circular(8),
            ),
            child: Text('رئيسية',
                style: AppTextStyles.labelSmall
                    .copyWith(color: Colors.white, fontSize: 9.5)),
          ),
        ),
    ]);
  }
}

class _NetworkTile extends StatelessWidget {
  const _NetworkTile({
    required this.url,
    required this.isPrimary,
    required this.onRemove,
  });

  final String url;
  final bool isPrimary;
  final VoidCallback onRemove;

  @override
  Widget build(BuildContext context) {
    return _TileScaffold(
      isPrimary: isPrimary,
      onRemove: onRemove,
      child: CachedNetworkImage(
        imageUrl: url,
        width: 96,
        height: 96,
        fit: BoxFit.cover,
        placeholder: (_, __) =>
            Container(width: 96, height: 96, color: AppColors.grey50),
        errorWidget: (_, __, ___) => Container(
          width: 96,
          height: 96,
          color: AppColors.grey50,
          child: const Icon(TablerIcons.photo,
              size: 30, color: AppColors.grey400),
        ),
      ),
    );
  }
}

class _LocalTile extends StatelessWidget {
  const _LocalTile({
    required this.file,
    required this.isPrimary,
    required this.onRemove,
  });

  final XFile file;
  final bool isPrimary;
  final VoidCallback onRemove;

  @override
  Widget build(BuildContext context) {
    return _TileScaffold(
      isPrimary: isPrimary,
      onRemove: onRemove,
      child: Image.file(
        File(file.path),
        width: 96,
        height: 96,
        fit: BoxFit.cover,
        errorBuilder: (_, __, ___) => Container(
          width: 96,
          height: 96,
          color: AppColors.grey50,
          child: const Icon(TablerIcons.photo,
              size: 30, color: AppColors.grey400),
        ),
      ),
    );
  }
}
