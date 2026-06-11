import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../models/chat_message.dart';

/// In RTL: my messages sit on the visual left, seller's on the visual right.
class ChatMessageBubble extends StatelessWidget {
  const ChatMessageBubble({super.key, required this.message});

  final ChatMessage message;

  static const _sellerBubbleBorder = Color(0xFFEAE8E1);

  @override
  Widget build(BuildContext context) {
    final isMe = message.isMe;
    return Column(
      crossAxisAlignment:
          isMe ? CrossAxisAlignment.end : CrossAxisAlignment.start,
      children: [
        ConstrainedBox(
          constraints: BoxConstraints(
            maxWidth: MediaQuery.of(context).size.width * 0.8,
          ),
          child: Container(
            padding:
                const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
            decoration: BoxDecoration(
              color: isMe ? AppColors.primary : Colors.white,
              border: isMe
                  ? null
                  : Border.all(color: _sellerBubbleBorder, width: 1),
              borderRadius: BorderRadius.only(
                topLeft: const Radius.circular(16),
                topRight: const Radius.circular(16),
                bottomLeft: Radius.circular(isMe ? 4 : 16),
                bottomRight: Radius.circular(isMe ? 16 : 4),
              ),
            ),
            child: Text(
              message.text,
              style: AppTextStyles.bodyMedium.copyWith(
                fontSize: 13.5,
                height: 1.6,
                color: isMe ? Colors.white : AppColors.navy,
              ),
            ),
          ),
        ),
        const SizedBox(height: 4),
        Row(mainAxisSize: MainAxisSize.min, children: [
          if (isMe) ...[
            const Icon(TablerIcons.checks,
                size: 14, color: AppColors.readTick),
            const SizedBox(width: 3),
          ],
          Text(message.time,
              style: AppTextStyles.labelSmall
                  .copyWith(color: AppColors.grey400)),
        ]),
      ],
    );
  }
}

class TypingIndicator extends StatefulWidget {
  const TypingIndicator({super.key});

  @override
  State<TypingIndicator> createState() => _TypingIndicatorState();
}

class _TypingIndicatorState extends State<TypingIndicator>
    with SingleTickerProviderStateMixin {
  late final AnimationController _controller = AnimationController(
    vsync: this,
    duration: const Duration(milliseconds: 1200),
  )..repeat();

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Align(
      alignment: AlignmentDirectional.centerStart,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 11),
        decoration: BoxDecoration(
          color: Colors.white,
          border: Border.all(
              color: ChatMessageBubble._sellerBubbleBorder, width: 1),
          borderRadius: const BorderRadius.only(
            topLeft: Radius.circular(16),
            topRight: Radius.circular(16),
            bottomLeft: Radius.circular(16),
            bottomRight: Radius.circular(4),
          ),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: List.generate(3, (i) {
            return AnimatedBuilder(
              animation: _controller,
              builder: (_, __) {
                final t = (_controller.value - i * 0.15) % 1.0;
                final bounce =
                    t < 0.3 ? (0.3 - (t - 0.15).abs() * 2) * 13.3 : 0.0;
                return Container(
                  width: 7,
                  height: 7,
                  margin: EdgeInsetsDirectional.only(start: i == 0 ? 0 : 4),
                  transform:
                      Matrix4.translationValues(0, -bounce.clamp(0, 4), 0),
                  decoration: BoxDecoration(
                    color: AppColors.grey400
                        .withValues(alpha: bounce > 0.5 ? 1 : 0.4),
                    shape: BoxShape.circle,
                  ),
                );
              },
            );
          }),
        ),
      ),
    );
  }
}
