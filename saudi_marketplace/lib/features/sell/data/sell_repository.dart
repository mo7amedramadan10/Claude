import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:supabase_flutter/supabase_flutter.dart';
import 'package:uuid/uuid.dart';

import '../../../core/config/env.dart';
import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/storage_urls.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../../my_listings/data/my_listings_mock_data.dart';
import '../models/sell_models.dart';

const _listingLifetime = Duration(days: 30);

abstract class SellRepository {
  Future<List<LookupOption>> fetchCategories();
  Future<List<LookupOption>> fetchCities();
  Future<EditableListingData> fetchListingForEdit(String listingId);

  Future<String> submit({required SellDraft draft, required String userId});

  Future<void> updateListing({
    required String listingId,
    required SellDraft draft,
    required List<ExistingImage> keptImages,
    required String userId,
  });
}

// ── Mock ──────────────────────────────────────────────────────────────────
class MockSellRepository implements SellRepository {
  static const _categories = [
    LookupOption(id: 'cat-1', label: 'سيارات'),
    LookupOption(id: 'cat-2', label: 'عقارات'),
    LookupOption(id: 'cat-3', label: 'جوالات'),
    LookupOption(id: 'cat-4', label: 'إلكترونيات'),
    LookupOption(id: 'cat-5', label: 'أثاث'),
    LookupOption(id: 'cat-6', label: 'أزياء'),
    LookupOption(id: 'cat-7', label: 'ألعاب وترفيه'),
    LookupOption(id: 'cat-8', label: 'وظائف'),
    LookupOption(id: 'cat-9', label: 'خدمات'),
    LookupOption(id: 'cat-10', label: 'حيوانات'),
    LookupOption(id: 'cat-11', label: 'رياضة'),
    LookupOption(id: 'cat-12', label: 'أخرى'),
  ];

  static const _cities = [
    LookupOption(id: 'city-1', label: 'الرياض'),
    LookupOption(id: 'city-2', label: 'جدة'),
    LookupOption(id: 'city-3', label: 'مكة المكرمة'),
    LookupOption(id: 'city-4', label: 'المدينة المنورة'),
    LookupOption(id: 'city-5', label: 'الدمام'),
    LookupOption(id: 'city-6', label: 'الخبر'),
    LookupOption(id: 'city-7', label: 'الطائف'),
    LookupOption(id: 'city-8', label: 'تبوك'),
    LookupOption(id: 'city-9', label: 'بريدة'),
    LookupOption(id: 'city-10', label: 'أبها'),
  ];

  @override
  Future<List<LookupOption>> fetchCategories() async => _categories;

  @override
  Future<List<LookupOption>> fetchCities() async => _cities;

  @override
  Future<EditableListingData> fetchListingForEdit(String listingId) async {
    await Future<void>.delayed(const Duration(milliseconds: 500));
    final item = MyListingsMockData.items.firstWhere(
      (i) => i.id == listingId,
      orElse: () => MyListingsMockData.items.first,
    );
    return EditableListingData(
      id: item.id,
      title: item.title,
      description: 'وصف تجريبي للإعلان — يمكنك تعديل هذا النص.',
      price: item.price,
      isNegotiable: true,
      condition: 'good',
      categoryId: 'cat-1',
      cityId: 'city-1',
      neighborhood: null,
      existingImages: item.imageUrl.isNotEmpty
          ? [
              ExistingImage(
                  id: 'img-mock',
                  storagePath: 'mock/path',
                  publicUrl: item.imageUrl),
            ]
          : [],
    );
  }

  @override
  Future<String> submit({
    required SellDraft draft,
    required String userId,
  }) async {
    await Future<void>.delayed(const Duration(milliseconds: 1200));
    return 'mock-listing-${DateTime.now().millisecondsSinceEpoch}';
  }

  @override
  Future<void> updateListing({
    required String listingId,
    required SellDraft draft,
    required List<ExistingImage> keptImages,
    required String userId,
  }) async {
    await Future<void>.delayed(const Duration(milliseconds: 900));
  }
}

// ── Supabase ──────────────────────────────────────────────────────────────
class SupabaseSellRepository implements SellRepository {
  SupabaseSellRepository(this._db);
  final SupabaseClient _db;
  final _uuid = const Uuid();

  @override
  Future<List<LookupOption>> fetchCategories() async {
    try {
      final data = await _db
          .from('categories')
          .select('id, name_ar')
          .eq('is_active', true)
          .order('sort_order', ascending: true);
      return (data as List)
          .map((r) =>
              LookupOption(id: r['id'] as String, label: r['name_ar'] as String))
          .toList();
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<List<LookupOption>> fetchCities() async {
    try {
      final data = await _db
          .from('cities')
          .select('id, name_ar')
          .eq('is_active', true)
          .order('sort_order', ascending: true);
      return (data as List)
          .map((r) =>
              LookupOption(id: r['id'] as String, label: r['name_ar'] as String))
          .toList();
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<EditableListingData> fetchListingForEdit(String listingId) async {
    try {
      final row = await _db
          .from('listings')
          .select('''
            id, title, description, price, is_price_negotiable,
            condition, category_id, city_id, neighborhood,
            listing_images(id, storage_path, is_primary, sort_order)
          ''')
          .eq('id', listingId)
          .single();

      final images = ((row['listing_images'] as List?) ?? [])
        ..sort((a, b) =>
            (a['sort_order'] as int).compareTo(b['sort_order'] as int));

      return EditableListingData(
        id: row['id'] as String,
        title: row['title'] as String,
        description: row['description'] as String,
        price: (row['price'] as num?)?.toDouble(),
        isNegotiable: (row['is_price_negotiable'] as bool?) ?? false,
        condition: (row['condition'] as String?) ?? 'good',
        categoryId: row['category_id'] as String,
        cityId: row['city_id'] as String,
        neighborhood: row['neighborhood'] as String?,
        existingImages: images
            .map((img) => ExistingImage(
                  id: img['id'] as String,
                  storagePath: img['storage_path'] as String,
                  publicUrl:
                      StorageUrls.listingImage(img['storage_path'] as String),
                ))
            .toList(),
      );
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<String> submit({
    required SellDraft draft,
    required String userId,
  }) async {
    try {
      final row = await _db
          .from('listings')
          .insert({
            'seller_id': userId,
            'category_id': draft.categoryId,
            'city_id': draft.cityId,
            'title': draft.title,
            'description': draft.description,
            'price': draft.price,
            'is_price_negotiable': draft.isNegotiable,
            'condition': draft.condition,
            'status': 'active',
            'neighborhood': draft.neighborhood,
            'expires_at': DateTime.now()
                .toUtc()
                .add(_listingLifetime)
                .toIso8601String(),
          })
          .select('id')
          .single();
      final listingId = row['id'] as String;
      await _uploadImages(listingId, userId, draft.newImages, offset: 0);
      return listingId;
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> updateListing({
    required String listingId,
    required SellDraft draft,
    required List<ExistingImage> keptImages,
    required String userId,
  }) async {
    try {
      // 1) تحديث حقول الإعلان
      await _db.from('listings').update({
        'title': draft.title,
        'description': draft.description,
        'price': draft.price,
        'is_price_negotiable': draft.isNegotiable,
        'condition': draft.condition,
        'category_id': draft.categoryId,
        'city_id': draft.cityId,
        'neighborhood': draft.neighborhood,
      }).eq('id', listingId);

      // 2) حذف جميع صفوف الصور القديمة وإعادة إدراجها بالترتيب الصحيح
      await _db.from('listing_images').delete().eq('listing_id', listingId);

      // 3) إعادة إدراج الصور المحتفظ بها (موجودة في التخزين مسبقاً)
      for (var i = 0; i < keptImages.length; i++) {
        await _db.from('listing_images').insert({
          'listing_id': listingId,
          'storage_path': keptImages[i].storagePath,
          'is_primary': i == 0 && draft.newImages.isEmpty,
          'sort_order': i,
        });
      }

      // 4) رفع وإدراج الصور الجديدة
      await _uploadImages(listingId, userId, draft.newImages,
          offset: keptImages.length);
    } catch (e) {
      throw mapException(e);
    }
  }

  Future<void> _uploadImages(
    String listingId,
    String userId,
    List newImages, {
    required int offset,
  }) async {
    for (var i = 0; i < newImages.length; i++) {
      final bytes = await newImages[i].readAsBytes();
      final path = '$userId/$listingId/${_uuid.v4()}.jpg';
      await _db.storage.from(StorageBuckets.listingImages).uploadBinary(
            path,
            bytes,
            fileOptions: const FileOptions(contentType: 'image/jpeg'),
          );
      await _db.from('listing_images').insert({
        'listing_id': listingId,
        'storage_path': path,
        'is_primary': offset + i == 0,
        'sort_order': offset + i,
      });
    }
  }
}

final sellRepositoryProvider = Provider<SellRepository>((ref) {
  if (!Env.isConfigured) return MockSellRepository();
  return SupabaseSellRepository(ref.watch(supabaseClientProvider));
});
