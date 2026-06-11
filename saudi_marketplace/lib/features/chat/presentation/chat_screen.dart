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
    final header = state.header;
    final isDark = Theme.of(context).brightness == Brightness.dark;

    // Auto-scroll whenever a message is added or typing state changes
    ref.listen(chatProvider(widget.chatId), (_, __) => _scrollToBottom());

    if (state.isLoading) {
      return Scaffold(
        backgroundColor:
            isDark ? AppColors.backgroundDark : AppColors.chatBackground,
        body: const Center(
          child: CircularProgressIndicator(color: AppColors.primary),
        ),
      );
    }

    if (state.errorMessage != null && state.messages.isEmpty) {
      return Scaffold(
        backgroundColor:
            isDark ? AppColors.backgroundDark : AppColors.chatBackground,
        appBar: AppBar(
          backgroundColor: Colors.transparent,
          leading: BackButton(onPressed: () => context.pop()),
        ),
        body: Center(
          child: Text(
            state.errorMessage!,
            textAlign: TextAlign.center,
            style: AppTextStyles.bodyMedium
                .copyWith(color: AppColors.grey600),
          ),
        ),
      );
    }

    return Scaffold(
      backgroundColor:
          isDark ? AppColors.backgroundDark : AppColors.chatBackground,
      body: SafeArea(
        child: Column(children: [
          ChatHeader(
            sellerName: header.otherName,
            avatarUrl: header.otherAvatarUrl,
            onBack: () => context.pop(),
          ),
          ChatListingBar(
            title: header.listingTitle,
            price: header.listingPrice,
            imageUrl: header.listingImageUrl,
            onTap: () => context
                .push(AppRoutes.productDetailsPath(header.listingId)),
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
