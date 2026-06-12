import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/config/env.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../../favorites/data/favorites_repository.dart';
import '../../saved/providers/saved_provider.dart';
import '../data/home_repository.dart';
import '../models/listing_model.dart';
import '../models/seller_model.dart';

class HomeState {
  const HomeState({
    this.selectedCategoryIndex = 0,
    this.isLoading = false,
    this.errorMessage,
    this.featuredListings = const [],
    this.nearbyListings = const [],
    this.suggestedListings = const [],
    this.sellers = const [],
  });

  final int selectedCategoryIndex;
  final bool isLoading;
  final String? errorMessage;
  final List<ListingModel> featuredListings;
  final List<ListingModel> nearbyListings;
  final List<ListingModel> suggestedListings;
  final List<SellerModel> sellers;

  HomeState copyWith({
    int? selectedCategoryIndex,
    bool? isLoading,
    String? errorMessage,
    List<ListingModel>? featuredListings,
    List<ListingModel>? nearbyListings,
    List<ListingModel>? suggestedListings,
    List<SellerModel>? sellers,
  }) =>
      HomeState(
        selectedCategoryIndex:
            selectedCategoryIndex ?? this.selectedCategoryIndex,
        isLoading: isLoading ?? this.isLoading,
        errorMessage: errorMessage,
        featuredListings: featuredListings ?? this.featuredListings,
        nearbyListings: nearbyListings ?? this.nearbyListings,
        suggestedListings: suggestedListings ?? this.suggestedListings,
        sellers: sellers ?? this.sellers,
      );
}

class HomeNotifier extends Notifier<HomeState> {
  @override
  HomeState build() {
    Future.microtask(refresh);
    return const HomeState(isLoading: true);
  }

  Future<void> refresh() async {
    state = state.copyWith(isLoading: true);
    try {
      final userId = ref.read(currentUserIdProvider);
      final feed = await ref
          .read(homeRepositoryProvider)
          .fetchHomeFeed(userId: userId);
      state = state.copyWith(
        isLoading: false,
        featuredListings: feed.featured,
        nearbyListings: feed.nearby,
        suggestedListings: feed.suggested,
        sellers: feed.sellers,
      );
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  void selectCategory(int index) =>
      state = state.copyWith(selectedCategoryIndex: index);

  void toggleFavorite(String id) {
    final wasFavorite = _isFavorite(id);
    _applyFavorite(id, !wasFavorite);

    final userId = ref.read(currentUserIdProvider);
    if (!Env.isConfigured || userId == null) return;

    final repo = ref.read(favoritesRepositoryProvider);
    final action =
        wasFavorite ? repo.remove(userId, id) : repo.add(userId, id);
    action.then((_) {
      // تحديث شاشة المحفوظات بعد نجاح التغيير
      ref.invalidate(savedProvider);
    }).catchError((_) {
      // تراجع محلي إذا فشل الطلب
      _applyFavorite(id, wasFavorite);
    });
  }

  bool _isFavorite(String id) {
    for (final list in [
      state.featuredListings,
      state.nearbyListings,
      state.suggestedListings,
    ]) {
      for (final l in list) {
        if (l.id == id) return l.isFavorite;
      }
    }
    return false;
  }

  void _applyFavorite(String id, bool value) {
    ListingModel apply(ListingModel l) =>
        l.id == id ? l.copyWith(isFavorite: value) : l;

    state = state.copyWith(
      featuredListings: state.featuredListings.map(apply).toList(),
      nearbyListings: state.nearbyListings.map(apply).toList(),
      suggestedListings: state.suggestedListings.map(apply).toList(),
    );
  }
}

final homeProvider =
    NotifierProvider<HomeNotifier, HomeState>(HomeNotifier.new);
