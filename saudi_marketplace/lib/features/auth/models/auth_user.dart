/// المستخدم المسجَّل حالياً — يكفي للجلسة، التفاصيل الكاملة في ProfileModel
class AuthUser {
  const AuthUser({
    required this.id,
    required this.username,
    this.displayName,
  });

  final String id;
  final String username;
  final String? displayName;

  String get nameOrUsername =>
      (displayName?.trim().isNotEmpty ?? false) ? displayName! : username;
}
