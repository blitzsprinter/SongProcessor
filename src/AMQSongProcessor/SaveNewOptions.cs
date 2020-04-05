namespace AMQSongProcessor
{
	public sealed class SaveNewOptions
	{
		public bool AddShowNameDirectory { get; set; }
		public bool AllowOverwrite { get; set; }
		public bool CreateDuplicateFile { get; set; }
		public string Directory { get; }

		public SaveNewOptions(string directory)
		{
			Directory = directory;
		}
	}
}