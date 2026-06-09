import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';
import 'package:go_router/go_router.dart';

import '../../../core/router/app_routes.dart';
import '../../../core/shared/widgets/listing_card.dart';
import '../../../core/shared/widgets/section_header.dart';
import '../../../core/theme/app_colors.dart';
import '../data/home_mock_data.dart';
import '../models/listing_model.dart';
import '../models/seller_model.dart';
import '../providers/home_provider.dart';
import 'widgets/home_ai_banner.dart';
import 'widgets/home_app_bar.dart';
import 'widgets/home_categories.dart';
import 'widgets/home_nearby_card.dart';
import 'widgets/home_seller_card.dart';

class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(homeProvider);
    final notifier = ref.read(homeProvider.notifier);

    return Scaffold(
      appBar: HomeAppBar(
        unreadMessages: 3,
        hasNotification: true,
        onSearchTap: () => context.push(AppRoutes.search),
        onNotificationTap: () => context.push(AppRoutes.notifications),
        onMessageTap: () => context.push(AppRoutes.chatList),
      ),
      body: Column(
        children: [
          HomeCategories(
            selectedIndex: state.selectedCategoryIndex,
            onCategoryTap: notifier.selectCategory,
          ),
          Expanded(
            child: ListView(
              padding: const EdgeInsets.only(bottom: 24),
              children: [
                HomeAiBanner(onTap: () => context.push(AppRoutes.search)),
                const SizedBox(height: 16),
                _ListingsSection(
                  title: 'إعلانات مميزة',
                  listings: state.featuredListings,
                  onFavorite: notifier.toggleFavorite,
                  onTap: (id) => context.push(AppRoutes.productDetailsPath(id)),
                ),
                const SizedBox(height: 16),
                _NearbySection(
                  listings: state.nearbyListings,
                  onTap: (id) => context.push(AppRoutes.productDetailsPath(id)),
                ),
                const SizedBox(height: 16),
                _ListingsSection(
                  title: 'مقترحة لك',
                  titleSuffix: const Icon(
                    TablerIcons.sparkles,
                    size: 14,
                    color: AppColors.ai,
                  ),
                  listings: state.suggestedListings,
                  onFavorite: notifier.toggleFavorite,
                  onTap: (id) => context.push(AppRoutes.productDetailsPath(id)),
                ),
                const SizedBox(height: 16),
                _SellersSection(sellers: HomeMockData.sellers),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

// ─── Sections ────────────────────────────────────────────────────────────────

class _ListingsSection extends StatelessWidget {
  const _ListingsSection({
    required this.title,
    required this.listings,
    required this.onFavorite,
    required this.onTap,
    this.titleSuffix,
  });

  final String title;
  final List<ListingModel> listings;
  final ValueChanged<String> onFavorite;
  final ValueChanged<String> onTap;
  final Widget? titleSuffix;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        SectionHeader(
          title: Row(mainAxisSize: MainAxisSize.min, children: [
            Text(title),
            if (titleSuffix != null) ...[
              const SizedBox(width: 5),
              titleSuffix!,
            ],
          ]),
          onViewAll: () {},
        ),
        const SizedBox(height: 10),
        SizedBox(
          height: 220,
          child: ListView.separated(
            scrollDirection: Axis.horizontal,
            itemCount: listings.length,
            separatorBuilder: (_, __) => const SizedBox(width: 10),
            itemBuilder: (_, i) => ListingCard(
              listing: listings[i],
              onFavoriteTap: () => onFavorite(listings[i].id),
              onTap: () => onTap(listings[i].id),
            ),
          ),
        ),
      ]),
    );
  }
}

class _NearbySection extends StatelessWidget {
  const _NearbySection({required this.listings, required this.onTap});

  final List<ListingModel> listings;
  final ValueChanged<String> onTap;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        SectionHeader(
          title: Row(mainAxisSize: MainAxisSize.min, children: [
            const Text('قريب منك'),
            const SizedBox(width: 5),
            const Icon(TablerIcons.map_2, size: 14, color: AppColors.primary),
          ]),
          onViewAll: () {},
        ),
        const SizedBox(height: 10),
        SizedBox(
          height: 178,
          child: ListView.separated(
            scrollDirection: Axis.horizontal,
            itemCount: listings.length,
            separatorBuilder: (_, __) => const SizedBox(width: 10),
            itemBuilder: (_, i) => HomeNearbyCard(
              listing: listings[i],
              onTap: () => onTap(listings[i].id),
            ),
          ),
        ),
      ]),
    );
  }
}

class _SellersSection extends StatelessWidget {
  const _SellersSection({required this.sellers});

  final List<SellerModel> sellers;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        SectionHeader(
          title: Row(mainAxisSize: MainAxisSize.min, children: [
            const Text('البائعون الموثقون'),
            const SizedBox(width: 5),
            const Icon(TablerIcons.shield_check,
                size: 14, color: AppColors.primary),
          ]),
          onViewAll: () {},
        ),
        const SizedBox(height: 10),
        SizedBox(
          height: 168,
          child: ListView.separated(
            scrollDirection: Axis.horizontal,
            itemCount: sellers.length,
            separatorBuilder: (_, __) => const SizedBox(width: 10),
            itemBuilder: (_, i) => HomeSellerCard(seller: sellers[i]),
          ),
        ),
      ]),
    );
  }
}
