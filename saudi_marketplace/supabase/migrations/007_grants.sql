-- ============================================================
-- BIKYA MARKETPLACE — MIGRATION 007: ROLE GRANTS
-- Fixes 42501 ("permission denied") on read/insert.
--
-- RLS (migration 004) controls WHICH ROWS a role may touch,
-- but Postgres also needs table-level GRANTs to allow the
-- operation TYPE at all. Some projects are missing the default
-- Supabase grants, so we set them explicitly here.
--
-- UPDATE is intentionally NOT granted broadly — the
-- column-level UPDATE grants from migration 004 are the
-- security boundary and are re-applied at the end.
--
-- Run AFTER 006_storage.sql. Safe to re-run (idempotent).
-- ============================================================

-- ── Schema usage ─────────────────────────────────────────────
GRANT USAGE ON SCHEMA public TO anon, authenticated;

-- ── Read access (RLS still restricts which rows are visible) ──
GRANT SELECT ON ALL TABLES IN SCHEMA public TO anon, authenticated;

-- ── Write access for signed-in users ─────────────────────────
-- (RLS policies decide which rows; tables without an INSERT/DELETE
--  policy stay locked because RLS denies by default.)
GRANT INSERT, DELETE ON ALL TABLES IN SCHEMA public TO authenticated;

-- ── Sequences (safety for any non-UUID defaults) ─────────────
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO authenticated;

-- ── Future tables inherit the same baseline ──────────────────
ALTER DEFAULT PRIVILEGES IN SCHEMA public
  GRANT SELECT ON TABLES TO anon, authenticated;
ALTER DEFAULT PRIVILEGES IN SCHEMA public
  GRANT INSERT, DELETE ON TABLES TO authenticated;

-- ============================================================
-- COLUMN-LEVEL UPDATE GUARDS (re-applied — see migration 004)
-- These are the real boundary: clients may update ONLY these
-- columns. Counters/verification/ratings move via triggers/RPC.
-- ============================================================
REVOKE UPDATE ON profiles      FROM anon, authenticated;
REVOKE UPDATE ON listings      FROM anon, authenticated;
REVOKE UPDATE ON conversations FROM anon, authenticated;
REVOKE UPDATE ON messages      FROM anon, authenticated;
REVOKE UPDATE ON notifications FROM anon, authenticated;

GRANT UPDATE (full_name, phone, avatar_url, bio, city_id, last_seen_at)
  ON profiles TO authenticated;

GRANT UPDATE (title, description, price, is_price_negotiable,
              is_price_hidden, condition, status, neighborhood,
              location_point, safety_note, category_id, city_id,
              sold_to_id, expires_at, deleted_at)
  ON listings TO authenticated;

GRANT UPDATE (is_archived_by_buyer, is_archived_by_seller)
  ON conversations TO authenticated;

GRANT UPDATE (is_read, read_at) ON messages      TO authenticated;
GRANT UPDATE (is_read, read_at) ON notifications TO authenticated;
