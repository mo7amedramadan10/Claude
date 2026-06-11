import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/config/env.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../../chat/data/chat_repository.dart';
import '../data/chat_list_mock_data.dart';
import '../data/chat_list_repository.dart';
import '../models/conversation_model.dart';

class ChatListState {
  const ChatListState({
    required this.conversations,
    this.isLoading = false,
    this.errorMessage,
  });

  final List<ConversationModel> conversations;
  final bool isLoading;
  final String? errorMessage;

  int get totalUnread =>
      conversations.fold(0, (sum, c) => sum + c.unreadCount);

  ChatListState copyWith({
    List<ConversationModel>? conversations,
    bool? isLoading,
    String? errorMessage,
  }) =>
      ChatListState(
        conversations: conversations ?? this.conversations,
        isLoading: isLoading ?? this.isLoading,
        errorMessage: errorMessage,
      );
}

class ChatListNotifier extends Notifier<ChatListState> {
  StreamSubscription<Object?>? _sub;

  @override
  ChatListState build() {
    ref.onDispose(() => _sub?.cancel());

    if (Env.isConfigured) {
      Future.microtask(refresh);
      _subscribeRealtime();
      return const ChatListState(conversations: [], isLoading: true);
    }
    return const ChatListState(conversations: ChatListMockData.conversations);
  }

  /// أي تغيير في جدول conversations → إعادة الجلب مع الـ joins
  /// (stream الـ Realtime يرجع صفوفاً خاماً بلا joins فلا يكفي وحده)
  void _subscribeRealtime() {
    final userId = ref.read(currentUserIdProvider);
    if (userId == null) return;

    _sub = ref
        .read(chatRepositoryProvider)
        .watchConversations(userId)
        .listen((_) => refresh());
  }

  Future<void> refresh() async {
    try {
      final userId = ref.read(currentUserIdProvider);
      final conversations = await ref
          .read(chatListRepositoryProvider)
          .fetchConversations(userId);
      state = state.copyWith(conversations: conversations, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  void markRead(String id) {
    state = state.copyWith(
      conversations: [
        for (final c in state.conversations)
          if (c.id != id)
            c
          else
            ConversationModel(
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
            ),
      ],
    );

    if (!Env.isConfigured) return;
    // RPC تتجاهل الفشل هنا — ستُصحَّح القيمة مع أول refresh
    ref.read(chatListRepositoryProvider).markConversationRead(id).ignore();
  }
}

final chatListProvider = NotifierProvider<ChatListNotifier, ChatListState>(
  ChatListNotifier.new,
);
