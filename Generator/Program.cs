namespace FLRC.AgeGradeCalculator.Generator;

public static class Program
{
	public static async Task Main()
	{
		await RoadGenerator.Run();
		await TrackGenerator.Run();
		await FieldGenerator.Run();
	}
}