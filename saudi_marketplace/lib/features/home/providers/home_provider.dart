import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/home_mock_data.dart';
import '../models/listing_model.dart';

class HomeState {
  const HomeState({
    this.selectedCategoryIndex = 0,
    this.featuredListings = HomeMockData.featuredListings,
    this.nearbyListings = HomeMockData.nearbyListings,
    this.suggestedListings = HomeMockData.suggestedListings,
  });

  final int selectedCategoryIndex;
  final List<ListingModel> featuredListings;
  final List<ListingModel> nearbyListings;
  final List<ListingModel> suggestedListings;

  HomeState copyWith({
    int? selectedCategoryIndex,
    List<ListingModel>? featuredListings,
    List<ListingModel>? nearbyListings,
    List<ListingModel>? suggestedListings,
  }) =>
      HomeState(
        selectedCategoryIndex:
            selectedCategoryIndex ?? this.selectedCategoryIndex,
        featuredListings: featuredListings ?? this.featuredListings,
        nearbyListings: nearbyListings ?? this.nearbyListings,
        suggestedListings: suggestedListings ?? this.suggestedListings,
      );
}

class HomeNotifier extends Notifier<HomeState> {
  @override
  HomeState build() => const HomeState();

  void selectCategory(int index) =>
      state = state.copyWith(selectedCategoryIndex: index);

  void toggleFavorite(String id) {
    ListingModel toggle(ListingModel l) =>
        l.id == id ? l.copyWith(isFavorite: !l.isFavorite) : l;

    state = state.copyWith(
      featuredListings: state.featuredListings.map(toggle).toList(),
      nearbyListings: state.nearbyListings.map(toggle).toList(),
      suggestedListings: state.suggestedListings.map(toggle).toList(),
    );
  }
}

final homeProvider =
    NotifierProvider<HomeNotifier, HomeState>(HomeNotifier.new);
