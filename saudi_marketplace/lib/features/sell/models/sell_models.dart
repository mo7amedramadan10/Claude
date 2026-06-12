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

/// صورة محفوظة مسبقاً في التخزين (وضع التعديل)
class ExistingImage {
  const ExistingImage({
    required this.id,
    required this.storagePath,
    required this.publicUrl,
  });

  final String id;
  final String storagePath;
  final String publicUrl;
}

/// بيانات إعلان موجود جاهزة للتعديل
class EditableListingData {
  const EditableListingData({
    required this.id,
    required this.title,
    required this.description,
    this.price,
    required this.isNegotiable,
    required this.condition,
    required this.categoryId,
    required this.cityId,
    this.neighborhood,
    required this.existingImages,
  });

  final String id;
  final String title;
  final String description;
  final double? price;
  final bool isNegotiable;
  final String condition;
  final String categoryId;
  final String cityId;
  final String? neighborhood;
  final List<ExistingImage> existingImages;
}

/// مسودة الإعلان (نشر جديد أو حفظ تعديل)
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
    required this.newImages,
  });

  final String title;
  final String description;
  final double? price;
  final bool isNegotiable;
  final String condition;
  final String categoryId;
  final String cityId;
  final String? neighborhood;
  final List<XFile> newImages; // صور جديدة لم تُرفَع بعد
}
