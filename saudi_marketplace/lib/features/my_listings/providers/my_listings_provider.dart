import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/supabase/supabase_providers.dart';
import '../data/my_listings_repository.dart';
import '../models/my_listing_item.dart';

enum ListingStatusFilter { all, active, sold, expired }

extension ListingStatusFilterX on ListingStatusFilter {
  String get labelAr => const {
        'all': 'الكل',
        'active': 'نشطة',
        'sold': 'مباعة',
        'expired': 'منتهية',
      }[name]!;

  String? get statusKey => this == ListingStatusFilter.all ? null : name;
}

class MyListingsState {
  const MyListingsState({
    this.listings = const [],
    this.filter = ListingStatusFilter.all,
    this.isLoading = false,
    this.errorMessage,
  });

  final List<MyListingItem> listings;
  final ListingStatusFilter filter;
  final bool isLoading;
  final String? errorMessage;

  List<MyListingItem> get filtered {
    final key = filter.statusKey;
    if (key == null) return listings;
    return listings.where((i) => i.status == key).toList();
  }

  int countFor(ListingStatusFilter f) {
    final key = f.statusKey;
    if (key == null) return listings.length;
    return listings.where((i) => i.status == key).length;
  }

  MyListingsState copyWith({
    List<MyListingItem>? listings,
    ListingStatusFilter? filter,
    bool? isLoading,
    String? errorMessage,
  }) =>
      MyListingsState(
        listings: listings ?? this.listings,
        filter: filter ?? this.filter,
        isLoading: isLoading ?? this.isLoading,
        errorMessage: errorMessage,
      );
}

class MyListingsNotifier extends AutoDisposeNotifier<MyListingsState> {
  bool _disposed = false;

  @override
  MyListingsState build() {
    ref.onDispose(() => _disposed = true);
    Future.microtask(_load);
    return const MyListingsState(isLoading: true);
  }

  Future<void> _load() async {
    final userId = ref.read(currentUserIdProvider);
    if (userId == null) {
      if (!_disposed) {
        state = const MyListingsState(
            errorMessage: 'سجّل الدخول لعرض إعلاناتك');
      }
      return;
    }
    try {
      state = state.copyWith(isLoading: true);
      final items =
          await ref.read(myListingsRepositoryProvider).fetchMyListings(userId);
      if (!_disposed) state = state.copyWith(listings: items, isLoading: false);
    } catch (e) {
      if (!_disposed) {
        state = state.copyWith(isLoading: false, errorMessage: e.toString());
      }
    }
  }

  Future<void> refresh() => _load();

  void setFilter(ListingStatusFilter f) =>
      state = state.copyWith(filter: f);

  Future<void> markAsSold(String id) async {
    final prev = state.listings;
    // تحديث متفائل
    state = state.copyWith(
      listings: [
        for (final i in state.listings)
          if (i.id == id) i.copyWith(status: 'sold') else i,
      ],
    );
    try {
      await ref.read(myListingsRepositoryProvider).markAsSold(id);
    } catch (_) {
      if (!_disposed) state = state.copyWith(listings: prev);
    }
  }

  Future<void> deleteItem(String id) async {
    final prev = state.listings;
    // إزالة متفائلة
    state = state.copyWith(
      listings: [for (final i in state.listings) if (i.id != id) i],
    );
    try {
      await ref.read(myListingsRepositoryProvider).softDelete(id);
    } catch (_) {
      if (!_disposed) state = state.copyWith(listings: prev);
    }
  }
}

final myListingsProvider =
    NotifierProvider.autoDispose<MyListingsNotifier, MyListingsState>(
        MyListingsNotifier.new);
