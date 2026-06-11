import '../models/conversation_model.dart';

abstract final class ChatListMockData {
  static const conversations = [
    ConversationModel(
      id: 'chat_1',
      sellerName: 'محمد العتيبي',
      sellerAvatarUrl:
          'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=160&q=80',
      listingTitle: 'BMW 530i 2022',
      listingImageUrl:
          'https://images.unsplash.com/photo-1555215695-3004980ad54e?w=160&q=80',
      listingPrice: '٨٢,٥٠٠',
      lastMessage: 'السيارة موجودة، تفضل المعاينة بعد الظهر',
      lastMessageTime: 'الآن',
      isOnline: true,
      unreadCount: 2,
      isLastMine: false,
      isRead: false,
    ),
    ConversationModel(
      id: 'chat_2',
      sellerName: 'خالد الدوسري',
      sellerAvatarUrl:
          'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=160&q=80',
      listingTitle: 'iPhone 15 Pro Max',
      listingImageUrl:
          'https://images.unsplash.com/photo-1592899677977-9c10ca588bbd?w=160&q=80',
      listingPrice: '٤,٢٠٠',
      lastMessage: 'أنا: هل فيه أي خدوش على الشاشة؟',
      lastMessageTime: 'منذ ٣ د',
      isOnline: true,
      unreadCount: 0,
      isLastMine: true,
      isRead: true,
    ),
    ConversationModel(
      id: 'chat_3',
      sellerName: 'سلطان الزهراني',
      sellerAvatarUrl:
          'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=160&q=80',
      listingTitle: 'MacBook Pro M3',
      listingImageUrl:
          'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=160&q=80',
      listingPrice: '٦,٥٠٠',
      lastMessage: 'شكراً لاهتمامك، السعر نهائي',
      lastMessageTime: 'أمس',
      isOnline: false,
      unreadCount: 0,
      isLastMine: false,
      isRead: true,
    ),
    ConversationModel(
      id: 'chat_4',
      sellerName: 'عبدالعزيز السلمي',
      sellerAvatarUrl:
          'https://images.unsplash.com/photo-1519345182560-3f2917c472ef?w=160&q=80',
      listingTitle: 'PlayStation 5 جديد',
      listingImageUrl:
          'https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=160&q=80',
      listingPrice: '١,٩٠٠',
      lastMessage: 'أنا: تمام، سأكون عندك الساعة ٦ مساءً',
      lastMessageTime: 'الاثنين',
      isOnline: false,
      unreadCount: 0,
      isLastMine: true,
      isRead: false,
    ),
    ConversationModel(
      id: 'chat_5',
      sellerName: 'فيصل القحطاني',
      sellerAvatarUrl:
          'https://images.unsplash.com/photo-1463453091185-61582044d556?w=160&q=80',
      listingTitle: 'أريكة L شكل فاخرة',
      listingImageUrl:
          'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=160&q=80',
      listingPrice: '٣,٨٠٠',
      lastMessage: 'يمكن توصيلها مجاناً داخل الرياض',
      lastMessageTime: 'الأحد',
      isOnline: false,
      unreadCount: 0,
      isLastMine: false,
      isRead: true,
    ),
  ];
}
