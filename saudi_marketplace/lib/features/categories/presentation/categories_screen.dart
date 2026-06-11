import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../data/categories_mock_data.dart';
import '../models/category_grid_item.dart';

class CategoriesScreen extends StatelessWidget {
  const CategoriesScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Scaffold(
      body: SafeArea(
        child: Column(children: [
          // ─── Header + search ───────────────────────────────────────
          Container(
            width: double.infinity,
            decoration: BoxDecoration(
              color: isDark ? AppColors.surfaceDark : AppColors.surfaceLight,
              border: const Border(
                bottom: BorderSide(color: AppColors.grey100, width: 1),
              ),
            ),
            padding: const EdgeInsets.fromLTRB(16, 6, 16, 14),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Padding(
                  padding: const EdgeInsets.fromLTRB(2, 6, 2, 12),
                  child: Text('التصنيفات',
                      style: AppTextStyles.displayMedium.copyWith(
                        fontSize: 21,
                        color: isDark ? Colors.white : AppColors.navy,
                      )),
                ),
                GestureDetector(
                  onTap: () => context.push(AppRoutes.search),
                  child: Container(
                    padding: const EdgeInsets.symmetric(
                        horizontal: 13, vertical: 11),
                    decoration: BoxDecoration(
                      color: isDark ? AppColors.cardDark : AppColors.grey50,
                      borderRadius: BorderRadius.circular(14),
                      border: Border.all(color: AppColors.grey200, width: 1),
                    ),
                    child: Row(children: [
                      const Icon(TablerIcons.search,
                          size: 18, color: AppColors.grey600),
                      const SizedBox(width: 9),
                      Text('ابحث في كل التصنيفات...',
                          style: AppTextStyles.bodySmall.copyWith(
                              fontSize: 13,
                              color: AppColors.grey500,
                              fontWeight: FontWeight.w500)),
                    ]),
                  ),
                ),
              ],
            ),
          ),

          // ─── Categories grid ───────────────────────────────────────
          Expanded(
            child: GridView.builder(
              padding: const EdgeInsets.fromLTRB(16, 16, 16, 20),
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 2,
                mainAxisSpacing: 11,
                crossAxisSpacing: 11,
                mainAxisExtent: 76,
              ),
              itemCount: CategoriesMockData.categories.length,
              itemBuilder: (_, i) => _CategoryCard(
                category: CategoriesMockData.categories[i],
                onTap: () => context.push(AppRoutes.search),
              ),
            ),
          ),
        ]),
      ),
    );
  }
}

class _CategoryCard extends StatelessWidget {
  const _CategoryCard({required this.category, required this.onTap});

  final CategoryGridItem category;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: isDark ? AppColors.cardDark : AppColors.surfaceLight,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(
            color: isDark ? AppColors.navyLight : AppColors.grey100,
            width: 1,
          ),
        ),
        child: Row(children: [
          Container(
            width: 46,
            height: 46,
            decoration: BoxDecoration(
              color: AppColors.primaryTint,
              borderRadius: BorderRadius.circular(13),
            ),
            child: Icon(category.icon, size: 23, color: AppColors.primary),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(category.name,
                    style: AppTextStyles.titleMedium.copyWith(
                      fontSize: 13.5,
                      fontWeight: FontWeight.w700,
                      color: isDark ? Colors.white : AppColors.navy,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis),
                const SizedBox(height: 2),
                Text('${category.count} إعلان',
                    style: AppTextStyles.labelLarge
                        .copyWith(color: AppColors.grey600)),
              ],
            ),
          ),
        ]),
      ),
    );
  }
}
