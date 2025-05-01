namespace FLRC.AgeGradeCalculator;

using RoadKey = (Category Category, byte Age, double Distance);

public static class Records
{
	public static readonly IReadOnlyDictionary<RoadKey, uint> Road = FLRC.AgeGradeCalculator.Road.Records;
	public static readonly IReadOnlyDictionary<(Category, byte, double), uint> All = Road;
}