import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/config/env.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../../favorites/data/favorites_repository.dart';
import '../data/listing_detail_mock_data.dart';
import '../data/listing_detail_repository.dart';
import '../models/listing_detail_model.dart';

class ListingDetailState {
  const ListingDetailState({
    required this.detail,
    this.activeImageIndex = 0,
    this.isFavorite = false,
    this.isLoading = false,
    this.errorMessage,
  });

  final ListingDetailModel detail;
  final int activeImageIndex;
  final bool isFavorite;
  final bool isLoading;
  final String? errorMessage;

  ListingDetailState copyWith({
    ListingDetailModel? detail,
    int? activeImageIndex,
    bool? isFavorite,
    bool? isLoading,
    String? errorMessage,
  }) =>
      ListingDetailState(
        detail: detail ?? this.detail,
        activeImageIndex: activeImageIndex ?? this.activeImageIndex,
        isFavorite: isFavorite ?? this.isFavorite,
        isLoading: isLoading ?? this.isLoading,
        errorMessage: errorMessage,
      );
}

class ListingDetailNotifier
    extends AutoDisposeFamilyNotifier<ListingDetailState, String> {
  @override
  ListingDetailState build(String arg) {
    if (Env.isConfigured) {
      Future.microtask(load);
    }
    return ListingDetailState(
      detail: ListingDetailMockData.detail,
      isLoading: Env.isConfigured,
    );
  }

  Future<void> load() async {
    state = state.copyWith(isLoading: true);
    try {
      final repo = ref.read(listingDetailRepositoryProvider);
      final userId = ref.read(currentUserIdProvider);
      final result = await repo.fetchDetail(arg, userId: userId);
      repo.registerView(arg);
      state = state.copyWith(
        detail: result.detail,
        isFavorite: result.isFavorite,
        isLoading: false,
      );
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  void selectImage(int index) =>
      state = state.copyWith(activeImageIndex: index);

  void toggleFavorite() {
    final wasFavorite = state.isFavorite;
    state = state.copyWith(isFavorite: !wasFavorite);

    final userId = ref.read(currentUserIdProvider);
    if (!Env.isConfigured || userId == null) return;

    final repo = ref.read(favoritesRepositoryProvider);
    final action =
        wasFavorite ? repo.remove(userId, arg) : repo.add(userId, arg);
    // تراجع محلي إذا فشل الطلب
    action.catchError((_) {
      state = state.copyWith(isFavorite: wasFavorite);
    });
  }
}

final listingDetailProvider = NotifierProvider.autoDispose
    .family<ListingDetailNotifier, ListingDetailState, String>(
        ListingDetailNotifier.new);
