using System;
using FLRC.AgeGradeCalculator;

namespace Generator
{
    public class DataPoint
    {
        public Category Category { get; set; }
        public byte Age { get; set; }
        public double Distance { get; set; }
        public TimeSpan Record { get; set; }
    }
}
