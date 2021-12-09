namespace SongProcessor;

public sealed record SaveNewOptions(
	bool AddShowNameDirectory,
	bool AllowOverwrite,
	bool CreateDuplicateFile
);
