namespace FLRC.AgeGradeCalculator;

public static class Track
{
	public static readonly IReadOnlyDictionary<(Category, byte, TrackEvent), double> Records = new Dictionary<(Category, byte, TrackEvent), double>
	{
		// age grades will be generated here
	};
}