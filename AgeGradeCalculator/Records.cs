namespace FLRC.AgeGradeCalculator;

using RoadKey = (Category Category, byte Age, double Distance);
using TrackKey = (Category Category, byte Age, TrackEvent Event);
using FieldKey = (Category Category, byte Age, FieldEvent Event);

public static class Records
{
	public static readonly IReadOnlyDictionary<RoadKey, uint> Road = FLRC.AgeGradeCalculator.Road.Records;
	public static readonly IReadOnlyDictionary<TrackKey, double> Track = FLRC.AgeGradeCalculator.Track.Records;
	public static readonly IReadOnlyDictionary<FieldKey, double> Field = FLRC.AgeGradeCalculator.Field.Records;
}