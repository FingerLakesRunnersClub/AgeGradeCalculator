using System;
using System.Threading.Tasks;
using Xunit;

namespace FLRC.AgeGradeCalculator.Tests
{
    public class AgeGradeCalculatorTests
    {
        [Fact]
        public async Task CanGetAgeGradeForWorldRecordAtStandardDistance()
        {
            //arrange
            var calculator = new AgeGradeCalculator(await Loader.Load());

            //act
            var ageGrade = calculator.GetAgeGrade(Category.M, 18, 1609.344, TimeSpan.Parse("0:03:47"));

            //assert
            Assert.Equal(100, ageGrade);
        }

        [Fact]
        public async Task CanGetAgeGradeForStandardDistanceAtMaxFactor()
        {
            //arrange
            var calculator = new AgeGradeCalculator(await Loader.Load());

            //act
            var ageGrade = calculator.GetAgeGrade(Category.M, 20, 1609.344, TimeSpan.Parse("0:04:15"));

            //assert
            Assert.Equal(89.0, ageGrade, 1);
        }

        [Fact]
        public async Task CanGetAgeGradeForStandardDistance()
        {
            //arrange
            var calculator = new AgeGradeCalculator(await Loader.Load());

            //act
            var ageGrade = calculator.GetAgeGrade(Category.M, 40, 1609.344, TimeSpan.Parse("0:04:30"));

            //assert
            Assert.Equal(88.5, ageGrade, 1);
        }

        [Fact]
        public async Task CanGetInterpolatedAgeGrade()
        {
            //arrange
            var calculator = new AgeGradeCalculator(await Loader.Load());

            //act
            var ageGrade = calculator.GetAgeGrade(Category.F, 50, 3000, TimeSpan.Parse("0:10:00"));

            //assert
            Assert.Equal(95.4, ageGrade, 1);
        }
    }
}