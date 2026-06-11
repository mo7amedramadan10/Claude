class SavedItem {
  const SavedItem({
    required this.id,
    required this.imageUrl,
    required this.title,
    required this.price,
    required this.city,
    required this.postedAt,
    this.isVerified = false,
  });

  final String id;
  final String imageUrl;
  final String title;
  final String price;
  final String city;
  final String postedAt;
  final bool isVerified;
}
