import 'package:freezed_annotation/freezed_annotation.dart';

part 'city_model.freezed.dart';
part 'city_model.g.dart';

@freezed
class CityModel with _$CityModel {
  const factory CityModel({
    required String id,
    @JsonKey(name: 'name_ar') required String nameAr,
    @JsonKey(name: 'name_en') required String nameEn,
    @JsonKey(name: 'region_ar') String? regionAr,
    @JsonKey(name: 'region_en') String? regionEn,
    double? latitude,
    double? longitude,
    @JsonKey(name: 'is_active') @Default(true) bool isActive,
    @JsonKey(name: 'sort_order') @Default(0) int sortOrder,
    @JsonKey(name: 'created_at') required DateTime createdAt,
  }) = _CityModel;

  factory CityModel.fromJson(Map<String, dynamic> json) =>
      _$CityModelFromJson(json);
}
