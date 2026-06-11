import 'package:flutter/widgets.dart';

import '../../home/models/seller_model.dart';

class SpecItem {
  const SpecItem({
    required this.icon,
    required this.value,
    required this.label,
  });

  final IconData icon;
  final String value;
  final String label;
}

class ListingDetailModel {
  const ListingDetailModel({
    required this.id,
    required this.title,
    required this.price,
    required this.location,
    required this.district,
    required this.postedAt,
    required this.description,
    required this.gallery,
    required this.specs,
    required this.seller,
    this.isVerified = false,
    this.verifiedNote = '',
    this.safetyNotice = '',
  });

  final String id;
  final String title;
  final double price;
  final String location;
  final String district;
  final String postedAt;
  final String description;
  final List<String> gallery;
  final List<SpecItem> specs;
  final SellerModel seller;
  final bool isVerified;
  final String verifiedNote;
  final String safetyNotice;
}
