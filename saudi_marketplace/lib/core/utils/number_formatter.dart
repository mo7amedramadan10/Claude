abstract final class NumberFormatter {
  static const _arabicDigits = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];

  static String formatPrice(double price) {
    return price
        .toStringAsFixed(0)
        .replaceAllMapped(
          RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'),
          (m) => '${m[1]},',
        );
  }

  static String toArabicDigits(int number) => number
      .toString()
      .split('')
      .map((c) => _arabicDigits[int.parse(c)])
      .join();
}
