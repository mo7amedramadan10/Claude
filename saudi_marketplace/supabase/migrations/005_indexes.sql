-- ============================================================
-- BIKYA MARKETPLACE — MIGRATION 005: PERFORMANCE INDEXES
-- Fix C2: PostgreSQL has no built-in 'arabic' FTS config —
--         Arabic search uses pg_trgm (trigram) indexes instead.
-- Soft-delete aware: hot-path indexes are partial on
--         (status = 'active' AND deleted_at IS NULL).
-- Run AFTER 004_rls.sql
-- ============================================================

-- ============================================================
-- LISTINGS — the hottest table
-- ============================================================

-- Browse feeds: newest active listings (home, category, search default)
CREATE INDEX idx_listings_active_created
  ON listings (created_at DESC)
  WHERE status = 'active' AND deleted_at IS NULL;

-- Category browsing
CREATE INDEX idx_listings_active_category
  ON listings (category_id, created_at DESC)
  WHERE status = 'active' AND deleted_at IS NULL;

-- City filter
CREATE INDEX idx_listings_active_city
  ON listings (city_id, created_at DESC)
  WHERE status = 'active' AND deleted_at IS NULL;

-- Price sort (lowest/highest) within active
CREATE INDEX idx_listings_active_price
  ON listings (price)
  WHERE status = 'active' AND deleted_at IS NULL AND price IS NOT NULL;

-- Featured carousel on home
CREATE INDEX idx_listings_featured
  ON listings (created_at DESC)
  WHERE is_featured = true AND status = 'active' AND deleted_at IS NULL;

-- Seller's own listings (account screen "إعلاناتي" — all statuses)
CREATE INDEX idx_listings_seller
  ON listings (seller_id, created_at DESC);

-- Nearby queries ("قريب منك") via PostGIS
CREATE INDEX idx_listings_location
  ON listings USING GIST (location_point)
  WHERE status = 'active' AND deleted_at IS NULL;

-- Expiry sweep job (pg_cron)
CREATE INDEX idx_listings_expiry
  ON listings (expires_at)
  WHERE status = 'active' AND deleted_at IS NULL;

-- ── Arabic text search (C2): trigram, not FTS ────────────────
-- Query pattern:  WHERE title ILIKE '%' || :q || '%'
--             or: WHERE title % :q   (similarity)
CREATE INDEX idx_listings_title_trgm
  ON listings USING GIN (title gin_trgm_ops);

CREATE INDEX idx_listings_description_trgm
  ON listings USING GIN (description gin_trgm_ops);

-- ============================================================
-- LISTING IMAGES
-- ============================================================
CREATE INDEX idx_listing_images_listing
  ON listing_images (listing_id, sort_order);

CREATE INDEX idx_listing_images_primary
  ON listing_images (listing_id)
  WHERE is_primary = true;

-- ============================================================
-- LISTING SPECS
-- ============================================================
CREATE INDEX idx_listing_specs_listing
  ON listing_specs (listing_id, sort_order);

-- ============================================================
-- FAVORITES
-- ============================================================
-- Saved screen: user's favorites newest-first
CREATE INDEX idx_favorites_user
  ON favorites (user_id, created_at DESC);

-- Reverse lookup + trigger maintenance
CREATE INDEX idx_favorites_listing
  ON favorites (listing_id);

-- ============================================================
-- CONVERSATIONS
-- ============================================================
-- Chat list for either role, sorted by recency
CREATE INDEX idx_conversations_buyer
  ON conversations (buyer_id, last_message_at DESC);

CREATE INDEX idx_conversations_seller
  ON conversations (seller_id, last_message_at DESC);

-- Open-or-create thread from listing detail
CREATE INDEX idx_conversations_listing
  ON conversations (listing_id);

-- ============================================================
-- MESSAGES
-- ============================================================
-- Conversation history, newest page first (cursor pagination)
CREATE INDEX idx_messages_conversation
  ON messages (conversation_id, created_at DESC);

-- mark_conversation_read scan
CREATE INDEX idx_messages_unread
  ON messages (conversation_id)
  WHERE is_read = false;

-- ============================================================
-- NOTIFICATIONS
-- ============================================================
CREATE INDEX idx_notifications_user
  ON notifications (user_id, created_at DESC);

-- Bell badge count
CREATE INDEX idx_notifications_unread
  ON notifications (user_id)
  WHERE is_read = false;

-- ============================================================
-- REVIEWS
-- ============================================================
-- Seller profile rating breakdown
CREATE INDEX idx_reviews_reviewee
  ON reviews (reviewee_id, created_at DESC);

-- ============================================================
-- REPORTS (moderation dashboard)
-- ============================================================
CREATE INDEX idx_reports_pending
  ON reports (created_at DESC)
  WHERE status = 'pending';

-- ============================================================
-- PROFILES
-- ============================================================
-- Verified sellers carousel on home
CREATE INDEX idx_profiles_verified_sellers
  ON profiles (rating_avg DESC)
  WHERE is_nafath_verified = true AND is_active = true AND deleted_at IS NULL;

-- ============================================================
-- USER DEVICES (push fan-out)
-- ============================================================
CREATE INDEX idx_user_devices_user
  ON user_devices (user_id);

-- ============================================================
-- BANNERS (home slider fetch)
-- ============================================================
CREATE INDEX idx_banners_active
  ON banners (sort_order)
  WHERE is_active = true;

-- ============================================================
-- SAVED SEARCHES
-- ============================================================
CREATE INDEX idx_saved_searches_user
  ON saved_searches (user_id, created_at DESC);
