import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../theme/app_colors.dart';
import '../../theme/app_text_styles.dart';

class AppBottomNavBar extends StatelessWidget {
  const AppBottomNavBar({
    super.key,
    required this.currentIndex,
    required this.onTap,
    required this.onSellTap,
  });

  final int currentIndex;
  final ValueChanged<int> onTap;
  final VoidCallback onSellTap;

  static const _items = [
    _NavItem(icon: TablerIcons.home, activeIcon: TablerIcons.smart_home, label: 'الرئيسية'),
    _NavItem(icon: TablerIcons.layout_grid, activeIcon: TablerIcons.layout_grid, label: 'التصنيفات'),
    _NavItem(icon: TablerIcons.heart, activeIcon: TablerIcons.heart_filled, label: 'المحفوظات'),
    _NavItem(icon: TablerIcons.user_circle, activeIcon: TablerIcons.user_circle, label: 'حسابي'),
  ];

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final bg = isDark ? AppColors.surfaceDark : AppColors.surfaceLight;
    final border = isDark ? AppColors.navyLight : AppColors.grey200;

    return Container(
      decoration: BoxDecoration(
        color: bg,
        border: Border(top: BorderSide(color: border, width: 0.5)),
      ),
      child: SafeArea(
        top: false,
        child: SizedBox(
          height: 64,
          child: Row(
            children: [
              // ─── Left pair (Home, Categories) ──────────────────────
              Expanded(child: _buildTab(context, 0)),
              Expanded(child: _buildTab(context, 1)),

              // ─── Center Sell FAB ───────────────────────────────────
              _SellButton(onTap: onSellTap),

              // ─── Right pair (Favorites, Profile) ───────────────────
              Expanded(child: _buildTab(context, 2)),
              Expanded(child: _buildTab(context, 3)),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildTab(BuildContext context, int index) {
    final isActive = currentIndex == index;
    final item = _items[index];
    final color = isActive ? AppColors.primary : AppColors.grey500;

    return GestureDetector(
      behavior: HitTestBehavior.opaque,
      onTap: () => onTap(index),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          // Active indicator dot
          AnimatedContainer(
            duration: const Duration(milliseconds: 200),
            height: 3,
            width: isActive ? 20 : 0,
            margin: const EdgeInsets.only(bottom: 4),
            decoration: BoxDecoration(
              color: AppColors.primary,
              borderRadius: BorderRadius.circular(2),
            ),
          ),
          Icon(
            isActive ? item.activeIcon : item.icon,
            size: 24,
            color: color,
          ),
          const SizedBox(height: 3),
          Text(
            item.label,
            style: AppTextStyles.labelSmall.copyWith(
              color: color,
              fontWeight: isActive ? FontWeight.w700 : FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }
}

// ─── Center sell button ───────────────────────────────────────────────────────

class _SellButton extends StatelessWidget {
  const _SellButton({required this.onTap});

  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 56,
        height: 56,
        margin: const EdgeInsets.symmetric(horizontal: 8),
        decoration: const BoxDecoration(
          color: AppColors.primary,
          shape: BoxShape.circle,
          boxShadow: [
            BoxShadow(
              color: Color(0x3D1A7A4A),
              blurRadius: 12,
              offset: Offset(0, 4),
            ),
          ],
        ),
        child: const Icon(TablerIcons.plus, color: Colors.white, size: 26),
      ),
    );
  }
}

// ─── Data class ──────────────────────────────────────────────────────────────

class _NavItem {
  const _NavItem({
    required this.icon,
    required this.activeIcon,
    required this.label,
  });

  final IconData icon;
  final IconData activeIcon;
  final String label;
}
