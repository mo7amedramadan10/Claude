import 'package:flutter/widgets.dart';

class CategoryGridItem {
  const CategoryGridItem({
    required this.icon,
    required this.name,
    required this.count,
  });

  final IconData icon;
  final String name;
  final String count;
}
