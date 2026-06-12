import 'package:flutter/material.dart';

import 'app_colors.dart';

abstract final class AppTextStyles {
  static const _font = 'Tajawal';

  static TextStyle get displayLarge => const TextStyle(
        fontFamily: _font, fontSize: 28, fontWeight: FontWeight.w700,
        color: AppColors.navy, height: 1.3);

  static TextStyle get displayMedium => const TextStyle(
        fontFamily: _font, fontSize: 22, fontWeight: FontWeight.w700,
        color: AppColors.navy, height: 1.3);

  static TextStyle get headlineLarge => const TextStyle(
        fontFamily: _font, fontSize: 18, fontWeight: FontWeight.w700,
        color: AppColors.navy, height: 1.4);

  static TextStyle get headlineMedium => const TextStyle(
        fontFamily: _font, fontSize: 15, fontWeight: FontWeight.w700,
        color: AppColors.navy, height: 1.4);

  static TextStyle get titleLarge => const TextStyle(
        fontFamily: _font, fontSize: 14, fontWeight: FontWeight.w600,
        color: AppColors.navy, height: 1.4);

  static TextStyle get titleMedium => const TextStyle(
        fontFamily: _font, fontSize: 13, fontWeight: FontWeight.w600,
        color: AppColors.navy, height: 1.4);

  static TextStyle get titleSmall => const TextStyle(
        fontFamily: _font, fontSize: 12, fontWeight: FontWeight.w600,
        color: AppColors.navy, height: 1.4);

  static TextStyle get bodyLarge => const TextStyle(
        fontFamily: _font, fontSize: 14, fontWeight: FontWeight.w400,
        color: AppColors.navy, height: 1.5);

  static TextStyle get bodyMedium => const TextStyle(
        fontFamily: _font, fontSize: 13, fontWeight: FontWeight.w400,
        color: AppColors.navy, height: 1.5);

  static TextStyle get bodySmall => const TextStyle(
        fontFamily: _font, fontSize: 12, fontWeight: FontWeight.w400,
        color: AppColors.grey500, height: 1.5);

  static TextStyle get labelLarge => const TextStyle(
        fontFamily: _font, fontSize: 11, fontWeight: FontWeight.w500,
        color: AppColors.grey500, height: 1.4);

  static TextStyle get labelSmall => const TextStyle(
        fontFamily: _font, fontSize: 10, fontWeight: FontWeight.w500,
        color: AppColors.grey500, height: 1.4);

  static TextStyle get price => const TextStyle(
        fontFamily: _font, fontSize: 14, fontWeight: FontWeight.w700,
        color: AppColors.primary, height: 1.3);

  static TextStyle get priceLarge => const TextStyle(
        fontFamily: _font, fontSize: 20, fontWeight: FontWeight.w700,
        color: AppColors.primary, height: 1.3);
}
