namespace SongProcessor.FFmpeg;

public readonly struct SourceInfo<T>
{
	public string File { get; }
	public T Info { get; }

	public SourceInfo(string file, T info)
	{
		File = file;
		Info = info;
	}
}