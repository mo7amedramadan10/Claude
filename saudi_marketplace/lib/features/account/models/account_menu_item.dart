import 'package:flutter/widgets.dart';

/// Icon tile tint variants used across the account menu rows.
enum MenuTint { green, navy, amber, red }

class AccountMenuItem {
  const AccountMenuItem({
    required this.icon,
    required this.label,
    this.tint = MenuTint.green,
    this.value,
    this.isDanger = false,
    this.route,
    this.isTabRoute = false,
  });

  final IconData icon;
  final String label;
  final MenuTint tint;

  /// Optional trailing value text (e.g. "١٢" or "موثّق").
  final String? value;

  /// Danger rows are red and have no chevron (e.g. logout).
  final bool isDanger;

  /// Route to navigate to when tapped; null means not wired yet.
  final String? route;

  /// Tab routes (shell branches) navigate with go instead of push.
  final bool isTabRoute;
}

class AccountMenuGroup {
  const AccountMenuGroup({required this.title, required this.items});

  final String title;
  final List<AccountMenuItem> items;
}

class AccountStat {
  const AccountStat({
    required this.value,
    required this.label,
    this.showStar = false,
  });

  final String value;
  final String label;
  final bool showStar;
}
