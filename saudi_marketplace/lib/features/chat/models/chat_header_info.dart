/// بيانات رأس شاشة المحادثة (الطرف الآخر + شريط الإعلان)
class ChatHeaderInfo {
  const ChatHeaderInfo({
    required this.otherName,
    required this.otherAvatarUrl,
    required this.listingId,
    required this.listingTitle,
    required this.listingPrice,
    required this.listingImageUrl,
  });

  final String otherName;
  final String otherAvatarUrl;
  final String listingId;
  final String listingTitle;
  final String listingPrice;
  final String listingImageUrl;

  static const mock = ChatHeaderInfo(
    otherName: 'محمد العتيبي',
    otherAvatarUrl:
        'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=120&q=80',
    listingId: '1',
    listingTitle: 'BMW 530i موديل 2022',
    listingPrice: '٨٢,٥٠٠',
    listingImageUrl:
        'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=160&q=80',
  );
}
