import '../models/my_listing_item.dart';

abstract final class MyListingsMockData {
  // قائمة فارغة — المستخدم الجديد يبدأ بدون إعلانات
  // تُضاف الإعلانات عبر MockMyListingsRepository.addItem() عند النشر
  static final items = <MyListingItem>[];
}
