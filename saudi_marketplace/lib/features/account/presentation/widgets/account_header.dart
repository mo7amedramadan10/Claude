import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_tabler_icons/flutter_tabler_icons.dart';

import '../../../../core/theme/app_colors.dart';
import '../../../../core/theme/app_text_styles.dart';
import '../../data/account_mock_data.dart';

class AccountHeader extends StatelessWidget {
  const AccountHeader({super.key});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.fromLTRB(20, 8, 20, 46),
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topRight,
          end: Alignment.bottomLeft,
          colors: [AppColors.navy, AppColors.navyLight],
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // ─── Title row ────────────────────────────────────────
          Row(
            children: [
              Text(
                'حسابي',
                style: AppTextStyles.headlineLarge.copyWith(
                  fontSize: 18,
                  color: Colors.white,
                ),
              ),
              const Spacer(),
              Container(
                width: 36,
                height: 36,
                decoration: BoxDecoration(
                  color: Colors.white.withOpacity(0.12),
                  shape: BoxShape.circle,
                ),
                child: const Icon(TablerIcons.settings,
                    size: 20, color: Colors.white),
              ),
            ],
          ),
          const SizedBox(height: 14),

          // ─── Profile row ──────────────────────────────────────
          Row(
            children: [
              Container(
                width: 72,
                height: 72,
                decoration: BoxDecoration(
                  shape: BoxShape.circle,
                  border: Border.all(
                    color: AppColors.aiSparkle.withOpacity(0.6),
                    width: 3,
                  ),
                ),
                clipBehavior: Clip.hardEdge,
                child: CachedNetworkImage(
                  imageUrl: AccountMockData.avatarUrl,
                  fit: BoxFit.cover,
                  placeholder: (_, __) =>
                      Container(color: AppColors.navyLight),
                  errorWidget: (_, __, ___) =>
                      Container(color: AppColors.navyLight),
                ),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      AccountMockData.userName,
                      style: AppTextStyles.headlineLarge.copyWith(
                        fontSize: 19,
                        color: Colors.white,
                      ),
                    ),
                    const SizedBox(height: 6),
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 10, vertical: 4),
                      decoration: BoxDecoration(
                        color: AppColors.aiSparkle.withOpacity(0.18),
                        borderRadius: BorderRadius.circular(20),
                        border: Border.all(
                          color: AppColors.aiSparkle.withOpacity(0.35),
                          width: 1,
                        ),
                      ),
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          const Icon(
                            TablerIcons.rosette_discount_check_filled,
                            size: 14,
                            color: AppColors.aiSparkle,
                          ),
                          const SizedBox(width: 5),
                          Text(
                            'موثّق عبر نفاذ وطني',
                            style: AppTextStyles.labelLarge.copyWith(
                              fontSize: 11.5,
                              fontWeight: FontWeight.w700,
                              color: AppColors.aiSparkle,
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 8),
                    Text(
                      AccountMockData.memberSince,
                      style: AppTextStyles.labelLarge.copyWith(
                        fontSize: 12,
                        color: const Color(0xFFA8C0D6),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
