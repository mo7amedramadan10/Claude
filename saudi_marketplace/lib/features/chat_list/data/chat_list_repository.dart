import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/config/env.dart';
import '../../../core/supabase/storage_urls.dart';
import '../../../core/utils/number_formatter.dart';
import '../../../core/utils/relative_time.dart';
import '../../chat/data/chat_repository.dart';
import '../../chat/models/conversation_model.dart' as backend;
import '../models/conversation_model.dart';
import 'chat_list_mock_data.dart';

/// يُعدّ المستخدم متصلاً إذا شوهد خلال آخر 5 دقائق
const _onlineWindow = Duration(minutes: 5);

extension ConversationUiMapper on backend.ConversationModel {
  ConversationModel toUiModel(String myId) {
    final other = otherParticipant(myId);
    final lastSeen = other?.lastSeenAt;
    final listingImage = listing?.primaryImage;
    final isLastMine = lastSenderId == myId;

    // علامات القراءة الزرقاء: رسالتي الأخيرة قُرئت إذا صفّر الطرف
    // الآخر عداده غير المقروء عبر mark_conversation_read
    final otherUnread =
        myId == buyerId ? sellerUnreadCount : buyerUnreadCount;

    return ConversationModel(
      id: id,
      sellerName: other?.displayName ?? 'مستخدم بيكيا',
      sellerAvatarUrl: other?.avatarUrl != null
          ? StorageUrls.avatar(other!.avatarUrl!)
          : '',
      listingTitle: listing?.title ?? '',
      listingImageUrl: listingImage != null
          ? StorageUrls.listingImage(listingImage.storagePath)
          : '',
      listingPrice: NumberFormatter.arabicPrice(listing?.price ?? 0),
      lastMessage: lastMessagePreview ?? '',
      lastMessageTime: RelativeTime.short(lastMessageAt),
      isOnline: lastSeen != null &&
          DateTime.now().difference(lastSeen) < _onlineWindow,
      unreadCount: unreadCountFor(myId),
      isLastMine: isLastMine,
      isRead: !isLastMine || otherUnread == 0,
    );
  }
}

abstract class ChatListRepository {
  Future<List<ConversationModel>> fetchConversations(String? userId);
  Future<void> markConversationRead(String conversationId);
}

/// مصدر تجريبي — يُستخدم تلقائياً قبل إعداد Supabase
class MockChatListRepository implements ChatListRepository {
  const MockChatListRepository();

  @override
  Future<List<ConversationModel>> fetchConversations(String? userId) async =>
      [...ChatListMockData.conversations];

  @override
  Future<void> markConversationRead(String conversationId) async {}
}

class SupabaseChatListRepository implements ChatListRepository {
  SupabaseChatListRepository(this._chat);
  final ChatRepository _chat;

  @override
  Future<List<ConversationModel>> fetchConversations(String? userId) async {
    // بدون تسجيل دخول لا توجد محادثات
    if (userId == null) return const [];
    final conversations = await _chat.fetchConversations(userId);
    return [
      for (final c in conversations)
        if (!c.isArchivedFor(userId)) c.toUiModel(userId),
    ];
  }

  @override
  Future<void> markConversationRead(String conversationId) =>
      _chat.markConversationRead(conversationId);
}

final chatListRepositoryProvider = Provider<ChatListRepository>((ref) {
  if (!Env.isConfigured) return const MockChatListRepository();
  return SupabaseChatListRepository(ref.watch(chatRepositoryProvider));
});
