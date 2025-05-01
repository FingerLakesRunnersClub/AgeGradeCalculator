namespace FLRC.AgeGradeCalculator;

using TrackKey = (Category Category, byte Age, TrackEvent Event);

public static class Track
{
	public static readonly IReadOnlyDictionary<TrackKey, double> Records = new Dictionary<TrackKey, double>
	{
		// age grades will be generated here
	};
}