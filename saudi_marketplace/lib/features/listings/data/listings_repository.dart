import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:supabase_flutter/supabase_flutter.dart';

import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../models/listing_model.dart';

// ── Column sets ──────────────────────────────────────────────
const _feedColumns = '''
  id, seller_id, category_id, city_id, title, description, price, currency,
  is_price_negotiable, is_price_hidden, condition, status,
  neighborhood, is_verified, is_featured, views_count,
  favorites_count, created_at, updated_at, expires_at, deleted_at,
  listing_images(id, listing_id, storage_path, is_primary, sort_order, created_at),
  city:cities(id, name_ar, name_en, is_active, sort_order, created_at)
''';

const _detailColumns = '''
  *,
  listing_images(*),
  listing_specs(*),
  city:cities(id, name_ar, name_en, is_active, sort_order, created_at),
  seller:public_profiles!seller_id(
    id, full_name, avatar_url, bio, city_id, is_nafath_verified,
    rating_avg, total_reviews, completed_deals,
    active_listings_count, last_seen_at, created_at
  )
''';

abstract class ListingsRepository {
  Future<List<ListingModel>> fetchFeed({
    String? categoryId,
    String? cityId,
    int limit = 20,
    int offset = 0,
  });

  Future<List<ListingModel>> fetchFeatured({int limit = 10});

  Future<List<ListingModel>> fetchBySeller(String sellerId);

  Future<List<ListingModel>> search(
    String query, {
    String? categoryId,
    String? cityId,
    double? minPrice,
    double? maxPrice,
    String? condition,
    int limit = 20,
    int offset = 0,
  });

  Future<ListingModel> fetchById(String id);

  Future<String> create(Map<String, dynamic> data);

  Future<void> update(String id, Map<String, dynamic> data);

  Future<void> softDelete(String id);

  Future<void> incrementViews(String id);

  Stream<List<ListingModel>> watchFeatured({int limit = 10});
}

class SupabaseListingsRepository implements ListingsRepository {
  SupabaseListingsRepository(this._db);
  final SupabaseClient _db;

  @override
  Future<List<ListingModel>> fetchFeed({
    String? categoryId,
    String? cityId,
    int limit = 20,
    int offset = 0,
  }) async {
    try {
      // الفلاتر تُضاف قبل order/range (قيد chaining في supabase v2)
      var q = _db
          .from('listings')
          .select(_feedColumns)
          .eq('status', 'active')
          .filter('deleted_at', 'is', null);

      if (categoryId != null) q = q.eq('category_id', categoryId);
      if (cityId != null)     q = q.eq('city_id', cityId);

      final data = await q
          .order('created_at', ascending: false)
          .range(offset, offset + limit - 1);
      return data.map(ListingModel.fromJson).toList();
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<List<ListingModel>> fetchFeatured({int limit = 10}) async {
    try {
      final data = await _db
          .from('listings')
          .select(_feedColumns)
          .eq('status', 'active')
          .eq('is_featured', true)
          .filter('deleted_at', 'is', null)
          .order('created_at', ascending: false)
          .limit(limit);
      return data.map(ListingModel.fromJson).toList();
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<List<ListingModel>> fetchBySeller(String sellerId) async {
    try {
      final data = await _db
          .from('listings')
          .select(_feedColumns)
          .eq('seller_id', sellerId)
          .filter('deleted_at', 'is', null)
          .order('created_at', ascending: false);
      return data.map(ListingModel.fromJson).toList();
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<List<ListingModel>> search(
    String query, {
    String? categoryId,
    String? cityId,
    double? minPrice,
    double? maxPrice,
    String? condition,
    int limit = 20,
    int offset = 0,
  }) async {
    try {
      var q = _db
          .from('listings')
          .select(_feedColumns)
          .eq('status', 'active')
          .filter('deleted_at', 'is', null)
          .ilike('title', '%$query%');

      if (categoryId != null) q = q.eq('category_id', categoryId);
      if (cityId != null)     q = q.eq('city_id', cityId);
      if (condition != null)  q = q.eq('condition', condition);
      if (minPrice != null)   q = q.gte('price', minPrice);
      if (maxPrice != null)   q = q.lte('price', maxPrice);

      final data = await q
          .order('created_at', ascending: false)
          .range(offset, offset + limit - 1);
      return data.map(ListingModel.fromJson).toList();
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<ListingModel> fetchById(String id) async {
    try {
      final data = await _db
          .from('listings')
          .select(_detailColumns)
          .eq('id', id)
          .single();
      return ListingModel.fromJson(data);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<String> create(Map<String, dynamic> data) async {
    try {
      final row = await _db
          .from('listings')
          .insert(data)
          .select('id')
          .single();
      return row['id'] as String;
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> update(String id, Map<String, dynamic> data) async {
    try {
      await _db.from('listings').update(data).eq('id', id);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> softDelete(String id) =>
      update(id, {'deleted_at': DateTime.now().toIso8601String()});

  @override
  Future<void> incrementViews(String id) async {
    try {
      await _db.rpc('increment_listing_views', params: {'p_listing_id': id});
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Stream<List<ListingModel>> watchFeatured({int limit = 10}) {
    return _db
        .from('listings')
        .stream(primaryKey: ['id'])
        .eq('is_featured', true)
        .order('created_at', ascending: false)
        .limit(limit)
        .map((rows) => rows
            .where((r) => r['status'] == 'active' && r['deleted_at'] == null)
            .map(ListingModel.fromJson)
            .toList());
  }
}

final listingsRepositoryProvider = Provider<ListingsRepository>((ref) {
  return SupabaseListingsRepository(ref.watch(supabaseClientProvider));
});
