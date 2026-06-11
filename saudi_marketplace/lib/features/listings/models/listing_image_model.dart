import 'package:freezed_annotation/freezed_annotation.dart';

part 'listing_image_model.freezed.dart';
part 'listing_image_model.g.dart';

@freezed
class ListingImageModel with _$ListingImageModel {
  const factory ListingImageModel({
    required String id,
    @JsonKey(name: 'listing_id') required String listingId,
    // Path in Storage: {user_id}/{listing_id}/{uuid}.jpg
    @JsonKey(name: 'storage_path') required String storagePath,
    @JsonKey(name: 'sort_order') @Default(0) int sortOrder,
    @JsonKey(name: 'is_primary') @Default(false) bool isPrimary,
    @JsonKey(name: 'created_at') required DateTime createdAt,
  }) = _ListingImageModel;

  factory ListingImageModel.fromJson(Map<String, dynamic> json) =>
      _$ListingImageModelFromJson(json);
}
