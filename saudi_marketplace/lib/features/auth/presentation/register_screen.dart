import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../providers/auth_provider.dart';
import 'widgets/auth_text_field.dart';

class RegisterScreen extends ConsumerStatefulWidget {
  const RegisterScreen({super.key});

  @override
  ConsumerState<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends ConsumerState<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _fullNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _obscurePassword = true;

  @override
  void dispose() {
    _fullNameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    FocusScope.of(context).unfocus();
    if (!(_formKey.currentState?.validate() ?? false)) return;

    final ok = await ref.read(authProvider.notifier).signUp(
          email: _emailController.text.trim(),
          password: _passwordController.text,
          fullName: _fullNameController.text.trim(),
        );
    if (ok && mounted) {
      if (context.canPop()) {
        context.pop();
      } else {
        context.go(AppRoutes.home);
      }
    }
  }

  String? _validateFullName(String? v) {
    final value = v?.trim() ?? '';
    if (value.isEmpty) return 'أدخل اسمك الكامل';
    if (value.length < 3) return 'الاسم قصير جداً';
    return null;
  }

  String? _validateEmail(String? v) {
    final value = v?.trim() ?? '';
    if (value.isEmpty) return 'أدخل البريد الإلكتروني';
    if (!RegExp(r'^[\w.+\-]+@[a-zA-Z0-9\-]+\.[a-zA-Z]{2,}$').hasMatch(value)) {
      return 'صيغة البريد الإلكتروني غير صحيحة';
    }
    return null;
  }

  String? _validatePassword(String? v) {
    if (v == null || v.isEmpty) return 'أدخل كلمة المرور';
    if (v.length < 6) return 'كلمة المرور ٦ أحرف على الأقل';
    return null;
  }

  @override
  Widget build(BuildContext context) {
    final auth = ref.watch(authProvider);

    return Scaffold(
      backgroundColor: Colors.white,
      appBar: AppBar(
        backgroundColor: Colors.white,
        elevation: 0,
        leading:
            BackButton(color: AppColors.navy, onPressed: () => context.pop()),
        title: Text('إنشاء حساب جديد',
            style: AppTextStyles.headlineLarge.copyWith(fontSize: 17)),
        centerTitle: true,
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(22, 16, 22, 24),
          child: Form(
            key: _formKey,
            child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('انضم إلى بيكيا وابدأ البيع والشراء',
                      style: AppTextStyles.bodyMedium
                          .copyWith(color: AppColors.grey600)),
                  const SizedBox(height: 22),
                  AuthTextField(
                    controller: _fullNameController,
                    label: 'الاسم الكامل',
                    hint: 'مثال: خالد العتيبي',
                    icon: TablerIcons.id_badge_2,
                    validator: _validateFullName,
                  ),
                  const SizedBox(height: 16),
                  AuthTextField(
                    controller: _emailController,
                    label: 'البريد الإلكتروني',
                    hint: 'example@gmail.com',
                    icon: TablerIcons.mail,
                    keyboardType: TextInputType.emailAddress,
                    validator: _validateEmail,
                  ),
                  const SizedBox(height: 16),
                  AuthTextField(
                    controller: _passwordController,
                    label: 'كلمة المرور',
                    hint: '••••••••',
                    icon: TablerIcons.lock,
                    obscureText: _obscurePassword,
                    textInputAction: TextInputAction.done,
                    validator: _validatePassword,
                    onSubmitted: (_) => _submit(),
                    suffix: IconButton(
                      onPressed: () => setState(
                          () => _obscurePassword = !_obscurePassword),
                      icon: Icon(
                        _obscurePassword
                            ? TablerIcons.eye
                            : TablerIcons.eye_off,
                        size: 20,
                        color: AppColors.grey500,
                      ),
                    ),
                  ),
                  if (auth.errorMessage != null) ...[
                    const SizedBox(height: 14),
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.symmetric(
                          horizontal: 13, vertical: 11),
                      decoration: BoxDecoration(
                        color: AppColors.error.withValues(alpha: 0.07),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(
                            color:
                                AppColors.error.withValues(alpha: 0.25)),
                      ),
                      child: Row(children: [
                        const Icon(TablerIcons.alert_circle,
                            size: 18, color: AppColors.error),
                        const SizedBox(width: 8),
                        Expanded(
                          child: Text(auth.errorMessage!,
                              style: AppTextStyles.bodyMedium
                                  .copyWith(color: AppColors.error)),
                        ),
                      ]),
                    ),
                  ],
                  const SizedBox(height: 24),
                  SizedBox(
                    width: double.infinity,
                    height: 52,
                    child: FilledButton(
                      onPressed: auth.isLoading ? null : _submit,
                      style: FilledButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        disabledBackgroundColor:
                            AppColors.primary.withValues(alpha: 0.6),
                        shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(14)),
                      ),
                      child: auth.isLoading
                          ? const SizedBox(
                              width: 22,
                              height: 22,
                              child: CircularProgressIndicator(
                                  strokeWidth: 2.5, color: Colors.white),
                            )
                          : Text('إنشاء الحساب',
                              style: AppTextStyles.headlineMedium.copyWith(
                                  color: Colors.white, fontSize: 15.5)),
                    ),
                  ),
                ]),
          ),
        ),
      ),
    );
  }
}
