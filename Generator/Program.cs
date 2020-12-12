using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FLRC.AgeGradeCalculator;
using OfficeOpenXml;

namespace Generator
{
    public static class Program
    {
        private const byte distanceRow = 2;
        private const byte ageCol = 1;
        private const byte minCol = 2;
        private const byte maxCol = 22;

        private static readonly IDictionary<Category, byte> minRow = new Dictionary<Category, byte> { { Category.F, 6 }, { Category.M, 5 } };
        private static readonly IDictionary<Category, byte> maxRow = new Dictionary<Category, byte> { { Category.F, 101 }, { Category.M, 100 } };

        public static async Task Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var dataPoints = Enum.GetValues<Category>().SelectMany(DataPointsForCategory);
            var content = dataPoints.Select(p => $"{{ new Identifier(Category.{p.Category}, {p.Age}, {p.Distance}), {p.Record.TotalSeconds} }}");
            var fileOutput = await File.ReadAllTextAsync("Records.cs");
            var newContent = fileOutput.Replace("//", string.Join(",\n            ", content));
            await File.WriteAllTextAsync("../../../../AgeGradeCalculator/Records.cs", newContent);
        }

        private static List<DataPoint> DataPointsForCategory(Category category)
        {
            var dataPoints = new List<DataPoint>();
            var package = new ExcelPackage(new FileInfo($"../../../../Age-Grade-Tables/2020 Files/{ParseCategory(category)}Road2020.xlsx"));
            var sheet = package.Workbook.Worksheets["AgeStanSec"];
            var distances = GetDistances(sheet);

            for (var row = minRow[category]; row <= maxRow[category]; row++)
            {
                var age = sheet.Cells[row, ageCol].GetValue<byte>();
                for (var col = minCol; col <= maxCol; col++)
                {
                    var distance = distances[col - minCol];
                    var record = sheet.Cells[row, col].GetValue<uint>();
                    dataPoints.Add(new DataPoint
                    {
                        Category = category,
                        Age = age,
                        Distance = distance,
                        Record = TimeSpan.FromSeconds(record)
                    });
                }
            }

            return dataPoints;
        }

        private static List<double> GetDistances(ExcelWorksheet sheet)
        {
            var distances = new List<double>();
            for (var col = minCol; col <= maxCol; col++)
                distances.Add(ParseDistance(sheet.Cells[distanceRow, col].GetValue<string>()));
            return distances;
        }

        private static string ParseCategory(Category category)
        {
            switch (category)
            {
                case Category.F:
                    return "female";
                case Category.M:
                    return "male";
            }
            return null;
        }

        private static double ParseDistance(string value)
        {
            const string marathon = "42.195 km";
            switch (value.ToLowerInvariant())
            {
                case "marathon":
                    return ParseDistance(marathon);
                case "h. mar":
                    return ParseDistance(marathon) / 2;
            }

            var split = Regex.Match(value, @"([\d\.]+)(.*)").Groups;
            if (split.Count < 2)
                return 0;

            var digits = double.Parse(split[1].Value.Trim());
            var units = split[2].Value.Trim();

            switch (units.ToLowerInvariant())
            {
                case "k":
                case "km":
                case "kms":
                    return digits * 1000;
                case "mi":
                case "mile":
                case "miles":
                    return digits * 1609.344;
            }

            return digits;
        }
    }
}
