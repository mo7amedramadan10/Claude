import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../data/saved_mock_data.dart';
import '../models/saved_item.dart';

class SavedState {
  const SavedState({required this.items});
  final List<SavedItem> items;

  SavedState copyWith({List<SavedItem>? items}) =>
      SavedState(items: items ?? this.items);
}

class SavedNotifier extends Notifier<SavedState> {
  @override
  SavedState build() => SavedState(items: [...SavedMockData.items]);

  void remove(String id) {
    state = state.copyWith(
      items: state.items.where((it) => it.id != id).toList(),
    );
  }
}

final savedProvider = NotifierProvider<SavedNotifier, SavedState>(
  SavedNotifier.new,
);
