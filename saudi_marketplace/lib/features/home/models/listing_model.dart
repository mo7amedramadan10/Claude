class ListingModel {
  const ListingModel({
    required this.id,
    required this.title,
    required this.price,
    required this.city,
    required this.imageUrl,
    this.postedAt = '',
    this.rating,
    this.distanceKm,
    this.isVerified = false,
    this.isAiRecommended = false,
    this.isFavorite = false,
  });

  final String id;
  final String title;
  final double price;
  final String city;
  final String imageUrl;
  final String postedAt;
  final double? rating;
  final double? distanceKm;
  final bool isVerified;
  final bool isAiRecommended;
  final bool isFavorite;

  ListingModel copyWith({bool? isFavorite}) => ListingModel(
        id: id,
        title: title,
        price: price,
        city: city,
        imageUrl: imageUrl,
        postedAt: postedAt,
        rating: rating,
        distanceKm: distanceKm,
        isVerified: isVerified,
        isAiRecommended: isAiRecommended,
        isFavorite: isFavorite ?? this.isFavorite,
      );
}
