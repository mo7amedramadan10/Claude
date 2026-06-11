import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/config/env.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../data/chat_repository.dart';
import '../data/chat_ui_mapper.dart';
import '../models/chat_header_info.dart';
import '../models/chat_message.dart';

class ChatState {
  const ChatState({
    this.messages = _initialMessages,
    this.isTyping = false,
    this.isLoading = false,
    this.errorMessage,
    this.header = ChatHeaderInfo.mock,
  });

  static const _initialMessages = [
    ChatMessage(
        text: 'السلام عليكم، السيارة لا زالت متوفرة؟',
        time: '10:32 ص',
        sender: MessageSender.me),
    ChatMessage(
        text: 'وعليكم السلام، نعم متوفرة وبحالة ممتازة',
        time: '10:34 ص',
        sender: MessageSender.seller),
    ChatMessage(
        text: 'ممكن آخر سعر؟', time: '10:35 ص', sender: MessageSender.me),
    ChatMessage(
        text: 'السعر ٨٢,٥٠٠ وقابل للتفاوض البسيط بعد المعاينة',
        time: '10:36 ص',
        sender: MessageSender.seller),
  ];

  static const quickReplies = [
    'هل السعر قابل للتفاوض؟',
    'متاح للمعاينة اليوم؟',
    'أرسل لي الموقع',
  ];

  final List<ChatMessage> messages;
  final bool isTyping;
  final bool isLoading;
  final String? errorMessage;
  final ChatHeaderInfo header;

  ChatState copyWith({
    List<ChatMessage>? messages,
    bool? isTyping,
    bool? isLoading,
    String? errorMessage,
    ChatHeaderInfo? header,
  }) =>
      ChatState(
        messages: messages ?? this.messages,
        isTyping: isTyping ?? this.isTyping,
        isLoading: isLoading ?? this.isLoading,
        errorMessage: errorMessage,
        header: header ?? this.header,
      );
}

/// arg = معرّف المحادثة (Supabase) أو معرّف تجريبي في وضع الـ mock
class ChatNotifier extends AutoDisposeFamilyNotifier<ChatState, String> {
  static const _replyPool = [
    'تمام، في خدمتك',
    'أكيد ولا يهمك',
    'حياك الله، بانتظارك في حي الملقا',
    'بكرة بعد العصر يناسبك للمعاينة؟',
  ];

  Timer? _replyTimer;
  StreamSubscription<Object?>? _sub;
  bool _disposed = false;

  @override
  ChatState build(String arg) {
    ref.onDispose(() {
      _disposed = true;
      _replyTimer?.cancel();
      _sub?.cancel();
    });

    if (Env.isConfigured) {
      Future.microtask(_load);
      return const ChatState(messages: [], isLoading: true);
    }
    return const ChatState();
  }

  Future<void> _load() async {
    final myId = ref.read(currentUserIdProvider);
    if (myId == null) {
      state = state.copyWith(
          isLoading: false, errorMessage: 'سجّل الدخول لعرض المحادثة');
      return;
    }

    try {
      final repo = ref.read(chatRepositoryProvider);
      final conversation = await repo.fetchConversationById(arg);
      if (_disposed) return;

      state = state.copyWith(
        header: conversation.toHeaderInfo(myId),
        isLoading: false,
      );

      // Realtime: القائمة تصل كاملة مع كل تغيير (أحدث أولاً)
      _sub = repo.watchMessages(arg).listen((rows) {
        if (_disposed) return;
        final hasUnreadIncoming =
            rows.any((m) => m.senderId != myId && !m.isRead);
        if (hasUnreadIncoming) {
          repo.markConversationRead(arg).ignore();
        }
        state = state.copyWith(
          messages: [for (final m in rows.reversed) m.toUiModel(myId)],
        );
      });
    } catch (e) {
      if (_disposed) return;
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  void send(String text) {
    final trimmed = text.trim();
    if (trimmed.isEmpty) return;

    if (Env.isConfigured) {
      _sendReal(trimmed);
    } else {
      _sendMock(trimmed);
    }
  }

  void _sendReal(String text) {
    final myId = ref.read(currentUserIdProvider);
    if (myId == null) return;

    // إدراج متفائل — بثّ Realtime سيستبدل القائمة بالصفوف الفعلية
    final optimistic =
        ChatMessage(text: text, time: 'الآن', sender: MessageSender.me);
    state = state.copyWith(messages: [...state.messages, optimistic]);

    ref
        .read(chatRepositoryProvider)
        .sendMessage(conversationId: arg, senderId: myId, content: text)
        .catchError((_) {
      if (_disposed) return;
      // تراجع: إزالة الرسالة المتفائلة عند الفشل
      state = state.copyWith(
        messages: [
          for (final m in state.messages)
            if (!identical(m, optimistic)) m,
        ],
        errorMessage: 'تعذّر إرسال الرسالة',
      );
    });
  }

  void _sendMock(String text) {
    state = state.copyWith(
      messages: [
        ...state.messages,
        ChatMessage(text: text, time: 'الآن', sender: MessageSender.me),
      ],
      isTyping: true,
    );

    _replyTimer?.cancel();
    _replyTimer = Timer(const Duration(milliseconds: 1200), () {
      final sellerCount =
          state.messages.where((m) => !m.isMe).length % _replyPool.length;
      state = state.copyWith(
        messages: [
          ...state.messages,
          ChatMessage(
              text: _replyPool[sellerCount],
              time: 'الآن',
              sender: MessageSender.seller),
        ],
        isTyping: false,
      );
    });
  }
}

final chatProvider = NotifierProvider.autoDispose
    .family<ChatNotifier, ChatState, String>(ChatNotifier.new);
