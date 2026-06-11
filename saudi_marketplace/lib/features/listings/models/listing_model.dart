import 'package:collection/collection.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

import '../../../core/models/category_model.dart';
import '../../../core/models/city_model.dart';
import '../../auth/models/profile_model.dart';
import 'listing_image_model.dart';
import 'listing_spec_model.dart';

part 'listing_model.freezed.dart';
part 'listing_model.g.dart';

// status values: active · sold · pending · rejected · expired · draft
// condition values: excellent · very_good · good · for_parts
@freezed
class ListingModel with _$ListingModel {
  const ListingModel._();

  const factory ListingModel({
    required String id,
    @JsonKey(name: 'seller_id') required String sellerId,
    @JsonKey(name: 'category_id') required String categoryId,
    @JsonKey(name: 'city_id') required String cityId,
    required String title,
    required String description,
    // NULL allowed for jobs / services / donations
    double? price,
    @Default('SAR') String currency,
    @JsonKey(name: 'is_price_negotiable') @Default(false) bool isPriceNegotiable,
    @JsonKey(name: 'is_price_hidden') @Default(false) bool isPriceHidden,
    @Default('good') String condition,
    @Default('active') String status,
    String? neighborhood,
    @JsonKey(name: 'is_verified') @Default(false) bool isVerified,
    @JsonKey(name: 'is_featured') @Default(false) bool isFeatured,
    @JsonKey(name: 'safety_note') String? safetyNote,
    @JsonKey(name: 'views_count') @Default(0) int viewsCount,
    @JsonKey(name: 'favorites_count') @Default(0) int favoritesCount,
    @JsonKey(name: 'contact_count') @Default(0) int contactCount,
    @JsonKey(name: 'sold_to_id') String? soldToId,
    @JsonKey(name: 'sold_at') DateTime? soldAt,
    @JsonKey(name: 'expires_at') required DateTime expiresAt,
    @JsonKey(name: 'deleted_at') DateTime? deletedAt,
    @JsonKey(name: 'created_at') required DateTime createdAt,
    @JsonKey(name: 'updated_at') required DateTime updatedAt,
    // Joined fields (present only when queried with select joins)
    @JsonKey(name: 'listing_images') List<ListingImageModel>? images,
    @JsonKey(name: 'listing_specs') List<ListingSpecModel>? specs,
    ProfileModel? seller,
    CityModel? city,
    CategoryModel? category,
  }) = _ListingModel;

  factory ListingModel.fromJson(Map<String, dynamic> json) =>
      _$ListingModelFromJson(json);

  bool get isActive   => status == 'active'   && deletedAt == null;
  bool get isSold     => status == 'sold';
  bool get isDeleted  => deletedAt != null;
  bool get hasPrice   => !isPriceHidden && price != null;

  ListingImageModel? get primaryImage =>
      images?.where((i) => i.isPrimary).firstOrNull ?? images?.firstOrNull;

  String get displayPrice {
    if (isPriceHidden) return 'اتصل بنا';
    if (price == null) return 'مجاني';
    final p = price!;
    final isWhole = p == p.floorToDouble();
    return isWhole ? '${p.toInt()} $currency' : '${p.toStringAsFixed(2)} $currency';
  }
}
