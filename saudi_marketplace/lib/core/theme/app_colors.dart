import 'package:flutter/material.dart';

abstract final class AppColors {
  // ─── Brand ──────────────────────────────────────────────────────
  static const primary = Color(0xFF1A7A4A);
  static const primaryDark = Color(0xFF15663E); // prices, links, verified
  static const primaryLight = Color(0xFFEAF5F0); // chips, badges, pills
  static const primaryTint = Color(0xFFF0F7F4); // category icon tiles
  static const aiSparkle = Color(0xFF5FD99A); // AI banner sparkles glyph

  // ─── Navy ────────────────────────────────────────────────────────
  static const navy = Color(0xFF0D1F3C);
  static const navyLight = Color(0xFF1A3A5C);
  static const navyMuted = Color(0xFFB8CCE0); // AI banner subtitle

  // ─── Neutral ─────────────────────────────────────────────────────
  static const grey50 = Color(0xFFF5F6F8);
  static const grey100 = Color(0xFFE8ECF0); // card borders
  static const grey200 = Color(0xFFE0E5EB); // input borders
  static const grey300 = Color(0xFFEEF1F4); // subtle borders
  static const grey400 = Color(0xFF9BA3B0); // inactive nav, timestamps
  static const grey500 = Color(0xFF8A92A0); // placeholder text
  static const grey600 = Color(0xFF6B7280); // secondary text, meta
  static const grey700 = Color(0xFF4B5563); // category labels

  // ─── Semantic ────────────────────────────────────────────────────
  static const error = Color(0xFFE63946); // badges, favorite heart
  static const warning = Color(0xFFF59E0B); // star ratings
  static const online = Color(0xFF22C55E); // presence dot
  static const readTick = Color(0xFF4A9FE6); // chat read receipts

  // ─── Safety notice ───────────────────────────────────────────────
  static const noticeBg = Color(0xFFFFF8EC);
  static const noticeBorder = Color(0xFFF6E6C5);
  static const noticeText = Color(0xFF8A6516);
  static const noticeIcon = Color(0xFFC98A12);

  // ─── Surface (Light) ─────────────────────────────────────────────
  static const surfaceLight = Color(0xFFFFFFFF);
  static const backgroundLight = Color(0xFFF5F6F8);
  static const chatBackground = Color(0xFFF2F1EC);

  // ─── Surface (Dark) ──────────────────────────────────────────────
  static const surfaceDark = Color(0xFF1C2B3A);
  static const backgroundDark = Color(0xFF0F1A27);
  static const cardDark = Color(0xFF1E2F40);
}
