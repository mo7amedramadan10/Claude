import 'package:flutter/material.dart';
import 'app_colors.dart';

abstract final class AppTextStyles {
  static const _family = 'Tajawal';

  // ─── Display ─────────────────────────────────────────────────────
  static const displayLarge = TextStyle(
    fontFamily: _family,
    fontSize: 28,
    fontWeight: FontWeight.w700,
    color: AppColors.navy,
    height: 1.3,
  );

  static const displayMedium = TextStyle(
    fontFamily: _family,
    fontSize: 22,
    fontWeight: FontWeight.w700,
    color: AppColors.navy,
    height: 1.3,
  );

  // ─── Headline ────────────────────────────────────────────────────
  static const headlineLarge = TextStyle(
    fontFamily: _family,
    fontSize: 18,
    fontWeight: FontWeight.w700,
    color: AppColors.navy,
    height: 1.4,
  );

  static const headlineMedium = TextStyle(
    fontFamily: _family,
    fontSize: 15,
    fontWeight: FontWeight.w700,
    color: AppColors.navy,
    height: 1.4,
  );

  // ─── Title ───────────────────────────────────────────────────────
  static const titleLarge = TextStyle(
    fontFamily: _family,
    fontSize: 14,
    fontWeight: FontWeight.w600,
    color: AppColors.navy,
    height: 1.4,
  );

  static const titleMedium = TextStyle(
    fontFamily: _family,
    fontSize: 13,
    fontWeight: FontWeight.w600,
    color: AppColors.navy,
    height: 1.4,
  );

  static const titleSmall = TextStyle(
    fontFamily: _family,
    fontSize: 12,
    fontWeight: FontWeight.w600,
    color: AppColors.navy,
    height: 1.4,
  );

  // ─── Body ────────────────────────────────────────────────────────
  static const bodyLarge = TextStyle(
    fontFamily: _family,
    fontSize: 14,
    fontWeight: FontWeight.w400,
    color: AppColors.navy,
    height: 1.5,
  );

  static const bodyMedium = TextStyle(
    fontFamily: _family,
    fontSize: 13,
    fontWeight: FontWeight.w400,
    color: AppColors.navy,
    height: 1.5,
  );

  static const bodySmall = TextStyle(
    fontFamily: _family,
    fontSize: 12,
    fontWeight: FontWeight.w400,
    color: AppColors.grey500,
    height: 1.5,
  );

  // ─── Label ───────────────────────────────────────────────────────
  static const labelLarge = TextStyle(
    fontFamily: _family,
    fontSize: 11,
    fontWeight: FontWeight.w500,
    color: AppColors.grey500,
    height: 1.4,
  );

  static const labelSmall = TextStyle(
    fontFamily: _family,
    fontSize: 10,
    fontWeight: FontWeight.w500,
    color: AppColors.grey500,
    height: 1.4,
  );

  // ─── Price ───────────────────────────────────────────────────────
  static const price = TextStyle(
    fontFamily: _family,
    fontSize: 14,
    fontWeight: FontWeight.w700,
    color: AppColors.primary,
    height: 1.3,
  );

  static const priceLarge = TextStyle(
    fontFamily: _family,
    fontSize: 20,
    fontWeight: FontWeight.w700,
    color: AppColors.primary,
    height: 1.3,
  );
}
