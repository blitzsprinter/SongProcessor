namespace SongProcessor.Gatherers;

public static class GathererUtils
{
	public static void ThrowIfInvalidResponse(this HttpResponseMessage response)
	{
		if (!response.IsSuccessStatusCode)
		{
			var msg = $"{response.RequestMessage?.RequestUri} returned {response.StatusCode}.";
			throw new HttpRequestException(msg, null, response.StatusCode);
		}
	}

	public static void ThrowUnableToFind(this IAnimeGatherer gatherer, int id)
		=> throw new KeyNotFoundException($"{id} cannot be found in {gatherer.Name}.");
}