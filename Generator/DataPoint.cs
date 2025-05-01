namespace FLRC.AgeGradeCalculator.Generator;

public class DataPoint<TEvent, TRecord>
	where TEvent : struct
	where TRecord : struct
{
	public Category Category { get; set; }
	public byte Age { get; set; }
	public TEvent Event { get; set; }
	public TRecord Record { get; set; }
}