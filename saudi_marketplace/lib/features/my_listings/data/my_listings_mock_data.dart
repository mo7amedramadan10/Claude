import '../models/my_listing_item.dart';

abstract final class MyListingsMockData {
  static final items = <MyListingItem>[
    MyListingItem(
      id: 'ml-1',
      title: 'BMW 530i موديل 2022 فل كامل',
      price: 82500,
      status: 'active',
      imageUrl:
          'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=200&q=80',
      viewsCount: 142,
      favoritesCount: 18,
      createdAt: DateTime.now().subtract(const Duration(hours: 2)),
      isVerified: true,
    ),
    MyListingItem(
      id: 'ml-2',
      title: 'آيفون 15 Pro Max 256GB تيتانيوم',
      price: 4200,
      status: 'active',
      imageUrl:
          'https://images.unsplash.com/photo-1591337676887-a217a6970a8a?w=200&q=80',
      viewsCount: 89,
      favoritesCount: 12,
      createdAt: DateTime.now().subtract(const Duration(days: 1)),
    ),
    MyListingItem(
      id: 'ml-3',
      title: 'كنبة ركنية عائلية بحالة ممتازة',
      price: 1800,
      status: 'sold',
      imageUrl:
          'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=200&q=80',
      viewsCount: 63,
      favoritesCount: 5,
      createdAt: DateTime.now().subtract(const Duration(days: 5)),
    ),
    MyListingItem(
      id: 'ml-4',
      title: 'دراجة هوائية رياضية Trinx',
      price: 350,
      status: 'expired',
      imageUrl:
          'https://images.unsplash.com/photo-1507035895480-2b3156c31fc8?w=200&q=80',
      viewsCount: 31,
      favoritesCount: 2,
      createdAt: DateTime.now().subtract(const Duration(days: 32)),
    ),
  ];
}
