using SongProcessor.Models;

namespace SongProcessor.Gatherers;

public sealed record GatherOptions(
	bool AddEndings,
	bool AddInserts,
	bool AddOpenings,
	bool AddSongs
)
{
	public bool CanBeGathered(SongType type)
	{
		return AddSongs && type switch
		{
			SongType.Op => AddOpenings,
			SongType.Ed => AddEndings,
			SongType.In => AddInserts,
			_ => throw new ArgumentOutOfRangeException(nameof(type)),
		};
	}
}
