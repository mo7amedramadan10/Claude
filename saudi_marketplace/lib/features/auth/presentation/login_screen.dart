import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../providers/auth_provider.dart';
import 'widgets/auth_text_field.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _obscurePassword = true;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    FocusScope.of(context).unfocus();
    if (!(_formKey.currentState?.validate() ?? false)) return;

    final ok = await ref.read(authProvider.notifier).signIn(
          _emailController.text.trim(),
          _passwordController.text,
        );
    if (ok && mounted) {
      // لو الشاشة انفتحت بـ push (من تاب حسابي) → ارجع لها بعد الدخول
      // لو كانت الـ initial route → اذهب للرئيسية
      if (context.canPop()) {
        context.pop();
      } else {
        context.go(AppRoutes.home);
      }
    }
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
      backgroundColor: AppColors.navy,
      body: Column(children: [
        const _BrandHero(),
        Expanded(
          child: Container(
            width: double.infinity,
            decoration: const BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
            ),
            child: SingleChildScrollView(
              padding: const EdgeInsets.fromLTRB(22, 28, 22, 24),
              child: Form(
                key: _formKey,
                child:
                    Column(crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                  Text('أهلاً بعودتك',
                      style: AppTextStyles.displayMedium
                          .copyWith(fontSize: 21)),
                  const SizedBox(height: 4),
                  Text('سجّل الدخول للمتابعة إلى حسابك',
                      style: AppTextStyles.bodyMedium
                          .copyWith(color: AppColors.grey600)),
                  const SizedBox(height: 24),
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
                    _ErrorBanner(message: auth.errorMessage!),
                  ],
                  const SizedBox(height: 22),
                  _SubmitButton(
                    isLoading: auth.isLoading,
                    onPressed: _submit,
                  ),
                  const SizedBox(height: 14),
                  Center(
                    child: TextButton(
                      onPressed: () => context.canPop()
                          ? context.pop()
                          : context.go(AppRoutes.home),
                      child: Text('الدخول كزائر',
                          style: AppTextStyles.titleMedium.copyWith(
                              color: AppColors.grey600, fontSize: 13.5)),
                    ),
                  ),
                  const SizedBox(height: 6),
                  Center(
                    child: Row(mainAxisSize: MainAxisSize.min, children: [
                      Text('ليس لديك حساب؟',
                          style: AppTextStyles.bodyMedium
                              .copyWith(color: AppColors.grey600)),
                      TextButton(
                        onPressed: () => context.push(AppRoutes.register),
                        style: TextButton.styleFrom(
                          padding:
                              const EdgeInsets.symmetric(horizontal: 6),
                          minimumSize: Size.zero,
                        ),
                        child: Text('أنشئ حساباً',
                            style: AppTextStyles.titleMedium.copyWith(
                                color: AppColors.primaryDark,
                                fontSize: 13.5)),
                      ),
                    ]),
                  ),
                ]),
              ),
            ),
          ),
        ),
      ]),
    );
  }
}

class _BrandHero extends StatelessWidget {
  const _BrandHero();

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [AppColors.navy, AppColors.navyLight],
        ),
      ),
      child: SafeArea(
        bottom: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(22, 36, 22, 34),
          child: Column(children: [
            Container(
              width: 72,
              height: 72,
              decoration: BoxDecoration(
                color: AppColors.primary,
                borderRadius: BorderRadius.circular(22),
                boxShadow: [
                  BoxShadow(
                    color: AppColors.primary.withValues(alpha: 0.4),
                    blurRadius: 24,
                    offset: const Offset(0, 8),
                  ),
                ],
              ),
              child: const Icon(TablerIcons.building_store,
                  size: 36, color: Colors.white),
            ),
            const SizedBox(height: 16),
            Text('بيكيا',
                style: AppTextStyles.displayLarge
                    .copyWith(color: Colors.white, fontSize: 30)),
            const SizedBox(height: 4),
            Text('سوقك السعودي للبيع والشراء',
                style: AppTextStyles.bodyMedium
                    .copyWith(color: AppColors.navyMuted)),
          ]),
        ),
      ),
    );
  }
}

class _ErrorBanner extends StatelessWidget {
  const _ErrorBanner({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(horizontal: 13, vertical: 11),
      decoration: BoxDecoration(
        color: AppColors.error.withValues(alpha: 0.07),
        borderRadius: BorderRadius.circular(12),
        border:
            Border.all(color: AppColors.error.withValues(alpha: 0.25)),
      ),
      child: Row(children: [
        const Icon(TablerIcons.alert_circle,
            size: 18, color: AppColors.error),
        const SizedBox(width: 8),
        Expanded(
          child: Text(message,
              style: AppTextStyles.bodyMedium
                  .copyWith(color: AppColors.error)),
        ),
      ]),
    );
  }
}

class _SubmitButton extends StatelessWidget {
  const _SubmitButton({required this.isLoading, required this.onPressed});

  final bool isLoading;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      width: double.infinity,
      height: 52,
      child: FilledButton(
        onPressed: isLoading ? null : onPressed,
        style: FilledButton.styleFrom(
          backgroundColor: AppColors.primary,
          disabledBackgroundColor:
              AppColors.primary.withValues(alpha: 0.6),
          shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14)),
        ),
        child: isLoading
            ? const SizedBox(
                width: 22,
                height: 22,
                child: CircularProgressIndicator(
                    strokeWidth: 2.5, color: Colors.white),
              )
            : Text('تسجيل الدخول',
                style: AppTextStyles.headlineMedium
                    .copyWith(color: Colors.white, fontSize: 15.5)),
      ),
    );
  }
}
