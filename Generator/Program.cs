using System.Text.RegularExpressions;
using OfficeOpenXml;

namespace FLRC.AgeGradeCalculator.Generator;

public static class Program
{
	private const byte DistanceRow = 2;
	private const byte MinRow = 6;
	private const byte MaxRow = 101;

	private const byte AgeCol = 1;
	private const byte MinCol = 2;
	private const byte MaxCol = 23;

	public static async Task Main()
	{
		ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
		var dataPoints = Enum.GetValues<Category>().SelectMany(DataPointsForCategory);
		var content = dataPoints.Select(p => $"{{ (Category.{p.Category}, {p.Age}, {p.Distance}), {p.Record.TotalSeconds} }}");
		var fileOutput = await File.ReadAllTextAsync("Records.cs");
		var newContent = fileOutput.Replace("// age grades will be generated here", string.Join(",\n\t\t", content));
		await File.WriteAllTextAsync("../../../../AgeGradeCalculator/Records.cs", newContent);
	}

	private static List<DataPoint> DataPointsForCategory(Category category)
	{
		var dataPoints = new List<DataPoint>();
		var fileInfo = new FileInfo($"../../../../Age-Grade-Tables/2025 Files/{ParseCategory(category)}Road2025.xlsx");
		if (!fileInfo.Exists)
		{
			throw new FileNotFoundException("Could not load age grade tables from " + fileInfo.FullName);
		}
		var package = new ExcelPackage(fileInfo);
		var sheet = package.Workbook.Worksheets["AgeStanSec"];
		var distances = GetDistances(sheet);

		for (var row = MinRow; row <= MaxRow; row++)
		{
			var age = sheet.Cells[row, AgeCol].GetValue<byte>();
			for (var col = MinCol; col <= MaxCol; col++)
			{
				var distance = distances[col - MinCol];
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
		for (var col = MinCol; col <= MaxCol; col++)
			distances.Add(ParseDistance(sheet.Cells[DistanceRow, col].GetValue<string>()));
		return distances;
	}

	private static string? ParseCategory(Category category)
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

	private static readonly Regex Regex = new(@"([\d\.]+)(.*)", default, TimeSpan.FromSeconds(1));

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

		var split = Regex.Match(value).Groups;
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