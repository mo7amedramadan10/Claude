import 'package:freezed_annotation/freezed_annotation.dart';

part 'listing_spec_model.freezed.dart';
part 'listing_spec_model.g.dart';

// Unified English spec_key examples:
// model_year · mileage · transmission · fuel_type
// memory · color · area_sqm · rooms · floor
@freezed
class ListingSpecModel with _$ListingSpecModel {
  const factory ListingSpecModel({
    required String id,
    @JsonKey(name: 'listing_id') required String listingId,
    @JsonKey(name: 'spec_key') required String specKey,
    @JsonKey(name: 'spec_value') required String specValue,
    String? icon,
    @JsonKey(name: 'sort_order') @Default(0) int sortOrder,
  }) = _ListingSpecModel;

  factory ListingSpecModel.fromJson(Map<String, dynamic> json) =>
      _$ListingSpecModelFromJson(json);
}
