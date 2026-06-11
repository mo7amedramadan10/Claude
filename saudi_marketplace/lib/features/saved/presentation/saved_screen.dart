import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_text_styles.dart';
import '../../../core/utils/number_formatter.dart';
import '../providers/saved_provider.dart';
import 'widgets/saved_card.dart';
import 'widgets/saved_empty_state.dart';

class SavedScreen extends ConsumerWidget {
  const SavedScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(savedProvider);
    final isDark = Theme.of(context).brightness == Brightness.dark;
    final items = state.items;

    return Scaffold(
      body: SafeArea(
        child: Column(children: [
          // ─── Header ─────────────────────────────────────────────
          Container(
            width: double.infinity,
            padding: const EdgeInsets.fromLTRB(16, 6, 16, 14),
            decoration: BoxDecoration(
              color: isDark ? AppColors.surfaceDark : Colors.white,
              border: const Border(
                bottom: BorderSide(color: AppColors.grey100, width: 1),
              ),
            ),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Text(
                  'المحفوظات',
                  style: AppTextStyles.displayMedium.copyWith(
                    fontSize: 21,
                    color: isDark ? Colors.white : AppColors.navy,
                  ),
                ),
                const Spacer(),
                if (items.isNotEmpty)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 3),
                    child: Text(
                      '${NumberFormatter.toArabicDigits(items.length)} إعلان محفوظ',
                      style: AppTextStyles.labelLarge.copyWith(
                        fontSize: 12.5,
                        fontWeight: FontWeight.w600,
                        color: AppColors.grey600,
                      ),
                    ),
                  ),
              ],
            ),
          ),

          // ─── Body ───────────────────────────────────────────────
          Expanded(
            child: state.isLoading && items.isEmpty
                ? const Center(
                    child:
                        CircularProgressIndicator(color: AppColors.primary),
                  )
                : items.isNotEmpty
                    ? RefreshIndicator(
                        onRefresh:
                            ref.read(savedProvider.notifier).refresh,
                        child: GridView.builder(
                          physics:
                              const AlwaysScrollableScrollPhysics(),
                          padding:
                              const EdgeInsets.fromLTRB(16, 16, 16, 20),
                          gridDelegate:
                              const SliverGridDelegateWithFixedCrossAxisCount(
                            crossAxisCount: 2,
                            mainAxisSpacing: 11,
                            crossAxisSpacing: 11,
                            mainAxisExtent: 210,
                          ),
                          itemCount: items.length,
                          itemBuilder: (_, i) {
                            final item = items[i];
                            return SavedCard(
                              item: item,
                              onTap: () => context.push(
                                  AppRoutes.productDetailsPath(item.id)),
                              onRemove: () => ref
                                  .read(savedProvider.notifier)
                                  .remove(item.id),
                            );
                          },
                        ),
                      )
                    : SavedEmptyState(
                        onBrowse: () => context.go(AppRoutes.home),
                      ),
          ),
        ]),
      ),
    );
  }
}
