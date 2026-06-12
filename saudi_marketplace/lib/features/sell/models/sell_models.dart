import 'package:image_picker/image_picker.dart';

/// خيار في قوائم الاختيار (فئة / مدينة)
class LookupOption {
  const LookupOption({required this.id, required this.label});

  final String id;
  final String label;
}

/// حالة المنتج — القيم مطابقة لقيد condition في قاعدة البيانات
abstract final class ListingCondition {
  static const values = ['excellent', 'very_good', 'good', 'for_parts'];

  static const labels = {
    'excellent': 'ممتازة',
    'very_good': 'جيدة جداً',
    'good': 'جيدة',
    'for_parts': 'قطع غيار',
  };
}

/// مسودة الإعلان الجاهزة للإرسال
class SellDraft {
  const SellDraft({
    required this.title,
    required this.description,
    this.price,
    required this.isNegotiable,
    required this.condition,
    required this.categoryId,
    required this.cityId,
    this.neighborhood,
    required this.images,
  });

  final String title;
  final String description;
  final double? price;
  final bool isNegotiable;
  final String condition;
  final String categoryId;
  final String cityId;
  final String? neighborhood;
  final List<XFile> images;
}
