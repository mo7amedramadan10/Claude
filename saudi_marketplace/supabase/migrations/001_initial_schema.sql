-- ============================================================
-- BIKYA MARKETPLACE — MIGRATION 001: INITIAL SCHEMA
-- 17 tables · triggers · soft delete · flexible pricing
-- Run order: 001 → 002 → 003 → 004 → 005 → 006
-- ============================================================

-- ── Extensions ───────────────────────────────────────────────
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "postgis";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ── Utility: auto-update updated_at ──────────────────────────
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ============================================================
-- 1. CITIES
-- ============================================================
CREATE TABLE cities (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name_ar     TEXT NOT NULL,
  name_en     TEXT NOT NULL,
  region_ar   TEXT,
  region_en   TEXT,
  latitude    NUMERIC(9,6),
  longitude   NUMERIC(9,6),
  is_active   BOOLEAN NOT NULL DEFAULT true,
  sort_order  INT     NOT NULL DEFAULT 0,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ============================================================
-- 2. CATEGORIES
-- ============================================================
CREATE TABLE categories (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name_ar       TEXT NOT NULL,
  name_en       TEXT NOT NULL,
  slug          TEXT NOT NULL UNIQUE,
  icon          TEXT NOT NULL,
  parent_id     UUID REFERENCES categories(id) ON DELETE SET NULL,
  listing_count INT  NOT NULL DEFAULT 0,
  is_active     BOOLEAN NOT NULL DEFAULT true,
  sort_order    INT     NOT NULL DEFAULT 0,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TRIGGER trg_categories_updated_at
  BEFORE UPDATE ON categories
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ============================================================
-- 3. PROFILES  (1:1 with auth.users · soft delete · KSA phone)
-- ============================================================
CREATE TABLE profiles (
  id                    UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
  full_name             TEXT NOT NULL,
  phone                 TEXT UNIQUE
                        CHECK (phone IS NULL OR phone ~ '^\+9665[0-9]{8}$'),
  avatar_url            TEXT,
  bio                   TEXT,
  city_id               UUID REFERENCES cities(id) ON DELETE SET NULL,
  is_nafath_verified    BOOLEAN NOT NULL DEFAULT false,
  nafath_verified_at    TIMESTAMPTZ,
  rating_avg            NUMERIC(3,2) NOT NULL DEFAULT 0.00,
  total_reviews         INT NOT NULL DEFAULT 0,
  completed_deals       INT NOT NULL DEFAULT 0,
  active_listings_count INT NOT NULL DEFAULT 0,
  is_active             BOOLEAN NOT NULL DEFAULT true,
  last_seen_at          TIMESTAMPTZ,
  deleted_at            TIMESTAMPTZ,                -- soft delete (I4)
  created_at            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at            TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TRIGGER trg_profiles_updated_at
  BEFORE UPDATE ON profiles
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- Auto-create profile row on new auth user
CREATE OR REPLACE FUNCTION handle_new_user()
RETURNS TRIGGER AS $$
BEGIN
  INSERT INTO public.profiles (id, full_name)
  VALUES (NEW.id, COALESCE(NEW.raw_user_meta_data->>'full_name', ''));
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

CREATE TRIGGER trg_auth_new_user
  AFTER INSERT ON auth.users
  FOR EACH ROW EXECUTE FUNCTION handle_new_user();

-- ============================================================
-- 4. APP SETTINGS  (terms, banners config, feature flags)
-- ============================================================
CREATE TABLE app_settings (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  key         TEXT NOT NULL UNIQUE,
  value       JSONB NOT NULL DEFAULT '{}',
  description TEXT,
  is_public   BOOLEAN NOT NULL DEFAULT true,
  updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TRIGGER trg_app_settings_updated_at
  BEFORE UPDATE ON app_settings
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- ============================================================
-- 5. BANNERS  (home slider / campaigns)
-- ============================================================
CREATE TABLE banners (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  title       TEXT NOT NULL,
  image_path  TEXT NOT NULL,
  target_type TEXT NOT NULL DEFAULT 'none' CHECK (target_type IN (
                'listing','category','url','screen','none')),
  target_id   TEXT,
  sort_order  INT NOT NULL DEFAULT 0,
  is_active   BOOLEAN NOT NULL DEFAULT true,
  starts_at   TIMESTAMPTZ,
  ends_at     TIMESTAMPTZ,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ============================================================
-- 6. LISTINGS  (soft delete · nullable/negotiable/hidden price)
-- ============================================================
CREATE TABLE listings (
  id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  seller_id           UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  category_id         UUID NOT NULL REFERENCES categories(id),
  city_id             UUID NOT NULL REFERENCES cities(id),
  title               TEXT NOT NULL,
  description         TEXT NOT NULL,
  price               NUMERIC(12,2)
                      CHECK (price IS NULL OR price >= 0),  -- NULL = jobs/services/donations
  currency            TEXT NOT NULL DEFAULT 'SAR',
  is_price_negotiable BOOLEAN NOT NULL DEFAULT false,
  is_price_hidden     BOOLEAN NOT NULL DEFAULT false,
  condition           TEXT NOT NULL CHECK (condition IN (
                        'excellent','very_good','good','for_parts')),
  status              TEXT NOT NULL DEFAULT 'active' CHECK (status IN (
                        'active','sold','pending','rejected','expired','draft')),
  neighborhood        TEXT,
  location_point      GEOGRAPHY(POINT, 4326),
  is_verified         BOOLEAN NOT NULL DEFAULT false,
  is_featured         BOOLEAN NOT NULL DEFAULT false,
  safety_note         TEXT,
  views_count         INT NOT NULL DEFAULT 0,
  favorites_count     INT NOT NULL DEFAULT 0,
  contact_count       INT NOT NULL DEFAULT 0,
  sold_to_id          UUID REFERENCES profiles(id) ON DELETE SET NULL,  -- deal completion (I2)
  sold_at             TIMESTAMPTZ,
  expires_at          TIMESTAMPTZ NOT NULL DEFAULT (NOW() + INTERVAL '60 days'),
  deleted_at          TIMESTAMPTZ,                  -- soft delete (amendment 4)
  created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TRIGGER trg_listings_updated_at
  BEFORE UPDATE ON listings
  FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- Deal completion: stamp sold_at + bump seller completed_deals (I2)
CREATE OR REPLACE FUNCTION handle_listing_sold()
RETURNS TRIGGER AS $$
BEGIN
  IF NEW.status = 'sold' AND OLD.status IS DISTINCT FROM 'sold' THEN
    NEW.sold_at := COALESCE(NEW.sold_at, NOW());
    UPDATE profiles SET completed_deals = completed_deals + 1
    WHERE id = NEW.seller_id;
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_listing_sold
  BEFORE UPDATE OF status ON listings
  FOR EACH ROW EXECUTE FUNCTION handle_listing_sold();

-- Cached counters: categories.listing_count + profiles.active_listings_count (I3)
CREATE OR REPLACE FUNCTION sync_listing_counters()
RETURNS TRIGGER AS $$
DECLARE
  old_active BOOLEAN := false;
  new_active BOOLEAN := false;
BEGIN
  IF TG_OP IN ('UPDATE','DELETE') THEN
    old_active := (OLD.status = 'active' AND OLD.deleted_at IS NULL);
  END IF;
  IF TG_OP IN ('INSERT','UPDATE') THEN
    new_active := (NEW.status = 'active' AND NEW.deleted_at IS NULL);
  END IF;

  IF TG_OP = 'UPDATE' AND OLD.category_id IS DISTINCT FROM NEW.category_id THEN
    IF old_active THEN
      UPDATE categories SET listing_count = GREATEST(listing_count - 1, 0)
      WHERE id = OLD.category_id;
    END IF;
    IF new_active THEN
      UPDATE categories SET listing_count = listing_count + 1
      WHERE id = NEW.category_id;
    END IF;
  ELSIF old_active <> new_active THEN
    IF new_active THEN
      UPDATE categories SET listing_count = listing_count + 1
      WHERE id = NEW.category_id;
    ELSE
      UPDATE categories SET listing_count = GREATEST(listing_count - 1, 0)
      WHERE id = COALESCE(OLD.category_id, NEW.category_id);
    END IF;
  END IF;

  IF old_active <> new_active THEN
    IF new_active THEN
      UPDATE profiles SET active_listings_count = active_listings_count + 1
      WHERE id = NEW.seller_id;
    ELSE
      UPDATE profiles SET active_listings_count = GREATEST(active_listings_count - 1, 0)
      WHERE id = COALESCE(OLD.seller_id, NEW.seller_id);
    END IF;
  END IF;

  RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_listings_counters
  AFTER INSERT OR DELETE OR UPDATE OF status, deleted_at, category_id ON listings
  FOR EACH ROW EXECUTE FUNCTION sync_listing_counters();

-- ============================================================
-- 7. LISTING IMAGES  (max 10 per listing)
-- ============================================================
CREATE TABLE listing_images (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  listing_id   UUID NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
  storage_path TEXT NOT NULL,   -- convention: {user_id}/{listing_id}/{uuid}.jpg (C5)
  sort_order   INT  NOT NULL DEFAULT 0,
  is_primary   BOOLEAN NOT NULL DEFAULT false,
  created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE OR REPLACE FUNCTION check_listing_images_limit()
RETURNS TRIGGER AS $$
BEGIN
  IF (SELECT COUNT(*) FROM listing_images
      WHERE listing_id = NEW.listing_id) >= 10 THEN
    RAISE EXCEPTION 'Maximum 10 images per listing';
  END IF;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_listing_images_limit
  BEFORE INSERT ON listing_images
  FOR EACH ROW EXECUTE FUNCTION check_listing_images_limit();

-- ============================================================
-- 8. LISTING SPECS  (V1 generic key-value — V2: category_attributes)
--    spec_key uses unified English keys: model_year, mileage,
--    transmission, fuel_type, memory, color, area_sqm, rooms ...
-- ============================================================
CREATE TABLE listing_specs (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  listing_id  UUID NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
  spec_key    TEXT NOT NULL,
  spec_value  TEXT NOT NULL,
  icon        TEXT,
  sort_order  INT NOT NULL DEFAULT 0
);

-- ============================================================
-- 9. FAVORITES
-- ============================================================
CREATE TABLE favorites (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id     UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  listing_id  UUID NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(user_id, listing_id)
);

CREATE OR REPLACE FUNCTION update_favorites_count()
RETURNS TRIGGER AS $$
BEGIN
  IF TG_OP = 'INSERT' THEN
    UPDATE listings SET favorites_count = favorites_count + 1
    WHERE id = NEW.listing_id;
  ELSIF TG_OP = 'DELETE' THEN
    UPDATE listings SET favorites_count = GREATEST(favorites_count - 1, 0)
    WHERE id = OLD.listing_id;
  END IF;
  RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_favorites_count
  AFTER INSERT OR DELETE ON favorites
  FOR EACH ROW EXECUTE FUNCTION update_favorites_count();

-- ============================================================
-- 10. CONVERSATIONS
-- ============================================================
CREATE TABLE conversations (
  id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  listing_id            UUID NOT NULL REFERENCES listings(id) ON DELETE CASCADE,
  buyer_id              UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  seller_id             UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  last_message_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  last_message_preview  TEXT,
  last_sender_id        UUID REFERENCES profiles(id),
  buyer_unread_count    INT NOT NULL DEFAULT 0,
  seller_unread_count   INT NOT NULL DEFAULT 0,
  is_archived_by_buyer  BOOLEAN NOT NULL DEFAULT false,
  is_archived_by_seller BOOLEAN NOT NULL DEFAULT false,
  created_at            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(listing_id, buyer_id),
  CHECK (buyer_id <> seller_id)
);

-- ============================================================
-- 11. MESSAGES
-- ============================================================
CREATE TABLE messages (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  conversation_id UUID NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
  sender_id       UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  content         TEXT NOT NULL,
  message_type    TEXT NOT NULL DEFAULT 'text' CHECK (message_type IN (
                    'text','image','quick_reply','system')),
  storage_path    TEXT,
  is_read         BOOLEAN NOT NULL DEFAULT false,
  read_at         TIMESTAMPTZ,
  created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Bump conversation metadata + unread counters on new message
CREATE OR REPLACE FUNCTION update_conversation_on_message()
RETURNS TRIGGER AS $$
DECLARE
  v_conv conversations%ROWTYPE;
BEGIN
  SELECT * INTO v_conv FROM conversations WHERE id = NEW.conversation_id;

  UPDATE conversations SET
    last_message_at      = NEW.created_at,
    last_message_preview = LEFT(NEW.content, 100),
    last_sender_id       = NEW.sender_id,
    buyer_unread_count   = CASE
      WHEN NEW.sender_id = v_conv.seller_id THEN buyer_unread_count + 1
      ELSE buyer_unread_count END,
    seller_unread_count  = CASE
      WHEN NEW.sender_id = v_conv.buyer_id THEN seller_unread_count + 1
      ELSE seller_unread_count END
  WHERE id = NEW.conversation_id;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

CREATE TRIGGER trg_message_update_conversation
  AFTER INSERT ON messages
  FOR EACH ROW EXECUTE FUNCTION update_conversation_on_message();

-- RPC: mark conversation read (I5 — counters only via SECURITY DEFINER)
CREATE OR REPLACE FUNCTION mark_conversation_read(p_conversation_id UUID)
RETURNS VOID AS $$
DECLARE
  v_conv conversations%ROWTYPE;
BEGIN
  SELECT * INTO v_conv FROM conversations WHERE id = p_conversation_id;

  IF auth.uid() NOT IN (v_conv.buyer_id, v_conv.seller_id) THEN
    RAISE EXCEPTION 'Not a participant of this conversation';
  END IF;

  UPDATE messages SET is_read = true, read_at = NOW()
  WHERE conversation_id = p_conversation_id
    AND sender_id <> auth.uid()
    AND is_read = false;

  IF auth.uid() = v_conv.buyer_id THEN
    UPDATE conversations SET buyer_unread_count = 0 WHERE id = p_conversation_id;
  ELSE
    UPDATE conversations SET seller_unread_count = 0 WHERE id = p_conversation_id;
  END IF;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- ============================================================
-- 12. NOTIFICATIONS
-- ============================================================
CREATE TABLE notifications (
  id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id    UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  type       TEXT NOT NULL CHECK (type IN (
               'new_message','listing_inquiry','listing_sold',
               'price_drop','new_review','listing_approved',
               'listing_rejected','system')),
  title      TEXT NOT NULL,
  body       TEXT NOT NULL,
  data       JSONB NOT NULL DEFAULT '{}',
  is_read    BOOLEAN NOT NULL DEFAULT false,
  read_at    TIMESTAMPTZ,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ============================================================
-- 13. REVIEWS  (only the registered buyer of a sold listing)
-- ============================================================
CREATE TABLE reviews (
  id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  reviewer_id     UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  reviewee_id     UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  listing_id      UUID REFERENCES listings(id) ON DELETE SET NULL,
  conversation_id UUID REFERENCES conversations(id) ON DELETE SET NULL,
  rating          SMALLINT NOT NULL CHECK (rating BETWEEN 1 AND 5),
  comment         TEXT,
  created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  UNIQUE(reviewer_id, conversation_id),
  CHECK (reviewer_id <> reviewee_id)
);

CREATE OR REPLACE FUNCTION update_profile_rating()
RETURNS TRIGGER AS $$
BEGIN
  UPDATE profiles SET
    rating_avg    = (SELECT ROUND(AVG(rating)::NUMERIC, 2)
                     FROM reviews WHERE reviewee_id = NEW.reviewee_id),
    total_reviews = (SELECT COUNT(*)
                     FROM reviews WHERE reviewee_id = NEW.reviewee_id)
  WHERE id = NEW.reviewee_id;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

CREATE TRIGGER trg_reviews_update_rating
  AFTER INSERT ON reviews
  FOR EACH ROW EXECUTE FUNCTION update_profile_rating();

-- ============================================================
-- 14. REPORTS
-- ============================================================
CREATE TABLE reports (
  id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  reporter_id      UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  listing_id       UUID REFERENCES listings(id) ON DELETE SET NULL,
  reported_user_id UUID REFERENCES profiles(id) ON DELETE SET NULL,
  reason           TEXT NOT NULL CHECK (reason IN (
                     'fraud','inappropriate','spam',
                     'wrong_category','fake_price','other')),
  description      TEXT,
  status           TEXT NOT NULL DEFAULT 'pending' CHECK (status IN (
                     'pending','reviewed','resolved','dismissed')),
  created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CHECK (listing_id IS NOT NULL OR reported_user_id IS NOT NULL)
);

-- ============================================================
-- 15. SAVED SEARCHES
-- ============================================================
CREATE TABLE saved_searches (
  id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id     UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  name        TEXT,
  query       TEXT,
  category_id UUID REFERENCES categories(id) ON DELETE SET NULL,
  city_id     UUID REFERENCES cities(id) ON DELETE SET NULL,
  min_price   NUMERIC(12,2),
  max_price   NUMERIC(12,2),
  condition   TEXT,
  created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ============================================================
-- 16. USER DEVICES  (FCM push tokens — I1)
-- ============================================================
CREATE TABLE user_devices (
  id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id        UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  fcm_token      TEXT NOT NULL UNIQUE,
  platform       TEXT NOT NULL CHECK (platform IN ('ios','android','web')),
  last_active_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  created_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ============================================================
-- 17. BLOCKED USERS  (I6)
-- ============================================================
CREATE TABLE blocked_users (
  blocker_id UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  blocked_id UUID NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  PRIMARY KEY (blocker_id, blocked_id),
  CHECK (blocker_id <> blocked_id)
);

-- ============================================================
-- SCHEDULED JOB (N1) — run AFTER enabling pg_cron:
-- Dashboard → Database → Extensions → enable "pg_cron", then:
--
-- SELECT cron.schedule(
--   'expire-listings', '0 3 * * *',
--   $$UPDATE listings SET status = 'expired'
--     WHERE status = 'active' AND expires_at < NOW()
--       AND deleted_at IS NULL$$
-- );
-- ============================================================
