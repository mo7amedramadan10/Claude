import 'package:supabase_flutter/supabase_flutter.dart';

sealed class AppException implements Exception {
  const AppException(this.message);
  final String message;

  @override
  String toString() => message;
}

final class NetworkFailure extends AppException {
  const NetworkFailure([super.message = 'تحقق من الاتصال بالإنترنت']);
}

final class AuthFailure extends AppException {
  const AuthFailure(super.message);
}

final class SessionExpired extends AppException {
  const SessionExpired([super.message = 'انتهت الجلسة، يرجى تسجيل الدخول مجدداً']);
}

final class PermissionDenied extends AppException {
  const PermissionDenied([super.message = 'غير مصرح بهذا الإجراء']);
}

final class NotFound extends AppException {
  const NotFound([super.message = 'العنصر غير موجود']);
}

final class ValidationError extends AppException {
  const ValidationError(super.message);
}

final class StorageFailure extends AppException {
  const StorageFailure(super.message);
}

final class UnknownError extends AppException {
  const UnknownError([super.message = 'حدث خطأ غير متوقع']);
}

AppException mapException(Object error) {
  if (error is PostgrestException) {
    return switch (error.code) {
      '42501'    => const PermissionDenied(),
      'PGRST116' => const NotFound(),
      '23505'    => const ValidationError('هذا العنصر موجود مسبقاً'),
      '23514'    => ValidationError(error.message),
      _          => ValidationError(error.message),
    };
  }
  if (error is AuthException) {
    final msg = error.message.toLowerCase();
    if (msg.contains('jwt') || msg.contains('session')) {
      return const SessionExpired();
    }
    return AuthFailure(error.message);
  }
  if (error is StorageException) {
    return StorageFailure(error.message);
  }
  final msg = error.toString().toLowerCase();
  if (msg.contains('network') ||
      msg.contains('socket') ||
      msg.contains('connection') ||
      msg.contains('unreachable')) {
    return const NetworkFailure();
  }
  return const UnknownError();
}
