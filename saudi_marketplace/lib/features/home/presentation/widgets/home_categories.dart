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
        padding: const EdgeInsets.fromLTRB(16, 14, 16, 14),
        child: Row(
          children: HomeMockData.categories.asMap().entries.map((e) {
            return Padding(
              padding: const EdgeInsets.only(left: 6),
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
      child: SizedBox(
        width: 58,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            AnimatedContainer(
              duration: const Duration(milliseconds: 200),
              width: 50,
              height: 50,
              decoration: BoxDecoration(
                color: isActive ? AppColors.primary : AppColors.primaryTint,
                borderRadius: BorderRadius.circular(15),
              ),
              child: Icon(
                category.icon,
                size: 23,
                color: isActive ? Colors.white : AppColors.primary,
              ),
            ),
            const SizedBox(height: 6),
            Text(
              category.label,
              style: AppTextStyles.labelLarge.copyWith(
                color: isActive ? AppColors.primaryDark : AppColors.grey700,
                fontWeight: isActive ? FontWeight.w700 : FontWeight.w600,
              ),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
            ),
          ],
        ),
      ),
    );
  }
}
