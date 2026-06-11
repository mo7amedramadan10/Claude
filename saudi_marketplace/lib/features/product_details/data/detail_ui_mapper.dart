import 'package:flutter/widgets.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../core/supabase/storage_urls.dart';
import '../../../core/utils/relative_time.dart';
import '../../home/data/home_ui_mappers.dart';
import '../../home/models/seller_model.dart';
import '../../listings/models/listing_model.dart' as backend;
import '../../listings/models/listing_spec_model.dart';
import '../models/listing_detail_model.dart';

class _SpecMeta {
  const _SpecMeta(this.icon, this.label);
  final IconData icon;
  final String label;
}

/// خريطة spec_key الموحّدة (إنجليزي في القاعدة) → أيقونة + تسمية عربية
const _specMeta = <String, _SpecMeta>{
  'model_year':   _SpecMeta(TablerIcons.calendar_event, 'الموديل'),
  'mileage':      _SpecMeta(TablerIcons.gauge, 'الممشى (كم)'),
  'transmission': _SpecMeta(TablerIcons.settings_2, 'ناقل الحركة'),
  'fuel_type':    _SpecMeta(TablerIcons.gas_station, 'الوقود'),
  'memory':       _SpecMeta(TablerIcons.device_sd_card, 'الذاكرة'),
  'color':        _SpecMeta(TablerIcons.palette, 'اللون'),
  'area_sqm':     _SpecMeta(TablerIcons.ruler_measure, 'المساحة (م²)'),
  'rooms':        _SpecMeta(TablerIcons.door, 'الغرف'),
  'floor':        _SpecMeta(TablerIcons.stairs, 'الدور'),
  'brand':        _SpecMeta(TablerIcons.tag, 'الماركة'),
  'condition':    _SpecMeta(TablerIcons.sparkles, 'الحالة'),
};

const _defaultSafetyNotice =
    'للسلامة: عاين السلعة قبل الدفع، وتمّ الصفقة وجهاً لوجه. '
    'لا تحوّل أي مبلغ مقدّماً قبل المعاينة.';

extension ListingDetailMapper on backend.ListingModel {
  ListingDetailModel toDetailModel() {
    final cityName = city?.nameAr ?? '';
    final district = neighborhood ?? cityName;

    final sortedImages = [...?images]
      ..sort((a, b) => a.sortOrder.compareTo(b.sortOrder));

    return ListingDetailModel(
      id: id,
      title: title,
      price: price ?? 0,
      location: neighborhood != null ? '$cityName، $neighborhood' : cityName,
      district: district,
      postedAt: RelativeTime.format(createdAt),
      description: description,
      // عنصر واحد فارغ على الأقل — الـ gallery يفهرس القائمة مباشرة
      // والرابط الفارغ يعرض errorWidget (أيقونة صورة مفقودة)
      gallery: sortedImages.isEmpty
          ? const ['']
          : [
              for (final img in sortedImages)
                StorageUrls.listingImage(img.storagePath),
            ],
      specs: [for (final s in _sortedSpecs) s.toSpecItem()],
      seller: seller?.toUiModel() ??
          const SellerModel(
            id: '',
            name: 'بائع بيكيا',
            avatarUrl: '',
            rating: 0,
            completedDeals: 0,
            isNafadhVerified: false,
          ),
      isVerified: isVerified,
      verifiedNote: isVerified ? 'إعلان موثق' : '',
      safetyNotice: safetyNote ?? _defaultSafetyNotice,
    );
  }

  List<ListingSpecModel> get _sortedSpecs =>
      [...?specs]..sort((a, b) => a.sortOrder.compareTo(b.sortOrder));
}

extension on ListingSpecModel {
  SpecItem toSpecItem() {
    final meta = _specMeta[specKey];
    return SpecItem(
      icon: meta?.icon ?? TablerIcons.info_circle,
      value: specValue,
      label: meta?.label ?? specKey,
    );
  }
}
