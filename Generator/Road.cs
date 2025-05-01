namespace FLRC.AgeGradeCalculator;

using RoadKey = (Category Category, byte Age, double Distance);

public static class Road
{
	public static readonly IReadOnlyDictionary<RoadKey, uint> Records = new Dictionary<RoadKey, uint>
	{
		// age grades will be generated here
	};
}