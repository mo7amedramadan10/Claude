import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../home/models/listing_model.dart';
import '../data/search_mock_data.dart';
import '../models/search_filters.dart';

class SearchState {
  const SearchState({
    this.query = 'سيارات',
    this.results = SearchMockData.results,
    this.resultsCount = SearchMockData.resultsCount,
    this.filters = const SearchFilters(),
  });

  final String query;
  final List<ListingModel> results;
  final int resultsCount;
  final SearchFilters filters;

  SearchState copyWith({
    String? query,
    List<ListingModel>? results,
    int? resultsCount,
    SearchFilters? filters,
  }) =>
      SearchState(
        query: query ?? this.query,
        results: results ?? this.results,
        resultsCount: resultsCount ?? this.resultsCount,
        filters: filters ?? this.filters,
      );
}

class SearchNotifier extends Notifier<SearchState> {
  @override
  SearchState build() => const SearchState();

  void cycleSort() => state = state.copyWith(
        filters: state.filters.copyWith(sort: state.filters.sort.next),
      );

  void selectCity(String city) =>
      state = state.copyWith(filters: state.filters.copyWith(city: city));

  void togglePriceRange(String range) => state = state.copyWith(
        filters: state.filters.copyWith(
          priceRange: () =>
              state.filters.priceRange == range ? null : range,
        ),
      );

  void toggleCondition(String condition) => state = state.copyWith(
        filters: state.filters.copyWith(
          condition: () =>
              state.filters.condition == condition ? null : condition,
        ),
      );

  void resetFilters() =>
      state = state.copyWith(filters: state.filters.reset());

  void toggleFavorite(String id) => state = state.copyWith(
        results: state.results
            .map((l) =>
                l.id == id ? l.copyWith(isFavorite: !l.isFavorite) : l)
            .toList(),
      );
}

final searchProvider =
    NotifierProvider<SearchNotifier, SearchState>(SearchNotifier.new);
