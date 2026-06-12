import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';

import '../../../core/config/env.dart';
import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../../my_listings/data/my_listings_repository.dart';
import '../../my_listings/models/my_listing_item.dart';
import '../../my_listings/providers/my_listings_provider.dart';
import '../data/sell_repository.dart';
import '../models/sell_models.dart';

const maxListingImages = 10;

class SellState {
  const SellState({
    this.images = const [],
    this.categories = const [],
    this.cities = const [],
    this.selectedCategory,
    this.selectedCity,
    this.condition = 'good',
    this.isNegotiable = true,
    this.isSubmitting = false,
    this.errorMessage,
  });

  final List<XFile> images;
  final List<LookupOption> categories;
  final List<LookupOption> cities;
  final LookupOption? selectedCategory;
  final LookupOption? selectedCity;
  final String condition;
  final bool isNegotiable;
  final bool isSubmitting;
  final String? errorMessage;

  SellState copyWith({
    List<XFile>? images,
    List<LookupOption>? categories,
    List<LookupOption>? cities,
    LookupOption? selectedCategory,
    LookupOption? selectedCity,
    String? condition,
    bool? isNegotiable,
    bool? isSubmitting,
    String? errorMessage,
  }) =>
      SellState(
        images: images ?? this.images,
        categories: categories ?? this.categories,
        cities: cities ?? this.cities,
        selectedCategory: selectedCategory ?? this.selectedCategory,
        selectedCity: selectedCity ?? this.selectedCity,
        condition: condition ?? this.condition,
        isNegotiable: isNegotiable ?? this.isNegotiable,
        isSubmitting: isSubmitting ?? this.isSubmitting,
        errorMessage: errorMessage,
      );
}

class SellNotifier extends AutoDisposeNotifier<SellState> {
  bool _disposed = false;

  @override
  SellState build() {
    ref.onDispose(() => _disposed = true);
    Future.microtask(_loadLookups);
    return const SellState();
  }

  Future<void> _loadLookups() async {
    try {
      final repo = ref.read(sellRepositoryProvider);
      final results =
          await Future.wait([repo.fetchCategories(), repo.fetchCities()]);
      if (_disposed) return;
      state = state.copyWith(categories: results[0], cities: results[1]);
    } catch (e) {
      if (!_disposed) state = state.copyWith(errorMessage: e.toString());
    }
  }

  void addImages(List<XFile> picked) {
    final remaining = maxListingImages - state.images.length;
    if (remaining <= 0) return;
    state = state.copyWith(
      images: [...state.images, ...picked.take(remaining)],
    );
  }

  void removeImage(int index) {
    state = state.copyWith(
      images: [
        for (var i = 0; i < state.images.length; i++)
          if (i != index) state.images[i],
      ],
    );
  }

  void selectCategory(LookupOption option) =>
      state = state.copyWith(selectedCategory: option);

  void selectCity(LookupOption option) =>
      state = state.copyWith(selectedCity: option);

  void setCondition(String value) =>
      state = state.copyWith(condition: value);

  void toggleNegotiable(bool value) =>
      state = state.copyWith(isNegotiable: value);

  /// يرجع معرّف الإعلان الجديد عند النجاح، أو null عند الفشل
  Future<String?> submit({
    required String title,
    required String description,
    double? price,
    String? neighborhood,
  }) async {
    final userId = ref.read(currentUserIdProvider);
    if (userId == null) {
      state = state.copyWith(errorMessage: 'سجّل الدخول لنشر إعلانك');
      return null;
    }
    final category = state.selectedCategory;
    final city = state.selectedCity;
    if (category == null || city == null) {
      state = state.copyWith(errorMessage: 'اختر الفئة والمدينة');
      return null;
    }

    state = state.copyWith(isSubmitting: true);
    final draft = SellDraft(
      title: title.trim(),
      description: description.trim(),
      price: price,
      isNegotiable: state.isNegotiable,
      condition: state.condition,
      categoryId: category.id,
      cityId: city.id,
      neighborhood: neighborhood?.trim().isEmpty ?? true
          ? null
          : neighborhood!.trim(),
      images: state.images,
    );

    try {
      final listingId = await ref
          .read(sellRepositoryProvider)
          .submit(draft: draft, userId: userId);
      _appendToMockMyListings(listingId, draft);
      ref.invalidate(myListingsProvider);
      return listingId;
    } on AppException catch (e) {
      if (!_disposed) {
        state = state.copyWith(isSubmitting: false, errorMessage: e.message);
      }
      return null;
    } catch (_) {
      if (!_disposed) {
        state = state.copyWith(
            isSubmitting: false, errorMessage: 'تعذّر نشر الإعلان');
      }
      return null;
    }
  }

  /// في الوضع التجريبي: يُضاف الإعلان لقائمة «إعلاناتي» مباشرة
  void _appendToMockMyListings(String listingId, SellDraft draft) {
    if (Env.isConfigured) return;
    final repo = ref.read(myListingsRepositoryProvider);
    if (repo is! MockMyListingsRepository) return;
    repo.addItem(MyListingItem(
      id: listingId,
      title: draft.title,
      price: draft.price,
      status: 'active',
      imageUrl: '',
      viewsCount: 0,
      favoritesCount: 0,
      createdAt: DateTime.now(),
    ));
  }
}

final sellProvider =
    NotifierProvider.autoDispose<SellNotifier, SellState>(SellNotifier.new);
