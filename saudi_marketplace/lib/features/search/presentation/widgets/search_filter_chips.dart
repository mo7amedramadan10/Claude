import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';

class SearchFilterChips extends StatelessWidget {
  const SearchFilterChips({
    super.key,
    required this.city,
    required this.onOpenFilters,
  });

  final String city;
  final VoidCallback onOpenFilters;

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      scrollDirection: Axis.horizontal,
      child: Row(children: [
        _FiltersChip(onTap: onOpenFilters),
        const SizedBox(width: 8),
        _CityChip(city: city, onTap: onOpenFilters),
        const SizedBox(width: 8),
        _DropdownChip(label: 'السعر', onTap: onOpenFilters),
        const SizedBox(width: 8),
        _DropdownChip(label: 'الحالة', onTap: onOpenFilters),
      ]),
    );
  }
}

class _FiltersChip extends StatelessWidget {
  const _FiltersChip({required this.onTap});

  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 13, vertical: 8),
        decoration: BoxDecoration(
          color: AppColors.navy,
          borderRadius: BorderRadius.circular(11),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          const Icon(TablerIcons.adjustments_horizontal,
              size: 16, color: Colors.white),
          const SizedBox(width: 6),
          Text('الفلاتر',
              style: AppTextStyles.bodySmall.copyWith(
                  fontSize: 12.5,
                  color: Colors.white,
                  fontWeight: FontWeight.w700)),
        ]),
      ),
    );
  }
}

class _CityChip extends StatelessWidget {
  const _CityChip({required this.city, required this.onTap});

  final String city;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 13, vertical: 8),
        decoration: BoxDecoration(
          color: AppColors.primaryLight,
          borderRadius: BorderRadius.circular(11),
          border: Border.all(color: const Color(0xFFCDE6D9), width: 1),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          const Icon(TablerIcons.map_pin,
              size: 15, color: AppColors.primaryDark),
          const SizedBox(width: 5),
          Text(city,
              style: AppTextStyles.bodySmall.copyWith(
                  fontSize: 12.5,
                  color: AppColors.primaryDark,
                  fontWeight: FontWeight.w600)),
          const SizedBox(width: 5),
          const Icon(TablerIcons.chevron_down,
              size: 14, color: AppColors.primaryDark),
        ]),
      ),
    );
  }
}

class _DropdownChip extends StatelessWidget {
  const _DropdownChip({required this.label, required this.onTap});

  final String label;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 13, vertical: 8),
        decoration: BoxDecoration(
          color: isDark ? AppColors.cardDark : AppColors.surfaceLight,
          borderRadius: BorderRadius.circular(11),
          border: Border.all(color: AppColors.grey200, width: 1),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          Text(label,
              style: AppTextStyles.bodySmall.copyWith(
                  fontSize: 12.5,
                  color: isDark ? Colors.white : AppColors.navy,
                  fontWeight: FontWeight.w600)),
          const SizedBox(width: 5),
          Icon(TablerIcons.chevron_down,
              size: 14, color: isDark ? Colors.white : AppColors.navy),
        ]),
      ),
    );
  }
}
