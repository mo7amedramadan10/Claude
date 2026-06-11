import 'package:freezed_annotation/freezed_annotation.dart';

import '../../auth/models/profile_model.dart';

part 'review_model.freezed.dart';
part 'review_model.g.dart';

@freezed
class ReviewModel with _$ReviewModel {
  const factory ReviewModel({
    required String id,
    @JsonKey(name: 'reviewer_id') required String reviewerId,
    @JsonKey(name: 'reviewee_id') required String revieweeId,
    @JsonKey(name: 'listing_id') String? listingId,
    @JsonKey(name: 'conversation_id') String? conversationId,
    required int rating,
    String? comment,
    @JsonKey(name: 'created_at') required DateTime createdAt,
    // Joined fields
    ProfileModel? reviewer,
  }) = _ReviewModel;

  factory ReviewModel.fromJson(Map<String, dynamic> json) =>
      _$ReviewModelFromJson(json);
}
