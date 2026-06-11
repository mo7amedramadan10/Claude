import '../../home/models/listing_model.dart';

abstract final class SearchMockData {
  static const resultsCount = 124;

  static const results = [
    ListingModel(
      id: 'sr1', title: 'BMW 530i 2022', price: 82500, city: 'الرياض',
      imageUrl: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=400&q=80',
      postedAt: 'منذ ساعتين', isVerified: true,
    ),
    ListingModel(
      id: 'sr2', title: 'Toyota Camry 2023', price: 68000, city: 'الرياض',
      imageUrl: 'https://images.unsplash.com/photo-1571607388263-1044f9ea01dd?w=400&q=80',
      postedAt: 'منذ ٣ أيام', isVerified: true,
    ),
    ListingModel(
      id: 'sr3', title: 'Ford Mustang 2021', price: 125000, city: 'جدة',
      imageUrl: 'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=400&q=80',
      postedAt: 'منذ يوم',
    ),
    ListingModel(
      id: 'sr4', title: 'Mercedes C200 2020', price: 95000, city: 'الرياض',
      imageUrl: 'https://images.unsplash.com/photo-1494976388531-d1058494cdd8?w=400&q=80',
      postedAt: 'منذ ٤ ساعات', isVerified: true,
    ),
    ListingModel(
      id: 'sr5', title: 'Lexus ES 2022', price: 110000, city: 'الدمام',
      imageUrl: 'https://images.unsplash.com/photo-1583121274602-3e2820c69888?w=400&q=80',
      postedAt: 'منذ يومين', isVerified: true,
    ),
    ListingModel(
      id: 'sr6', title: 'Audi A6 2021', price: 138000, city: 'جدة',
      imageUrl: 'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=400&q=80',
      postedAt: 'منذ ٥ أيام',
    ),
    ListingModel(
      id: 'sr7', title: 'Porsche 911 2019', price: 320000, city: 'الرياض',
      imageUrl: 'https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=400&q=80',
      postedAt: 'منذ أسبوع', isVerified: true,
    ),
    ListingModel(
      id: 'sr8', title: 'Nissan Patrol 2023', price: 245000, city: 'مكة',
      imageUrl: 'https://images.unsplash.com/photo-1542362567-b07e54358753?w=400&q=80',
      postedAt: 'منذ ٦ ساعات', isVerified: true,
    ),
  ];
}
