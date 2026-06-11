import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../models/category_grid_item.dart';

abstract final class CategoriesMockData {
  static const categories = [
    CategoryGridItem(icon: TablerIcons.car, name: 'سيارات', count: '٣,٤٢١'),
    CategoryGridItem(
        icon: TablerIcons.building_community, name: 'عقارات', count: '٢,١٠٥'),
    CategoryGridItem(
        icon: TablerIcons.device_mobile, name: 'جوالات', count: '٥,٨٧٠'),
    CategoryGridItem(
        icon: TablerIcons.device_laptop, name: 'إلكترونيات', count: '٤,٢٣٠'),
    CategoryGridItem(icon: TablerIcons.armchair, name: 'أثاث', count: '١,٩٨٠'),
    CategoryGridItem(icon: TablerIcons.shirt, name: 'أزياء', count: '٣,٦٥٠'),
    CategoryGridItem(
        icon: TablerIcons.device_gamepad_2,
        name: 'ألعاب وترفيه',
        count: '١,٥٣٠'),
    CategoryGridItem(
        icon: TablerIcons.briefcase, name: 'وظائف', count: '٨٩٠'),
    CategoryGridItem(icon: TablerIcons.tools, name: 'خدمات', count: '١,٢٤٠'),
    CategoryGridItem(icon: TablerIcons.paw, name: 'حيوانات', count: '٦٧٠'),
    CategoryGridItem(
        icon: TablerIcons.ball_football, name: 'رياضة', count: '٩٤٠'),
    CategoryGridItem(icon: TablerIcons.dots, name: 'أخرى', count: '٢,٣١٠'),
  ];
}
