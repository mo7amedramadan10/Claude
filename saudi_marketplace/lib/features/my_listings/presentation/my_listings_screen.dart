import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/utils/number_formatter.dart';
import '../providers/my_listings_provider.dart';
import 'widgets/my_listing_card.dart';

class MyListingsScreen extends ConsumerWidget {
  const MyListingsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(myListingsProvider);
    final notifier = ref.read(myListingsProvider.notifier);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Scaffold(
      backgroundColor:
          isDark ? AppColors.backgroundDark : AppColors.backgroundLight,
      body: SafeArea(
        child: Column(children: [
          _Header(onAddTap: () => context.push(AppRoutes.sellItem)),
          // ─── Filter tabs ──────────────────────────────────────
          _FilterBar(state: state, onFilterChanged: notifier.setFilter),
          // ─── Body ─────────────────────────────────────────────
          Expanded(child: _Body(state: state, notifier: notifier)),
        ]),
      ),
    );
  }
}

// ── Header ─────────────────────────────────────────────────────────────────
class _Header extends StatelessWidget {
  const _Header({required this.onAddTap});
  final VoidCallback onAddTap;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Container(
      padding: const EdgeInsets.fromLTRB(16, 10, 12, 10),
      color: isDark ? AppColors.backgroundDark : Colors.white,
      child: Row(children: [
        IconButton(
          icon: const Icon(TablerIcons.arrow_right),
          color: isDark ? Colors.white : AppColors.navy,
          onPressed: () => context.pop(),
          padding: EdgeInsets.zero,
          constraints: const BoxConstraints(),
        ),
        const SizedBox(width: 10),
        Text(
          'إعلاناتي',
          style: AppTextStyles.headlineLarge
              .copyWith(color: isDark ? Colors.white : AppColors.navy),
        ),
        const Spacer(),
        FilledButton.icon(
          onPressed: onAddTap,
          icon: const Icon(TablerIcons.plus, size: 16),
          label: const Text('أضف إعلاناً'),
          style: FilledButton.styleFrom(
            backgroundColor: AppColors.primary,
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 9),
            textStyle: AppTextStyles.titleMedium.copyWith(fontSize: 13),
            shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12)),
          ),
        ),
      ]),
    );
  }
}

// ── Filter Bar ─────────────────────────────────────────────────────────────
class _FilterBar extends StatelessWidget {
  const _FilterBar({required this.state, required this.onFilterChanged});
  final MyListingsState state;
  final void Function(ListingStatusFilter) onFilterChanged;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;
    return Container(
      color: isDark ? AppColors.backgroundDark : Colors.white,
      child: Column(children: [
        Divider(
          height: 1,
          color: isDark ? AppColors.navyLight : AppColors.grey100,
        ),
        SingleChildScrollView(
          scrollDirection: Axis.horizontal,
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
          child: Row(
            children: ListingStatusFilter.values.map((f) {
              final isSelected = state.filter == f;
              final count = state.countFor(f);
              return Padding(
                padding: const EdgeInsets.only(left: 8),
                child: _FilterChip(
                  label: f.labelAr,
                  count: count,
                  isSelected: isSelected,
                  onTap: () => onFilterChanged(f),
                ),
              );
            }).toList(),
          ),
        ),
      ]),
    );
  }
}

class _FilterChip extends StatelessWidget {
  const _FilterChip({
    required this.label,
    required this.count,
    required this.isSelected,
    required this.onTap,
  });
  final String label;
  final int count;
  final bool isSelected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 180),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 7),
        decoration: BoxDecoration(
          color: isSelected ? AppColors.primary : AppColors.grey50,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(
            color: isSelected ? AppColors.primary : AppColors.grey200,
          ),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          Text(
            label,
            style: AppTextStyles.titleSmall.copyWith(
              color: isSelected ? Colors.white : AppColors.grey700,
              fontWeight: FontWeight.w600,
            ),
          ),
          if (count > 0) ...[
            const SizedBox(width: 6),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 1),
              decoration: BoxDecoration(
                color: isSelected
                    ? Colors.white.withValues(alpha: 0.25)
                    : AppColors.grey200,
                borderRadius: BorderRadius.circular(10),
              ),
              child: Text(
                NumberFormatter.toArabicDigits(count),
                style: AppTextStyles.labelSmall.copyWith(
                  color: isSelected ? Colors.white : AppColors.grey700,
                  fontWeight: FontWeight.w700,
                ),
              ),
            ),
          ],
        ]),
      ),
    );
  }
}

// ── Body ───────────────────────────────────────────────────────────────────
class _Body extends ConsumerWidget {
  const _Body({required this.state, required this.notifier});
  final MyListingsState state;
  final MyListingsNotifier notifier;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    if (state.isLoading) {
      return const Center(
          child: CircularProgressIndicator(color: AppColors.primary));
    }

    if (state.errorMessage != null && state.listings.isEmpty) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(mainAxisSize: MainAxisSize.min, children: [
            const Icon(TablerIcons.alert_circle,
                size: 42, color: AppColors.grey400),
            const SizedBox(height: 12),
            Text(state.errorMessage!,
                textAlign: TextAlign.center,
                style: AppTextStyles.bodyMedium
                    .copyWith(color: AppColors.grey600)),
          ]),
        ),
      );
    }

    final items = state.filtered;

    return RefreshIndicator(
      color: AppColors.primary,
      onRefresh: notifier.refresh,
      child: items.isEmpty
          ? _EmptyState(filter: state.filter)
          : ListView.separated(
              padding: const EdgeInsets.fromLTRB(14, 14, 14, 24),
              itemCount: items.length,
              separatorBuilder: (_, __) => const SizedBox(height: 12),
              itemBuilder: (ctx, i) {
                final item = items[i];
                return MyListingCard(
                  item: item,
                  onEdit: () => ctx.push(AppRoutes.sellItem),
                  onMarkAsSold: () => notifier.markAsSold(item.id),
                  onDelete: () => notifier.deleteItem(item.id),
                );
              },
            ),
    );
  }
}

// ── Empty State ────────────────────────────────────────────────────────────
class _EmptyState extends StatelessWidget {
  const _EmptyState({required this.filter});
  final ListingStatusFilter filter;

  @override
  Widget build(BuildContext context) {
    final (icon, title, sub) = switch (filter) {
      ListingStatusFilter.all => (
          TablerIcons.clipboard_text,
          'لا يوجد إعلانات بعد',
          'أضف إعلانك الأول وابدأ البيع الآن'
        ),
      ListingStatusFilter.active => (
          TablerIcons.bolt,
          'لا يوجد إعلانات نشطة',
          'إعلاناتك النشطة ستظهر هنا'
        ),
      ListingStatusFilter.sold => (
          TablerIcons.check_circle,
          'لا يوجد مبيعات بعد',
          'الإعلانات المكتملة ستظهر هنا'
        ),
      ListingStatusFilter.expired => (
          TablerIcons.clock_off,
          'لا يوجد إعلانات منتهية',
          'الإعلانات المنتهية ستظهر هنا'
        ),
    };

    return LayoutBuilder(
      builder: (_, constraints) => SingleChildScrollView(
        physics: const AlwaysScrollableScrollPhysics(),
        child: SizedBox(
          height: constraints.maxHeight,
          child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Container(
                  width: 80,
                  height: 80,
                  decoration: BoxDecoration(
                    color: AppColors.primaryLight,
                    shape: BoxShape.circle,
                  ),
                  child: Icon(icon, size: 38, color: AppColors.primary),
                ),
                const SizedBox(height: 16),
                Text(title,
                    style: AppTextStyles.headlineMedium
                        .copyWith(fontSize: 16)),
                const SizedBox(height: 6),
                Text(sub,
                    textAlign: TextAlign.center,
                    style: AppTextStyles.bodyMedium
                        .copyWith(color: AppColors.grey600)),
              ]),
        ),
      ),
    );
  }
}
