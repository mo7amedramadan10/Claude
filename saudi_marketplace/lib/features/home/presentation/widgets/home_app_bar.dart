import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';

class HomeAppBar extends StatelessWidget implements PreferredSizeWidget {
  const HomeAppBar({
    super.key,
    required this.onSearchTap,
    required this.onNotificationTap,
    required this.onMessageTap,
    this.unreadMessages = 0,
    this.hasNotification = false,
  });

  final VoidCallback onSearchTap;
  final VoidCallback onNotificationTap;
  final VoidCallback onMessageTap;
  final int unreadMessages;
  final bool hasNotification;

  @override
  Size get preferredSize => const Size.fromHeight(96);

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final bg = isDark ? AppColors.surfaceDark : AppColors.surfaceLight;
    final border = isDark ? AppColors.navyLight : AppColors.grey200;

    return Container(
      color: bg,
      child: SafeArea(
        bottom: false,
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 8, 16, 8),
              child: Row(
                children: [
                  _LocationChip(),
                  const Spacer(),
                  _IconBadgeButton(
                    icon: TablerIcons.message_circle,
                    badgeCount: unreadMessages,
                    onTap: onMessageTap,
                  ),
                  const SizedBox(width: 8),
                  _IconBadgeButton(
                    icon: TablerIcons.bell,
                    showDot: hasNotification,
                    onTap: onNotificationTap,
                  ),
                ],
              ),
            ),
            Divider(height: 0.5, color: border),
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 10, 16, 10),
              child: _SearchBar(onTap: onSearchTap),
            ),
          ],
        ),
      ),
    );
  }
}

class _LocationChip extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
      decoration: BoxDecoration(
        color: AppColors.primaryLight,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(mainAxisSize: MainAxisSize.min, children: [
        const Icon(TablerIcons.map_pin, size: 14, color: AppColors.primary),
        const SizedBox(width: 5),
        Text('الرياض',
            style: AppTextStyles.bodySmall
                .copyWith(color: AppColors.primary, fontWeight: FontWeight.w500)),
        const SizedBox(width: 3),
        const Icon(TablerIcons.chevron_down, size: 12, color: AppColors.primary),
      ]),
    );
  }
}

class _IconBadgeButton extends StatelessWidget {
  const _IconBadgeButton({
    required this.icon,
    required this.onTap,
    this.badgeCount = 0,
    this.showDot = false,
  });

  final IconData icon;
  final VoidCallback onTap;
  final int badgeCount;
  final bool showDot;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 36, height: 36,
        decoration: BoxDecoration(
          color: isDark ? AppColors.cardDark : AppColors.grey50,
          shape: BoxShape.circle,
        ),
        child: Stack(children: [
          Center(
            child: Icon(icon, size: 18,
                color: isDark ? Colors.white : AppColors.navy),
          ),
          if (badgeCount > 0)
            Positioned(
              top: 4, left: 4,
              child: Container(
                constraints: const BoxConstraints(minWidth: 16, minHeight: 16),
                padding: const EdgeInsets.symmetric(horizontal: 3),
                decoration: BoxDecoration(
                  color: AppColors.error,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.white, width: 1.5),
                ),
                child: Center(
                  child: Text(badgeCount.toString(),
                      style: AppTextStyles.labelSmall
                          .copyWith(color: Colors.white, fontSize: 8)),
                ),
              ),
            ),
          if (showDot && badgeCount == 0)
            Positioned(
              top: 6, left: 6,
              child: Container(
                width: 8, height: 8,
                decoration: BoxDecoration(
                  color: AppColors.error,
                  shape: BoxShape.circle,
                  border: Border.all(color: Colors.white, width: 1.5),
                ),
              ),
            ),
        ]),
      ),
    );
  }
}

class _SearchBar extends StatelessWidget {
  const _SearchBar({required this.onTap});
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
        decoration: BoxDecoration(
          color: isDark ? AppColors.cardDark : AppColors.grey50,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: AppColors.grey200, width: 0.5),
        ),
        child: Row(children: [
          const Icon(TablerIcons.search, size: 18, color: AppColors.grey500),
          const SizedBox(width: 8),
          Expanded(
            child: Text('ابحث عن سيارات، جوالات، أثاث...',
                style: AppTextStyles.bodySmall),
          ),
          const Icon(TablerIcons.sparkles, size: 15, color: AppColors.ai),
          const SizedBox(width: 8),
          const Icon(TablerIcons.microphone, size: 18, color: AppColors.grey500),
          const SizedBox(width: 8),
          const Icon(TablerIcons.camera, size: 18, color: AppColors.grey500),
        ]),
      ),
    );
  }
}
