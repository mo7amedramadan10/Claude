import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';

class ChatQuickReplies extends StatelessWidget {
  const ChatQuickReplies({
    super.key,
    required this.replies,
    required this.onReplyTap,
  });

  final List<String> replies;
  final ValueChanged<String> onReplyTap;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 47,
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
        itemCount: replies.length,
        separatorBuilder: (_, __) => const SizedBox(width: 8),
        itemBuilder: (_, i) => GestureDetector(
          onTap: () => onReplyTap(replies[i]),
          child: Container(
            padding:
                const EdgeInsets.symmetric(horizontal: 13, vertical: 7),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(20),
              border: Border.all(color: AppColors.grey200, width: 1),
            ),
            child: Text(
              replies[i],
              style: AppTextStyles.bodySmall.copyWith(
                fontSize: 12.5,
                color: AppColors.primaryDark,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class ChatInputBar extends StatefulWidget {
  const ChatInputBar({super.key, required this.onSend, this.onAttachTap});

  final ValueChanged<String> onSend;
  final VoidCallback? onAttachTap;

  @override
  State<ChatInputBar> createState() => _ChatInputBarState();
}

class _ChatInputBarState extends State<ChatInputBar> {
  final _controller = TextEditingController();

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _submit() {
    widget.onSend(_controller.text);
    _controller.clear();
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Container(
      decoration: BoxDecoration(
        color: isDark ? AppColors.surfaceDark : AppColors.surfaceLight,
        border: const Border(
          top: BorderSide(color: AppColors.grey100, width: 1),
        ),
      ),
      padding: const EdgeInsets.fromLTRB(12, 10, 12, 10),
      child: SafeArea(
        top: false,
        child: Row(children: [
          GestureDetector(
            onTap: widget.onAttachTap,
            child: Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: isDark ? AppColors.cardDark : AppColors.grey50,
                shape: BoxShape.circle,
              ),
              child: const Icon(TablerIcons.paperclip,
                  size: 21, color: AppColors.grey600),
            ),
          ),
          const SizedBox(width: 9),
          Expanded(
            child: TextField(
              controller: _controller,
              onSubmitted: (_) => _submit(),
              textInputAction: TextInputAction.send,
              style: AppTextStyles.bodyMedium.copyWith(
                fontSize: 13.5,
                color: isDark ? Colors.white : AppColors.navy,
              ),
              decoration: InputDecoration(
                hintText: 'اكتب رسالة...',
                hintStyle: AppTextStyles.bodyMedium.copyWith(
                    fontSize: 13.5, color: AppColors.grey400),
                isDense: true,
                filled: true,
                fillColor:
                    isDark ? AppColors.cardDark : const Color(0xFFF7F8FA),
                contentPadding: const EdgeInsets.symmetric(
                    horizontal: 16, vertical: 11),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(22),
                  borderSide:
                      const BorderSide(color: AppColors.grey200, width: 1),
                ),
                enabledBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(22),
                  borderSide:
                      const BorderSide(color: AppColors.grey200, width: 1),
                ),
                focusedBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(22),
                  borderSide:
                      const BorderSide(color: AppColors.primary, width: 1),
                ),
              ),
            ),
          ),
          const SizedBox(width: 9),
          GestureDetector(
            onTap: _submit,
            child: Container(
              width: 44,
              height: 44,
              decoration: const BoxDecoration(
                color: AppColors.primary,
                shape: BoxShape.circle,
                boxShadow: [
                  BoxShadow(
                    color: Color(0x591A7A4A),
                    blurRadius: 12,
                    offset: Offset(0, 4),
                  ),
                ],
              ),
              // Mirrored for RTL so the send arrow points outward
              child: Transform.flip(
                flipX: true,
                child: const Icon(TablerIcons.send,
                    size: 20, color: Colors.white),
              ),
            ),
          ),
        ]),
      ),
    );
  }
}
