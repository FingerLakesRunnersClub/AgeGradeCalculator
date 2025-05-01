using System.Text.RegularExpressions;

namespace FLRC.AgeGradeCalculator.Generator;

public static partial class Patterns
{
	[GeneratedRegex(@" (\d+)")]
	public static partial Regex ExtraSpaceInDistance();
}