import 'package:freezed_annotation/freezed_annotation.dart';

part 'profile_model.freezed.dart';
part 'profile_model.g.dart';

@freezed
class ProfileModel with _$ProfileModel {
  const ProfileModel._();

  const factory ProfileModel({
    required String id,
    @JsonKey(name: 'full_name') required String fullName,
    // phone is only returned when reading own profile (not in public_profiles view)
    String? phone,
    @JsonKey(name: 'avatar_url') String? avatarUrl,
    String? bio,
    @JsonKey(name: 'city_id') String? cityId,
    @JsonKey(name: 'is_nafath_verified') @Default(false) bool isNafathVerified,
    @JsonKey(name: 'nafath_verified_at') DateTime? nafathVerifiedAt,
    @JsonKey(name: 'rating_avg') @Default(0.0) double ratingAvg,
    @JsonKey(name: 'total_reviews') @Default(0) int totalReviews,
    @JsonKey(name: 'completed_deals') @Default(0) int completedDeals,
    @JsonKey(name: 'active_listings_count') @Default(0) int activeListingsCount,
    @JsonKey(name: 'is_active') @Default(true) bool isActive,
    @JsonKey(name: 'last_seen_at') DateTime? lastSeenAt,
    @JsonKey(name: 'deleted_at') DateTime? deletedAt,
    @JsonKey(name: 'created_at') required DateTime createdAt,
    @JsonKey(name: 'updated_at') required DateTime updatedAt,
  }) = _ProfileModel;

  factory ProfileModel.fromJson(Map<String, dynamic> json) =>
      _$ProfileModelFromJson(json);

  bool get isDeleted => deletedAt != null;
  String get displayName => fullName.isNotEmpty ? fullName : 'مستخدم بيكيا';
}
