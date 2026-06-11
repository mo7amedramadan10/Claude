import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/chat_list_mock_data.dart';
import '../models/conversation_model.dart';

class ChatListState {
  const ChatListState({required this.conversations});
  final List<ConversationModel> conversations;

  int get totalUnread =>
      conversations.fold(0, (sum, c) => sum + c.unreadCount);
}

class ChatListNotifier extends Notifier<ChatListState> {
  @override
  ChatListState build() =>
      const ChatListState(conversations: ChatListMockData.conversations);

  void markRead(String id) {
    state = ChatListState(
      conversations: state.conversations.map((c) {
        if (c.id != id) return c;
        return ConversationModel(
          id: c.id,
          sellerName: c.sellerName,
          sellerAvatarUrl: c.sellerAvatarUrl,
          listingTitle: c.listingTitle,
          listingImageUrl: c.listingImageUrl,
          listingPrice: c.listingPrice,
          lastMessage: c.lastMessage,
          lastMessageTime: c.lastMessageTime,
          isOnline: c.isOnline,
          unreadCount: 0,
          isLastMine: c.isLastMine,
          isRead: true,
        );
      }).toList(),
    );
  }
}

final chatListProvider = NotifierProvider<ChatListNotifier, ChatListState>(
  ChatListNotifier.new,
);
