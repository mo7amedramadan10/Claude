import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../models/chat_message.dart';

class ChatState {
  const ChatState({
    this.messages = _initialMessages,
    this.isTyping = false,
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

  ChatState copyWith({List<ChatMessage>? messages, bool? isTyping}) =>
      ChatState(
        messages: messages ?? this.messages,
        isTyping: isTyping ?? this.isTyping,
      );
}

class ChatNotifier extends AutoDisposeFamilyNotifier<ChatState, String> {
  // Prototype simulation — replace with the realtime messaging layer.
  static const _replyPool = [
    'تمام، في خدمتك',
    'أكيد ولا يهمك',
    'حياك الله، بانتظارك في حي الملقا',
    'بكرة بعد العصر يناسبك للمعاينة؟',
  ];

  Timer? _replyTimer;

  @override
  ChatState build(String arg) {
    ref.onDispose(() => _replyTimer?.cancel());
    return const ChatState();
  }

  void send(String text) {
    final trimmed = text.trim();
    if (trimmed.isEmpty) return;

    state = state.copyWith(
      messages: [
        ...state.messages,
        ChatMessage(text: trimmed, time: 'الآن', sender: MessageSender.me),
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
