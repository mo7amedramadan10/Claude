import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../providers/sell_provider.dart';
import 'widgets/sell_photo_picker.dart';
import 'widgets/sell_pickers.dart';

class SellScreen extends ConsumerStatefulWidget {
  const SellScreen({super.key, this.editingId});

  /// إذا غير null → وضع التعديل
  final String? editingId;

  @override
  ConsumerState<SellScreen> createState() => _SellScreenState();
}

class _SellScreenState extends ConsumerState<SellScreen> {
  final _formKey = GlobalKey<FormState>();
  final _titleController = TextEditingController();
  final _priceController = TextEditingController();
  final _neighborhoodController = TextEditingController();
  final _descriptionController = TextEditingController();

  bool _controllersLoaded = false;

  @override
  void initState() {
    super.initState();
    if (widget.editingId != null) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        ref.read(sellProvider.notifier).initEdit(widget.editingId!);
      });
    }
  }

  @override
  void dispose() {
    _titleController.dispose();
    _priceController.dispose();
    _neighborhoodController.dispose();
    _descriptionController.dispose();
    super.dispose();
  }

  double? _parsePrice(String raw) {
    var s = raw.trim().replaceAll(',', '');
    const eastern = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
    for (var i = 0; i < eastern.length; i++) {
      s = s.replaceAll(eastern[i], '$i');
    }
    return double.tryParse(s);
  }

  Future<void> _submit() async {
    FocusScope.of(context).unfocus();
    if (!(_formKey.currentState?.validate() ?? false)) return;

    final state = ref.read(sellProvider);
    if (state.selectedCategory == null || state.selectedCity == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('اختر الفئة والمدينة أولاً')),
      );
      return;
    }

    final listingId = await ref.read(sellProvider.notifier).submit(
          title: _titleController.text,
          description: _descriptionController.text,
          price: _parsePrice(_priceController.text),
          neighborhood: _neighborhoodController.text,
        );

    if (listingId != null && mounted) {
      final isEdit = ref.read(sellProvider).isEditMode;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text(isEdit
                ? 'تم حفظ التعديلات بنجاح ✅'
                : 'تم نشر إعلانك بنجاح 🎉')),
      );
      context.pushReplacement(AppRoutes.myListings);
    }
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(sellProvider);
    final notifier = ref.read(sellProvider.notifier);
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final isEditMode = state.isEditMode;

    // ملء الحقول النصية عند وصول بيانات الإعلان (مرة واحدة فقط)
    ref.listen(
      sellProvider.select((s) => s.editableData),
      (_, data) {
        if (data != null && !_controllersLoaded) {
          _controllersLoaded = true;
          _titleController.text = data.title;
          _descriptionController.text = data.description;
          _priceController.text =
              data.price != null ? data.price!.toStringAsFixed(0) : '';
          _neighborhoodController.text = data.neighborhood ?? '';
        }
      },
    );

    return Scaffold(
      backgroundColor: isDark ? AppColors.backgroundDark : Colors.white,
      appBar: AppBar(
        backgroundColor: isDark ? AppColors.backgroundDark : Colors.white,
        elevation: 0,
        leading: BackButton(
          color: isDark ? Colors.white : AppColors.navy,
          onPressed: () => context.pop(),
        ),
        title: Text(
          isEditMode ? 'تعديل الإعلان' : 'أضف إعلاناً',
          style: AppTextStyles.headlineLarge.copyWith(
              fontSize: 17,
              color: isDark ? Colors.white : AppColors.navy),
        ),
        centerTitle: true,
      ),
      body: SafeArea(
        child: state.isLoadingEdit
            ? const Center(
                child: CircularProgressIndicator(color: AppColors.primary))
            : Form(
                key: _formKey,
                child: ListView(
                  padding: const EdgeInsets.fromLTRB(20, 14, 20, 24),
                  children: [
                    SellPhotoPicker(
                      existingImages: state.existingImages,
                      newImages: state.newImages,
                      onAdd: notifier.addImages,
                      onRemoveExisting: notifier.removeExistingImage,
                      onRemoveNew: notifier.removeNewImage,
                    ),
                    const SizedBox(height: 20),
                    _SellField(
                      controller: _titleController,
                      label: 'عنوان الإعلان',
                      hint: 'مثال: آيفون 15 برو ماكس 256GB',
                      icon: TablerIcons.text_caption,
                      validator: (v) {
                        final value = v?.trim() ?? '';
                        if (value.isEmpty) return 'أدخل عنوان الإعلان';
                        if (value.length < 4) return 'العنوان قصير جداً';
                        return null;
                      },
                    ),
                    const SizedBox(height: 16),
                    SellLookupField(
                      label: 'الفئة',
                      placeholder: 'اختر فئة الإعلان',
                      icon: TablerIcons.category,
                      options: state.categories,
                      selected: state.selectedCategory,
                      onSelect: notifier.selectCategory,
                    ),
                    const SizedBox(height: 16),
                    SellLookupField(
                      label: 'المدينة',
                      placeholder: 'اختر المدينة',
                      icon: TablerIcons.map_pin,
                      options: state.cities,
                      selected: state.selectedCity,
                      onSelect: notifier.selectCity,
                    ),
                    const SizedBox(height: 16),
                    _SellField(
                      controller: _neighborhoodController,
                      label: 'الحي (اختياري)',
                      hint: 'مثال: حي الملقا',
                      icon: TablerIcons.building_community,
                    ),
                    const SizedBox(height: 18),
                    ConditionChips(
                      selected: state.condition,
                      onSelect: notifier.setCondition,
                    ),
                    const SizedBox(height: 18),
                    _SellField(
                      controller: _priceController,
                      label: 'السعر بالريال (اتركه فارغاً إذا مجاني)',
                      hint: 'مثال: 4200',
                      icon: TablerIcons.currency_riyal,
                      keyboardType: TextInputType.number,
                      validator: (v) {
                        final value = v?.trim() ?? '';
                        if (value.isEmpty) return null;
                        if (_parsePrice(value) == null) {
                          return 'أدخل سعراً صحيحاً';
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 10),
                    _NegotiableSwitch(
                      value: state.isNegotiable,
                      onChanged: notifier.toggleNegotiable,
                    ),
                    const SizedBox(height: 18),
                    _SellField(
                      controller: _descriptionController,
                      label: 'الوصف',
                      hint: 'اشرح تفاصيل المنتج، حالته، وسبب البيع...',
                      icon: TablerIcons.align_right,
                      maxLines: 5,
                      validator: (v) {
                        final value = v?.trim() ?? '';
                        if (value.isEmpty) return 'أدخل وصف الإعلان';
                        if (value.length < 10) return 'الوصف قصير جداً';
                        return null;
                      },
                    ),
                    if (state.errorMessage != null) ...[
                      const SizedBox(height: 14),
                      _ErrorBanner(message: state.errorMessage!),
                    ],
                    const SizedBox(height: 22),
                    _SubmitButton(
                      isSubmitting: state.isSubmitting,
                      isEditMode: isEditMode,
                      onPressed: _submit,
                    ),
                  ],
                ),
              ),
      ),
    );
  }
}

// ── Shared widgets ────────────────────────────────────────────────────────

class _SellField extends StatelessWidget {
  const _SellField({
    required this.controller,
    required this.label,
    required this.hint,
    required this.icon,
    this.maxLines = 1,
    this.keyboardType,
    this.validator,
  });

  final TextEditingController controller;
  final String label;
  final String hint;
  final IconData icon;
  final int maxLines;
  final TextInputType? keyboardType;
  final String? Function(String?)? validator;

  @override
  Widget build(BuildContext context) {
    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Text(label,
          style: AppTextStyles.titleMedium
              .copyWith(color: AppColors.grey700, fontSize: 13.5)),
      const SizedBox(height: 8),
      TextFormField(
        controller: controller,
        maxLines: maxLines,
        keyboardType: keyboardType,
        validator: validator,
        style: AppTextStyles.bodyLarge.copyWith(fontSize: 14.5),
        decoration: InputDecoration(
          hintText: hint,
          hintStyle:
              AppTextStyles.bodyMedium.copyWith(color: AppColors.grey500),
          prefixIcon: maxLines == 1
              ? Icon(icon, size: 20, color: AppColors.grey500)
              : null,
          filled: true,
          fillColor: AppColors.grey50,
          contentPadding:
              const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
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

class _NegotiableSwitch extends StatelessWidget {
  const _NegotiableSwitch({required this.value, required this.onChanged});

  final bool value;
  final void Function(bool) onChanged;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 4),
      decoration: BoxDecoration(
        color: AppColors.grey50,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppColors.grey200),
      ),
      child: Row(children: [
        const Icon(TablerIcons.arrows_exchange,
            size: 20, color: AppColors.grey500),
        const SizedBox(width: 10),
        Expanded(
          child: Text('السعر قابل للتفاوض',
              style: AppTextStyles.bodyLarge.copyWith(fontSize: 14)),
        ),
        Switch(
          value: value,
          onChanged: onChanged,
          activeColor: AppColors.primary,
        ),
      ]),
    );
  }
}

class _ErrorBanner extends StatelessWidget {
  const _ErrorBanner({required this.message});
  final String message;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding:
          const EdgeInsets.symmetric(horizontal: 13, vertical: 11),
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
  const _SubmitButton({
    required this.isSubmitting,
    required this.isEditMode,
    required this.onPressed,
  });

  final bool isSubmitting;
  final bool isEditMode;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 52,
      child: FilledButton(
        onPressed: isSubmitting ? null : onPressed,
        style: FilledButton.styleFrom(
          backgroundColor: AppColors.primary,
          disabledBackgroundColor:
              AppColors.primary.withValues(alpha: 0.6),
          shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14)),
        ),
        child: isSubmitting
            ? const SizedBox(
                width: 22,
                height: 22,
                child: CircularProgressIndicator(
                    strokeWidth: 2.5, color: Colors.white),
              )
            : Text(
                isEditMode ? 'حفظ التعديلات' : 'نشر الإعلان',
                style: AppTextStyles.headlineMedium
                    .copyWith(color: Colors.white, fontSize: 15.5),
              ),
      ),
    );
  }
}
