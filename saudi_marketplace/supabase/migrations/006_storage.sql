-- ============================================================
-- BIKYA MARKETPLACE — MIGRATION 006: STORAGE BUCKETS
-- Fix C5: upload path convention unified with policies —
--         first folder is ALWAYS the uploader's auth.uid()
--
--   listing-images:   {user_id}/{listing_id}/{uuid}.jpg
--   avatars:          {user_id}/avatar.jpg
--   chat-attachments: {user_id}/{conversation_id}/{uuid}.jpg
--   banners:          managed by service role only
--
-- Run AFTER 005_indexes.sql — LAST migration file.
-- ============================================================

-- ── Buckets ──────────────────────────────────────────────────
INSERT INTO storage.buckets (id, name, public, file_size_limit, allowed_mime_types)
VALUES
  ('listing-images',   'listing-images',   true,  5242880,   -- 5 MB
   ARRAY['image/jpeg','image/png','image/webp']),
  ('avatars',          'avatars',          true,  2097152,   -- 2 MB
   ARRAY['image/jpeg','image/png','image/webp']),
  ('chat-attachments', 'chat-attachments', false, 10485760,  -- 10 MB
   ARRAY['image/jpeg','image/png','image/webp','application/pdf']),
  ('banners',          'banners',          true,  5242880,   -- 5 MB
   ARRAY['image/jpeg','image/png','image/webp']);

-- ============================================================
-- LISTING IMAGES — public read, owner-folder write (C5)
-- ============================================================
CREATE POLICY "listing-images: public read"
  ON storage.objects FOR SELECT
  USING (bucket_id = 'listing-images');

CREATE POLICY "listing-images: owner folder upload"
  ON storage.objects FOR INSERT
  WITH CHECK (
    bucket_id = 'listing-images'
    AND auth.role() = 'authenticated'
    AND (storage.foldername(name))[1] = auth.uid()::TEXT
  );

CREATE POLICY "listing-images: owner folder delete"
  ON storage.objects FOR DELETE
  USING (
    bucket_id = 'listing-images'
    AND (storage.foldername(name))[1] = auth.uid()::TEXT
  );

-- ============================================================
-- AVATARS — public read, owner-folder write/replace
-- ============================================================
CREATE POLICY "avatars: public read"
  ON storage.objects FOR SELECT
  USING (bucket_id = 'avatars');

CREATE POLICY "avatars: owner folder upload"
  ON storage.objects FOR INSERT
  WITH CHECK (
    bucket_id = 'avatars'
    AND auth.role() = 'authenticated'
    AND (storage.foldername(name))[1] = auth.uid()::TEXT
  );

CREATE POLICY "avatars: owner folder update"
  ON storage.objects FOR UPDATE
  USING (
    bucket_id = 'avatars'
    AND (storage.foldername(name))[1] = auth.uid()::TEXT
  );

CREATE POLICY "avatars: owner folder delete"
  ON storage.objects FOR DELETE
  USING (
    bucket_id = 'avatars'
    AND (storage.foldername(name))[1] = auth.uid()::TEXT
  );

-- ============================================================
-- CHAT ATTACHMENTS — private: only conversation participants
-- Path: {sender_id}/{conversation_id}/{uuid}.ext
-- ============================================================
CREATE POLICY "chat-attachments: participants read"
  ON storage.objects FOR SELECT
  USING (
    bucket_id = 'chat-attachments'
    AND EXISTS (
      SELECT 1 FROM conversations c
      WHERE c.id::TEXT = (storage.foldername(name))[2]
        AND auth.uid() IN (c.buyer_id, c.seller_id)
    )
  );

CREATE POLICY "chat-attachments: participant upload to own folder"
  ON storage.objects FOR INSERT
  WITH CHECK (
    bucket_id = 'chat-attachments'
    AND (storage.foldername(name))[1] = auth.uid()::TEXT
    AND EXISTS (
      SELECT 1 FROM conversations c
      WHERE c.id::TEXT = (storage.foldername(name))[2]
        AND auth.uid() IN (c.buyer_id, c.seller_id)
    )
  );

-- ============================================================
-- BANNERS — public read, writes via service role only
-- (no INSERT/UPDATE/DELETE policies for authenticated)
-- ============================================================
CREATE POLICY "banners: public read"
  ON storage.objects FOR SELECT
  USING (bucket_id = 'banners');
