import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:supabase_flutter/supabase_flutter.dart' hide AuthUser;

import '../../../core/auth/mock_session.dart';
import '../../../core/config/env.dart';
import '../../../core/exceptions/app_exception.dart';
import '../../../core/supabase/supabase_providers.dart';
import '../models/auth_user.dart';

abstract class AuthRepository {
  AuthUser? get currentUser;

  Future<AuthUser> signIn({
    required String email,
    required String password,
  });

  Future<AuthUser> signUp({
    required String email,
    required String password,
    required String fullName,
  });

  Future<void> signOut();
}

class MockAuthRepository implements AuthRepository {
  static const _mockUserId = 'mock-user-0001';

  @override
  AuthUser? get currentUser => MockSession.isLoggedIn
      ? AuthUser(
          id: MockSession.userId!,
          email: MockSession.username ?? 'user@mock.local',
          displayName: MockSession.displayName,
        )
      : null;

  @override
  Future<AuthUser> signIn({
    required String email,
    required String password,
  }) async {
    await Future<void>.delayed(const Duration(milliseconds: 700));
    MockSession.start(id: _mockUserId, name: email.trim());
    return currentUser!;
  }

  @override
  Future<AuthUser> signUp({
    required String email,
    required String password,
    required String fullName,
  }) async {
    await Future<void>.delayed(const Duration(milliseconds: 700));
    MockSession.start(
        id: _mockUserId, name: email.trim(), display: fullName.trim());
    return currentUser!;
  }

  @override
  Future<void> signOut() async => MockSession.clear();
}

class SupabaseAuthRepository implements AuthRepository {
  SupabaseAuthRepository(this._db);
  final SupabaseClient _db;

  @override
  AuthUser? get currentUser {
    final u = _db.auth.currentUser;
    if (u == null) return null;
    return AuthUser(
      id: u.id,
      email: u.email ?? '',
      displayName: u.userMetadata?['full_name'] as String?,
    );
  }

  @override
  Future<AuthUser> signIn({
    required String email,
    required String password,
  }) async {
    try {
      final res = await _db.auth.signInWithPassword(
        email: email.trim(),
        password: password,
      );
      if (res.user == null) {
        throw const AuthFailure('تعذّر تسجيل الدخول، حاول مجدداً');
      }
      return currentUser!;
    } on AuthException catch (e) {
      if (e.message.toLowerCase().contains('invalid')) {
        throw const AuthFailure('البريد الإلكتروني أو كلمة المرور غير صحيحة');
      }
      throw mapException(e);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<AuthUser> signUp({
    required String email,
    required String password,
    required String fullName,
  }) async {
    try {
      final res = await _db.auth.signUp(
        email: email.trim(),
        password: password,
        data: {'full_name': fullName.trim()},
      );
      if (res.user == null) {
        throw const AuthFailure('تعذّر إنشاء الحساب، حاول مجدداً');
      }
      return currentUser!;
    } on AuthException catch (e) {
      if (e.message.toLowerCase().contains('already registered')) {
        throw const ValidationError('البريد الإلكتروني مسجَّل مسبقاً');
      }
      throw mapException(e);
    } catch (e) {
      throw mapException(e);
    }
  }

  @override
  Future<void> signOut() async {
    try {
      await _db.auth.signOut();
    } catch (e) {
      throw mapException(e);
    }
  }
}

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  if (!Env.isConfigured) return MockAuthRepository();
  return SupabaseAuthRepository(ref.watch(supabaseClientProvider));
});
