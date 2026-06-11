import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/config/env.dart';
import '../../../core/supabase/storage_urls.dart';
import '../../../core/utils/number_formatter.dart';
import '../../../core/utils/relative_time.dart';
import '../../favorites/data/favorites_repository.dart';
import '../../listings/models/listing_model.dart' as backend;
import '../models/saved_item.dart';
import 'saved_mock_data.dart';

extension SavedItemMapper on backend.ListingModel {
  SavedItem toSavedItem() {
    final image = primaryImage;
    return SavedItem(
      id: id,
      imageUrl:
          image != null ? StorageUrls.listingImage(image.storagePath) : '',
      title: title,
      price: NumberFormatter.arabicPrice(price ?? 0),
      city: city?.nameAr ?? '',
      postedAt: RelativeTime.format(createdAt),
      isVerified: isVerified,
    );
  }
}

abstract class SavedRepository {
  Future<List<SavedItem>> fetchSavedItems(String? userId);
  Future<void> remove(String userId, String listingId);
}

/// مصدر تجريبي — يُستخدم تلقائياً قبل إعداد Supabase
class MockSavedRepository implements SavedRepository {
  const MockSavedRepository();

  @override
  Future<List<SavedItem>> fetchSavedItems(String? userId) async =>
      [...SavedMockData.items];

  @override
  Future<void> remove(String userId, String listingId) async {}
}

class SupabaseSavedRepository implements SavedRepository {
  SupabaseSavedRepository(this._favorites);
  final FavoritesRepository _favorites;

  @override
  Future<List<SavedItem>> fetchSavedItems(String? userId) async {
    // بدون تسجيل دخول لا توجد مفضلات
    if (userId == null) return const [];
    final listings = await _favorites.fetchUserFavorites(userId);
    return [for (final l in listings) l.toSavedItem()];
  }

  @override
  Future<void> remove(String userId, String listingId) =>
      _favorites.remove(userId, listingId);
}

final savedRepositoryProvider = Provider<SavedRepository>((ref) {
  if (!Env.isConfigured) return const MockSavedRepository();
  return SupabaseSavedRepository(ref.watch(favoritesRepositoryProvider));
});
