import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:supabase_flutter/supabase_flutter.dart';
import 'package:uuid/uuid.dart';

import '../../../core/config/env.dart';
import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/storage_urls.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../models/sell_models.dart';

/// مدة صلاحية الإعلان الافتراضية
const _listingLifetime = Duration(days: 30);

abstract class SellRepository {
  Future<List<LookupOption>> fetchCategories();
  Future<List<LookupOption>> fetchCities();

  /// ينشئ الإعلان ويرفع صوره، ويرجع معرّف الإعلان الجديد
  Future<String> submit({required SellDraft draft, required String userId});
}

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
  Future<String> submit({
    required SellDraft draft,
    required String userId,
  }) async {
    // محاكاة رفع الصور والإرسال
    await Future<void>.delayed(const Duration(milliseconds: 1200));
    return 'mock-listing-${DateTime.now().millisecondsSinceEpoch}';
  }
}

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
          .map((r) => LookupOption(
              id: r['id'] as String, label: r['name_ar'] as String))
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
          .map((r) => LookupOption(
              id: r['id'] as String, label: r['name_ar'] as String))
          .toList();
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
      // 1) إنشاء صف الإعلان
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

      // 2) رفع الصور ثم تسجيلها — المسار: {user_id}/{listing_id}/{uuid}.jpg
      for (var i = 0; i < draft.images.length; i++) {
        final bytes = await draft.images[i].readAsBytes();
        final path = '$userId/$listingId/${_uuid.v4()}.jpg';

        await _db.storage.from(StorageBuckets.listingImages).uploadBinary(
              path,
              bytes,
              fileOptions: const FileOptions(contentType: 'image/jpeg'),
            );

        await _db.from('listing_images').insert({
          'listing_id': listingId,
          'storage_path': path,
          'is_primary': i == 0,
          'sort_order': i,
        });
      }

      return listingId;
    } catch (e) {
      throw mapException(e);
    }
  }
}

final sellRepositoryProvider = Provider<SellRepository>((ref) {
  if (!Env.isConfigured) return MockSellRepository();
  return SupabaseSellRepository(ref.watch(supabaseClientProvider));
});
