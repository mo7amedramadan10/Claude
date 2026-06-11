import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/config/env.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../data/saved_mock_data.dart';
import '../data/saved_repository.dart';
import '../models/saved_item.dart';

class SavedState {
  const SavedState({
    required this.items,
    this.isLoading = false,
    this.errorMessage,
  });

  final List<SavedItem> items;
  final bool isLoading;
  final String? errorMessage;

  SavedState copyWith({
    List<SavedItem>? items,
    bool? isLoading,
    String? errorMessage,
  }) =>
      SavedState(
        items: items ?? this.items,
        isLoading: isLoading ?? this.isLoading,
        errorMessage: errorMessage,
      );
}

class SavedNotifier extends Notifier<SavedState> {
  @override
  SavedState build() {
    if (Env.isConfigured) {
      Future.microtask(refresh);
      return const SavedState(items: [], isLoading: true);
    }
    return SavedState(items: [...SavedMockData.items]);
  }

  Future<void> refresh() async {
    state = state.copyWith(isLoading: true);
    try {
      final userId = ref.read(currentUserIdProvider);
      final items =
          await ref.read(savedRepositoryProvider).fetchSavedItems(userId);
      state = state.copyWith(items: items, isLoading: false);
    } catch (e) {
      state = state.copyWith(isLoading: false, errorMessage: e.toString());
    }
  }

  void remove(String id) {
    final previous = state.items;
    state = state.copyWith(
      items: previous.where((it) => it.id != id).toList(),
    );

    final userId = ref.read(currentUserIdProvider);
    if (!Env.isConfigured || userId == null) return;

    // تراجع محلي إذا فشل الحذف
    ref.read(savedRepositoryProvider).remove(userId, id).catchError((_) {
      state = state.copyWith(items: previous);
    });
  }
}

final savedProvider = NotifierProvider<SavedNotifier, SavedState>(
  SavedNotifier.new,
);
