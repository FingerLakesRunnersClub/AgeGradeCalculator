using System.Text.Json;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;

namespace FLRC.AgeGradeCalculator.Generator;

using Factors = Dictionary<(Category category, byte age, TrackEvent eventName), double>;
using Records = Dictionary<(Category category, TrackEvent eventName), TimeSpan>;

public static class TrackGenerator
{
	public static async Task Run()
	{
		var worldRecords = await GetWorldRecords();
		var ageFactors = GetAgeFactors();
		var dataPoints = CalculateAgeGrades(ageFactors, worldRecords);
		var content = dataPoints.Select(p => $"{{ (Category.{p.Category}, {p.Age}, TrackEvent.{Enum.GetName(p.Event)}), {p.Record.TotalSeconds:F2} }}");
		var fileOutput = await File.ReadAllTextAsync("Track.cs");
		var newContent = fileOutput.Replace("// age grades will be generated here", string.Join($",{Environment.NewLine}\t\t", content));
		await File.WriteAllTextAsync("../../../../AgeGradeCalculator/Track.cs", newContent);
	}

	private static IEnumerable<DataPoint<TrackEvent>> CalculateAgeGrades(Factors ageFactors, Records worldRecords)
		=> ageFactors
			.Where(f => worldRecords.ContainsKey((f.Key.category, f.Key.eventName)))
			.Select(f => new DataPoint<TrackEvent> { Age = f.Key.age, Category = f.Key.category, Event = f.Key.eventName, Record = TimeSpan.FromSeconds(worldRecords[(f.Key.category, f.Key.eventName)].TotalSeconds / f.Value) });

	private static Factors GetAgeFactors()
	{
		var factors = new Factors();

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

	private static async Task<Records> GetWorldRecords()
	{
		var records = new Records();
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
				var key = (category, ParseEvent(discipline, category));
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
				: discipline switch
				{
					"50 Metres" => TrackEvent._50m,
					"55 Metres" => TrackEvent._55m,
					"60 Metres" => TrackEvent._60m,
					"100 Metres" => TrackEvent._100m,
					"200 Metres" => TrackEvent._200m,
					"300 Metres" => TrackEvent._300m,
					"400 Metres" => TrackEvent._400m,
					"600 Metres" => TrackEvent._600m,
					"800 Metres" => TrackEvent._800m,
					"1000 Metres" => TrackEvent._1000m,
					"1500 Metres" => TrackEvent._1500m,
					"1600 Metres" => TrackEvent._1600m,
					"Mile" => TrackEvent._1mi,
					"2000 Metres" => TrackEvent._2000m,
					"3000 Metres" => TrackEvent._3000m,
					"3200 Metres" => TrackEvent._3200m,
					"2 Mile" => TrackEvent._2mi,
					"5000 Metres" => TrackEvent._5000m,
					"10,000 Metres" => TrackEvent._10000m,
					"50 Metres Hurdles" => TrackEvent._50mH,
					"55 Metres Hurdles" => TrackEvent._55mH,
					"60 Metres Hurdles" => TrackEvent._60mH,
					"60m Hurdles" => TrackEvent._60mH,
					"100 Metres Hurdles" => TrackEvent._100mH,
					"110 Metres Hurdles" => TrackEvent._110mH,
					"Short Hurdles" => category == Category.F ? TrackEvent._100mH : TrackEvent._110mH,
					"400 Metres Hurdles" => TrackEvent._400mH,
					"Long Hurdles" => TrackEvent._400mH,
					"3000 Metres Steeplechase" => TrackEvent._3000mSC,
					"Steeple Chase" => TrackEvent._3000mSC,
					_ => throw new ArgumentException($"'{discipline}' not supported", nameof(discipline))
				};

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