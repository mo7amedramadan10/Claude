import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/listing_detail_mock_data.dart';
import '../models/listing_detail_model.dart';

class ListingDetailState {
  const ListingDetailState({
    required this.detail,
    this.activeImageIndex = 0,
    this.isFavorite = false,
  });

  final ListingDetailModel detail;
  final int activeImageIndex;
  final bool isFavorite;

  ListingDetailState copyWith({int? activeImageIndex, bool? isFavorite}) =>
      ListingDetailState(
        detail: detail,
        activeImageIndex: activeImageIndex ?? this.activeImageIndex,
        isFavorite: isFavorite ?? this.isFavorite,
      );
}

class ListingDetailNotifier
    extends AutoDisposeFamilyNotifier<ListingDetailState, String> {
  @override
  ListingDetailState build(String arg) =>
      const ListingDetailState(detail: ListingDetailMockData.detail);

  void selectImage(int index) =>
      state = state.copyWith(activeImageIndex: index);

  void toggleFavorite() =>
      state = state.copyWith(isFavorite: !state.isFavorite);
}

final listingDetailProvider = NotifierProvider.autoDispose
    .family<ListingDetailNotifier, ListingDetailState, String>(
        ListingDetailNotifier.new);
