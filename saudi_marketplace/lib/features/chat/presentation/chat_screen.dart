import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../providers/chat_provider.dart';
import 'widgets/chat_header.dart';
import 'widgets/chat_input_bar.dart';
import 'widgets/chat_listing_bar.dart';
import 'widgets/chat_message_bubble.dart';

class ChatScreen extends ConsumerStatefulWidget {
  const ChatScreen({super.key, required this.chatId});

  final String chatId;

  @override
  ConsumerState<ChatScreen> createState() => _ChatScreenState();
}

class _ChatScreenState extends ConsumerState<ChatScreen> {
  final _scrollController = ScrollController();

  @override
  void dispose() {
    _scrollController.dispose();
    super.dispose();
  }

  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (_scrollController.hasClients) {
        _scrollController.animateTo(
          _scrollController.position.maxScrollExtent,
          duration: const Duration(milliseconds: 250),
          curve: Curves.easeOut,
        );
      }
    });
  }

  void _send(String text) {
    ref.read(chatProvider(widget.chatId).notifier).send(text);
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(chatProvider(widget.chatId));
    final isDark = Theme.of(context).brightness == Brightness.dark;

    // Auto-scroll whenever a message is added or typing state changes
    ref.listen(chatProvider(widget.chatId), (_, __) => _scrollToBottom());

    return Scaffold(
      backgroundColor:
          isDark ? AppColors.backgroundDark : AppColors.chatBackground,
      body: SafeArea(
        child: Column(children: [
          ChatHeader(
            sellerName: 'محمد العتيبي',
            avatarUrl:
                'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=120&q=80',
            onBack: () => context.pop(),
          ),
          ChatListingBar(
            title: 'BMW 530i موديل 2022',
            price: '٨٢,٥٠٠',
            imageUrl:
                'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=160&q=80',
            onTap: () => context.push(AppRoutes.productDetailsPath('1')),
          ),
          Expanded(
            child: ListView.separated(
              controller: _scrollController,
              padding: const EdgeInsets.fromLTRB(14, 16, 14, 8),
              itemCount: state.messages.length +
                  1 + // date chip
                  (state.isTyping ? 1 : 0),
              separatorBuilder: (_, __) => const SizedBox(height: 13),
              itemBuilder: (_, i) {
                if (i == 0) return const _DateChip(label: 'اليوم');
                if (i <= state.messages.length) {
                  return ChatMessageBubble(message: state.messages[i - 1]);
                }
                return const TypingIndicator();
              },
            ),
          ),
          ChatQuickReplies(
            replies: ChatState.quickReplies,
            onReplyTap: _send,
          ),
          ChatInputBar(onSend: _send),
        ]),
      ),
    );
  }
}

class _DateChip extends StatelessWidget {
  const _DateChip({required this.label});

  final String label;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
        decoration: BoxDecoration(
          color: AppColors.navy.withValues(alpha: 0.07),
          borderRadius: BorderRadius.circular(20),
        ),
        child: Text(
          label,
          style: AppTextStyles.labelLarge.copyWith(
            color: const Color(0xFF5A6577),
            fontWeight: FontWeight.w600,
          ),
        ),
      ),
    );
  }
}
