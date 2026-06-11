class ConversationModel {
  const ConversationModel({
    required this.id,
    required this.sellerName,
    required this.sellerAvatarUrl,
    required this.listingTitle,
    required this.listingImageUrl,
    required this.listingPrice,
    required this.lastMessage,
    required this.lastMessageTime,
    this.isOnline = false,
    this.unreadCount = 0,
    this.isLastMine = false,
    this.isRead = true,
  });

  final String id;
  final String sellerName;
  final String sellerAvatarUrl;
  final String listingTitle;
  final String listingImageUrl;
  final String listingPrice;
  final String lastMessage;
  final String lastMessageTime;
  final bool isOnline;
  final int unreadCount;
  final bool isLastMine;
  final bool isRead;
}
