import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:supabase_flutter/supabase_flutter.dart';

import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../../listings/models/listing_model.dart';

// Listing columns needed for the Saved screen cards
const _savedListingColumns = '''
  id, seller_id, category_id, city_id, title, price, currency,
  is_price_negotiable, is_price_hidden, condition, status,
  neighborhood, is_verified, is_featured, views_count,
  favorites_count, created_at, updated_at, expires_at, deleted_at,
  listing_images(id, storage_path, is_primary, sort_order)
''';

abstract class FavoritesRepository {
  Future<List<ListingModel>> fetchUserFavorites(String userId);
  Future<bool> isFavorited(String userId, String listingId);
  Future<void> add(String userId, String listingId);
  Future<void> remove(String userId, String listingId);
  Stream<List<String>> watchFavoritedIds(String userId);
}

class SupabaseFavoritesRepository implements FavoritesRepository {
  SupabaseFavoritesRepository(this._db);
  final SupabaseClient _db;

  @override
  Future<List<ListingModel>> fetchUserFavorites(String userId) async {
    try {
      final data = await _db
          .from('favorites')
          .select('id, created_at, listings($_savedListingColumns)')
          .eq('user_id', userId)
          .order('created_at', ascending: false);

      return (data as List)
          .map((row) => ListingModel.fromJson(row['listings'] as Map<String, dynamic>))
          .where((l) => !l.isDeleted && l.status != 'rejected')
          .toList();
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<bool> isFavorited(String userId, String listingId) async {
    try {
      final data = await _db
          .from('favorites')
          .select('id')
          .eq('user_id', userId)
          .eq('listing_id', listingId)
          .limit(1);
      return (data as List).isNotEmpty;
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> add(String userId, String listingId) async {
    try {
      await _db.from('favorites').insert({
        'user_id': userId,
        'listing_id': listingId,
      });
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> remove(String userId, String listingId) async {
    try {
      await _db
          .from('favorites')
          .delete()
          .eq('user_id', userId)
          .eq('listing_id', listingId);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Stream<List<String>> watchFavoritedIds(String userId) {
    return _db
        .from('favorites')
        .stream(primaryKey: ['id'])
        .eq('user_id', userId)
        .map((rows) => rows.map((r) => r['listing_id'] as String).toList());
  }
}

final favoritesRepositoryProvider = Provider<FavoritesRepository>((ref) {
  return SupabaseFavoritesRepository(ref.watch(supabaseClientProvider));
});
