using System.Diagnostics.CodeAnalysis;

namespace SongProcessor.UI.Converters
{
	public sealed class ValueCollection
	{
		private readonly IList<object> _Values;
		private int _Index;

		public ValueCollection(IList<object> values)
		{
			_Values = values;
		}

		public TRet ConvertNextValue<TRet>()
		{
			if (_Values[_Index++] is not TRet t)
			{
				throw InvalidType(new[] { typeof(TRet) });
			}
			return t;
		}

		public TRet ConvertNextValue<TParam, TRet>(TParam param, params IMaybeFunc<TParam, TRet>[] uses)
		{
			var value = _Values[_Index++];
			foreach (var use in uses)
			{
				if (use.CanUse(value))
				{
					return use.Use(value, param);
				}
			}

			throw InvalidType(uses.Select(x => x.RequiredType));
		}

		public TRet ConvertNextValue<TRet>(params IMaybeFunc<TRet>[] uses)
		{
			var value = _Values[_Index++];
			foreach (var use in uses)
			{
				if (use.CanUse(value))
				{
					return use.Use(value);
				}
			}

			throw InvalidType(uses.Select(x => x.RequiredType));
		}

		public TRet ConvertNextValue<T1, T2, TRet>(
			Func<T1, TRet> f1,
			Func<T2, TRet> f2)
		{
			return _Values[_Index++] switch
			{
				T1 t1 => f1(t1),
				T2 t2 => f2(t2),
				_ => throw InvalidType(new[] { typeof(T1), typeof(T2) }),
			};
		}

		public TRet ConvertNextValue<T1, T2, T3, TRet>(
			Func<T1, TRet> f1,
			Func<T2, TRet> f2,
			Func<T3, TRet> f3)
		{
			return _Values[_Index++] switch
			{
				T1 t1 => f1(t1),
				T2 t2 => f2(t2),
				T3 t3 => f3(t3),
				_ => throw InvalidType(new[] { typeof(T1), typeof(T2), typeof(T3) }),
			};
		}

		public TRet ConvertNextValue<T1, T2, T3, T4, TRet>(
			Func<T1, TRet> f1,
			Func<T2, TRet> f2,
			Func<T3, TRet> f3,
			Func<T4, TRet> f4)
		{
			return _Values[_Index++] switch
			{
				T1 t1 => f1(t1),
				T2 t2 => f2(t2),
				T3 t3 => f3(t3),
				T4 t4 => f4(t4),
				_ => throw InvalidType(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }),
			};
		}

		public TRet ConvertNextValue<T1, T2, T3, T4, T5, TRet>(
			Func<T1, TRet> f1,
			Func<T2, TRet> f2,
			Func<T3, TRet> f3,
			Func<T4, TRet> f4,
			Func<T5, TRet> f5)
		{
			return _Values[_Index++] switch
			{
				T1 t1 => f1(t1),
				T2 t2 => f2(t2),
				T3 t3 => f3(t3),
				T4 t4 => f4(t4),
				T5 t5 => f5(t5),
				_ => throw InvalidType(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }),
			};
		}

		public bool TryConvertNextValue<T>([NotNullWhen(true)] out T result)
		{
			if (_Values[_Index] is T t)
			{
				result = t;
				++_Index;
				return true;
			}

			result = default!;
			return false;
		}

		private InvalidCastException InvalidType(IEnumerable<Type> types)
		{
			var t = string.Join(" ,", types);
			return new InvalidCastException($"Invalid value at index {--_Index}. Expected {t}");
		}
	}
}