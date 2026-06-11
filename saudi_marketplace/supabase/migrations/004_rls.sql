-- ============================================================
-- BIKYA MARKETPLACE — MIGRATION 004: ROW LEVEL SECURITY
-- Fixes: C1 (phone leak) · C3 (open tables) · C4 (read receipts)
--        I5 (counter tampering) · I6 (block checks) · I2 (review gate)
-- Run AFTER 003_seed_categories.sql
-- ============================================================

-- ── Enable RLS on ALL tables (C3) ────────────────────────────
ALTER TABLE cities         ENABLE ROW LEVEL SECURITY;
ALTER TABLE categories     ENABLE ROW LEVEL SECURITY;
ALTER TABLE profiles       ENABLE ROW LEVEL SECURITY;
ALTER TABLE app_settings   ENABLE ROW LEVEL SECURITY;
ALTER TABLE banners        ENABLE ROW LEVEL SECURITY;
ALTER TABLE listings       ENABLE ROW LEVEL SECURITY;
ALTER TABLE listing_images ENABLE ROW LEVEL SECURITY;
ALTER TABLE listing_specs  ENABLE ROW LEVEL SECURITY;
ALTER TABLE favorites      ENABLE ROW LEVEL SECURITY;
ALTER TABLE conversations  ENABLE ROW LEVEL SECURITY;
ALTER TABLE messages       ENABLE ROW LEVEL SECURITY;
ALTER TABLE notifications  ENABLE ROW LEVEL SECURITY;
ALTER TABLE reviews        ENABLE ROW LEVEL SECURITY;
ALTER TABLE reports        ENABLE ROW LEVEL SECURITY;
ALTER TABLE saved_searches ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_devices   ENABLE ROW LEVEL SECURITY;
ALTER TABLE blocked_users  ENABLE ROW LEVEL SECURITY;

-- ── Counter trigger functions must bypass RLS (I5) ───────────
ALTER FUNCTION update_favorites_count() SECURITY DEFINER;
ALTER FUNCTION sync_listing_counters()  SECURITY DEFINER;
ALTER FUNCTION handle_listing_sold()    SECURITY DEFINER;

-- ============================================================
-- C1 — PUBLIC PROFILES VIEW (no phone, no soft-deleted)
-- The app reads OTHER users' data ONLY through this view.
-- Full `profiles` row (incl. phone) is readable by owner only.
-- ============================================================
CREATE VIEW public_profiles AS
SELECT
  id, full_name, avatar_url, bio, city_id,
  is_nafath_verified, rating_avg, total_reviews,
  completed_deals, active_listings_count,
  last_seen_at, created_at
FROM profiles
WHERE is_active = true AND deleted_at IS NULL;

-- ── PROFILES ─────────────────────────────────────────────────
CREATE POLICY "profiles: owner reads full row"
  ON profiles FOR SELECT
  USING (auth.uid() = id);

CREATE POLICY "profiles: owner can update"
  ON profiles FOR UPDATE
  USING (auth.uid() = id);

-- Column-level guard: owner cannot self-grant verification,
-- ratings, or counters — those move only via triggers/admin.
REVOKE UPDATE ON profiles FROM anon, authenticated;
GRANT UPDATE (full_name, phone, avatar_url, bio, city_id, last_seen_at)
  ON profiles TO authenticated;

-- ── CITIES (read-only reference data) ────────────────────────
CREATE POLICY "cities: public read active"
  ON cities FOR SELECT
  USING (is_active = true);

-- ── CATEGORIES (read-only reference data) ────────────────────
CREATE POLICY "categories: public read active"
  ON categories FOR SELECT
  USING (is_active = true);

-- ── APP SETTINGS (public keys only) ──────────────────────────
CREATE POLICY "app_settings: public read public keys"
  ON app_settings FOR SELECT
  USING (is_public = true);

-- ── BANNERS (active + within schedule) ───────────────────────
CREATE POLICY "banners: public read active scheduled"
  ON banners FOR SELECT
  USING (
    is_active = true
    AND (starts_at IS NULL OR starts_at <= NOW())
    AND (ends_at   IS NULL OR ends_at   >= NOW())
  );

-- ============================================================
-- LISTINGS (soft-delete aware)
-- ============================================================
CREATE POLICY "listings: read active or own"
  ON listings FOR SELECT
  USING (
    (status = 'active' AND deleted_at IS NULL)
    OR seller_id = auth.uid()
  );

CREATE POLICY "listings: authenticated can create"
  ON listings FOR INSERT
  WITH CHECK (auth.uid() = seller_id);

CREATE POLICY "listings: seller can update own"
  ON listings FOR UPDATE
  USING (auth.uid() = seller_id AND deleted_at IS NULL);

-- No DELETE policy: deletion is soft (UPDATE deleted_at).
-- Column guard: seller cannot self-verify/feature or fake counters.
REVOKE UPDATE ON listings FROM anon, authenticated;
GRANT UPDATE (title, description, price, is_price_negotiable,
              is_price_hidden, condition, status, neighborhood,
              location_point, safety_note, category_id, city_id,
              sold_to_id, expires_at, deleted_at)
  ON listings TO authenticated;

-- RPC: views counter (clients have no UPDATE grant on views_count)
CREATE OR REPLACE FUNCTION increment_listing_views(p_listing_id UUID)
RETURNS VOID AS $$
BEGIN
  UPDATE listings SET views_count = views_count + 1
  WHERE id = p_listing_id AND status = 'active' AND deleted_at IS NULL;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- ── LISTING IMAGES ───────────────────────────────────────────
CREATE POLICY "listing_images: read with visible listing"
  ON listing_images FOR SELECT
  USING (
    EXISTS (SELECT 1 FROM listings l WHERE l.id = listing_id
            AND ((l.status = 'active' AND l.deleted_at IS NULL)
                 OR l.seller_id = auth.uid()))
  );

CREATE POLICY "listing_images: seller can insert"
  ON listing_images FOR INSERT
  WITH CHECK (
    auth.uid() = (SELECT seller_id FROM listings WHERE id = listing_id)
  );

CREATE POLICY "listing_images: seller can delete"
  ON listing_images FOR DELETE
  USING (
    auth.uid() = (SELECT seller_id FROM listings WHERE id = listing_id)
  );

-- ── LISTING SPECS ────────────────────────────────────────────
CREATE POLICY "listing_specs: read with visible listing"
  ON listing_specs FOR SELECT
  USING (
    EXISTS (SELECT 1 FROM listings l WHERE l.id = listing_id
            AND ((l.status = 'active' AND l.deleted_at IS NULL)
                 OR l.seller_id = auth.uid()))
  );

CREATE POLICY "listing_specs: seller can manage"
  ON listing_specs FOR ALL
  USING (
    auth.uid() = (SELECT seller_id FROM listings WHERE id = listing_id)
  );

-- ── FAVORITES ────────────────────────────────────────────────
CREATE POLICY "favorites: owner can read"
  ON favorites FOR SELECT
  USING (auth.uid() = user_id);

CREATE POLICY "favorites: owner can insert"
  ON favorites FOR INSERT
  WITH CHECK (auth.uid() = user_id);

CREATE POLICY "favorites: owner can delete"
  ON favorites FOR DELETE
  USING (auth.uid() = user_id);

-- ============================================================
-- CONVERSATIONS (block-aware, I6)
-- ============================================================
CREATE POLICY "conversations: participants can read"
  ON conversations FOR SELECT
  USING (auth.uid() IN (buyer_id, seller_id));

CREATE POLICY "conversations: buyer creates valid thread"
  ON conversations FOR INSERT
  WITH CHECK (
    auth.uid() = buyer_id
    AND seller_id = (SELECT seller_id FROM listings WHERE id = listing_id)
    AND NOT EXISTS (
      SELECT 1 FROM blocked_users b
      WHERE (b.blocker_id = seller_id AND b.blocked_id = buyer_id)
         OR (b.blocker_id = buyer_id  AND b.blocked_id = seller_id)
    )
  );

CREATE POLICY "conversations: participants can update"
  ON conversations FOR UPDATE
  USING (auth.uid() IN (buyer_id, seller_id));

-- Column guard (I5): direct client updates limited to archive flags.
-- Unread counters move only via SECURITY DEFINER trigger + RPC.
REVOKE UPDATE ON conversations FROM anon, authenticated;
GRANT UPDATE (is_archived_by_buyer, is_archived_by_seller)
  ON conversations TO authenticated;

-- ============================================================
-- MESSAGES (block-aware send · C4 read receipts)
-- ============================================================
CREATE POLICY "messages: participants can read"
  ON messages FOR SELECT
  USING (
    EXISTS (SELECT 1 FROM conversations c
            WHERE c.id = conversation_id
              AND auth.uid() IN (c.buyer_id, c.seller_id))
  );

CREATE POLICY "messages: participant can send unless blocked"
  ON messages FOR INSERT
  WITH CHECK (
    auth.uid() = sender_id
    AND EXISTS (SELECT 1 FROM conversations c
                WHERE c.id = conversation_id
                  AND auth.uid() IN (c.buyer_id, c.seller_id)
                  AND NOT EXISTS (
                    SELECT 1 FROM blocked_users b
                    WHERE (b.blocker_id = c.buyer_id  AND b.blocked_id = c.seller_id)
                       OR (b.blocker_id = c.seller_id AND b.blocked_id = c.buyer_id)))
  );

-- C4: recipient may mark received messages read (columns guarded below)
CREATE POLICY "messages: recipient can mark read"
  ON messages FOR UPDATE
  USING (
    sender_id <> auth.uid()
    AND EXISTS (SELECT 1 FROM conversations c
                WHERE c.id = conversation_id
                  AND auth.uid() IN (c.buyer_id, c.seller_id))
  );

REVOKE UPDATE ON messages FROM anon, authenticated;
GRANT UPDATE (is_read, read_at) ON messages TO authenticated;

-- ── NOTIFICATIONS (insert = service role / triggers only) ────
CREATE POLICY "notifications: owner can read"
  ON notifications FOR SELECT
  USING (auth.uid() = user_id);

CREATE POLICY "notifications: owner can mark read"
  ON notifications FOR UPDATE
  USING (auth.uid() = user_id);

REVOKE UPDATE ON notifications FROM anon, authenticated;
GRANT UPDATE (is_read, read_at) ON notifications TO authenticated;

-- ============================================================
-- REVIEWS (I2: only the registered buyer of a sold listing)
-- ============================================================
CREATE POLICY "reviews: anyone can read"
  ON reviews FOR SELECT USING (true);

CREATE POLICY "reviews: buyer of sold listing can create"
  ON reviews FOR INSERT
  WITH CHECK (
    auth.uid() = reviewer_id
    AND EXISTS (
      SELECT 1 FROM listings l
      WHERE l.id = listing_id
        AND l.status = 'sold'
        AND l.sold_to_id = auth.uid()
        AND l.seller_id  = reviewee_id
    )
  );

-- ── REPORTS ──────────────────────────────────────────────────
CREATE POLICY "reports: reporter can submit"
  ON reports FOR INSERT
  WITH CHECK (auth.uid() = reporter_id);

CREATE POLICY "reports: reporter can read own"
  ON reports FOR SELECT
  USING (auth.uid() = reporter_id);

-- ── SAVED SEARCHES ───────────────────────────────────────────
CREATE POLICY "saved_searches: owner full access"
  ON saved_searches FOR ALL
  USING (auth.uid() = user_id);

-- ── USER DEVICES ─────────────────────────────────────────────
CREATE POLICY "user_devices: owner full access"
  ON user_devices FOR ALL
  USING (auth.uid() = user_id)
  WITH CHECK (auth.uid() = user_id);

-- ── BLOCKED USERS ────────────────────────────────────────────
CREATE POLICY "blocked_users: blocker full access"
  ON blocked_users FOR ALL
  USING (auth.uid() = blocker_id)
  WITH CHECK (auth.uid() = blocker_id);

-- ============================================================
-- REALTIME — expose chat + notifications over websockets
-- (RLS SELECT policies above still apply to realtime events)
-- ============================================================
ALTER PUBLICATION supabase_realtime ADD TABLE messages;
ALTER PUBLICATION supabase_realtime ADD TABLE conversations;
ALTER PUBLICATION supabase_realtime ADD TABLE notifications;
