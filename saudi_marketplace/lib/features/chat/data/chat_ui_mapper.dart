import '../../../core/supabase/storage_urls.dart';
import '../../../core/utils/number_formatter.dart';
import '../models/chat_header_info.dart';
import '../models/chat_message.dart';
import '../models/conversation_model.dart' as backend;
import '../models/message_model.dart';

/// «10:32 ص» — ساعة 12 مع ص/م
String formatClock(DateTime time) {
  final local = time.toLocal();
  final h = local.hour % 12 == 0 ? 12 : local.hour % 12;
  final m = local.minute.toString().padLeft(2, '0');
  final suffix = local.hour < 12 ? 'ص' : 'م';
  return '$h:$m $suffix';
}

extension MessageUiMapper on MessageModel {
  ChatMessage toUiModel(String myId) => ChatMessage(
        text: content,
        time: formatClock(createdAt),
        sender: senderId == myId ? MessageSender.me : MessageSender.seller,
      );
}

extension ChatHeaderMapper on backend.ConversationModel {
  ChatHeaderInfo toHeaderInfo(String myId) {
    final other = otherParticipant(myId);
    final listingImage = listing?.primaryImage;
    return ChatHeaderInfo(
      otherName: other?.displayName ?? 'مستخدم بيكيا',
      otherAvatarUrl: other?.avatarUrl != null
          ? StorageUrls.avatar(other!.avatarUrl!)
          : '',
      listingId: listingId,
      listingTitle: listing?.title ?? '',
      listingPrice: NumberFormatter.arabicPrice(listing?.price ?? 0),
      listingImageUrl: listingImage != null
          ? StorageUrls.listingImage(listingImage.storagePath)
          : '',
    );
  }
}
