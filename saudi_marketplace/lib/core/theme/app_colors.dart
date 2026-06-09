import 'package:flutter/material.dart';

abstract final class AppColors {
  // ─── Brand ──────────────────────────────────────────────────────
  static const primary = Color(0xFF1A7A4A);
  static const primaryLight = Color(0xFFEAF5F0);
  static const primaryDark = Color(0xFF145E38);

  // ─── Navy ────────────────────────────────────────────────────────
  static const navy = Color(0xFF0D1F3C);
  static const navyLight = Color(0xFF1A3A5C);

  // ─── Neutral ─────────────────────────────────────────────────────
  static const grey50 = Color(0xFFF5F6F8);
  static const grey100 = Color(0xFFE8ECF0);
  static const grey200 = Color(0xFFE0E5EB);
  static const grey400 = Color(0xFFBCC2CC);
  static const grey500 = Color(0xFF9BA3B0);
  static const grey700 = Color(0xFF555555);

  // ─── Semantic ────────────────────────────────────────────────────
  static const error = Color(0xFFFF4757);
  static const warning = Color(0xFFF59E0B);
  static const ai = Color(0xFF7C3AED);

  // ─── Surface (Light) ─────────────────────────────────────────────
  static const surfaceLight = Color(0xFFFFFFFF);
  static const backgroundLight = Color(0xFFF5F6F8);

  // ─── Surface (Dark) ──────────────────────────────────────────────
  static const surfaceDark = Color(0xFF1C2B3A);
  static const backgroundDark = Color(0xFF0F1A27);
  static const cardDark = Color(0xFF1E2F40);
}
