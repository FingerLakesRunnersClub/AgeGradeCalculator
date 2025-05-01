using Xunit;

namespace FLRC.AgeGradeCalculator.Tests;

public class TrackTests
{
	public static IEnumerable<object[]> TrackEvents => Enum.GetValues<TrackEvent>().Select(e => new object[] { e });

	[Theory]
	[MemberData(nameof(TrackEvents))]
	public void TrackEventIsInRecords(TrackEvent e)
	{
		//act
		var records = Records.Track.Where(r => r.Key.Event == e);

		//assert
		Assert.NotEmpty(records);
	}

	[Fact]
	public void CanGetAgeGradeForWorldRecord()
	{
		//arrange
		const Category category = Category.M;
		const byte age = 20;
		const TrackEvent eventName = TrackEvent._1mi;
		var time = TimeSpan.Parse("0:03:43.13");

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, eventName, time);

		//assert
		Assert.Equal(100, ageGrade);
	}

	[Fact]
	public void CanGetAgeGradeForMaxFactor()
	{
		//arrange
		const Category category = Category.M;
		const byte age = 20;
		const TrackEvent eventName = TrackEvent._1mi;
		var time = TimeSpan.Parse("0:04:15");

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, eventName, time);

		//assert
		Assert.Equal(87.5, ageGrade, 1);
	}

	[Fact]
	public void CanGetAgeGradeForOtherFactor()
	{
		//arrange
		const Category category = Category.M;
		const byte age = 40;
		const TrackEvent eventName = TrackEvent._1mi;
		var time = TimeSpan.Parse("0:04:30");

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, eventName, time);

		//assert
		Assert.Equal(86.9, ageGrade, 1);
	}

	[Fact]
	public void ReturnsNextAgeUpWhenTooYoung()
	{
		//arrange
		const Category category = Category.F;
		const byte age = 4;
		const TrackEvent eventName = TrackEvent._3000m;
		var time = TimeSpan.Parse("0:28:17.2");

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, eventName, time);

		//assert
		Assert.Equal(50, ageGrade);
	}

	[Fact]
	public void ReturnsNextAgeDownWhenTooOld()
	{
		//arrange
		const Category category = Category.M;
		const byte age = 31;
		const TrackEvent eventName = TrackEvent._50m;
		var time = TimeSpan.Parse("0:00:11.12");

		//act
		var ageGrade = AgeGradeCalculator.GetAgeGrade(category, age, eventName, time);

		//assert
		Assert.Equal(50, ageGrade);
	}
}