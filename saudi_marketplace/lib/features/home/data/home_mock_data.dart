import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../models/category_model.dart';
import '../models/listing_model.dart';
import '../models/seller_model.dart';

abstract final class HomeMockData {
  static const categories = [
    CategoryModel(id: 'cars', label: 'سيارات', icon: TablerIcons.car),
    CategoryModel(id: 'real_estate', label: 'عقارات', icon: TablerIcons.building),
    CategoryModel(id: 'phones', label: 'جوالات', icon: TablerIcons.device_mobile),
    CategoryModel(id: 'electronics', label: 'إلكترونيات', icon: TablerIcons.device_laptop),
    CategoryModel(id: 'furniture', label: 'أثاث', icon: TablerIcons.armchair),
    CategoryModel(id: 'fashion', label: 'أزياء', icon: TablerIcons.shirt),
    CategoryModel(id: 'jobs', label: 'وظائف', icon: TablerIcons.briefcase),
    CategoryModel(id: 'more', label: 'المزيد', icon: TablerIcons.grid_dots),
  ];

  static const featuredListings = [
    ListingModel(
      id: '1', title: 'BMW 530i موديل 2022', price: 82500, city: 'الرياض',
      imageUrl: 'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=300&q=80',
      postedAt: 'منذ ساعتين', rating: 4.9, isVerified: true, isFavorite: true,
    ),
    ListingModel(
      id: '2', title: 'iPhone 15 Pro Max', price: 4200, city: 'جدة',
      imageUrl: 'https://images.unsplash.com/photo-1592899677977-9c10ca588bbd?w=300&q=80',
      postedAt: 'منذ ٤ ساعات', rating: 4.8, isVerified: true,
    ),
    ListingModel(
      id: '3', title: 'أريكة L شكل فاخرة', price: 3800, city: 'الرياض',
      imageUrl: 'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=300&q=80',
      postedAt: 'منذ يوم', rating: 4.7, isVerified: true,
    ),
  ];

  static const nearbyListings = [
    ListingModel(
      id: '4', title: 'MacBook Pro M3', price: 6500, city: 'الرياض',
      imageUrl: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=300&q=80',
      distanceKm: 1.2,
    ),
    ListingModel(
      id: '5', title: 'PlayStation 5 جديد', price: 1900, city: 'الرياض',
      imageUrl: 'https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=300&q=80',
      distanceKm: 2.7,
    ),
    ListingModel(
      id: '6', title: 'Rolex Submariner', price: 38000, city: 'الرياض',
      imageUrl: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=300&q=80',
      distanceKm: 3.1,
    ),
  ];

  static const suggestedListings = [
    ListingModel(
      id: '7', title: 'Toyota Camry 2023', price: 68000, city: 'الرياض',
      imageUrl: 'https://images.unsplash.com/photo-1571607388263-1044f9ea01dd?w=300&q=80',
      postedAt: 'منذ ٣ أيام', rating: 5.0, isAiRecommended: true,
    ),
    ListingModel(
      id: '8', title: 'Samsung Galaxy S24 Ultra', price: 3100, city: 'جدة',
      imageUrl: 'https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=300&q=80',
      postedAt: 'منذ ٥ ساعات', rating: 4.6, isAiRecommended: true, isFavorite: true,
    ),
    ListingModel(
      id: '9', title: 'Sony A7IV كاميرا', price: 9800, city: 'الدمام',
      imageUrl: 'https://images.unsplash.com/photo-1555529669-e69e7aa0ba9a?w=300&q=80',
      postedAt: 'منذ يومين', rating: 4.8, isAiRecommended: true,
    ),
  ];

  static const sellers = [
    SellerModel(id: 's1', name: 'محمد العتيبي', rating: 5.0, completedDeals: 243,
      avatarUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=100&q=80'),
    SellerModel(id: 's2', name: 'فيصل الزهراني', rating: 4.9, completedDeals: 187,
      avatarUrl: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=100&q=80'),
    SellerModel(id: 's3', name: 'عبدالله القحطاني', rating: 4.8, completedDeals: 96,
      avatarUrl: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=100&q=80'),
    SellerModel(id: 's4', name: 'خالد المالكي', rating: 4.9, completedDeals: 312,
      avatarUrl: 'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=100&q=80'),
  ];
}
