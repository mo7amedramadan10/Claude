import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:supabase_flutter/supabase_flutter.dart';

import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../models/profile_model.dart';

// أعمدة view public_profiles كما عُرّف في 004_rls.sql — لا تزد عليها
const _publicColumns = '''
  id, full_name, avatar_url, bio, city_id, is_nafath_verified,
  rating_avg, total_reviews, completed_deals,
  active_listings_count, last_seen_at, created_at
''';

abstract class ProfileRepository {
  Future<ProfileModel> fetchCurrentUser();
  Future<ProfileModel> fetchById(String id);
  Future<List<ProfileModel>> fetchVerifiedSellers({int limit = 10});
  Future<void> updateProfile(String id, Map<String, dynamic> data);
  Future<void> updateLastSeen(String id);
  Future<void> softDelete(String id);
  Future<bool> isPhoneTaken(String phone);
}

class SupabaseProfileRepository implements ProfileRepository {
  SupabaseProfileRepository(this._db);
  final SupabaseClient _db;

  @override
  Future<ProfileModel> fetchCurrentUser() async {
    final uid = _db.auth.currentUser?.id;
    if (uid == null) throw const SessionExpired();
    try {
      final data = await _db
          .from('profiles')
          .select()
          .eq('id', uid)
          .single();
      return ProfileModel.fromJson(data);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<ProfileModel> fetchById(String id) async {
    try {
      // بيانات المستخدمين الآخرين تُقرأ من view public_profiles فقط
      final data = await _db
          .from('public_profiles')
          .select(_publicColumns)
          .eq('id', id)
          .single();
      return ProfileModel.fromJson(data);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<List<ProfileModel>> fetchVerifiedSellers({int limit = 10}) async {
    try {
      // view public_profiles يخفي رقم الجوال ويستبعد المحذوفين
      final data = await _db
          .from('public_profiles')
          .select()
          .eq('is_nafath_verified', true)
          .order('rating_avg', ascending: false)
          .limit(limit);
      return (data as List)
          .map((r) => ProfileModel.fromJson(r as Map<String, dynamic>))
          .toList();
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> updateProfile(String id, Map<String, dynamic> data) async {
    try {
      await _db.from('profiles').update(data).eq('id', id);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> updateLastSeen(String id) async {
    try {
      await _db.from('profiles').update({
        'last_seen_at': DateTime.now().toUtc().toIso8601String(),
      }).eq('id', id);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> softDelete(String id) async {
    try {
      await _db.from('profiles').update({
        'deleted_at': DateTime.now().toUtc().toIso8601String(),
        'is_active': false,
      }).eq('id', id);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<bool> isPhoneTaken(String phone) async {
    try {
      final data = await _db
          .from('profiles')
          .select('id')
          .eq('phone', phone)
          .limit(1);
      return (data as List).isNotEmpty;
    } catch (e) {
      throw mapException(e);
    }
  }
}

final profileRepositoryProvider = Provider<ProfileRepository>((ref) {
  return SupabaseProfileRepository(ref.watch(supabaseClientProvider));
});
