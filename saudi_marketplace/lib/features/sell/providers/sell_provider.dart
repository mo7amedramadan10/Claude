import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image_picker/image_picker.dart';

import '../../../core/config/env.dart';
import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../../home/providers/home_provider.dart';
import '../../my_listings/data/my_listings_repository.dart';
import '../../my_listings/models/my_listing_item.dart';
import '../../my_listings/providers/my_listings_provider.dart';
import '../data/sell_repository.dart';
import '../models/sell_models.dart';

const maxListingImages = 10;

class SellState {
  const SellState({
    this.categories = const [],
    this.cities = const [],
    this.isLoadingLookups = true,
    this.lookupsError,
    this.selectedCategory,
    this.selectedCity,
    this.condition = 'good',
    this.isNegotiable = true,
    this.existingImages = const [],
    this.newImages = const [],
    this.editingListingId,
    this.editableData,
    this.isLoadingEdit = false,
    this.isSubmitting = false,
    this.errorMessage,
  });

  final List<LookupOption> categories;
  final List<LookupOption> cities;

  /// تحميل قوائم الفئات/المدن جارٍ
  final bool isLoadingLookups;

  /// خطأ عند تحميل الفئات/المدن (مستقل عن أخطاء النشر)
  final String? lookupsError;

  final LookupOption? selectedCategory;
  final LookupOption? selectedCity;
  final String condition;
  final bool isNegotiable;

  /// صور محفوظة في الـ storage (وضع التعديل)
  final List<ExistingImage> existingImages;

  /// صور جديدة اختارها المستخدم من الجهاز
  final List<XFile> newImages;

  /// إذا غير null → وضع التعديل
  final String? editingListingId;

  /// بيانات الإعلان المُحمَّلة للتعديل (تُستخدم مرة واحدة لملء الحقول)
  final EditableListingData? editableData;

  final bool isLoadingEdit;
  final bool isSubmitting;
  final String? errorMessage;

  bool get isEditMode => editingListingId != null;

  int get totalImages => existingImages.length + newImages.length;

  SellState copyWith({
    List<LookupOption>? categories,
    List<LookupOption>? cities,
    bool? isLoadingLookups,
    String? lookupsError,
    LookupOption? selectedCategory,
    LookupOption? selectedCity,
    String? condition,
    bool? isNegotiable,
    List<ExistingImage>? existingImages,
    List<XFile>? newImages,
    String? editingListingId,
    EditableListingData? editableData,
    bool? isLoadingEdit,
    bool? isSubmitting,
    String? errorMessage,
  }) =>
      SellState(
        categories: categories ?? this.categories,
        cities: cities ?? this.cities,
        isLoadingLookups: isLoadingLookups ?? this.isLoadingLookups,
        lookupsError: lookupsError,
        selectedCategory: selectedCategory ?? this.selectedCategory,
        selectedCity: selectedCity ?? this.selectedCity,
        condition: condition ?? this.condition,
        isNegotiable: isNegotiable ?? this.isNegotiable,
        existingImages: existingImages ?? this.existingImages,
        newImages: newImages ?? this.newImages,
        editingListingId: editingListingId ?? this.editingListingId,
        editableData: editableData ?? this.editableData,
        isLoadingEdit: isLoadingEdit ?? this.isLoadingEdit,
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
    state = state.copyWith(isLoadingLookups: true);
    try {
      final repo = ref.read(sellRepositoryProvider);
      final results =
          await Future.wait([repo.fetchCategories(), repo.fetchCities()]);
      if (_disposed) return;
      state = state.copyWith(
        categories: results[0],
        cities: results[1],
        isLoadingLookups: false,
      );
    } catch (e, st) {
      // الخطأ الخام في الـ console لتشخيص مشاكل RLS / الشبكة على الويب
      debugPrint('[بيكيا] فشل تحميل الفئات/المدن: $e\n$st');
      if (!_disposed) {
        state = state.copyWith(
          isLoadingLookups: false,
          lookupsError: 'تعذّر تحميل الفئات والمدن — اضغط لإعادة المحاولة',
        );
      }
    }
  }

  /// إعادة محاولة تحميل القوائم (عند الضغط على حقل فارغ)
  Future<void> reloadLookups() => _loadLookups();

  /// يُستدعى عند فتح النموذج في وضع التعديل
  Future<void> initEdit(String listingId) async {
    state = state.copyWith(
        editingListingId: listingId, isLoadingEdit: true);
    try {
      final data = await ref
          .read(sellRepositoryProvider)
          .fetchListingForEdit(listingId);
      if (_disposed) return;

      // مطابقة الفئة والمدينة بعد تحميل القوائم
      final matchedCategory = state.categories.isEmpty
          ? null
          : state.categories.firstWhere((c) => c.id == data.categoryId,
              orElse: () => state.categories.first);
      final matchedCity = state.cities.isEmpty
          ? null
          : state.cities.firstWhere((c) => c.id == data.cityId,
              orElse: () => state.cities.first);

      state = state.copyWith(
        editableData: data,
        existingImages: data.existingImages,
        condition: data.condition,
        isNegotiable: data.isNegotiable,
        selectedCategory: matchedCategory,
        selectedCity: matchedCity,
        isLoadingEdit: false,
      );
    } catch (e) {
      if (!_disposed) {
        state = state.copyWith(
            isLoadingEdit: false, errorMessage: e.toString());
      }
    }
  }

  // ── Image management ─────────────────────────────────────────────────────

  void addImages(List<XFile> picked) {
    final remaining = maxListingImages - state.totalImages;
    if (remaining <= 0) return;
    state = state.copyWith(
      newImages: [...state.newImages, ...picked.take(remaining)],
    );
  }

  void removeExistingImage(int index) {
    state = state.copyWith(
      existingImages: [
        for (var i = 0; i < state.existingImages.length; i++)
          if (i != index) state.existingImages[i],
      ],
    );
  }

  void removeNewImage(int index) {
    state = state.copyWith(
      newImages: [
        for (var i = 0; i < state.newImages.length; i++)
          if (i != index) state.newImages[i],
      ],
    );
  }

  // ── Form fields ──────────────────────────────────────────────────────────

  void selectCategory(LookupOption option) =>
      state = state.copyWith(selectedCategory: option);

  void selectCity(LookupOption option) =>
      state = state.copyWith(selectedCity: option);

  void setCondition(String value) => state = state.copyWith(condition: value);

  void toggleNegotiable(bool value) =>
      state = state.copyWith(isNegotiable: value);

  // ── Submit ───────────────────────────────────────────────────────────────

  /// يرجع معرّف الإعلان عند النجاح، أو null عند الفشل
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
      neighborhood:
          (neighborhood?.trim().isEmpty ?? true) ? null : neighborhood!.trim(),
      newImages: state.newImages,
    );

    try {
      final repo = ref.read(sellRepositoryProvider);

      if (state.isEditMode) {
        // ── وضع التعديل ─────────────────────────────────────────────
        await repo.updateListing(
          listingId: state.editingListingId!,
          draft: draft,
          keptImages: state.existingImages,
          userId: userId,
        );
        _updateMockItem(state.editingListingId!, draft);
        ref.invalidate(myListingsProvider);
        return state.editingListingId;
      } else {
        // ── إعلان جديد ──────────────────────────────────────────────
        final listingId =
            await repo.submit(draft: draft, userId: userId);
        _appendToMockMyListings(listingId, draft);
        ref.invalidate(myListingsProvider);
        ref.invalidate(homeProvider);
        return listingId;
      }
    } on AppException catch (e) {
      if (!_disposed) {
        state = state.copyWith(isSubmitting: false, errorMessage: e.message);
      }
      return null;
    } catch (_) {
      if (!_disposed) {
        state = state.copyWith(
            isSubmitting: false, errorMessage: 'حدث خطأ، حاول مجدداً');
      }
      return null;
    }
  }

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

  void _updateMockItem(String listingId, SellDraft draft) {
    if (Env.isConfigured) return;
    final repo = ref.read(myListingsRepositoryProvider);
    if (repo is! MockMyListingsRepository) return;
    repo.updateItem(listingId,
        (item) => MyListingItem(
              id: item.id,
              title: draft.title,
              price: draft.price,
              status: item.status,
              imageUrl: state.existingImages.isNotEmpty
                  ? state.existingImages.first.publicUrl
                  : item.imageUrl,
              viewsCount: item.viewsCount,
              favoritesCount: item.favoritesCount,
              createdAt: item.createdAt,
              isVerified: item.isVerified,
            ));
  }
}

final sellProvider =
    NotifierProvider.autoDispose<SellNotifier, SellState>(SellNotifier.new);
