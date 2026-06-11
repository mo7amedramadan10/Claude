import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../home/models/seller_model.dart';
import '../models/listing_detail_model.dart';

abstract final class ListingDetailMockData {
  static const detail = ListingDetailModel(
    id: '1',
    title: 'BMW 530i موديل 2022',
    price: 82500,
    location: 'الرياض، حي الملقا',
    district: 'حي الملقا، الرياض',
    postedAt: 'منذ ساعتين',
    description:
        'سيارة فل كامل، ماشية ٤٥ ألف كم فقط، صيانة منتظمة لدى الوكالة، '
        'الصبغة وكالة وما فيها أي حوادث. المكينة والقير ممتازة، الإطارات '
        'جديدة. السيارة بحالة الوكالة وقابلة للفحص لدى أي مركز معتمد.',
    gallery: [
      'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=700&q=80',
      'https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=700&q=80',
      'https://images.unsplash.com/photo-1494976388531-d1058494cdd8?w=700&q=80',
      'https://images.unsplash.com/photo-1606664515524-ed2f786a0bd6?w=700&q=80',
    ],
    specs: [
      SpecItem(icon: TablerIcons.calendar_event, value: '2022', label: 'الموديل'),
      SpecItem(icon: TablerIcons.gauge, value: '٤٥ ألف', label: 'الممشى (كم)'),
      SpecItem(icon: TablerIcons.settings_2, value: 'أوتوماتيك', label: 'ناقل الحركة'),
      SpecItem(icon: TablerIcons.gas_station, value: 'بنزين', label: 'الوقود'),
    ],
    seller: SellerModel(
      id: 's1',
      name: 'محمد العتيبي',
      rating: 5.0,
      completedDeals: 243,
      avatarUrl:
          'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=120&q=80',
    ),
    isVerified: true,
    verifiedNote: 'إعلان موثق · فحص فني مرفق',
    safetyNotice:
        'للسلامة: عاين السيارة قبل الدفع، وتمّ الصفقة وجهاً لوجه. '
        'لا تحوّل أي مبلغ مقدّماً قبل المعاينة.',
  );
}
