namespace SongProcessor.FFmpeg;

public record FFmpegInput(
	string File,
	IReadOnlyDictionary<string, string>? Args
);