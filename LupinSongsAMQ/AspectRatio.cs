using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace LupinSongsAMQ
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public readonly struct AspectRatio : IEquatable<AspectRatio>
	{
		public int Height { get; }
		public float Ratio => Width / Height;
		public int Width { get; }

		private string DebuggerDisplay => ToString();

		public AspectRatio(int width, int height)
		{
			Width = width;
			Height = height;
		}

		public static bool operator !=(AspectRatio item1, AspectRatio item2)
			=> !(item1 == item2);

		public static bool operator ==(AspectRatio item1, AspectRatio item2)
			=> item1.Equals(item2);

		public override bool Equals(object obj)
			=> Equals(obj as AspectRatio?);

		public bool Equals([AllowNull] AspectRatio? other)
		{
			if (other == null)
			{
				return false;
			}
			return Equals(other.Value);
		}

		public bool Equals(AspectRatio other)
			=> Height == other.Height && Width == other.Width;

		public override int GetHashCode()
			=> HashCode.Combine(Width, Height);

		public override string ToString()
			=> ToString('/');

		public string ToString(char separator)
			=> Width.ToString() + separator + Height.ToString();
	}
}