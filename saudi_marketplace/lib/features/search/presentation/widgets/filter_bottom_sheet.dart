import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../models/search_filters.dart';
import '../../providers/search_provider.dart';

Future<void> showFilterBottomSheet(BuildContext context) {
  return showModalBottomSheet(
    context: context,
    backgroundColor: Colors.transparent,
    barrierColor: AppColors.navy.withValues(alpha: 0.45),
    isScrollControlled: true,
    builder: (_) => const FilterBottomSheet(),
  );
}

class FilterBottomSheet extends ConsumerWidget {
  const FilterBottomSheet({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(searchProvider);
    final notifier = ref.read(searchProvider.notifier);
    final filters = state.filters;
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Container(
      constraints: BoxConstraints(
        maxHeight: MediaQuery.of(context).size.height * 0.88,
      ),
      decoration: BoxDecoration(
        color: isDark ? AppColors.surfaceDark : AppColors.surfaceLight,
        borderRadius: const BorderRadius.vertical(top: Radius.circular(24)),
      ),
      padding: const EdgeInsets.fromLTRB(20, 8, 20, 24),
      child: SingleChildScrollView(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisSize: MainAxisSize.min,
          children: [
            Center(
              child: Container(
                width: 40,
                height: 5,
                margin: const EdgeInsets.only(top: 6, bottom: 16),
                decoration: BoxDecoration(
                  color: AppColors.grey200,
                  borderRadius: BorderRadius.circular(3),
                ),
              ),
            ),
            Row(children: [
              Text('الفلاتر',
                  style: AppTextStyles.headlineLarge.copyWith(
                      color: isDark ? Colors.white : AppColors.navy)),
              const Spacer(),
              GestureDetector(
                onTap: notifier.resetFilters,
                child: Text('إعادة تعيين',
                    style: AppTextStyles.bodySmall.copyWith(
                        color: AppColors.primaryDark,
                        fontWeight: FontWeight.w700)),
              ),
            ]),
            const SizedBox(height: 18),
            _ChipSection(
              title: 'المدينة',
              options: SearchFilters.cities,
              isSelected: (c) => filters.city == c,
              onTap: notifier.selectCity,
            ),
            _ChipSection(
              title: 'نطاق السعر',
              options: SearchFilters.priceRanges,
              isSelected: (p) => filters.priceRange == p,
              onTap: notifier.togglePriceRange,
            ),
            _ChipSection(
              title: 'الحالة',
              options: SearchFilters.conditions,
              isSelected: (c) => filters.condition == c,
              onTap: notifier.toggleCondition,
            ),
            const SizedBox(height: 4),
            _ApplyButton(
              count: state.resultsCount,
              onTap: () => Navigator.of(context).pop(),
            ),
          ],
        ),
      ),
    );
  }
}

class _ChipSection extends StatelessWidget {
  const _ChipSection({
    required this.title,
    required this.options,
    required this.isSelected,
    required this.onTap,
  });

  final String title;
  final List<String> options;
  final bool Function(String) isSelected;
  final ValueChanged<String> onTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(title,
            style: AppTextStyles.titleLarge.copyWith(
                color: isDark ? Colors.white : AppColors.navy)),
        const SizedBox(height: 10),
        Wrap(
          spacing: 8,
          runSpacing: 8,
          children: options.map((option) {
            final selected = isSelected(option);
            return GestureDetector(
              onTap: () => onTap(option),
              child: Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 15, vertical: 9),
                decoration: BoxDecoration(
                  color: selected
                      ? AppColors.primary
                      : (isDark
                          ? AppColors.cardDark
                          : AppColors.surfaceLight),
                  borderRadius: BorderRadius.circular(11),
                  border: Border.all(
                    color:
                        selected ? AppColors.primary : AppColors.grey200,
                    width: 1,
                  ),
                ),
                child: Text(
                  option,
                  style: AppTextStyles.bodySmall.copyWith(
                    fontSize: 12.5,
                    fontWeight: FontWeight.w600,
                    color: selected
                        ? Colors.white
                        : (isDark ? Colors.white : AppColors.navy),
                  ),
                ),
              ),
            );
          }).toList(),
        ),
        const SizedBox(height: 20),
      ],
    );
  }
}

class _ApplyButton extends StatelessWidget {
  const _ApplyButton({required this.count, required this.onTap});

  final int count;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        height: 52,
        width: double.infinity,
        decoration: BoxDecoration(
          color: AppColors.primary,
          borderRadius: BorderRadius.circular(14),
          boxShadow: const [
            BoxShadow(
              color: Color(0x521A7A4A),
              blurRadius: 16,
              offset: Offset(0, 6),
            ),
          ],
        ),
        child: Center(
          child: Text(
            'عرض $count نتيجة',
            style: AppTextStyles.headlineMedium.copyWith(color: Colors.white),
          ),
        ),
      ),
    );
  }
}
