-- ============================================================
-- BIKYA MARKETPLACE — MIGRATION 003: SEED CATEGORIES
-- 12 top-level categories · run AFTER 002_seed_cities.sql
--
-- NOTE: listing_count starts at 0 — it is maintained
-- automatically by trg_listings_counters as real listings
-- are created. Do NOT seed fake counts.
--
-- icon = flutter_tabler_icons name (TablerIcons.<icon>)
-- ============================================================

INSERT INTO categories (name_ar, name_en, slug, icon, sort_order) VALUES
('سيارات',        'Cars',        'cars',         'car',                1),
('عقارات',        'Real Estate', 'real-estate',  'building_community', 2),
('جوالات',        'Phones',      'phones',       'device_mobile',      3),
('إلكترونيات',    'Electronics', 'electronics',  'device_laptop',      4),
('أثاث',          'Furniture',   'furniture',    'armchair',           5),
('أزياء',         'Fashion',     'fashion',      'shirt',              6),
('ألعاب وترفيه',  'Gaming',      'gaming',       'device_gamepad_2',   7),
('وظائف',         'Jobs',        'jobs',         'briefcase',          8),
('خدمات',         'Services',    'services',     'tools',              9),
('حيوانات',       'Animals',     'animals',      'paw',                10),
('رياضة',         'Sports',      'sports',       'ball_football',      11),
('أخرى',          'Other',       'other',        'dots',               12);
