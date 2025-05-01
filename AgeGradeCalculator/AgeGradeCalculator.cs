namespace FLRC.AgeGradeCalculator;

using RoadKey = (Category Category, byte Age, double Distance);
using TrackKey = (Category Category, byte Age, TrackEvent Event);

public static class AgeGradeCalculator
{
	public static readonly double[] RoadDistances = [1609.344, 5000, 6000, 6437.376, 8000, 8046.72, 10000, 12000, 15000, 16093.44, 20000, 21097.5, 25000, 30000, 42195, 50000, 80467.2, 100000, 150000, 160934.4, 200000];

	public static double GetAgeGrade(Category category, byte age, double distance, TimeSpan time)
	{
		if (distance < RoadDistances.First() || distance > RoadDistances.Last() || age is < 5 or > 100)
			return 0;

		var key = (category, age, distance);
		var best = RoadDistances.Contains(distance)
			? Records.Road[key]
			: Interpolate(Records.Road, key);

		return 100 * best / time.TotalSeconds;
	}

	private static double Interpolate(IReadOnlyDictionary<RoadKey, uint> records, RoadKey key)
	{
		var prev = RoadDistances.Last(d => d <= key.Distance);
		var next = RoadDistances.First(d => d >= key.Distance);
		var factor = (key.Distance - prev) / (next - prev);

		var prevAgeGrade = records[(key.Category, key.Age, prev)];
		var nextAgeGrade = records[(key.Category, key.Age, next)];

		return prevAgeGrade * (1 - factor) + nextAgeGrade * factor;
	}


	public static double GetAgeGrade(Category category, byte age, TrackEvent eventName, TimeSpan time)
	{
		var key = (category, age, eventName);
		var best = Records.Track.TryGetValue(key, out var match)
			? match
			: Interpolate(Records.Track, key);

		return 100 * best / time.TotalSeconds;
	}

	private static double Interpolate(IReadOnlyDictionary<TrackKey, double> records, TrackKey key)
	{
		const byte pivotAge = 25;
		var eventRecords = records.Where(r => r.Key.Category == key.Category && r.Key.Event == key.Event).OrderBy(r => r.Key.Age).ToArray();
		var closest = key.Age > pivotAge
			? eventRecords.Last(r => r.Key.Age <= key.Age)
			: eventRecords.First(r => r.Key.Age >= key.Age);

		return closest.Value;
	}
}