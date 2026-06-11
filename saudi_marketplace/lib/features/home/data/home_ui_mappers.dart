import '../../../core/supabase/storage_urls.dart';
import '../../../core/utils/relative_time.dart';
import '../../auth/models/profile_model.dart';
import '../../listings/models/listing_model.dart' as backend;
import '../models/listing_model.dart';
import '../models/seller_model.dart';

/// تحويل موديلات Supabase إلى موديلات عرض الشاشة الرئيسية
extension ListingUiMapper on backend.ListingModel {
  ListingModel toUiModel({bool isFavorite = false}) {
    final image = primaryImage;
    return ListingModel(
      id: id,
      title: title,
      price: price ?? 0,
      city: city?.nameAr ?? '',
      imageUrl: image != null
          ? StorageUrls.listingImage(image.storagePath)
          : '',
      postedAt: RelativeTime.format(createdAt),
      rating: seller?.ratingAvg,
      isVerified: isVerified,
      isFavorite: isFavorite,
    );
  }
}

extension SellerUiMapper on ProfileModel {
  SellerModel toUiModel() => SellerModel(
        id: id,
        name: displayName,
        avatarUrl: avatarUrl != null
            ? StorageUrls.avatar(avatarUrl!)
            : '',
        rating: ratingAvg,
        completedDeals: completedDeals,
        isNafadhVerified: isNafathVerified,
      );
}
