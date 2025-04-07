namespace FLRC.AgeGradeCalculator;

public static class Road
{
	public static readonly IReadOnlyDictionary<(Category, byte, double), uint> Records = new Dictionary<(Category, byte, double), uint>
	{
		// age grades will be generated here
	};
}