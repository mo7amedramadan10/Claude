import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:supabase_flutter/supabase_flutter.dart';

import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../models/profile_model.dart';

// Columns safe to return for OTHER users (mirrors public_profiles view)
const _publicColumns = '''
  id, full_name, avatar_url, bio, city_id, is_nafath_verified,
  nafath_verified_at, rating_avg, total_reviews, completed_deals,
  active_listings_count, is_active, last_seen_at, created_at, updated_at
''';

abstract class ProfileRepository {
  Future<ProfileModel> fetchCurrentUser();
  Future<ProfileModel> fetchById(String id);
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
      final data = await _db
          .from('profiles')
          .select(_publicColumns)
          .eq('id', id)
          .filter('deleted_at', 'is', null)
          .eq('is_active', true)
          .single();
      return ProfileModel.fromJson(data);
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
