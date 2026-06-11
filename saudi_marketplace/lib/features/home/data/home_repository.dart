import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/config/env.dart';
import '../../auth/data/profile_repository.dart';
import '../../favorites/data/favorites_repository.dart';
import '../../listings/data/listings_repository.dart';
import '../models/listing_model.dart';
import '../models/seller_model.dart';
import 'home_mock_data.dart';
import 'home_ui_mappers.dart';

class HomeFeedData {
  const HomeFeedData({
    required this.featured,
    required this.nearby,
    required this.suggested,
    required this.sellers,
  });

  final List<ListingModel> featured;
  final List<ListingModel> nearby;
  final List<ListingModel> suggested;
  final List<SellerModel> sellers;
}

abstract class HomeRepository {
  Future<HomeFeedData> fetchHomeFeed({String? userId});
}

/// مصدر تجريبي — يُستخدم تلقائياً قبل إعداد Supabase
class MockHomeRepository implements HomeRepository {
  const MockHomeRepository();

  @override
  Future<HomeFeedData> fetchHomeFeed({String? userId}) async {
    return const HomeFeedData(
      featured: HomeMockData.featuredListings,
      nearby: HomeMockData.nearbyListings,
      suggested: HomeMockData.suggestedListings,
      sellers: HomeMockData.sellers,
    );
  }
}

class SupabaseHomeRepository implements HomeRepository {
  SupabaseHomeRepository(this._listings, this._profiles, this._favorites);

  final ListingsRepository _listings;
  final ProfileRepository _profiles;
  final FavoritesRepository _favorites;

  @override
  Future<HomeFeedData> fetchHomeFeed({String? userId}) async {
    // تشغيل الطلبات بالتوازي ثم انتظارها
    final featuredFuture = _listings.fetchFeatured(limit: 10);
    final feedFuture = _listings.fetchFeed(limit: 20);
    final sellersFuture = _profiles.fetchVerifiedSellers(limit: 10);
    final favoritesFuture =
        userId != null ? _favorites.fetchUserFavorites(userId) : null;

    final featured = await featuredFuture;
    final feed = await feedFuture;
    final sellers = await sellersFuture;
    final favoriteIds =
        (await favoritesFuture)?.map((l) => l.id).toSet() ?? const <String>{};

    // V1: «قريب منك» = أحدث 6 إعلانات، «مقترحة لك» = البقية
    // (الترتيب الجغرافي PostGIS يُضاف في مرحلة لاحقة عبر RPC)
    return HomeFeedData(
      featured: [
        for (final l in featured)
          l.toUiModel(isFavorite: favoriteIds.contains(l.id)),
      ],
      nearby: [for (final l in feed.take(6)) l.toUiModel()],
      suggested: [
        for (final l in feed.skip(6))
          l.toUiModel(isFavorite: favoriteIds.contains(l.id)),
      ],
      sellers: [for (final s in sellers) s.toUiModel()],
    );
  }
}

final homeRepositoryProvider = Provider<HomeRepository>((ref) {
  if (!Env.isConfigured) return const MockHomeRepository();
  return SupabaseHomeRepository(
    ref.watch(listingsRepositoryProvider),
    ref.watch(profileRepositoryProvider),
    ref.watch(favoritesRepositoryProvider),
  );
});
