enum SortOption {
  newest('الأحدث'),
  priceLowest('الأقل سعرًا'),
  priceHighest('الأعلى سعرًا');

  const SortOption(this.label);

  final String label;

  SortOption get next =>
      SortOption.values[(index + 1) % SortOption.values.length];
}

class SearchFilters {
  const SearchFilters({
    this.city = 'الرياض',
    this.priceRange,
    this.condition,
    this.sort = SortOption.newest,
  });

  static const cities = ['الكل', 'الرياض', 'جدة', 'الدمام', 'مكة'];
  static const priceRanges = ['أقل من ٥٠ ألف', '٥٠–١٥٠ ألف', 'أكثر من ١٥٠ ألف'];
  static const conditions = ['ممتازة', 'جيدة جدًا', 'جيدة'];

  final String city;
  final String? priceRange;
  final String? condition;
  final SortOption sort;

  SearchFilters copyWith({
    String? city,
    String? Function()? priceRange,
    String? Function()? condition,
    SortOption? sort,
  }) =>
      SearchFilters(
        city: city ?? this.city,
        priceRange: priceRange != null ? priceRange() : this.priceRange,
        condition: condition != null ? condition() : this.condition,
        sort: sort ?? this.sort,
      );

  SearchFilters reset() => const SearchFilters(city: 'الكل');
}
