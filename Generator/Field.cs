namespace FLRC.AgeGradeCalculator;

using FieldKey = (Category Category, byte Age, FieldEvent Event);

public static class Field
{
	public static readonly IReadOnlyDictionary<FieldKey, double> Records = new Dictionary<FieldKey, double>
	{
		// age grades will be generated here
	};
}