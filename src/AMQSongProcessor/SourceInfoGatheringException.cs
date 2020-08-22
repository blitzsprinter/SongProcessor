using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using AdvorangesUtils;

using AMQSongProcessor.Converters;
using AMQSongProcessor.Models;
using AMQSongProcessor.Utils;

namespace AMQSongProcessor
{

	public class SourceInfoGatheringException : Exception
	{
		public SourceInfoGatheringException()
		{
		}

		public SourceInfoGatheringException(string message) : base(message)
		{
		}

		public SourceInfoGatheringException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}