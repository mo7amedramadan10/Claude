import 'package:flutter/material.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../data/home_mock_data.dart';
import '../../models/category_model.dart';

class HomeCategories extends StatelessWidget {
  const HomeCategories({
    super.key,
    required this.selectedIndex,
    required this.onCategoryTap,
  });

  final int selectedIndex;
  final ValueChanged<int> onCategoryTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Container(
      color: isDark ? AppColors.surfaceDark : AppColors.surfaceLight,
      child: SingleChildScrollView(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.fromLTRB(16, 10, 16, 10),
        child: Row(
          children: HomeMockData.categories.asMap().entries.map((e) {
            return Padding(
              padding: const EdgeInsets.only(left: 4),
              child: _CategoryItem(
                category: e.value,
                isActive: e.key == selectedIndex,
                onTap: () => onCategoryTap(e.key),
              ),
            );
          }).toList(),
        ),
      ),
    );
  }
}

class _CategoryItem extends StatelessWidget {
  const _CategoryItem({
    required this.category,
    required this.isActive,
    required this.onTap,
  });

  final CategoryModel category;
  final bool isActive;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          AnimatedContainer(
            duration: const Duration(milliseconds: 200),
            width: 48, height: 48,
            decoration: BoxDecoration(
              color: isActive ? AppColors.primary : AppColors.primaryLight,
              borderRadius: BorderRadius.circular(14),
            ),
            child: Icon(
              category.icon,
              size: 22,
              color: isActive ? Colors.white : AppColors.primary,
            ),
          ),
          const SizedBox(height: 5),
          Text(
            category.label,
            style: AppTextStyles.labelSmall.copyWith(
              color: isActive ? AppColors.primary : AppColors.grey700,
              fontWeight: isActive ? FontWeight.w700 : FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}
