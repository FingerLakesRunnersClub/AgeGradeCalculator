namespace FLRC.AgeGradeCalculator.Generator;

public class DataPoint<T> where T : struct
{
	public Category Category { get; set; }
	public byte Age { get; set; }
	public T Event { get; set; }
	public TimeSpan Record { get; set; }
}