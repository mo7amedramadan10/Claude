class AuthUser {
  const AuthUser({
    required this.id,
    required this.email,
    this.displayName,
  });

  final String id;
  final String email;
  final String? displayName;

  String get nameOrEmail =>
      (displayName?.trim().isNotEmpty ?? false) ? displayName! : email;
}
