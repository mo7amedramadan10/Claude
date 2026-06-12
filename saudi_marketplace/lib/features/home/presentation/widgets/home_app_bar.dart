import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';

class HomeAppBar extends StatelessWidget {
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
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final bg = isDark ? AppColors.surfaceDark : AppColors.surfaceLight;

    return Container(
      decoration: BoxDecoration(
        color: bg,
        border: const Border(
          bottom: BorderSide(color: AppColors.grey100, width: 1),
        ),
      ),
      child: SafeArea(
        bottom: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(16, 8, 16, 12),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(children: [
                const _BrandLockup(),
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
              ]),
              const SizedBox(height: 10),
              const _LocationPill(),
              const SizedBox(height: 10),
              _SearchField(onTap: onSearchTap),
            ],
          ),
        ),
      ),
    );
  }
}

// ─── Brand lockup: mark + wordmark ───────────────────────────────────────────

class _BrandLockup extends StatelessWidget {
  const _BrandLockup();

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Row(children: [
      Container(
        width: 36,
        height: 36,
        decoration: BoxDecoration(
          gradient: const LinearGradient(
            begin: Alignment.topRight,
            end: Alignment.bottomLeft,
            colors: [AppColors.primary, AppColors.primaryDark],
          ),
          borderRadius: BorderRadius.circular(11),
          boxShadow: const [
            BoxShadow(
              color: Color(0x521A7A4A),
              blurRadius: 12,
              offset: Offset(0, 4),
            ),
          ],
        ),
        child: const Icon(TablerIcons.arrows_exchange,
            size: 21, color: Colors.white),
      ),
      const SizedBox(width: 9),
      Text(
        'بيكيا',
        style: AppTextStyles.displayMedium.copyWith(
          fontSize: 24,
          color: isDark ? Colors.white : AppColors.navy,
          height: 1,
        ),
      ),
    ]);
  }
}

class _LocationPill extends StatelessWidget {
  const _LocationPill();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 11, vertical: 6),
      decoration: BoxDecoration(
        color: AppColors.primaryLight,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(mainAxisSize: MainAxisSize.min, children: [
        const Icon(TablerIcons.map_pin, size: 15, color: AppColors.primaryDark),
        const SizedBox(width: 5),
        Text('الرياض',
            style: AppTextStyles.bodySmall.copyWith(
                color: AppColors.primaryDark, fontWeight: FontWeight.w600)),
        const SizedBox(width: 5),
        const Icon(TablerIcons.chevron_down,
            size: 13, color: AppColors.primaryDark),
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
        width: 38,
        height: 38,
        decoration: BoxDecoration(
          color: isDark ? AppColors.cardDark : AppColors.grey50,
          shape: BoxShape.circle,
        ),
        child: Stack(children: [
          Center(
            child: Icon(icon,
                size: 20, color: isDark ? Colors.white : AppColors.navy),
          ),
          if (badgeCount > 0)
            Positioned(
              top: 3,
              left: 3,
              child: Container(
                constraints: const BoxConstraints(minWidth: 17, minHeight: 17),
                padding: const EdgeInsets.symmetric(horizontal: 3),
                decoration: BoxDecoration(
                  color: AppColors.error,
                  borderRadius: BorderRadius.circular(9),
                  border: Border.all(color: Colors.white, width: 2),
                ),
                child: Center(
                  child: Text('$badgeCount',
                      style: AppTextStyles.labelSmall
                          .copyWith(color: Colors.white, fontSize: 9)),
                ),
              ),
            ),
          if (showDot && badgeCount == 0)
            Positioned(
              top: 6,
              left: 8,
              child: Container(
                width: 9,
                height: 9,
                decoration: BoxDecoration(
                  color: AppColors.error,
                  shape: BoxShape.circle,
                  border: Border.all(color: Colors.white, width: 2),
                ),
              ),
            ),
        ]),
      ),
    );
  }
}

class _SearchField extends StatelessWidget {
  const _SearchField({required this.onTap});

  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 13, vertical: 11),
        decoration: BoxDecoration(
          color: isDark ? AppColors.cardDark : AppColors.grey50,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: AppColors.grey200, width: 1),
        ),
        child: Row(children: [
          const Icon(TablerIcons.search, size: 19, color: AppColors.grey600),
          const SizedBox(width: 9),
          Expanded(
            child: Text(
              'ابحث عن سيارات، جوالات، أثاث...',
              style: AppTextStyles.bodySmall.copyWith(
                  color: AppColors.grey500,
                  fontSize: 13,
                  fontWeight: FontWeight.w500),
            ),
          ),
          const Icon(TablerIcons.microphone,
              size: 19, color: AppColors.grey600),
          const SizedBox(width: 12),
          const Icon(TablerIcons.camera, size: 19, color: AppColors.grey600),
        ]),
      ),
    );
  }
}
