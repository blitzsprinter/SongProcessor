namespace SongProcessor.FFmpeg
{
	public readonly struct SourceInfo<T>
	{
		public T Info { get; }
		public string Path { get; }

		public SourceInfo(string path, T info)
		{
			Path = path;
			Info = info;
		}
	}
}