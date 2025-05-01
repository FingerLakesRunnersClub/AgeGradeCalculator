using Xunit;

namespace FLRC.AgeGradeCalculator.Tests;

public class AgeGradeCalculatorTests
{
	public static IEnumerable<object[]> RoadDistances => AgeGradeCalculator.RoadDistances.Select(d => new object[] { d });

	[Theory]
	[MemberData(nameof(RoadDistances))]
	public void RoadDistanceIsInRecords(double distance)
	{
		//act
		var key = (Category.F, (byte)18, distance);

		//assert
		Assert.Contains(key, Records.Road.Keys);
	}

	[Fact]
	public void CanGetAgeGradeForWorldRecordAtStandardDistance()
	{
		//arrange
		const Category category = Category.M;
		const byte age = 20;
		const double distance = 1609.344;
		var time = TimeSpan.Parse("0:03:52");

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, distance, time);

		//assert
		Assert.Equal(100, ageGrade);
	}

	[Fact]
	public void CanGetAgeGradeForStandardDistanceAtMaxFactor()
	{
		//arrange
		const Category category = Category.M;
		const byte age = 20;
		const double distance = 1609.344;
		var time = TimeSpan.Parse("0:04:15");

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, distance, time);

		//assert
		Assert.Equal(91.0, ageGrade, 1);
	}

	[Fact]
	public void CanGetAgeGradeForStandardDistance()
	{
		//arrange
		const Category category = Category.M;
		const byte age = 40;
		const double distance = 1609.344;
		var time = TimeSpan.Parse("0:04:30");

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, distance, time);

		//assert
		Assert.Equal(89.3, ageGrade, 1);
	}

	[Fact]
	public void CanGetInterpolatedAgeGrade()
	{
		//arrange
		const Category category = Category.F;
		const byte age = 50;
		const double distance = 3000;
		var time = TimeSpan.Parse("0:10:00");

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, distance, time);

		//assert
		Assert.Equal(94.1, ageGrade, 1);
	}

	[Fact]
	public void ReturnsZeroWhenTooYoung()
	{
		//arrange
		const Category category = Category.F;
		const byte age = 4;
		const double distance = 3000;
		var time = TimeSpan.Parse("0:10:00");

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, distance, time);

		//assert
		Assert.Equal(0, ageGrade);
	}

	[Fact]
	public void ReturnsZeroWhenTooOld()
	{
		//arrange
		const Category category = Category.F;
		const byte age = 101;
		const double distance = 3000;
		var time = TimeSpan.Parse("0:10:00");

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, distance, time);

		//assert
		Assert.Equal(0, ageGrade);
	}

	[Fact]
	public void ReturnsZeroWhenTooShort()
	{
		//arrange
		const Category category = Category.F;
		const byte age = 18;
		const double distance = 1500;
		var time = TimeSpan.Parse("0:04:00");

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, distance, time);

		//assert
		Assert.Equal(0, ageGrade);
	}

	[Fact]
	public void ReturnsZeroWhenTooLong()
	{
		//arrange
		const Category category = Category.F;
		const byte age = 18;
		const double distance = 250000;
		var time = TimeSpan.FromHours(25);

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, distance, time);

		//assert
		Assert.Equal(0, ageGrade);
	}
}