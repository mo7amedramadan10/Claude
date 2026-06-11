# Bikya — Supabase Database Setup

## ترتيب التنفيذ (إلزامي)

نفّذ الملفات بالترتيب في **SQL Editor** داخل لوحة Supabase:

| # | الملف | المحتوى |
|---|---|---|
| 1 | `migrations/001_initial_schema.sql` | 17 جدولاً + triggers + دوال RPC |
| 2 | `migrations/002_seed_cities.sql` | 20 مدينة سعودية |
| 3 | `migrations/003_seed_categories.sql` | 12 تصنيفاً |
| 4 | `migrations/004_rls.sql` | سياسات الأمان + public_profiles view |
| 5 | `migrations/005_indexes.sql` | 26 فهرس أداء |
| 6 | `migrations/006_storage.sql` | 4 buckets + سياسات الرفع |

## خطوات الإعداد الكاملة

1. **إنشاء المشروع:** [supabase.com](https://supabase.com) → New Project
   - Region: `eu-central-1` (Frankfurt) أو `ap-south-1` (Mumbai) — الأقرب للسعودية
   - احفظ كلمة مرور قاعدة البيانات في مكان آمن
2. **تنفيذ الـ migrations:** SQL Editor → New query → الصق محتوى كل ملف بالترتيب → Run
3. **تفعيل pg_cron:** Database → Extensions → فعّل `pg_cron`، ثم نفّذ سطر الجدولة الموجود (معلّقاً) آخر ملف 001
4. **إعداد المصادقة:** Authentication → Providers → فعّل Phone (OTP) و Email
5. **المفاتيح:** Settings → API → انسخ `Project URL` و `anon public key` — سنحتاجهما في Flutter لاحقاً

## اصطلاحات مسارات التخزين (لا تخالفها)

```
listing-images:   {user_id}/{listing_id}/{uuid}.jpg
avatars:          {user_id}/avatar.jpg
chat-attachments: {user_id}/{conversation_id}/{uuid}.ext
banners:          (رفع عبر service role فقط)
```

المجلد الأول **يجب** أن يكون `auth.uid()` للمستخدم الرافع — سياسات الـ Storage ترفض أي مسار آخر.

## نقاط مهمة للتطوير

- بيانات البائعين تُقرأ من view `public_profiles` — **ليس** من `profiles` (يُخفي رقم الجوال)
- تعليم الرسائل كمقروءة عبر RPC: `mark_conversation_read(conversation_id)`
- زيادة المشاهدات عبر RPC: `increment_listing_views(listing_id)`
- حذف الإعلان = `UPDATE listings SET deleted_at = NOW()` — لا DELETE
- الإشعارات تُدرج عبر service role / Edge Functions فقط
