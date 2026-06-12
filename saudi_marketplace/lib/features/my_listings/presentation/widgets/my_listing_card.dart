import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../../../core/utils/number_formatter.dart';
import '../../../../core/utils/relative_time.dart';
import '../../models/my_listing_item.dart';

class MyListingCard extends StatelessWidget {
  const MyListingCard({
    super.key,
    required this.item,
    required this.onEdit,
    required this.onMarkAsSold,
    required this.onDelete,
  });

  final MyListingItem item;
  final VoidCallback onEdit;
  final VoidCallback onMarkAsSold;
  final VoidCallback onDelete;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Container(
      decoration: BoxDecoration(
        color: isDark ? AppColors.cardDark : Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
            color: isDark ? AppColors.navyLight : AppColors.grey100),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // ─── Main row ──────────────────────────────────────────
          Padding(
            padding: const EdgeInsets.all(12),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Content (left in RTL)
                Expanded(child: _Content(item: item)),
                const SizedBox(width: 12),
                // Thumbnail (right in RTL = leading)
                _Thumbnail(imageUrl: item.imageUrl),
              ],
            ),
          ),
          // ─── Divider + action bar ───────────────────────────────
          Divider(
            height: 1,
            color: isDark ? AppColors.navyLight : AppColors.grey100,
          ),
          _ActionBar(
            item: item,
            onEdit: onEdit,
            onMore: () => _showMoreSheet(context),
          ),
        ],
      ),
    );
  }

  void _showMoreSheet(BuildContext context) {
    showModalBottomSheet<void>(
      context: context,
      shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (_) => _MoreSheet(
        item: item,
        onMarkAsSold: () {
          Navigator.pop(context);
          onMarkAsSold();
        },
        onDelete: () {
          Navigator.pop(context);
          _confirmDelete(context);
        },
      ),
    );
  }

  void _confirmDelete(BuildContext context) {
    showDialog<void>(
      context: context,
      builder: (_) => AlertDialog(
        title: const Text('حذف الإعلان'),
        content: Text(
          'هل أنت متأكد من حذف إعلان "${item.title}"؟',
          style: AppTextStyles.bodyMedium,
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('إلغاء'),
          ),
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              onDelete();
            },
            style: TextButton.styleFrom(foregroundColor: AppColors.error),
            child: const Text('حذف'),
          ),
        ],
      ),
    );
  }
}

// ── Thumbnail ─────────────────────────────────────────────────────────────
class _Thumbnail extends StatelessWidget {
  const _Thumbnail({required this.imageUrl});
  final String imageUrl;

  @override
  Widget build(BuildContext context) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(12),
      child: imageUrl.isNotEmpty
          ? CachedNetworkImage(
              imageUrl: imageUrl,
              width: 92,
              height: 92,
              fit: BoxFit.cover,
              placeholder: (_, __) => Container(
                  width: 92, height: 92, color: AppColors.grey50),
              errorWidget: (_, __, ___) => _placeholder(),
            )
          : _placeholder(),
    );
  }

  Widget _placeholder() => Container(
        width: 92,
        height: 92,
        color: AppColors.grey50,
        child: const Icon(TablerIcons.photo,
            size: 30, color: AppColors.grey400),
      );
}

// ── Content ───────────────────────────────────────────────────────────────
class _Content extends StatelessWidget {
  const _Content({required this.item});
  final MyListingItem item;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Row(children: [
        _StatusBadge(status: item.status),
        if (item.isVerified) ...[
          const SizedBox(width: 6),
          const Icon(TablerIcons.rosette_discount_check,
              size: 14, color: AppColors.primaryDark),
        ],
      ]),
      const SizedBox(height: 6),
      Text(
        item.title,
        maxLines: 2,
        overflow: TextOverflow.ellipsis,
        style: AppTextStyles.titleLarge.copyWith(
          fontSize: 14,
          color: isDark ? Colors.white : AppColors.navy,
        ),
      ),
      const SizedBox(height: 6),
      if (item.price != null)
        Text(
          '${NumberFormatter.arabicPrice(item.price!)} ريال',
          style: AppTextStyles.priceLarge.copyWith(
              fontSize: 16, color: AppColors.primaryDark),
        ),
      const SizedBox(height: 8),
      // ─── Stats row ─────────────────────────────────────────
      Row(children: [
        _Stat(icon: TablerIcons.eye, count: item.viewsCount),
        const SizedBox(width: 14),
        _Stat(icon: TablerIcons.heart, count: item.favoritesCount),
        const Spacer(),
        Text(
          RelativeTime.format(item.createdAt),
          style: AppTextStyles.labelLarge
              .copyWith(fontSize: 11, color: AppColors.grey400),
        ),
      ]),
    ]);
  }
}

class _Stat extends StatelessWidget {
  const _Stat({required this.icon, required this.count});
  final IconData icon;
  final int count;

  @override
  Widget build(BuildContext context) {
    return Row(mainAxisSize: MainAxisSize.min, children: [
      Icon(icon, size: 13, color: AppColors.grey500),
      const SizedBox(width: 3),
      Text(
        NumberFormatter.toArabicDigits(count),
        style: AppTextStyles.labelLarge
            .copyWith(fontSize: 12, color: AppColors.grey600),
      ),
    ]);
  }
}

// ── Status Badge ──────────────────────────────────────────────────────────
class _StatusBadge extends StatelessWidget {
  const _StatusBadge({required this.status});
  final String status;

  static const _config = <String, (Color, Color, String)>{
    'active':   (Color(0xFFEAF5F0), AppColors.primaryDark, 'نشط'),
    'sold':     (Color(0xFFEEF1F6), AppColors.navyLight,   'مباع'),
    'expired':  (Color(0xFFF1F2F4), AppColors.grey600,     'منتهي'),
    'draft':    (Color(0xFFFDF4E3), AppColors.noticeIcon,   'مسودة'),
    'pending':  (Color(0xFFFDF4E3), AppColors.noticeIcon,   'قيد المراجعة'),
    'rejected': (Color(0xFFFCEEEF), AppColors.error,       'مرفوض'),
  };

  @override
  Widget build(BuildContext context) {
    final (bg, fg, label) = _config[status] ?? _config['draft']!;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 3),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Text(label,
          style: AppTextStyles.labelLarge
              .copyWith(fontSize: 11.5, color: fg, fontWeight: FontWeight.w700)),
    );
  }
}

// ── Action Bar ────────────────────────────────────────────────────────────
class _ActionBar extends StatelessWidget {
  const _ActionBar({
    required this.item,
    required this.onEdit,
    required this.onMore,
  });
  final MyListingItem item;
  final VoidCallback onEdit;
  final VoidCallback onMore;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      child: Row(children: [
        Expanded(
          child: OutlinedButton.icon(
            onPressed: item.isSold || item.isExpired ? null : onEdit,
            icon: const Icon(TablerIcons.edit, size: 16),
            label: const Text('تعديل'),
            style: OutlinedButton.styleFrom(
              foregroundColor: AppColors.primaryDark,
              side: const BorderSide(color: AppColors.primary),
              padding:
                  const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              textStyle: AppTextStyles.titleMedium.copyWith(fontSize: 13),
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10)),
            ),
          ),
        ),
        const SizedBox(width: 10),
        OutlinedButton(
          onPressed: onMore,
          style: OutlinedButton.styleFrom(
            foregroundColor: AppColors.grey600,
            side: const BorderSide(color: AppColors.grey200),
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
            shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(10)),
          ),
          child: const Icon(TablerIcons.dots, size: 20),
        ),
      ]),
    );
  }
}

// ── More bottom sheet ─────────────────────────────────────────────────────
class _MoreSheet extends StatelessWidget {
  const _MoreSheet({
    required this.item,
    required this.onMarkAsSold,
    required this.onDelete,
  });
  final MyListingItem item;
  final VoidCallback onMarkAsSold;
  final VoidCallback onDelete;

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 8),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          // Handle
          Container(
            width: 40,
            height: 4,
            margin: const EdgeInsets.only(bottom: 12),
            decoration: BoxDecoration(
                color: AppColors.grey200,
                borderRadius: BorderRadius.circular(2)),
          ),
          if (item.isActive)
            _SheetTile(
              icon: TablerIcons.check,
              label: 'وضّح كمباع',
              color: AppColors.primaryDark,
              onTap: onMarkAsSold,
            ),
          _SheetTile(
            icon: TablerIcons.trash,
            label: 'حذف الإعلان',
            color: AppColors.error,
            onTap: onDelete,
          ),
        ]),
      ),
    );
  }
}

class _SheetTile extends StatelessWidget {
  const _SheetTile({
    required this.icon,
    required this.label,
    required this.color,
    required this.onTap,
  });
  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return ListTile(
      leading: Icon(icon, color: color, size: 22),
      title: Text(label,
          style: AppTextStyles.titleLarge
              .copyWith(color: color, fontSize: 14.5)),
      onTap: onTap,
    );
  }
}
