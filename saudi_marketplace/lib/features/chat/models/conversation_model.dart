import 'package:freezed_annotation/freezed_annotation.dart';

import '../../auth/models/profile_model.dart';
import '../../listings/models/listing_model.dart';

part 'conversation_model.freezed.dart';
part 'conversation_model.g.dart';

@freezed
class ConversationModel with _$ConversationModel {
  const ConversationModel._();

  const factory ConversationModel({
    required String id,
    @JsonKey(name: 'listing_id') required String listingId,
    @JsonKey(name: 'buyer_id') required String buyerId,
    @JsonKey(name: 'seller_id') required String sellerId,
    @JsonKey(name: 'last_message_at') required DateTime lastMessageAt,
    @JsonKey(name: 'last_message_preview') String? lastMessagePreview,
    @JsonKey(name: 'last_sender_id') String? lastSenderId,
    @JsonKey(name: 'buyer_unread_count') @Default(0) int buyerUnreadCount,
    @JsonKey(name: 'seller_unread_count') @Default(0) int sellerUnreadCount,
    @JsonKey(name: 'is_archived_by_buyer') @Default(false) bool isArchivedByBuyer,
    @JsonKey(name: 'is_archived_by_seller') @Default(false) bool isArchivedBySeller,
    @JsonKey(name: 'created_at') required DateTime createdAt,
    // Joined fields
    ProfileModel? buyer,
    ProfileModel? seller,
    ListingModel? listing,
  }) = _ConversationModel;

  factory ConversationModel.fromJson(Map<String, dynamic> json) =>
      _$ConversationModelFromJson(json);

  int unreadCountFor(String userId) =>
      userId == buyerId ? buyerUnreadCount : sellerUnreadCount;

  bool isArchivedFor(String userId) =>
      userId == buyerId ? isArchivedByBuyer : isArchivedBySeller;

  ProfileModel? otherParticipant(String myId) =>
      myId == buyerId ? seller : buyer;
}
