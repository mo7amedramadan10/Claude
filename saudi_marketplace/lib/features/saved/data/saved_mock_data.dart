import '../models/saved_item.dart';

abstract final class SavedMockData {
  static const items = [
    SavedItem(
      id: '1',
      imageUrl:
          'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400&q=80',
      title: 'BMW 530i 2022',
      price: '٨٢,٥٠٠',
      city: 'الرياض',
      postedAt: 'منذ ساعتين',
      isVerified: true,
    ),
    SavedItem(
      id: '2',
      imageUrl:
          'https://images.unsplash.com/photo-1592899677977-9c10ca588bbd?w=400&q=80',
      title: 'iPhone 15 Pro Max',
      price: '٤,٢٠٠',
      city: 'جدة',
      postedAt: 'منذ ٤ ساعات',
      isVerified: true,
    ),
    SavedItem(
      id: '3',
      imageUrl:
          'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400&q=80',
      title: 'MacBook Pro M3',
      price: '٦,٥٠٠',
      city: 'الرياض',
      postedAt: 'منذ يوم',
      isVerified: false,
    ),
    SavedItem(
      id: '4',
      imageUrl:
          'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400&q=80',
      title: 'Rolex Submariner',
      price: '٣٨,٠٠٠',
      city: 'جدة',
      postedAt: 'منذ يومين',
      isVerified: true,
    ),
    SavedItem(
      id: '5',
      imageUrl:
          'https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=400&q=80',
      title: 'PlayStation 5 جديد',
      price: '١,٩٠٠',
      city: 'الدمام',
      postedAt: 'منذ ٣ أيام',
      isVerified: false,
    ),
    SavedItem(
      id: '6',
      imageUrl:
          'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=400&q=80',
      title: 'أريكة L شكل فاخرة',
      price: '٣,٨٠٠',
      city: 'الرياض',
      postedAt: 'منذ ٥ أيام',
      isVerified: true,
    ),
  ];
}
