import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../data/auth_repository.dart';
import '../models/auth_user.dart';

class AuthSessionState {
  const AuthSessionState({
    this.user,
    this.isLoading = false,
    this.errorMessage,
  });

  final AuthUser? user;
  final bool isLoading;
  final String? errorMessage;

  bool get isLoggedIn => user != null;

  AuthSessionState copyWith({
    AuthUser? user,
    bool? isLoading,
    String? errorMessage,
  }) =>
      AuthSessionState(
        user: user ?? this.user,
        isLoading: isLoading ?? this.isLoading,
        errorMessage: errorMessage,
      );
}

class AuthNotifier extends Notifier<AuthSessionState> {
  @override
  AuthSessionState build() {
    // استعادة الجلسة المحفوظة (Supabase يخزّنها محلياً)
    return AuthSessionState(
      user: ref.read(authRepositoryProvider).currentUser,
    );
  }

  Future<bool> signIn(String email, String password) async {
    state = state.copyWith(isLoading: true);
    try {
      final user = await ref
          .read(authRepositoryProvider)
          .signIn(email: email, password: password);
      state = AuthSessionState(user: user);
      _refreshUserScopedProviders();
      return true;
    } on AppException catch (e) {
      state = AuthSessionState(errorMessage: e.message);
      return false;
    } catch (_) {
      state = const AuthSessionState(errorMessage: 'حدث خطأ غير متوقع');
      return false;
    }
  }

  Future<bool> signUp({
    required String email,
    required String password,
    required String fullName,
  }) async {
    state = state.copyWith(isLoading: true);
    try {
      final user = await ref.read(authRepositoryProvider).signUp(
            email: email,
            password: password,
            fullName: fullName,
          );
      state = AuthSessionState(user: user);
      _refreshUserScopedProviders();
      return true;
    } on AppException catch (e) {
      state = AuthSessionState(errorMessage: e.message);
      return false;
    } catch (_) {
      state = const AuthSessionState(errorMessage: 'حدث خطأ غير متوقع');
      return false;
    }
  }

  Future<void> signOut() async {
    try {
      await ref.read(authRepositoryProvider).signOut();
    } finally {
      state = const AuthSessionState();
      _refreshUserScopedProviders();
    }
  }

  void clearError() => state = state.copyWith();

  /// المزوّدات المرتبطة بهوية المستخدم تُعاد قراءتها بعد تغيّر الجلسة
  void _refreshUserScopedProviders() {
    ref.invalidate(currentUserIdProvider);
  }
}

final authProvider =
    NotifierProvider<AuthNotifier, AuthSessionState>(AuthNotifier.new);
