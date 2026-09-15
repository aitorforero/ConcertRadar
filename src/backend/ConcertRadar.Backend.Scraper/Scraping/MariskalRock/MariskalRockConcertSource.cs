using AngleSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace ConcertRadar.Backend.Scraper.Scraping.MariskalRock;

public sealed class MariskalRockConcertSource(
	HttpClient httpClient,
	ILogger<MariskalRockConcertSource> logger) : IConcertSource
{
	public string Name { get; } = "Mariskal Rock";

	private static readonly string[]  monthNames = { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

    private static readonly string pageUrl =  "https://mariskalrock.com/guia-de-conciertos/";
	private static readonly string inicialRegex = @"<h1>(?><strong>)?(?<inicial>[A-Z#])(?></strong>)?</h1>(?<grupos>[\s\S]*?(?=(<h1>|</div>)))";
	private static readonly string grupoRegex = @"<p><strong>(?<grupos>.*?)</strong></p>(?<fechas>[\s\S]*?)(?=(<p><strong>)|$)";

	private static readonly string fechaRegex = @"<p>(?<dia>\d{1,2}) (?<mes>\w+)\s-\s(?<ciudad>.*?)\s-\s(?<sala>.*)</p>";

	public async Task<IReadOnlyCollection<ScrapedEvent>> GetEventsAsync(
		CancellationToken cancellationToken)
	{

		var spanishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid");
		DateTime startTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, spanishTimeZone);
		var events = new List<ScrapedEvent>();

		if (!Uri.TryCreate(pageUrl, UriKind.Absolute, out var url))
		{
			logger.LogDebug("Fuente HTML deshabilitada: falta URL.");
			return [];
		}

		httpClient.Timeout = TimeSpan.FromSeconds(60);

		string html;

		try 
		{
			html = await httpClient.GetStringAsync(url, cancellationToken);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Error al obtener el contenido de la página.");
			return [];
		}

		foreach (Match match in Regex.Matches(html, inicialRegex))
		{
			events.AddRange(ParseGrupos(match.Groups["grupos"].Value, startTime));
		}

		return events;
	}

	private IReadOnlyCollection<ScrapedEvent> ParseGrupos(string html, DateTime startTime)
	{
		var events = new List<ScrapedEvent>();

		foreach (Match match in Regex.Matches(html, grupoRegex))
		{
			events.AddRange(ParseFechas(match.Groups["fechas"].Value, match.Groups["grupos"].Value, startTime));
		}

		return events;
	}

	private IReadOnlyCollection<ScrapedEvent> ParseFechas(string html, string name, DateTime startTime)
	{
		var events = new List<ScrapedEvent>();

		foreach (Match match in Regex.Matches(html, fechaRegex))
		{
			logger.LogInformation("Procesando Fecha: {Fecha}", match.Groups["fecha"].Value);
			if (!int.TryParse(match.Groups["dia"].Value, out int dia))
			{
				logger.LogWarning("No se pudo parsear el día: {Dia}", match.Groups["dia"].Value);
				continue;
			}

			if (monthNames.IndexOf(match.Groups["mes"].Value) is int mes && mes < 1)
			{
				logger.LogWarning("No se pudo parsear el mes: {Mes}", match.Groups["mes"].Value);
				continue;
			}

			var eventDate = new DateTime(startTime.Year, mes, dia);
			if(eventDate < startTime)
			{
				eventDate = new DateTime(startTime.Year + 1, mes, dia);
			}

			var eventName = name;

			if(name.Contains(":"))
			{
				eventName = name.Split(':')[0].Trim();
				name = name.Split(':')[1].Trim();
			} 

			var bandPerformances = name.Split(new[] { " + " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(bN =>new ScrapedPerformance(
					bN.Trim(),
					eventDate))
				.ToArray();

			var scrapedEvent = new ScrapedEvent(
				Name,
				string.Empty,
				name,
				eventDate,
				null,
				match.Groups["sala"].Value,
				null,
				null,
				match.Groups["ciudad"].Value,
				null,
				bandPerformances);

			events.Add(scrapedEvent);


		}

		return events;
	}
}