import 'package:flutter/material.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';

class AuthTextField extends StatelessWidget {
  const AuthTextField({
    super.key,
    required this.controller,
    required this.label,
    required this.hint,
    required this.icon,
    this.obscureText = false,
    this.suffix,
    this.keyboardType,
    this.textInputAction = TextInputAction.next,
    this.validator,
    this.onSubmitted,
  });

  final TextEditingController controller;
  final String label;
  final String hint;
  final IconData icon;
  final bool obscureText;
  final Widget? suffix;
  final TextInputType? keyboardType;
  final TextInputAction textInputAction;
  final String? Function(String?)? validator;
  final void Function(String)? onSubmitted;

  @override
  Widget build(BuildContext context) {
    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Text(label,
          style: AppTextStyles.titleMedium
              .copyWith(color: AppColors.grey700, fontSize: 13.5)),
      const SizedBox(height: 8),
      TextFormField(
        controller: controller,
        obscureText: obscureText,
        keyboardType: keyboardType,
        textInputAction: textInputAction,
        validator: validator,
        onFieldSubmitted: onSubmitted,
        style: AppTextStyles.bodyLarge.copyWith(fontSize: 14.5),
        decoration: InputDecoration(
          hintText: hint,
          hintStyle:
              AppTextStyles.bodyMedium.copyWith(color: AppColors.grey500),
          prefixIcon: Icon(icon, size: 20, color: AppColors.grey500),
          suffixIcon: suffix,
          filled: true,
          fillColor: AppColors.grey50,
          contentPadding:
              const EdgeInsets.symmetric(horizontal: 14, vertical: 15),
          border: _border(AppColors.grey200),
          enabledBorder: _border(AppColors.grey200),
          focusedBorder: _border(AppColors.primary, width: 1.5),
          errorBorder: _border(AppColors.error),
          focusedErrorBorder: _border(AppColors.error, width: 1.5),
          errorStyle:
              AppTextStyles.bodySmall.copyWith(color: AppColors.error),
        ),
      ),
    ]);
  }

  OutlineInputBorder _border(Color color, {double width = 1}) =>
      OutlineInputBorder(
        borderRadius: BorderRadius.circular(14),
        borderSide: BorderSide(color: color, width: width),
      );
}
