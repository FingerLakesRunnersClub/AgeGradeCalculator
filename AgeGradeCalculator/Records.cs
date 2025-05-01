namespace FLRC.AgeGradeCalculator;

using RoadKey = (Category Category, byte Age, double Distance);
using TrackKey = (Category Category, byte Age, TrackEvent Event);

public static class Records
{
	public static readonly IReadOnlyDictionary<RoadKey, uint> Road = FLRC.AgeGradeCalculator.Road.Records;
	public static readonly IReadOnlyDictionary<TrackKey, double> Track = FLRC.AgeGradeCalculator.Track.Records;
}