using Xunit;

namespace FLRC.AgeGradeCalculator.Tests;

public class FieldTests
{
	public static IEnumerable<object[]> FieldEvents => Enum.GetValues<FieldEvent>().Select(e => new object[] { e });

	[Theory]
	[MemberData(nameof(FieldEvents))]
	public void FieldEventIsInRecords(FieldEvent e)
	{
		//act
		var records = Records.Field.Where(r => r.Key.Event == e);

		//assert
		Assert.NotEmpty(records);
	}

	[Fact]
	public void CanGetAgeGradeForWorldRecord()
	{
		//arrange
		const Category category = Category.M;
		const byte age = 20;
		const FieldEvent eventName = FieldEvent.LongJump;
		const double performance = 8.95;

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, eventName, performance);

		//assert
		Assert.Equal(100, ageGrade);
	}

	[Fact]
	public void CanGetAgeGradeForMaxFactor()
	{
		//arrange
		const Category category = Category.M;
		const byte age = 20;
		const FieldEvent eventName = FieldEvent.LongJump;
		const double performance = 8;

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, eventName, performance);

		//assert
		Assert.Equal(89.4, ageGrade, 1);
	}

	[Fact]
	public void CanGetAgeGradeForOtherFactor()
	{
		//arrange
		const Category category = Category.M;
		const byte age = 40;
		const FieldEvent eventName = FieldEvent.LongJump;
		const double performance = 8;

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, eventName, performance);

		//assert
		Assert.Equal(98.0, ageGrade, 1);
	}

	[Fact]
	public void ReturnsNextAgeUpWhenTooYoung()
	{
		//arrange
		const Category category = Category.F;
		const byte age = 4;
		const FieldEvent eventName = FieldEvent.LongJump;
		const double performance = 1.61;

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, eventName, performance);

		//assert
		Assert.Equal(50.2, ageGrade, 1);
	}

	[Fact]
	public void ReturnsNextAgeDownWhenTooOld()
	{
		//arrange
		const Category category = Category.M;
		const byte age = 111;
		const FieldEvent eventName = FieldEvent.LongJump;
		const double performance = 0.23;

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, eventName, performance);

		//assert
		Assert.Equal(51.1, ageGrade, 1);
	}
}