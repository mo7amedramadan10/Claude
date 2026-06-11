import 'package:supabase_flutter/supabase_flutter.dart';

/// أسماء الـ buckets — مطابقة لملف 006_storage.sql
abstract final class StorageBuckets {
  static const listingImages   = 'listing-images';
  static const avatars         = 'avatars';
  static const chatAttachments = 'chat-attachments';
  static const banners         = 'banners';
}

abstract final class StorageUrls {
  /// رابط عام لصورة في bucket عام (listing-images / avatars / banners)
  static String publicUrl(String bucket, String path) {
    return Supabase.instance.client.storage.from(bucket).getPublicUrl(path);
  }

  static String listingImage(String path) =>
      publicUrl(StorageBuckets.listingImages, path);

  static String avatar(String path) =>
      publicUrl(StorageBuckets.avatars, path);

  static String banner(String path) =>
      publicUrl(StorageBuckets.banners, path);

  /// رابط مؤقّت موقّع لمرفقات المحادثات (bucket خاص)
  static Future<String> chatAttachment(
    String path, {
    int expiresInSeconds = 3600,
  }) {
    return Supabase.instance.client.storage
        .from(StorageBuckets.chatAttachments)
        .createSignedUrl(path, expiresInSeconds);
  }
}
