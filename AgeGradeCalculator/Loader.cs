using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FLRC.AgeGradeCalculator
{
    public static class Loader
    {
        public static async Task<IDictionary<Identifier, uint>> Load()
        {
            var ageGrades = new Dictionary<Identifier, uint>();

            foreach (Category category in Enum.GetValues(typeof(Category)))
            {
                var lines = await File.ReadAllLinesAsync($"{category}.csv");
                foreach (var line in lines)
                {
                    var fields = line.Split(',');
                    for (var x = 1; x <= AgeGradeCalculator.StandardDistances.Count; x++)
                        ageGrades[new Identifier(category, byte.Parse(fields[0]), AgeGradeCalculator.StandardDistances[x-1])] = uint.Parse(fields[x]);
                }
            }

            return ageGrades;
        }
    }
}
