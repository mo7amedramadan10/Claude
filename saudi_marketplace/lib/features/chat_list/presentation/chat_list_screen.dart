import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../providers/chat_list_provider.dart';
import 'widgets/conversation_tile.dart';

class ChatListScreen extends ConsumerWidget {
  const ChatListScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(chatListProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final conversations = state.conversations;
    final totalUnread = state.totalUnread;

    return Scaffold(
      body: SafeArea(
        child: Column(children: [
          // ─── Header ─────────────────────────────────────────────
          Container(
            padding: const EdgeInsets.fromLTRB(16, 6, 16, 14),
            decoration: BoxDecoration(
              color: isDark ? AppColors.surfaceDark : Colors.white,
              border: const Border(
                bottom: BorderSide(color: AppColors.grey100, width: 1),
              ),
            ),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.center,
              children: [
                IconButton(
                  onPressed: () => context.pop(),
                  icon: const Icon(TablerIcons.arrow_right),
                  style: IconButton.styleFrom(
                    backgroundColor:
                        isDark ? AppColors.cardDark : AppColors.grey50,
                    minimumSize: const Size(38, 38),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(11),
                    ),
                  ),
                  color: isDark ? Colors.white : AppColors.navy,
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: [
                      Text(
                        'المحادثات',
                        style: AppTextStyles.displayMedium.copyWith(
                          fontSize: 20,
                          color: isDark ? Colors.white : AppColors.navy,
                        ),
                      ),
                      if (totalUnread > 0) ...[
                        const SizedBox(width: 8),
                        Container(
                          constraints: const BoxConstraints(minWidth: 20),
                          height: 20,
                          padding:
                              const EdgeInsets.symmetric(horizontal: 6),
                          decoration: BoxDecoration(
                            color: AppColors.error,
                            borderRadius: BorderRadius.circular(10),
                          ),
                          child: Center(
                            child: Text(
                              '$totalUnread',
                              style: const TextStyle(
                                fontSize: 11,
                                fontWeight: FontWeight.w700,
                                color: Colors.white,
                                height: 1,
                              ),
                            ),
                          ),
                        ),
                      ],
                    ],
                  ),
                ),
              ],
            ),
          ),

          // ─── List ───────────────────────────────────────────────
          Expanded(
            child: state.isLoading && conversations.isEmpty
                ? const Center(
                    child:
                        CircularProgressIndicator(color: AppColors.primary),
                  )
                : conversations.isEmpty
                    ? _EmptyState()
                    : Container(
                        color:
                            isDark ? AppColors.backgroundDark : Colors.white,
                        child: RefreshIndicator(
                          onRefresh:
                              ref.read(chatListProvider.notifier).refresh,
                          child: ListView.builder(
                            physics:
                                const AlwaysScrollableScrollPhysics(),
                            itemCount: conversations.length,
                            itemBuilder: (_, i) {
                              final conv = conversations[i];
                              return ConversationTile(
                                conversation: conv,
                                isLast: i == conversations.length - 1,
                                onTap: () {
                                  ref
                                      .read(chatListProvider.notifier)
                                      .markRead(conv.id);
                                  context.push(
                                    AppRoutes.chatDetailPath(conv.id),
                                  );
                                },
                              );
                            },
                          ),
                        ),
                      ),
          ),
        ]),
      ),
    );
  }
}

class _EmptyState extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 96,
              height: 96,
              decoration: const BoxDecoration(
                color: AppColors.primaryTint,
                shape: BoxShape.circle,
              ),
              child: const Icon(
                TablerIcons.message_circle,
                size: 46,
                color: AppColors.primary,
              ),
            ),
            const SizedBox(height: 18),
            Text(
              'لا توجد محادثات',
              style: AppTextStyles.titleLarge.copyWith(
                fontSize: 17,
                fontWeight: FontWeight.w700,
                color: AppColors.navy,
              ),
            ),
            const SizedBox(height: 7),
            Text(
              'تواصل مع البائعين مباشرةً من صفحة الإعلان وستظهر محادثاتك هنا.',
              textAlign: TextAlign.center,
              style: AppTextStyles.bodySmall.copyWith(
                fontSize: 13,
                color: AppColors.grey600,
                height: 1.6,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
