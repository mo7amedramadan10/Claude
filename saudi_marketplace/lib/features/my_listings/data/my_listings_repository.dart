import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:supabase_flutter/supabase_flutter.dart';

import '../../../core/config/env.dart';
import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/storage_urls.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../../listings/models/listing_model.dart';
import '../models/my_listing_item.dart';
import 'my_listings_mock_data.dart';

// اسم مستعار داخلي — يستخدم كل الأعمدة ويضيف صور الإعلان
const _myListingsColumns = '''
  id, seller_id, category_id, city_id, title, description, price, currency,
  is_price_negotiable, is_price_hidden, condition, status, neighborhood,
  is_verified, is_featured, safety_note, views_count, favorites_count,
  contact_count, sold_to_id, sold_at, expires_at, deleted_at,
  created_at, updated_at,
  listing_images(id, listing_id, storage_path, is_primary, sort_order, created_at)
''';

extension _ListingToMyItem on ListingModel {
  MyListingItem toMyItem() {
    final img = primaryImage;
    return MyListingItem(
      id: id,
      title: title,
      price: price,
      status: status,
      imageUrl: img != null ? StorageUrls.listingImage(img.storagePath) : '',
      viewsCount: viewsCount,
      favoritesCount: favoritesCount,
      createdAt: createdAt,
      isVerified: isVerified,
    );
  }
}

abstract class MyListingsRepository {
  Future<List<MyListingItem>> fetchMyListings(String userId);
  Future<void> markAsSold(String listingId);
  Future<void> softDelete(String listingId);
}

class MockMyListingsRepository implements MyListingsRepository {
  final _items = List<MyListingItem>.from(MyListingsMockData.items);

  /// تُستخدم من شاشة «أضف إعلاناً» في الوضع التجريبي
  void addItem(MyListingItem item) => _items.insert(0, item);

  /// تُستخدم من شاشة «تعديل الإعلان» في الوضع التجريبي
  void updateItem(String id, MyListingItem Function(MyListingItem) updater) {
    final index = _items.indexWhere((i) => i.id == id);
    if (index != -1) _items[index] = updater(_items[index]);
  }

  @override
  Future<List<MyListingItem>> fetchMyListings(String userId) async {
    await Future<void>.delayed(const Duration(milliseconds: 600));
    return List.from(_items);
  }

  @override
  Future<void> markAsSold(String listingId) async =>
      Future<void>.delayed(const Duration(milliseconds: 300));

  @override
  Future<void> softDelete(String listingId) async {
    await Future<void>.delayed(const Duration(milliseconds: 300));
    _items.removeWhere((i) => i.id == listingId);
  }
}

class SupabaseMyListingsRepository implements MyListingsRepository {
  SupabaseMyListingsRepository(this._db);
  final SupabaseClient _db;

  @override
  Future<List<MyListingItem>> fetchMyListings(String userId) async {
    try {
      final data = await _db
          .from('listings')
          .select(_myListingsColumns)
          .eq('seller_id', userId)
          .filter('deleted_at', 'is', null)
          .order('created_at', ascending: false);
      return (data as List)
          .map((r) => ListingModel.fromJson(r as Map<String, dynamic>).toMyItem())
          .toList();
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> markAsSold(String listingId) async {
    try {
      await _db.from('listings').update({
        'status': 'sold',
        'sold_at': DateTime.now().toUtc().toIso8601String(),
      }).eq('id', listingId);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> softDelete(String listingId) async {
    try {
      await _db.from('listings').update({
        'deleted_at': DateTime.now().toUtc().toIso8601String(),
      }).eq('id', listingId);
    } catch (e) {
      throw mapException(e);
    }
  }
}

final myListingsRepositoryProvider = Provider<MyListingsRepository>((ref) {
  if (!Env.isConfigured) return MockMyListingsRepository();
  return SupabaseMyListingsRepository(ref.watch(supabaseClientProvider));
});
