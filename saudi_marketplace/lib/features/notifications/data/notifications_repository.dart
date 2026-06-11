import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:supabase_flutter/supabase_flutter.dart';

import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../models/notification_model.dart';

abstract class NotificationsRepository {
  Future<List<NotificationModel>> fetchUserNotifications(
    String userId, {
    int limit = 30,
    int offset = 0,
  });
  Future<void> markRead(String notificationId);
  Future<void> markAllRead(String userId);
  Future<int> fetchUnreadCount(String userId);
  Stream<List<NotificationModel>> watchUserNotifications(String userId);
}

class SupabaseNotificationsRepository implements NotificationsRepository {
  SupabaseNotificationsRepository(this._db);
  final SupabaseClient _db;

  @override
  Future<List<NotificationModel>> fetchUserNotifications(
    String userId, {
    int limit = 30,
    int offset = 0,
  }) async {
    try {
      final data = await _db
          .from('notifications')
          .select()
          .eq('user_id', userId)
          .order('created_at', ascending: false)
          .range(offset, offset + limit - 1);
      return (data as List)
          .map((r) => NotificationModel.fromJson(r as Map<String, dynamic>))
          .toList();
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> markRead(String notificationId) async {
    try {
      await _db.from('notifications').update({
        'is_read': true,
        'read_at': DateTime.now().toUtc().toIso8601String(),
      }).eq('id', notificationId);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> markAllRead(String userId) async {
    try {
      await _db
          .from('notifications')
          .update({
            'is_read': true,
            'read_at': DateTime.now().toUtc().toIso8601String(),
          })
          .eq('user_id', userId)
          .eq('is_read', false);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<int> fetchUnreadCount(String userId) async {
    try {
      final data = await _db
          .from('notifications')
          .select('id', const FetchOptions(count: CountOption.exact))
          .eq('user_id', userId)
          .eq('is_read', false);
      return data.count ?? 0;
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Stream<List<NotificationModel>> watchUserNotifications(String userId) {
    return _db
        .from('notifications')
        .stream(primaryKey: ['id'])
        .eq('user_id', userId)
        .order('created_at', ascending: false)
        .limit(30)
        .map((rows) => rows
            .map((r) =>
                NotificationModel.fromJson(r as Map<String, dynamic>))
            .toList());
  }
}

final notificationsRepositoryProvider =
    Provider<NotificationsRepository>((ref) {
  return SupabaseNotificationsRepository(ref.watch(supabaseClientProvider));
});
