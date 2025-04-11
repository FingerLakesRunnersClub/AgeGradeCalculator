namespace FLRC.AgeGradeCalculator;

public static class Records
{
	public static readonly IReadOnlyDictionary<(Category, byte, double), uint> Road = FLRC.AgeGradeCalculator.Road.Records;
	public static readonly IReadOnlyDictionary<(Category, byte, TrackEvent), double> Track = FLRC.AgeGradeCalculator.Track.Records;
}