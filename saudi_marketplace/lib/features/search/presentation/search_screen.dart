import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/shared/widgets/listing_card.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../providers/search_provider.dart';
import 'widgets/filter_bottom_sheet.dart';
import 'widgets/search_filter_chips.dart';
import 'widgets/search_top_bar.dart';

class SearchScreen extends ConsumerWidget {
  const SearchScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(searchProvider);
    final notifier = ref.read(searchProvider.notifier);
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Scaffold(
      body: SafeArea(
        child: Column(children: [
          // ─── Top bar + filter chips ────────────────────────────────
          Container(
            decoration: BoxDecoration(
              color: isDark ? AppColors.surfaceDark : AppColors.surfaceLight,
              border: const Border(
                bottom: BorderSide(color: AppColors.grey100, width: 1),
              ),
            ),
            padding: const EdgeInsets.fromLTRB(16, 6, 16, 12),
            child: Column(children: [
              SearchTopBar(
                query: state.query,
                onBack: () => context.pop(),
                onClear: () {},
              ),
              const SizedBox(height: 12),
              SearchFilterChips(
                city: state.filters.city,
                onOpenFilters: () => showFilterBottomSheet(context),
              ),
            ]),
          ),

          // ─── Results header ────────────────────────────────────────
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 13, 16, 10),
            child: Row(children: [
              Text.rich(TextSpan(
                text: '${state.resultsCount}',
                style: AppTextStyles.bodySmall.copyWith(
                  color: isDark ? Colors.white : AppColors.navy,
                  fontWeight: FontWeight.w700,
                ),
                children: [
                  TextSpan(
                    text: ' نتيجة',
                    style: AppTextStyles.bodySmall.copyWith(
                        color: AppColors.grey600,
                        fontWeight: FontWeight.w600),
                  ),
                ],
              )),
              const Spacer(),
              GestureDetector(
                onTap: notifier.cycleSort,
                child: Row(children: [
                  const Icon(TablerIcons.arrows_sort,
                      size: 16, color: AppColors.primaryDark),
                  const SizedBox(width: 5),
                  Text(state.filters.sort.label,
                      style: AppTextStyles.bodySmall.copyWith(
                          fontSize: 12.5,
                          color: AppColors.primaryDark,
                          fontWeight: FontWeight.w700)),
                ]),
              ),
            ]),
          ),

          // ─── Results grid ──────────────────────────────────────────
          Expanded(
            child: GridView.builder(
              padding: const EdgeInsets.fromLTRB(16, 0, 16, 20),
              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 2,
                mainAxisSpacing: 11,
                crossAxisSpacing: 11,
                childAspectRatio: 0.82,
              ),
              itemCount: state.results.length,
              itemBuilder: (_, i) {
                final listing = state.results[i];
                return ListingCard(
                  listing: listing,
                  width: null,
                  imageHeight: 120,
                  onFavoriteTap: () => notifier.toggleFavorite(listing.id),
                  onTap: () =>
                      context.push(AppRoutes.productDetailsPath(listing.id)),
                );
              },
            ),
          ),
        ]),
      ),
    );
  }
}
