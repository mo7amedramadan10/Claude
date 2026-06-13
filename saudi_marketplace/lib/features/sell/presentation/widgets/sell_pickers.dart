import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../models/sell_models.dart';

/// صف اختيار يفتح bottom sheet بقائمة خيارات (فئة / مدينة)
class SellLookupField extends StatelessWidget {
  const SellLookupField({
    super.key,
    required this.label,
    required this.placeholder,
    required this.icon,
    required this.options,
    required this.selected,
    required this.onSelect,
    this.isLoading = false,
    this.errorText,
    this.onRetry,
  });

  final String label;
  final String placeholder;
  final IconData icon;
  final List<LookupOption> options;
  final LookupOption? selected;
  final void Function(LookupOption) onSelect;

  /// تحميل القائمة جارٍ
  final bool isLoading;

  /// نص خطأ تحميل القائمة (إن وُجد)
  final String? errorText;

  /// إعادة محاولة التحميل عند الضغط على حقل فارغ
  final Future<void> Function()? onRetry;

  void _openSheet(BuildContext context) {
    showModalBottomSheet<void>(
      context: context,
      shape: const RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (sheetContext) => SafeArea(
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Container(
            width: 40,
            height: 4,
            margin: const EdgeInsets.only(top: 10, bottom: 8),
            decoration: BoxDecoration(
                color: AppColors.grey200,
                borderRadius: BorderRadius.circular(2)),
          ),
          Padding(
            padding: const EdgeInsets.symmetric(vertical: 6),
            child: Text(label,
                style: AppTextStyles.headlineMedium.copyWith(fontSize: 15)),
          ),
          Flexible(
            child: ListView.separated(
              shrinkWrap: true,
              padding: const EdgeInsets.only(bottom: 12),
              itemCount: options.length,
              separatorBuilder: (_, __) => const Divider(
                  height: 1, indent: 20, endIndent: 20,
                  color: AppColors.grey100),
              itemBuilder: (_, i) {
                final option = options[i];
                final isSelected = option.id == selected?.id;
                return ListTile(
                  title: Text(option.label,
                      style: AppTextStyles.titleLarge.copyWith(
                        fontSize: 14,
                        color: isSelected
                            ? AppColors.primaryDark
                            : AppColors.navy,
                        fontWeight: isSelected
                            ? FontWeight.w700
                            : FontWeight.w500,
                      )),
                  trailing: isSelected
                      ? const Icon(TablerIcons.check,
                          size: 19, color: AppColors.primaryDark)
                      : null,
                  onTap: () {
                    onSelect(option);
                    Navigator.pop(sheetContext);
                  },
                );
              },
            ),
          ),
        ]),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final hasValue = selected != null;
    final hasError = errorText != null;
    final isEmpty = options.isEmpty;

    // الضغط: تحميل ⇐ معطّل · فارغ ⇐ إعادة محاولة · فيه خيارات ⇐ فتح القائمة
    final VoidCallback? onTap = isLoading
        ? null
        : (isEmpty ? (onRetry == null ? null : () => onRetry!()) : () => _openSheet(context));

    final Color borderColor =
        hasError ? AppColors.error.withValues(alpha: 0.4) : AppColors.grey200;

    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Text(label,
          style: AppTextStyles.titleMedium
              .copyWith(color: AppColors.grey700, fontSize: 13.5)),
      const SizedBox(height: 8),
      InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(14),
        child: Container(
          padding:
              const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
          decoration: BoxDecoration(
            color: AppColors.grey50,
            borderRadius: BorderRadius.circular(14),
            border: Border.all(color: borderColor),
          ),
          child: _buildContent(hasValue),
        ),
      ),
    ]);
  }

  Widget _buildContent(bool hasValue) {
    // ── جارٍ التحميل ──────────────────────────────────────────
    if (isLoading) {
      return Row(children: [
        const SizedBox(
          width: 18,
          height: 18,
          child: CircularProgressIndicator(
              strokeWidth: 2, color: AppColors.primary),
        ),
        const SizedBox(width: 12),
        Text('جارٍ التحميل...',
            style: AppTextStyles.bodyLarge
                .copyWith(fontSize: 14, color: AppColors.grey500)),
      ]);
    }

    // ── خطأ تحميل القائمة ────────────────────────────────────
    if (errorText != null) {
      return Row(children: [
        const Icon(TablerIcons.alert_circle,
            size: 20, color: AppColors.error),
        const SizedBox(width: 10),
        Expanded(
          child: Text(errorText!,
              style: AppTextStyles.bodyMedium
                  .copyWith(fontSize: 13, color: AppColors.error)),
        ),
        const Icon(TablerIcons.refresh, size: 18, color: AppColors.error),
      ]);
    }

    // ── الحالة الطبيعية ──────────────────────────────────────
    return Row(children: [
      Icon(icon, size: 20, color: AppColors.grey500),
      const SizedBox(width: 10),
      Expanded(
        child: Text(
          hasValue ? selected!.label : placeholder,
          style: AppTextStyles.bodyLarge.copyWith(
            fontSize: 14,
            color: hasValue ? AppColors.navy : AppColors.grey500,
          ),
        ),
      ),
      const Icon(TablerIcons.chevron_down,
          size: 18, color: AppColors.grey400),
    ]);
  }
}

/// شرائح اختيار حالة المنتج
class ConditionChips extends StatelessWidget {
  const ConditionChips({
    super.key,
    required this.selected,
    required this.onSelect,
  });

  final String selected;
  final void Function(String) onSelect;

  @override
  Widget build(BuildContext context) {
    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Text('حالة المنتج',
          style: AppTextStyles.titleMedium
              .copyWith(color: AppColors.grey700, fontSize: 13.5)),
      const SizedBox(height: 10),
      Wrap(
        spacing: 8,
        runSpacing: 8,
        children: ListingCondition.values.map((value) {
          final isSelected = value == selected;
          return GestureDetector(
            onTap: () => onSelect(value),
            child: AnimatedContainer(
              duration: const Duration(milliseconds: 160),
              padding: const EdgeInsets.symmetric(
                  horizontal: 16, vertical: 9),
              decoration: BoxDecoration(
                color:
                    isSelected ? AppColors.primary : AppColors.grey50,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(
                  color: isSelected
                      ? AppColors.primary
                      : AppColors.grey200,
                ),
              ),
              child: Text(
                ListingCondition.labels[value]!,
                style: AppTextStyles.titleSmall.copyWith(
                  fontSize: 12.5,
                  color: isSelected ? Colors.white : AppColors.grey700,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ),
          );
        }).toList(),
      ),
    ]);
  }
}
