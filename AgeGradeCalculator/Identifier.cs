using System;

namespace FLRC.AgeGradeCalculator
{
    public class Identifier : Tuple<Category?, byte, double>
    {
        public Category? Category => Item1;
        public byte Age => Item2;
        public double Distance => Item3;

        public Identifier(Category? category, byte age, double distance) : base(category, age, distance)
        {
        }
    }
}
