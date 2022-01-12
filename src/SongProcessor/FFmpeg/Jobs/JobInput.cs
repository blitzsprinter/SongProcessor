namespace SongProcessor.FFmpeg.Jobs;

public record JobInput(
	string File,
	IReadOnlyDictionary<string, string>? Args
);