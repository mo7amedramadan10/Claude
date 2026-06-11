/// تنسيق الوقت النسبي بالعربية: «منذ ساعتين»، «منذ ٣ أيام» ...
abstract final class RelativeTime {
  static String format(DateTime time) {
    final diff = DateTime.now().difference(time);

    if (diff.inMinutes < 1) return 'الآن';
    if (diff.inMinutes < 60) return 'منذ ${_unit(diff.inMinutes, 'دقيقة', 'دقيقتين', 'دقائق')}';
    if (diff.inHours < 24) return 'منذ ${_unit(diff.inHours, 'ساعة', 'ساعتين', 'ساعات')}';
    if (diff.inDays < 7) return 'منذ ${_unit(diff.inDays, 'يوم', 'يومين', 'أيام')}';
    if (diff.inDays < 30) return 'منذ ${_unit(diff.inDays ~/ 7, 'أسبوع', 'أسبوعين', 'أسابيع')}';
    if (diff.inDays < 365) return 'منذ ${_unit(diff.inDays ~/ 30, 'شهر', 'شهرين', 'أشهر')}';
    return 'منذ ${_unit(diff.inDays ~/ 365, 'سنة', 'سنتين', 'سنوات')}';
  }

  /// صيغة قصيرة لقوائم المحادثات: «الآن»، «٥ د»، «٣ س»، «أمس»، «٤ ي»
  static String short(DateTime time) {
    final diff = DateTime.now().difference(time);

    if (diff.inMinutes < 1) return 'الآن';
    if (diff.inMinutes < 60) return '${_arabicDigits(diff.inMinutes)} د';
    if (diff.inHours < 24) return '${_arabicDigits(diff.inHours)} س';
    if (diff.inDays < 2) return 'أمس';
    if (diff.inDays < 7) return '${_arabicDigits(diff.inDays)} ي';
    return '${_arabicDigits(diff.inDays ~/ 7)} أ';
  }

  static String _unit(int n, String one, String two, String plural) {
    if (n <= 1) return one;
    if (n == 2) return two;
    if (n <= 10) return '${_arabicDigits(n)} $plural';
    return '${_arabicDigits(n)} $one';
  }

  static String _arabicDigits(int n) {
    const western = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
    const eastern = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
    var s = '$n';
    for (var i = 0; i < western.length; i++) {
      s = s.replaceAll(western[i], eastern[i]);
    }
    return s;
  }
}
