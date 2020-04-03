using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Converters;
using AMQSongProcessor.Models;

namespace AMQSongProcessor
{

	public sealed class SaveNewOptions
	{
		public bool AllowOverwrite { get; set; }
		public bool CreateDuplicateFile { get; set; }
		public string Directory { get; }

		public SaveNewOptions(string directory)
		{
			Directory = directory;
		}
	}
}