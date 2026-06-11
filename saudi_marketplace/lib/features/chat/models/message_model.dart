import 'package:freezed_annotation/freezed_annotation.dart';

part 'message_model.freezed.dart';
part 'message_model.g.dart';

// message_type values: text · image · quick_reply · system
@freezed
class MessageModel with _$MessageModel {
  const MessageModel._();

  const factory MessageModel({
    required String id,
    @JsonKey(name: 'conversation_id') required String conversationId,
    @JsonKey(name: 'sender_id') required String senderId,
    required String content,
    @JsonKey(name: 'message_type') @Default('text') String messageType,
    // Storage path for image messages: {sender_id}/{conversation_id}/{uuid}.ext
    @JsonKey(name: 'storage_path') String? storagePath,
    @JsonKey(name: 'is_read') @Default(false) bool isRead,
    @JsonKey(name: 'read_at') DateTime? readAt,
    @JsonKey(name: 'created_at') required DateTime createdAt,
  }) = _MessageModel;

  factory MessageModel.fromJson(Map<String, dynamic> json) =>
      _$MessageModelFromJson(json);

  bool isFromUser(String userId) => senderId == userId;
  bool get isImage => messageType == 'image';
  bool get isSystem => messageType == 'system';
}
