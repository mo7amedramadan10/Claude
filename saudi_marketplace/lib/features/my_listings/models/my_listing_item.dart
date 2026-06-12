class MyListingItem {
  const MyListingItem({
    required this.id,
    required this.title,
    this.price,
    required this.status,
    required this.imageUrl,
    required this.viewsCount,
    required this.favoritesCount,
    required this.createdAt,
    this.isVerified = false,
  });

  final String id;
  final String title;
  final double? price;
  final String status; // active | sold | expired | draft | pending | rejected
  final String imageUrl;
  final int viewsCount;
  final int favoritesCount;
  final DateTime createdAt;
  final bool isVerified;

  bool get isActive  => status == 'active';
  bool get isSold    => status == 'sold';
  bool get isExpired => status == 'expired';
  bool get isDraft   => status == 'draft';
  bool get isPending => status == 'pending';

  MyListingItem copyWith({String? status}) => MyListingItem(
        id: id,
        title: title,
        price: price,
        status: status ?? this.status,
        imageUrl: imageUrl,
        viewsCount: viewsCount,
        favoritesCount: favoritesCount,
        createdAt: createdAt,
        isVerified: isVerified,
      );
}
