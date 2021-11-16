namespace FLRC.AgeGradeCalculator;

public static class AgeGradeCalculator
{
	public static readonly IList<double> Distances = new[] { 1609.344, 5000, 6000, 6437.376, 8000, 8046.72, 10000, 12000, 15000, 16093.44, 20000, 21097.5, 25000, 30000, 42195, 50000, 80467.2, 100000, 150000, 160934.4, 200000 };

	public static double GetAgeGrade(Category category, byte age, double distance, TimeSpan time)
	{
		if (distance < Distances.First() || age < 5 || age > 100)
			return 0;

		var identifier = new Identifier(category, age, distance);
		var best = Distances.Contains(distance)
			? Records.All[identifier]
			: Interpolate(identifier);

		return 100 * best / time.TotalSeconds;
	}

	private static double Interpolate(Identifier identifier)
	{
		var distance = identifier.Distance;
		var prev = Distances.Last(d => d <= distance);
		var next = Distances.First(d => d >= distance);
		var factor = (distance - prev) / (next - prev);

		var prevAgeGrade = Records.All[new Identifier(identifier.Category, identifier.Age, prev)];
		var nextAgeGrade = Records.All[new Identifier(identifier.Category, identifier.Age, next)];

		return prevAgeGrade * (1 - factor) + nextAgeGrade * factor;
	}
}