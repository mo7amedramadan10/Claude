import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:supabase_flutter/supabase_flutter.dart';

import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../models/conversation_model.dart';
import '../models/message_model.dart';

// بيانات الطرف الآخر تُقرأ من view public_profiles (RLS يمنع profiles)
const _participantColumns = '''
  id, full_name, avatar_url, rating_avg, is_nafath_verified,
  last_seen_at, created_at
''';

const _conversationColumns = '''
  id, listing_id, buyer_id, seller_id, last_message_at,
  last_message_preview, last_sender_id, buyer_unread_count,
  seller_unread_count, is_archived_by_buyer, is_archived_by_seller,
  created_at,
  buyer:public_profiles!buyer_id($_participantColumns),
  seller:public_profiles!seller_id($_participantColumns),
  listing:listings(
    id, seller_id, category_id, city_id, title, description, price,
    currency, is_price_hidden, condition, status, expires_at,
    created_at, updated_at,
    listing_images(id, listing_id, storage_path, is_primary, sort_order, created_at)
  )
''';

abstract class ChatRepository {
  Future<List<ConversationModel>> fetchConversations(String userId);
  Future<ConversationModel> fetchConversationById(String id);
  Future<ConversationModel> fetchOrCreateConversation({
    required String listingId,
    required String buyerId,
    required String sellerId,
  });
  Future<List<MessageModel>> fetchMessages(
    String conversationId, {
    int limit = 40,
    DateTime? before,
  });
  Future<void> sendMessage({
    required String conversationId,
    required String senderId,
    required String content,
    String messageType = 'text',
    String? storagePath,
  });
  Future<void> markConversationRead(String conversationId);
  Future<void> archiveConversation(String conversationId, String userId);
  Stream<List<MessageModel>> watchMessages(String conversationId);
  Stream<List<ConversationModel>> watchConversations(String userId);
}

class SupabaseChatRepository implements ChatRepository {
  SupabaseChatRepository(this._db);
  final SupabaseClient _db;

  @override
  Future<List<ConversationModel>> fetchConversations(String userId) async {
    try {
      final data = await _db
          .from('conversations')
          .select(_conversationColumns)
          .or('buyer_id.eq.$userId,seller_id.eq.$userId')
          .order('last_message_at', ascending: false);
      return (data as List)
          .map((r) => ConversationModel.fromJson(r as Map<String, dynamic>))
          .toList();
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<ConversationModel> fetchConversationById(String id) async {
    try {
      final data = await _db
          .from('conversations')
          .select(_conversationColumns)
          .eq('id', id)
          .single();
      return ConversationModel.fromJson(data);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<ConversationModel> fetchOrCreateConversation({
    required String listingId,
    required String buyerId,
    required String sellerId,
  }) async {
    try {
      // Try fetching existing conversation first
      final existing = await _db
          .from('conversations')
          .select(_conversationColumns)
          .eq('listing_id', listingId)
          .eq('buyer_id', buyerId)
          .maybeSingle();

      if (existing != null) {
        return ConversationModel.fromJson(existing as Map<String, dynamic>);
      }

      // Create new conversation
      final created = await _db
          .from('conversations')
          .insert({
            'listing_id': listingId,
            'buyer_id': buyerId,
            'seller_id': sellerId,
          })
          .select(_conversationColumns)
          .single();
      return ConversationModel.fromJson(created as Map<String, dynamic>);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<List<MessageModel>> fetchMessages(
    String conversationId, {
    int limit = 40,
    DateTime? before,
  }) async {
    try {
      var q = _db
          .from('messages')
          .select()
          .eq('conversation_id', conversationId);

      if (before != null) {
        q = q.lt('created_at', before.toIso8601String());
      }

      final data = await q
          .order('created_at', ascending: false)
          .limit(limit);
      return (data as List)
          .map((r) => MessageModel.fromJson(r as Map<String, dynamic>))
          .toList();
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> sendMessage({
    required String conversationId,
    required String senderId,
    required String content,
    String messageType = 'text',
    String? storagePath,
  }) async {
    try {
      await _db.from('messages').insert({
        'conversation_id': conversationId,
        'sender_id': senderId,
        'content': content,
        'message_type': messageType,
        if (storagePath != null) 'storage_path': storagePath,
      });
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> markConversationRead(String conversationId) async {
    try {
      await _db.rpc(
        'mark_conversation_read',
        params: {'p_conversation_id': conversationId},
      );
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> archiveConversation(
      String conversationId, String userId) async {
    try {
      final conv = await _db
          .from('conversations')
          .select('buyer_id')
          .eq('id', conversationId)
          .single();

      final isBuyer = (conv['buyer_id'] as String) == userId;
      await _db.from('conversations').update(
        isBuyer
            ? {'is_archived_by_buyer': true}
            : {'is_archived_by_seller': true},
      ).eq('id', conversationId);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Stream<List<MessageModel>> watchMessages(String conversationId) {
    return _db
        .from('messages')
        .stream(primaryKey: ['id'])
        .eq('conversation_id', conversationId)
        .order('created_at', ascending: false)
        .limit(40)
        .map((rows) => rows
            .map((r) => MessageModel.fromJson(r as Map<String, dynamic>))
            .toList());
  }

  @override
  Stream<List<ConversationModel>> watchConversations(String userId) {
    return _db
        .from('conversations')
        .stream(primaryKey: ['id'])
        .order('last_message_at', ascending: false)
        .map((rows) => rows
            .where((r) =>
                r['buyer_id'] == userId || r['seller_id'] == userId)
            .map((r) =>
                ConversationModel.fromJson(r as Map<String, dynamic>))
            .toList());
  }
}

final chatRepositoryProvider = Provider<ChatRepository>((ref) {
  return SupabaseChatRepository(ref.watch(supabaseClientProvider));
});
