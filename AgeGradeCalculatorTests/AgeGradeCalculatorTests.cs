using System;
using Xunit;

namespace FLRC.AgeGradeCalculator.Tests
{
    public class AgeGradeCalculatorTests
    {
        [Fact]
        public void CanGetAgeGradeForWorldRecordAtStandardDistance()
        {
            //arrange
            const Category category = Category.M;
            const byte age = 18;
            const double distance = 1609.344;
            var time = TimeSpan.Parse("0:03:47");

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
            Assert.Equal(89.0, ageGrade, 1);
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
            Assert.Equal(88.5, ageGrade, 1);
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
            Assert.Equal(95.4, ageGrade, 1);
        }
    }
}