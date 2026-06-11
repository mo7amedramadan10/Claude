import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/config/env.dart';
import '../../favorites/data/favorites_repository.dart';
import '../../listings/data/listings_repository.dart';
import '../models/listing_detail_model.dart';
import 'detail_ui_mapper.dart';
import 'listing_detail_mock_data.dart';

class ListingDetailResult {
  const ListingDetailResult({required this.detail, required this.isFavorite});

  final ListingDetailModel detail;
  final bool isFavorite;
}

abstract class ListingDetailRepository {
  Future<ListingDetailResult> fetchDetail(String listingId, {String? userId});

  /// زيادة عداد المشاهدات — fire and forget
  void registerView(String listingId);
}

/// مصدر تجريبي — يُستخدم تلقائياً قبل إعداد Supabase
class MockListingDetailRepository implements ListingDetailRepository {
  const MockListingDetailRepository();

  @override
  Future<ListingDetailResult> fetchDetail(
    String listingId, {
    String? userId,
  }) async {
    return const ListingDetailResult(
      detail: ListingDetailMockData.detail,
      isFavorite: false,
    );
  }

  @override
  void registerView(String listingId) {}
}

class SupabaseListingDetailRepository implements ListingDetailRepository {
  SupabaseListingDetailRepository(this._listings, this._favorites);

  final ListingsRepository _listings;
  final FavoritesRepository _favorites;

  @override
  Future<ListingDetailResult> fetchDetail(
    String listingId, {
    String? userId,
  }) async {
    final listingFuture = _listings.fetchById(listingId);
    final favoriteFuture = userId != null
        ? _favorites.isFavorited(userId, listingId)
        : Future.value(false);

    final listing = await listingFuture;
    final isFavorite = await favoriteFuture;

    return ListingDetailResult(
      detail: listing.toDetailModel(),
      isFavorite: isFavorite,
    );
  }

  @override
  void registerView(String listingId) {
    // تجاهل الفشل — المشاهدات ليست حرجة
    _listings.incrementViews(listingId).ignore();
  }
}

final listingDetailRepositoryProvider =
    Provider<ListingDetailRepository>((ref) {
  if (!Env.isConfigured) return const MockListingDetailRepository();
  return SupabaseListingDetailRepository(
    ref.watch(listingsRepositoryProvider),
    ref.watch(favoritesRepositoryProvider),
  );
});
