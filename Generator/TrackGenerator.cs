using System.Text.Json;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;

namespace FLRC.AgeGradeCalculator.Generator;

using TrackKey = (Category Category, byte Age, TrackEvent Event);

public static class TrackGenerator
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
		var content = group.Select(g => $"{{ (Category.{g.Key.Category}, {g.Key.Age}, TrackEvent.{Enum.GetName(g.Key.Event)}), {g.Min(r => r.Record.TotalSeconds):F2} }}");
		var fileOutput = await File.ReadAllTextAsync("Track.cs");
		var newContent = fileOutput.Replace("// age grades will be generated here", string.Join($",{Environment.NewLine}\t\t", content));
		await File.WriteAllTextAsync("../../../../AgeGradeCalculator/Track.cs", newContent);
	}

	private static IEnumerable<DataPoint<TrackEvent>> GetOpenAgeGrades(Dictionary<TrackKey, TimeSpan> worldRecords)
		=> worldRecords.SelectMany(r => Enumerable.Range(20, 11).Select(age => new DataPoint<TrackEvent> { Category = r.Key.Category, Age = (byte)age, Event = r.Key.Event, Record = r.Value }));

	private static async Task<IEnumerable<DataPoint<TrackEvent>>> GetJuniorAgeGrades()
	{
		var file = await File.ReadAllTextAsync("Data/junior-records.htm");
		var categories = file.Split("NOTES")[0].Split("BOYS")[1].Split("GIRLS");
		return ParseJuniorAgeGrades(Category.M, categories[0])
			.Union(ParseJuniorAgeGrades(Category.F, categories[1]));
	}

	private static List<DataPoint<TrackEvent>> ParseJuniorAgeGrades(Category category, string data)
	{
		var dataPoints = new List<DataPoint<TrackEvent>>();

		var events = data.Split("<b>", StringSplitOptions.TrimEntries);
		foreach (var e in events.TakeWhile(e => e.Length > 1))
		{
			var lines = e.Split(Environment.NewLine);
			var eventName = lines[0].Replace("</b>", "");
			if (IgnoreEvent(eventName))
				continue;

			var discipline = ParseEvent(eventName, category);
			foreach (var line in lines.Skip(2).TakeWhile(l => l.Length > 11))
			{
				var col1 = line[..2].Trim();
				var col2 = line[3..11].Trim();
				if (col1.Length == 0 || col2.Length == 0 || col1 == "*" || col1 == "#")
					continue;

				var age = byte.Parse(col1);
				var performance = col2.Replace("A", "").Replace("i", "").Trim();
				var time = TimeSpan.Parse(performance.Contains(':') ? $"0:{performance}" : $"0:00:{performance}");
				var dataPoint = new DataPoint<TrackEvent> { Category = category, Age = age, Event = discipline, Record = time };
				dataPoints.Add(dataPoint);
			}
		}

		return dataPoints;
	}

	private static IEnumerable<DataPoint<TrackEvent>> CalculateAgeGrades(Dictionary<TrackKey, double> ageFactors, Dictionary<TrackKey, TimeSpan> worldRecords)
		=> ageFactors
			.Where(f => worldRecords.ContainsKey((f.Key.Category, 0, f.Key.Event)))
			.Select(f => new DataPoint<TrackEvent> { Age = f.Key.Age, Category = f.Key.Category, Event = f.Key.Event, Record = TimeSpan.FromSeconds(worldRecords[(f.Key.Category, 0, f.Key.Event)].TotalSeconds / f.Value) });

	private static Dictionary<TrackKey, double> GetAgeFactors()
	{
		var factors = new Dictionary<TrackKey, double>();

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

					var trackEvent = ParseEvent(eventName, category);
					var factor = double.Parse(line[x].Text);
					factors.Add((category, age, trackEvent), factor);
				}

				dataStart += events.Count + 1;
			}
		}

		return factors;
	}

	private static async Task<Dictionary<TrackKey, TimeSpan>> GetWorldRecords()
	{
		var records = new Dictionary<TrackKey, TimeSpan>();
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
				var time = TimeSpan.Parse(performance.Contains(':') ? $"0:{performance}" : $"0:00:{performance}");
				var key = (category, (byte)0, ParseEvent(discipline, category));
				records.TryAdd(key, time);
			}
		}

		return records;
	}

	private static TrackEvent ParseEvent(string discipline, Category category)
		=> Enum.TryParse<TrackEvent>($"_{discipline}", out var trackMatch)
			? trackMatch
			: Enum.TryParse<TrackEvent>($"{discipline.Replace(" ", "")}", out var fieldMatch)
				? fieldMatch
				: FormatEventName(discipline) switch
				{
					"50 metres" => TrackEvent._50m,
					"55 metres" => TrackEvent._55m,
					"60 metres" => TrackEvent._60m,
					"100 metres" => TrackEvent._100m,
					"200 metres" => TrackEvent._200m,
					"300 metres" => TrackEvent._300m,
					"400 metres" => TrackEvent._400m,
					"500 metres" => TrackEvent._500m,
					"600 metres" => TrackEvent._600m,
					"800 metres" => TrackEvent._800m,
					"1000 metres" => TrackEvent._1000m,
					"1500 metres" => TrackEvent._1500m,
					"1600 metres" => TrackEvent._1600m,
					"mile" => TrackEvent._1mi,
					"1 mile" => TrackEvent._1mi,
					"2000 metres" => TrackEvent._2000m,
					"3000 metres" => TrackEvent._3000m,
					"3200 metres" => TrackEvent._3200m,
					"2 mile" => TrackEvent._2mi,
					"2 miles" => TrackEvent._2mi,
					"5000 metres" => TrackEvent._5000m,
					"10000 metres" => TrackEvent._10000m,
					"50 metres hurdles" => TrackEvent._50mH,
					"55 metres hurdles" => TrackEvent._55mH,
					"60 metres hurdles" => TrackEvent._60mH,
					"60m hurdles" => TrackEvent._60mH,
					"100 metres hurdles" => TrackEvent._100mH,
					"110 metres hurdles" => TrackEvent._110mH,
					"short hurdles" => category == Category.F ? TrackEvent._100mH : TrackEvent._110mH,
					"400 metres hurdles" => TrackEvent._400mH,
					"long hurdles" => TrackEvent._400mH,
					"2000 metres steeplechase" => TrackEvent._2000mSC,
					"3000 metres steeplechase" => TrackEvent._3000mSC,
					"steeple chase" => TrackEvent._3000mSC,
					_ => throw new ArgumentException($"'{discipline}' not supported", nameof(discipline))
				};

	private static string FormatEventName(string discipline)
		=> Patterns.ExtraSpaceInDistance()
			.Replace(discipline, "$1")
			.Replace(",", "")
			.Split('(')[0]
			.Trim()
			.ToLower();

	private static Category ParseCategory(string? category) => category == "men" ? Category.M : Category.F;

	private static bool IgnoreEvent(string discipline)
		=> discipline.Contains("medley", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("relay", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("walk", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("kilometres", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("short track", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("road", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("marathon", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("athlon", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("hour", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("jump", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("vault", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("shot", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("discus", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("javelin", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("hammer", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("weight", StringComparison.InvariantCultureIgnoreCase)
		   || discipline.Contains("throw", StringComparison.InvariantCultureIgnoreCase);

	private static async Task<JsonElement.ArrayEnumerator> GetWorldRecordsJSON()
	{
		var file = await File.ReadAllBytesAsync("Data/world-records.json");
		var reader = new Utf8JsonReader(file);
		var json = JsonElement.ParseValue(ref reader);
		return json.GetProperty("data").GetProperty("getRecordsDetailByCategory").EnumerateArray();
	}
}