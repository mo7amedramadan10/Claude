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
import '../../chat_list/providers/chat_list_provider.dart';
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
    final unreadMessages = ref.watch(
        chatListProvider.select((s) => s.totalUnread));

    return Scaffold(
      body: Column(
        children: [
          // App bar lives in the body so its height stays intrinsic
          HomeAppBar(
            unreadMessages: unreadMessages,
            hasNotification: true,
            onSearchTap: () => context.push(AppRoutes.search),
            onNotificationTap: () => context.push(AppRoutes.notifications),
            onMessageTap: () => context.push(AppRoutes.chatList),
          ),
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
        const SizedBox(height: 12),
        SizedBox(
          height: 210,
          child: ListView.separated(
            scrollDirection: Axis.horizontal,
            itemCount: listings.length,
            separatorBuilder: (_, __) => const SizedBox(width: 11),
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
            const SizedBox(width: 6),
            const Icon(TablerIcons.map_2, size: 16, color: AppColors.primary),
          ]),
          onViewAll: () {},
        ),
        const SizedBox(height: 12),
        SizedBox(
          height: 190,
          child: ListView.separated(
            scrollDirection: Axis.horizontal,
            itemCount: listings.length,
            separatorBuilder: (_, __) => const SizedBox(width: 11),
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
            const SizedBox(width: 6),
            const Icon(TablerIcons.rosette_discount_check,
                size: 16, color: AppColors.primary),
          ]),
          onViewAll: () {},
        ),
        const SizedBox(height: 12),
        SizedBox(
          height: 160,
          child: ListView.separated(
            scrollDirection: Axis.horizontal,
            itemCount: sellers.length,
            separatorBuilder: (_, __) => const SizedBox(width: 11),
            itemBuilder: (_, i) => HomeSellerCard(seller: sellers[i]),
          ),
        ),
      ]),
    );
  }
}
