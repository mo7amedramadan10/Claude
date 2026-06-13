import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:supabase_flutter/supabase_flutter.dart';

import '../auth/mock_session.dart';
import '../config/env.dart';

final supabaseClientProvider = Provider<SupabaseClient>((ref) {
  return Supabase.instance.client;
});

/// معرّف المستخدم الحالي — من Supabase عند التهيئة،
/// ومن الجلسة التجريبية في وضع الـ mock.
/// يراقب authStateProvider ليتحدّث تلقائياً عند الدخول/الخروج.
final currentUserIdProvider = Provider<String?>((ref) {
  if (!Env.isConfigured) return MockSession.userId;
  ref.watch(authStateProvider); // يعيد الحساب عند كل تغيّر في حالة الجلسة
  return Supabase.instance.client.auth.currentUser?.id;
});

final authStateProvider = StreamProvider<AuthState>((ref) {
  return Supabase.instance.client.auth.onAuthStateChange;
});
