import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../theme/app_colors.dart';
import '../../theme/app_text_styles.dart';

class AppBottomNavBar extends StatelessWidget {
  const AppBottomNavBar({
    super.key,
    required this.currentIndex,
    required this.onTap,
  });

  final int currentIndex;
  final ValueChanged<int> onTap;

  static const _items = [
    _NavItem(icon: TablerIcons.home, label: 'الرئيسية'),
    _NavItem(icon: TablerIcons.layout_grid, label: 'التصنيفات'),
    _NavItem(
        icon: TablerIcons.heart,
        activeIcon: TablerIcons.heart_filled,
        label: 'المحفوظات'),
    _NavItem(icon: TablerIcons.user_circle, label: 'حسابي'),
  ];

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Container(
      decoration: BoxDecoration(
        color: isDark ? AppColors.surfaceDark : AppColors.surfaceLight,
        border: const Border(
          top: BorderSide(color: AppColors.grey200, width: 1),
        ),
      ),
      child: SafeArea(
        top: false,
        child: SizedBox(
          height: 62,
          child: Row(
            children: List.generate(
              _items.length,
              (i) => Expanded(child: _buildTab(i)),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildTab(int index) {
    final isActive = currentIndex == index;
    final item = _items[index];
    final color = isActive ? AppColors.primaryDark : AppColors.grey400;

    return GestureDetector(
      behavior: HitTestBehavior.opaque,
      onTap: () => onTap(index),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            isActive ? (item.activeIcon ?? item.icon) : item.icon,
            size: 24,
            color: color,
          ),
          const SizedBox(height: 4),
          Text(
            item.label,
            style: AppTextStyles.labelLarge.copyWith(
              color: color,
              fontWeight: isActive ? FontWeight.w700 : FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }
}

class _NavItem {
  const _NavItem({required this.icon, this.activeIcon, required this.label});

  final IconData icon;
  final IconData? activeIcon;
  final String label;
}
