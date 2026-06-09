class SellerModel {
  const SellerModel({
    required this.id,
    required this.name,
    required this.avatarUrl,
    required this.rating,
    required this.completedDeals,
    this.isNafadhVerified = true,
  });

  final String id;
  final String name;
  final String avatarUrl;
  final double rating;
  final int completedDeals;
  final bool isNafadhVerified;
}
