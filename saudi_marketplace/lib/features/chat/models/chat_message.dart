enum MessageSender { me, seller }

class ChatMessage {
  const ChatMessage({
    required this.text,
    required this.time,
    required this.sender,
  });

  final String text;
  final String time;
  final MessageSender sender;

  bool get isMe => sender == MessageSender.me;
}
