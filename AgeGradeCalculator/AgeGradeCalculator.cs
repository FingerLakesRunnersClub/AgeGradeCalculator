namespace FLRC.AgeGradeCalculator;

using RoadKey = (Category Category, byte Age, double Distance);

public static class AgeGradeCalculator
{
	public static readonly double[] Distances = Records.Road.Keys.Select(k => k.Distance).Distinct().OrderBy(d => d).ToArray();

	public static double GetAgeGrade(Category category, byte age, double distance, TimeSpan time)
	{
		if (distance < Distances.First() || age is < 5 or > 100)
			return 0;

		var key = (category, age, distance);
		var best = Distances.Contains(distance)
			? Records.Road[key]
			: Interpolate(Records.Road, key);

		return 100 * best / time.TotalSeconds;
	}

	private static double Interpolate(IReadOnlyDictionary<RoadKey, uint> records, RoadKey key)
	{
		var distance = key.Distance;
		var prev = Distances.Last(d => d <= distance);
		var next = Distances.First(d => d >= distance);
		var factor = (distance - prev) / (next - prev);

		var prevAgeGrade = records[(key.Category, key.Age, prev)];
		var nextAgeGrade = records[(key.Category, key.Age, next)];

		return prevAgeGrade * (1 - factor) + nextAgeGrade * factor;
	}
}