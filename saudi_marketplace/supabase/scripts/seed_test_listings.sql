-- ============================================================
-- بيكيا — سيد اعلانات تجريبية
-- المستخدم: 7b5ff23a-8f1a-42a6-b65a-7bdff84b34c5
--
-- ما يفعله هذا السكربت:
--  1. يُصلح الاعلانات الموجودة لهذا اليوزر (يملأ أي حقل فارغ)
--  2. يُضيف 6 اعلانات جديدة بحالات مختلفة (active × 3 + sold + expired + draft)
--  3. يُضيف صور لكل اعلان في listing_images
--  4. يُضيف مواصفات في listing_specs لبعض الاعلانات
--
-- شغّله من: Supabase Dashboard → SQL Editor
-- ============================================================

DO $$
DECLARE
  v_user_id   UUID := '7b5ff23a-8f1a-42a6-b65a-7bdff84b34c5';

  -- مدن
  v_riyadh    UUID;
  v_jeddah    UUID;
  v_dammam    UUID;

  -- فئات
  v_cars      UUID;
  v_phones    UUID;
  v_electronics UUID;
  v_furniture UUID;
  v_sports    UUID;
  v_fashion   UUID;

  -- معرفات الاعلانات
  v_l1 UUID := gen_random_uuid();   -- سيارة  — active
  v_l2 UUID := gen_random_uuid();   -- جوال   — active
  v_l3 UUID := gen_random_uuid();   -- لابتوب — active
  v_l4 UUID := gen_random_uuid();   -- أثاث   — sold
  v_l5 UUID := gen_random_uuid();   -- رياضة  — expired
  v_l6 UUID := gen_random_uuid();   -- أزياء  — draft

BEGIN

  -- ── 0. التأكد من وجود الـ profile ─────────────────────────
  IF NOT EXISTS (SELECT 1 FROM profiles WHERE id = v_user_id) THEN
    RAISE EXCEPTION 'Profile % غير موجود — تأكد من تسجيل الدخول بهذا المستخدم أولاً', v_user_id;
  END IF;

  -- ── 1. جلب IDs المدن والفئات ──────────────────────────────
  SELECT id INTO v_riyadh     FROM cities WHERE name_ar = 'الرياض'    LIMIT 1;
  SELECT id INTO v_jeddah     FROM cities WHERE name_ar = 'جدة'       LIMIT 1;
  SELECT id INTO v_dammam     FROM cities WHERE name_ar = 'الدمام'    LIMIT 1;

  SELECT id INTO v_cars       FROM categories WHERE slug = 'cars'       LIMIT 1;
  SELECT id INTO v_phones     FROM categories WHERE slug = 'phones'     LIMIT 1;
  SELECT id INTO v_electronics FROM categories WHERE slug = 'electronics' LIMIT 1;
  SELECT id INTO v_furniture  FROM categories WHERE slug = 'furniture'  LIMIT 1;
  SELECT id INTO v_sports     FROM categories WHERE slug = 'sports'     LIMIT 1;
  SELECT id INTO v_fashion    FROM categories WHERE slug = 'fashion'    LIMIT 1;

  IF v_riyadh IS NULL OR v_cars IS NULL THEN
    RAISE EXCEPTION 'لم يُعثر على المدن أو الفئات — شغّل migrations 002 و 003 أولاً';
  END IF;

  -- ── 2. إصلاح الاعلانات الموجودة (يملأ أي حقل required فارغ) ──
  UPDATE listings
  SET
    description = COALESCE(NULLIF(TRIM(description), ''), 'وصف الإعلان — تم الإضافة تلقائياً بواسطة سكربت الاختبار'),
    expires_at  = COALESCE(expires_at, NOW() + INTERVAL '60 days'),
    updated_at  = COALESCE(updated_at, NOW())
  WHERE seller_id = v_user_id;

  RAISE NOTICE 'تم إصلاح الاعلانات الموجودة للمستخدم %', v_user_id;

  -- ── 3. إضافة الاعلانات الجديدة ───────────────────────────

  -- ▸ إعلان 1: سيارة Toyota  — active
  INSERT INTO listings (
    id, seller_id, category_id, city_id,
    title, description, price, currency,
    is_price_negotiable, is_price_hidden,
    condition, status, neighborhood,
    is_verified, is_featured,
    views_count, favorites_count, contact_count,
    expires_at, created_at, updated_at
  ) VALUES (
    v_l1, v_user_id, v_cars, v_riyadh,
    'تويوتا كامري 2021 — نظيفة جداً',
    'سيارة كامري موديل 2021 بحالة ممتازة، لون أبيض لؤلؤي، قطعت 45,000 كم فقط. مواصفات كاملة، السيارة بدون حوادث، جاهزة للنقل الفوري. السبب: سفر دراسي.',
    65000, 'SAR',
    true, false,
    'excellent', 'active', 'حي النخيل',
    false, true,
    128, 14, 6,
    NOW() + INTERVAL '30 days',
    NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days'
  ) ON CONFLICT (id) DO NOTHING;

  -- ▸ إعلان 2: جوال Samsung  — active
  INSERT INTO listings (
    id, seller_id, category_id, city_id,
    title, description, price, currency,
    is_price_negotiable, is_price_hidden,
    condition, status, neighborhood,
    is_verified, is_featured,
    views_count, favorites_count, contact_count,
    expires_at, created_at, updated_at
  ) VALUES (
    v_l2, v_user_id, v_phones, v_jeddah,
    'Samsung Galaxy S24 Ultra — مع الكرتونة',
    'جوال سامسونج S24 Ultra 256GB، لون Titanium Gray، مستخدم أسبوعين فقط. يأتي مع الكرتونة الأصلية، الشاحن، وغطاء أصلي. السعر نهائي.',
    4200, 'SAR',
    false, false,
    'very_good', 'active', 'حي الروضة',
    false, false,
    73, 8, 3,
    NOW() + INTERVAL '45 days',
    NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days'
  ) ON CONFLICT (id) DO NOTHING;

  -- ▸ إعلان 3: لابتوب MacBook  — active
  INSERT INTO listings (
    id, seller_id, category_id, city_id,
    title, description, price, currency,
    is_price_negotiable, is_price_hidden,
    condition, status, neighborhood,
    is_verified, is_featured,
    views_count, favorites_count, contact_count,
    expires_at, created_at, updated_at
  ) VALUES (
    v_l3, v_user_id, v_electronics, v_riyadh,
    'MacBook Pro M3 — 16 بوصة — رام 36GB',
    'ماك بوك برو آخر اصدار M3 Pro، 36GB RAM، 512GB SSD. اشتريته من إكس سايتي قبل 3 أشهر، لا يزال في فترة الضمان. ملحقاته: شاحن MagSafe + غطاء حماية.',
    9800, 'SAR',
    true, false,
    'excellent', 'active', 'حي العليا',
    false, false,
    210, 21, 9,
    NOW() + INTERVAL '60 days',
    NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day'
  ) ON CONFLICT (id) DO NOTHING;

  -- ▸ إعلان 4: أثاث  — sold
  INSERT INTO listings (
    id, seller_id, category_id, city_id,
    title, description, price, currency,
    is_price_negotiable, is_price_hidden,
    condition, status, neighborhood,
    is_verified, is_featured,
    views_count, favorites_count, contact_count,
    sold_at,
    expires_at, created_at, updated_at
  ) VALUES (
    v_l4, v_user_id, v_furniture, v_dammam,
    'طقم كنب إيطالي 7 مقاعد — تم البيع',
    'طقم كنب فاخر من ماركة Natuzzi الإيطالية، لون كريمي، 7 مقاعد (3+2+2). حالة جيدة جداً، مستخدم سنتين. تم البيع.',
    8500, 'SAR',
    false, false,
    'good', 'sold', NULL,
    false, false,
    445, 32, 15,
    NOW() - INTERVAL '3 days',
    NOW() + INTERVAL '27 days',
    NOW() - INTERVAL '10 days', NOW() - INTERVAL '3 days'
  ) ON CONFLICT (id) DO NOTHING;

  -- ▸ إعلان 5: رياضة  — expired
  INSERT INTO listings (
    id, seller_id, category_id, city_id,
    title, description, price, currency,
    is_price_negotiable, is_price_hidden,
    condition, status, neighborhood,
    is_verified, is_featured,
    views_count, favorites_count, contact_count,
    expires_at, created_at, updated_at
  ) VALUES (
    v_l5, v_user_id, v_sports, v_riyadh,
    'دراجة هوائية Trek — منتهية الصلاحية',
    'دراجة Trek FX3 Disc بحالة جيدة، 21 سرعة. مناسبة للرياضة اليومية والتنزه. تحتاج ضبطاً بسيطاً.',
    1200, 'SAR',
    true, false,
    'good', 'expired', NULL,
    false, false,
    88, 5, 2,
    NOW() - INTERVAL '5 days',
    NOW() - INTERVAL '65 days', NOW() - INTERVAL '5 days'
  ) ON CONFLICT (id) DO NOTHING;

  -- ▸ إعلان 6: أزياء  — draft
  INSERT INTO listings (
    id, seller_id, category_id, city_id,
    title, description, price, currency,
    is_price_negotiable, is_price_hidden,
    condition, status, neighborhood,
    is_verified, is_featured,
    views_count, favorites_count, contact_count,
    expires_at, created_at, updated_at
  ) VALUES (
    v_l6, v_user_id, v_fashion, v_jeddah,
    'ساعة Rolex Submariner — مسودة',
    'ساعة رولكس سبمارينر أصلية موديل 2022، مع جميع الأوراق والكرتونة. السعر قابل للنقاش للجادين فقط.',
    85000, 'SAR',
    true, false,
    'excellent', 'draft', 'حي الزهراء',
    false, false,
    0, 0, 0,
    NOW() + INTERVAL '60 days',
    NOW(), NOW()
  ) ON CONFLICT (id) DO NOTHING;

  RAISE NOTICE 'تم إضافة 6 اعلانات جديدة';

  -- ── 4. صور الاعلانات (listing_images) ───────────────────
  -- نستخدم مسارات تجريبية بنفس هيكل storage: {user_id}/{listing_id}/{uuid}.jpg

  -- صور اعلان 1 (سيارة) — 3 صور
  INSERT INTO listing_images (listing_id, storage_path, is_primary, sort_order)
  VALUES
    (v_l1, v_user_id || '/' || v_l1 || '/photo_01.jpg', true,  0),
    (v_l1, v_user_id || '/' || v_l1 || '/photo_02.jpg', false, 1),
    (v_l1, v_user_id || '/' || v_l1 || '/photo_03.jpg', false, 2)
  ON CONFLICT DO NOTHING;

  -- صور اعلان 2 (جوال) — 2 صور
  INSERT INTO listing_images (listing_id, storage_path, is_primary, sort_order)
  VALUES
    (v_l2, v_user_id || '/' || v_l2 || '/photo_01.jpg', true,  0),
    (v_l2, v_user_id || '/' || v_l2 || '/photo_02.jpg', false, 1)
  ON CONFLICT DO NOTHING;

  -- صور اعلان 3 (لابتوب) — 3 صور
  INSERT INTO listing_images (listing_id, storage_path, is_primary, sort_order)
  VALUES
    (v_l3, v_user_id || '/' || v_l3 || '/photo_01.jpg', true,  0),
    (v_l3, v_user_id || '/' || v_l3 || '/photo_02.jpg', false, 1),
    (v_l3, v_user_id || '/' || v_l3 || '/photo_03.jpg', false, 2)
  ON CONFLICT DO NOTHING;

  -- صور اعلان 4 (أثاث) — 2 صور
  INSERT INTO listing_images (listing_id, storage_path, is_primary, sort_order)
  VALUES
    (v_l4, v_user_id || '/' || v_l4 || '/photo_01.jpg', true,  0),
    (v_l4, v_user_id || '/' || v_l4 || '/photo_02.jpg', false, 1)
  ON CONFLICT DO NOTHING;

  -- صور اعلان 5 (رياضة) — 1 صورة
  INSERT INTO listing_images (listing_id, storage_path, is_primary, sort_order)
  VALUES
    (v_l5, v_user_id || '/' || v_l5 || '/photo_01.jpg', true, 0)
  ON CONFLICT DO NOTHING;

  RAISE NOTICE 'تم إضافة صور الاعلانات';

  -- ── 5. مواصفات الاعلانات (listing_specs) ────────────────

  -- مواصفات السيارة (إعلان 1)
  INSERT INTO listing_specs (listing_id, spec_key, spec_value, sort_order)
  VALUES
    (v_l1, 'الماركة',     'تويوتا',    0),
    (v_l1, 'الموديل',     'كامري',     1),
    (v_l1, 'السنة',       '2021',      2),
    (v_l1, 'المسافة',     '45,000 كم', 3),
    (v_l1, 'اللون',       'أبيض',      4),
    (v_l1, 'ناقل الحركة', 'أوتوماتيك', 5),
    (v_l1, 'نوع الوقود',  'بنزين',     6)
  ON CONFLICT DO NOTHING;

  -- مواصفات الجوال (إعلان 2)
  INSERT INTO listing_specs (listing_id, spec_key, spec_value, sort_order)
  VALUES
    (v_l2, 'الماركة',   'Samsung',        0),
    (v_l2, 'الموديل',   'Galaxy S24 Ultra', 1),
    (v_l2, 'التخزين',   '256 GB',         2),
    (v_l2, 'الرام',     '12 GB',          3),
    (v_l2, 'اللون',     'Titanium Gray',  4)
  ON CONFLICT DO NOTHING;

  -- مواصفات اللابتوب (إعلان 3)
  INSERT INTO listing_specs (listing_id, spec_key, spec_value, sort_order)
  VALUES
    (v_l3, 'الماركة',   'Apple',       0),
    (v_l3, 'الموديل',   'MacBook Pro', 1),
    (v_l3, 'المعالج',   'M3 Pro',      2),
    (v_l3, 'الرام',     '36 GB',       3),
    (v_l3, 'التخزين',   '512 GB SSD',  4),
    (v_l3, 'الشاشة',    '16 بوصة',    5)
  ON CONFLICT DO NOTHING;

  RAISE NOTICE 'تم إضافة مواصفات الاعلانات';

  -- ── 6. تحقق نهائي ───────────────────────────────────────
  RAISE NOTICE '────────────────────────────────';
  RAISE NOTICE 'ملخص الاعلانات للمستخدم %:', v_user_id;
  RAISE NOTICE '  active  : %', (SELECT COUNT(*) FROM listings WHERE seller_id = v_user_id AND status = 'active'  AND deleted_at IS NULL);
  RAISE NOTICE '  sold    : %', (SELECT COUNT(*) FROM listings WHERE seller_id = v_user_id AND status = 'sold'    AND deleted_at IS NULL);
  RAISE NOTICE '  expired : %', (SELECT COUNT(*) FROM listings WHERE seller_id = v_user_id AND status = 'expired' AND deleted_at IS NULL);
  RAISE NOTICE '  draft   : %', (SELECT COUNT(*) FROM listings WHERE seller_id = v_user_id AND status = 'draft'   AND deleted_at IS NULL);
  RAISE NOTICE '  المجموع : %', (SELECT COUNT(*) FROM listings WHERE seller_id = v_user_id AND deleted_at IS NULL);
  RAISE NOTICE '────────────────────────────────';

END $$;

-- ── تحقق بسيط بعد التنفيذ ────────────────────────────────────
SELECT
  l.id,
  l.title,
  l.status,
  l.price,
  l.condition,
  c.name_ar  AS city,
  cat.name_ar AS category,
  (SELECT COUNT(*) FROM listing_images li WHERE li.listing_id = l.id) AS images_count,
  (SELECT COUNT(*) FROM listing_specs  ls WHERE ls.listing_id = l.id) AS specs_count,
  l.created_at
FROM listings l
JOIN cities     c   ON c.id   = l.city_id
JOIN categories cat ON cat.id = l.category_id
WHERE l.seller_id = '7b5ff23a-8f1a-42a6-b65a-7bdff84b34c5'
  AND l.deleted_at IS NULL
ORDER BY l.created_at DESC;
