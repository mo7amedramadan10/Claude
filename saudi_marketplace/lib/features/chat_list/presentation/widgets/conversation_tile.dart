import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../models/conversation_model.dart';

class ConversationTile extends StatelessWidget {
  const ConversationTile({
    super.key,
    required this.conversation,
    required this.onTap,
    required this.isLast,
  });

  final ConversationModel conversation;
  final VoidCallback onTap;
  final bool isLast;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final c = conversation;
    final hasUnread = c.unreadCount > 0;

    return InkWell(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 13),
        decoration: BoxDecoration(
          border: isLast
              ? null
              : Border(
                  bottom: BorderSide(
                    color: isDark
                        ? AppColors.navyLight
                        : const Color(0xFFF1F3F6),
                    width: 1,
                  ),
                ),
        ),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // ─── Avatar + online dot ─────────────────────────────
            Stack(
              children: [
                Container(
                  width: 50,
                  height: 50,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    border: Border.all(
                      color: AppColors.primary.withOpacity(0.35),
                      width: 2,
                    ),
                  ),
                  clipBehavior: Clip.hardEdge,
                  child: CachedNetworkImage(
                    imageUrl: c.sellerAvatarUrl,
                    fit: BoxFit.cover,
                    placeholder: (_, __) =>
                        Container(color: AppColors.grey100),
                    errorWidget: (_, __, ___) =>
                        Container(color: AppColors.grey100),
                  ),
                ),
                if (c.isOnline)
                  Positioned(
                    bottom: 1,
                    left: 1,
                    child: Container(
                      width: 12,
                      height: 12,
                      decoration: BoxDecoration(
                        color: AppColors.online,
                        shape: BoxShape.circle,
                        border: Border.all(
                          color: isDark
                              ? AppColors.cardDark
                              : Colors.white,
                          width: 2,
                        ),
                      ),
                    ),
                  ),
              ],
            ),
            const SizedBox(width: 13),

            // ─── Content ─────────────────────────────────────────
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          c.sellerName,
                          style: AppTextStyles.titleMedium.copyWith(
                            fontSize: 14,
                            fontWeight: FontWeight.w700,
                            color: isDark ? Colors.white : AppColors.navy,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                      Text(
                        c.lastMessageTime,
                        style: AppTextStyles.labelLarge.copyWith(
                          fontSize: 11,
                          color: hasUnread
                              ? AppColors.primary
                              : AppColors.grey400,
                          fontWeight: hasUnread
                              ? FontWeight.w700
                              : FontWeight.w500,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 3),
                  // Listing label
                  Text(
                    c.listingTitle,
                    style: AppTextStyles.labelLarge.copyWith(
                      fontSize: 11.5,
                      color: AppColors.primary,
                      fontWeight: FontWeight.w600,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 4),
                  Row(
                    children: [
                      // Read ticks for my messages
                      if (c.isLastMine) ...[
                        Icon(
                          TablerIcons.checks,
                          size: 13,
                          color: c.isRead
                              ? AppColors.readTick
                              : AppColors.grey400,
                        ),
                        const SizedBox(width: 4),
                      ],
                      Expanded(
                        child: Text(
                          c.lastMessage,
                          style: AppTextStyles.bodySmall.copyWith(
                            fontSize: 12.5,
                            color: hasUnread
                                ? (isDark ? Colors.white70 : AppColors.navy)
                                : AppColors.grey600,
                            fontWeight: hasUnread
                                ? FontWeight.w600
                                : FontWeight.w400,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                      if (hasUnread) ...[
                        const SizedBox(width: 8),
                        Container(
                          constraints:
                              const BoxConstraints(minWidth: 20),
                          height: 20,
                          padding:
                              const EdgeInsets.symmetric(horizontal: 6),
                          decoration: BoxDecoration(
                            color: AppColors.primary,
                            borderRadius: BorderRadius.circular(10),
                          ),
                          child: Center(
                            child: Text(
                              '${c.unreadCount}',
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
                ],
              ),
            ),

            // ─── Listing thumbnail ───────────────────────────────
            const SizedBox(width: 10),
            ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: CachedNetworkImage(
                imageUrl: c.listingImageUrl,
                width: 46,
                height: 46,
                fit: BoxFit.cover,
                placeholder: (_, __) =>
                    Container(color: AppColors.grey100),
                errorWidget: (_, __, ___) =>
                    Container(color: AppColors.grey100),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
