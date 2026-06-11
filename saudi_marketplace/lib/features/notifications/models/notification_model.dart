import 'package:freezed_annotation/freezed_annotation.dart';

part 'notification_model.freezed.dart';
part 'notification_model.g.dart';

// type values:
// new_message · listing_inquiry · listing_sold · price_drop
// new_review · listing_approved · listing_rejected · system
@freezed
class NotificationModel with _$NotificationModel {
  const NotificationModel._();

  const factory NotificationModel({
    required String id,
    @JsonKey(name: 'user_id') required String userId,
    required String type,
    required String title,
    required String body,
    @Default(<String, dynamic>{}) Map<String, dynamic> data,
    @JsonKey(name: 'is_read') @Default(false) bool isRead,
    @JsonKey(name: 'read_at') DateTime? readAt,
    @JsonKey(name: 'created_at') required DateTime createdAt,
  }) = _NotificationModel;

  factory NotificationModel.fromJson(Map<String, dynamic> json) =>
      _$NotificationModelFromJson(json);

  bool get isUnread => !isRead;
}
