import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import 'app_colors.dart';

abstract final class AppTextStyles {
  static TextStyle get displayLarge => GoogleFonts.tajawal(
        fontSize: 28, fontWeight: FontWeight.w700,
        color: AppColors.navy, height: 1.3);

  static TextStyle get displayMedium => GoogleFonts.tajawal(
        fontSize: 22, fontWeight: FontWeight.w700,
        color: AppColors.navy, height: 1.3);

  static TextStyle get headlineLarge => GoogleFonts.tajawal(
        fontSize: 18, fontWeight: FontWeight.w700,
        color: AppColors.navy, height: 1.4);

  static TextStyle get headlineMedium => GoogleFonts.tajawal(
        fontSize: 15, fontWeight: FontWeight.w700,
        color: AppColors.navy, height: 1.4);

  static TextStyle get titleLarge => GoogleFonts.tajawal(
        fontSize: 14, fontWeight: FontWeight.w600,
        color: AppColors.navy, height: 1.4);

  static TextStyle get titleMedium => GoogleFonts.tajawal(
        fontSize: 13, fontWeight: FontWeight.w600,
        color: AppColors.navy, height: 1.4);

  static TextStyle get titleSmall => GoogleFonts.tajawal(
        fontSize: 12, fontWeight: FontWeight.w600,
        color: AppColors.navy, height: 1.4);

  static TextStyle get bodyLarge => GoogleFonts.tajawal(
        fontSize: 14, fontWeight: FontWeight.w400,
        color: AppColors.navy, height: 1.5);

  static TextStyle get bodyMedium => GoogleFonts.tajawal(
        fontSize: 13, fontWeight: FontWeight.w400,
        color: AppColors.navy, height: 1.5);

  static TextStyle get bodySmall => GoogleFonts.tajawal(
        fontSize: 12, fontWeight: FontWeight.w400,
        color: AppColors.grey500, height: 1.5);

  static TextStyle get labelLarge => GoogleFonts.tajawal(
        fontSize: 11, fontWeight: FontWeight.w500,
        color: AppColors.grey500, height: 1.4);

  static TextStyle get labelSmall => GoogleFonts.tajawal(
        fontSize: 10, fontWeight: FontWeight.w500,
        color: AppColors.grey500, height: 1.4);

  static TextStyle get price => GoogleFonts.tajawal(
        fontSize: 14, fontWeight: FontWeight.w700,
        color: AppColors.primary, height: 1.3);

  static TextStyle get priceLarge => GoogleFonts.tajawal(
        fontSize: 20, fontWeight: FontWeight.w700,
        color: AppColors.primary, height: 1.3);
}
