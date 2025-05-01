using System.Globalization;
using System.Text.Json;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;

namespace FLRC.AgeGradeCalculator.Generator;

using FieldKey = (Category Category, byte Age, FieldEvent Event);

public static class FieldGenerator
{
	public static async Task Run()
	{
		var worldRecords = await GetWorldRecords();
		var openAgeGrades = GetOpenAgeGrades(worldRecords);

		var juniors = GetJuniorAgeGrades();

		var ageFactors = GetAgeFactors();
		var masters = CalculateAgeGrades(ageFactors, worldRecords);

		var dataPoints = openAgeGrades.Union(masters).Union(await juniors)
			.OrderBy(d => d.Event)
			.ThenBy(d => d.Category)
			.ThenBy(d => d.Age);
		var group = dataPoints.GroupBy(p => (p.Category, p.Age, p.Event));
		var content = group.Select(g => $"{{ (Category.{g.Key.Category}, {g.Key.Age}, FieldEvent.{Enum.GetName(g.Key.Event)}), {g.Max(r => r.Record):F2} }}");
		var fileOutput = await File.ReadAllTextAsync("Field.cs");
		var newContent = fileOutput.Replace("// age grades will be generated here", string.Join($",{Environment.NewLine}\t\t", content));
		await File.WriteAllTextAsync("../../../../AgeGradeCalculator/Field.cs", newContent);
	}

	private static IEnumerable<DataPoint<FieldEvent, double>> GetOpenAgeGrades(Dictionary<FieldKey, double> worldRecords)
		=> worldRecords.SelectMany(r => Enumerable.Range(20, 11).Select(age => new DataPoint<FieldEvent, double> { Category = r.Key.Category, Age = (byte)age, Event = r.Key.Event, Record = r.Value }));

	private static async Task<IEnumerable<DataPoint<FieldEvent, double>>> GetJuniorAgeGrades()
	{
		var file = await File.ReadAllTextAsync("Data/junior-records.htm");
		var categories = file.Split("NOTES")[0].Split("BOYS")[1].Split("GIRLS");
		return ParseJuniorAgeGrades(Category.M, categories[0])
			.Union(ParseJuniorAgeGrades(Category.F, categories[1]));
	}

	private static List<DataPoint<FieldEvent, double>> ParseJuniorAgeGrades(Category category, string data)
	{
		var dataPoints = new List<DataPoint<FieldEvent, double>>();

		var events = data.Split("<b>", StringSplitOptions.TrimEntries);
		foreach (var e in events.TakeWhile(e => e.Length > 1))
		{
			var lines = e.Split(Environment.NewLine);
			var eventName = lines[0].Replace("</b>", "");
			if (IgnoreEvent(eventName))
				continue;

			var discipline = ParseEvent(eventName);
			foreach (var line in lines.Skip(2).TakeWhile(l => l.Length > 11))
			{
				var col1 = line[..2].Trim();
				var col2 = line[3..11].Trim();
				if (col1.Length == 0 || col2.Length == 0 || col1 == "*" || col1 == "#")
					continue;

				var age = byte.Parse(col1);
				var performance = col2.Replace("A", "").Replace("i", "").Trim();
				var time = double.Parse(performance);
				var dataPoint = new DataPoint<FieldEvent, double> { Category = category, Age = age, Event = discipline, Record = time };
				dataPoints.Add(dataPoint);
			}
		}

		return dataPoints;
	}

	private static IEnumerable<DataPoint<FieldEvent, double>> CalculateAgeGrades(Dictionary<FieldKey, double> ageFactors, Dictionary<FieldKey, double> worldRecords)
		=> ageFactors
			.Where(f => worldRecords.ContainsKey((f.Key.Category, 0, f.Key.Event)))
			.Select(f => new DataPoint<FieldEvent, double> { Age = f.Key.Age, Category = f.Key.Category, Event = f.Key.Event, Record = worldRecords[(f.Key.Category, 0, f.Key.Event)] / f.Value });

	private static Dictionary<FieldKey, double> GetAgeFactors()
	{
		var factors = new Dictionary<FieldKey, double>();

		var pdf = PdfDocument.Open("Data/2023-Age-Factors-WMA.pdf");
		var pages = pdf.GetPages().Where(p => p.Text.Contains("One-Year Age Factors", StringComparison.InvariantCultureIgnoreCase));

		var options = new DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions
		{
			WithinLineMultiplier = 0.25,
			BetweenLineMultiplier = 2
		};
		var docstrum = new DocstrumBoundingBoxes(options);

		foreach (var page in pages)
		{
			var category = page.Text.Contains("Women", StringComparison.InvariantCultureIgnoreCase)
				? Category.F
				: Category.M;

			var words = page.GetWords().ToList();
			var firstDataRowHeader = words.First(w => w.Text is "30" or "71");
			var dataStart = words.IndexOf(firstDataRowHeader);

			const int pageHeaderLength = 5;
			const int pageFooterLength = 6;
			var headerWordCount = dataStart - pageHeaderLength;
			var headerWords = words.Skip(pageHeaderLength).Take(headerWordCount);
			var events = docstrum.GetBlocks(headerWords.Where(h => h.Text != "Age"));

			while (dataStart < words.Count - pageHeaderLength - pageFooterLength)
			{
				var line = words.Skip(dataStart).Take(events.Count + 1).ToArray();
				var age = byte.Parse(line[0].Text);

				for (var x = 1; x < line.Length; x++)
				{
					var eventName = events[x - 1].Text.Replace(Environment.NewLine, " ").Replace('\n', ' ');
					if (IgnoreEvent(eventName))
						continue;

					var fieldEvent = ParseEvent(eventName);
					var factor = double.Parse(line[x].Text);
					factors.Add((category, age, fieldEvent), factor);
				}

				dataStart += events.Count + 1;
			}
		}

		return factors;
	}

	private static async Task<Dictionary<FieldKey, double>> GetWorldRecords()
	{
		var records = new Dictionary<FieldKey, double>();
		var json = await GetWorldRecordsJSON();
		foreach (var division in json)
		{
			var gender = division.GetProperty("gender").GetString();
			var category = ParseCategory(gender);

			var results = division.GetProperty("results").EnumerateArray();
			foreach (var result in results)
			{
				var discipline = result.GetProperty("discipline").GetString() ?? string.Empty;
				if (IgnoreEvent(discipline))
					continue;

				var performance = result.GetProperty("performance").GetString() ?? string.Empty;
				var time = double.Parse(performance);
				var key = (category, (byte)0, ParseEvent(discipline));
				records.TryAdd(key, time);
			}
		}

		return records;
	}

	private static readonly TextInfo TextInfo = CultureInfo.InvariantCulture.TextInfo;

	private static FieldEvent ParseEvent(string discipline)
		=> Enum.TryParse<FieldEvent>($"{TextInfo.ToTitleCase(discipline.Split("(")[0].ToLowerInvariant()).Replace(" ", "")}", out var fieldMatch)
				? fieldMatch
				: discipline switch
				{
					"Discus" => FieldEvent.DiscusThrow,
					"Hammer" => FieldEvent.HammerThrow,
					"Javelin" => FieldEvent.JavelinThrow,
					_ => throw new ArgumentException($"'{discipline}' not supported", nameof(discipline))
				};

	private static Category ParseCategory(string? category) => category == "men" ? Category.M : Category.F;

	private static bool IgnoreEvent(string discipline)
		=> !discipline.Contains("jump", StringComparison.InvariantCultureIgnoreCase)
		   && !discipline.Contains("vault", StringComparison.InvariantCultureIgnoreCase)
		   && !discipline.Contains("put", StringComparison.InvariantCultureIgnoreCase)
		   && !discipline.Contains("discus", StringComparison.InvariantCultureIgnoreCase)
		   && !discipline.Contains("hammer", StringComparison.InvariantCultureIgnoreCase)
		   && !discipline.Contains("javelin", StringComparison.InvariantCultureIgnoreCase)
		   && !discipline.Contains("throw", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("standing", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("ball", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("weight", StringComparison.InvariantCultureIgnoreCase);

	private static async Task<JsonElement.ArrayEnumerator> GetWorldRecordsJSON()
	{
		var file = await File.ReadAllBytesAsync("Data/world-records.json");
		var reader = new Utf8JsonReader(file);
		var json = JsonElement.ParseValue(ref reader);
		return json.GetProperty("data").GetProperty("getRecordsDetailByCategory").EnumerateArray();
	}
}