import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../core/router/app_routes.dart';
import '../models/account_menu_item.dart';

abstract final class AccountMockData {
  static const userName = 'عبدالله الشمري';
  static const memberSince = 'عضو منذ ٢٠٢٢';
  static const avatarUrl =
      'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=160&q=80';

  static const stats = [
    AccountStat(value: '١٢', label: 'إعلاناتي'),
    AccountStat(value: '٤٨', label: 'صفقة مكتملة'),
    AccountStat(value: '4.9', label: 'تقييمي', showStar: true),
  ];

  static const groups = [
    AccountMenuGroup(
      title: 'نشاطي',
      items: [
        AccountMenuItem(
          icon: TablerIcons.clipboard_text,
          label: 'إعلاناتي',
          value: '١٢',
          route: AppRoutes.myListings,
        ),
        AccountMenuItem(icon: TablerIcons.receipt_2, label: 'صفقاتي'),
        AccountMenuItem(
          icon: TablerIcons.heart,
          label: 'المحفوظات',
          tint: MenuTint.red,
          route: AppRoutes.favorites,
          isTabRoute: true,
        ),
        AccountMenuItem(
            icon: TablerIcons.search, label: 'عمليات بحث محفوظة'),
      ],
    ),
    AccountMenuGroup(
      title: 'الحساب',
      items: [
        AccountMenuItem(
          icon: TablerIcons.wallet,
          label: 'المحفظة والمدفوعات',
          tint: MenuTint.navy,
        ),
        AccountMenuItem(
          icon: TablerIcons.rosette_discount_check_filled,
          label: 'التوثيق · نفاذ وطني',
          value: 'موثّق',
        ),
        AccountMenuItem(
          icon: TablerIcons.bell,
          label: 'الإشعارات',
          tint: MenuTint.navy,
          route: AppRoutes.notifications,
        ),
        AccountMenuItem(
          icon: TablerIcons.settings,
          label: 'الإعدادات',
          tint: MenuTint.navy,
        ),
      ],
    ),
    AccountMenuGroup(
      title: 'أخرى',
      items: [
        AccountMenuItem(
          icon: TablerIcons.headset,
          label: 'المساعدة والدعم',
          tint: MenuTint.amber,
        ),
        AccountMenuItem(
          icon: TablerIcons.info_circle,
          label: 'عن بيكيا',
          tint: MenuTint.amber,
        ),
        AccountMenuItem(
          icon: TablerIcons.logout,
          label: 'تسجيل الخروج',
          tint: MenuTint.red,
          isDanger: true,
        ),
      ],
    ),
  ];
}
