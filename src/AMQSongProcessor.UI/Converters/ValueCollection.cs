using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using AdvorangesUtils;

namespace AMQSongProcessor.UI.Converters
{
	public sealed class ValueCollection
	{
		private readonly IList<object> _Values;
		private int _Index;

		public ValueCollection(IList<object> values)
		{
			_Values = values;
		}

		public T ConvertNextValue<T>()
		{
			if (!(_Values[_Index] is T t))
			{
				throw InvalidType(new[] { typeof(T) });
			}

			++_Index;
			return t;
		}

		public InvalidCastException InvalidType(IEnumerable<Type> types)
		{
			var t = types.Join(x => x.ToString(), " or ");
			return new InvalidCastException($"Invalid value at index {_Index}. Expected {t}");
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

		public TRet UseNextValue<T1, T2, TRet>(
			Func<T1, TRet> f1,
			Func<T2, TRet> f2)
		{
			if (TryConvertNextValue<T1>(out var t1))
			{
				return f1(t1);
			}
			if (TryConvertNextValue<T2>(out var t2))
			{
				return f2(t2);
			}
			throw InvalidType(new[] { typeof(T1), typeof(T2) });
		}

		public TRet UseNextValue<T1, T2, T3, TRet>(
			Func<T1, TRet> f1,
			Func<T2, TRet> f2,
			Func<T3, TRet> f3)
		{
			if (TryConvertNextValue<T1>(out var t1))
			{
				return f1(t1);
			}
			if (TryConvertNextValue<T2>(out var t2))
			{
				return f2(t2);
			}
			if (TryConvertNextValue<T3>(out var t3))
			{
				return f3(t3);
			}
			throw InvalidType(new[] { typeof(T1), typeof(T2), typeof(T3) });
		}

		public TRet UseNextValue<T1, T2, T3, T4, TRet>(
			Func<T1, TRet> f1,
			Func<T2, TRet> f2,
			Func<T3, TRet> f3,
			Func<T4, TRet> f4)
		{
			if (TryConvertNextValue<T1>(out var t1))
			{
				return f1(t1);
			}
			if (TryConvertNextValue<T2>(out var t2))
			{
				return f2(t2);
			}
			if (TryConvertNextValue<T3>(out var t3))
			{
				return f3(t3);
			}
			if (TryConvertNextValue<T4>(out var t4))
			{
				return f4(t4);
			}
			throw InvalidType(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
		}

		public TRet UseNextValue<T1, T2, T3, T4, T5, TRet>(
			Func<T1, TRet> f1,
			Func<T2, TRet> f2,
			Func<T3, TRet> f3,
			Func<T4, TRet> f4,
			Func<T5, TRet> f5)
		{
			if (TryConvertNextValue<T1>(out var t1))
			{
				return f1(t1);
			}
			if (TryConvertNextValue<T2>(out var t2))
			{
				return f2(t2);
			}
			if (TryConvertNextValue<T3>(out var t3))
			{
				return f3(t3);
			}
			if (TryConvertNextValue<T4>(out var t4))
			{
				return f4(t4);
			}
			if (TryConvertNextValue<T5>(out var t5))
			{
				return f5(t5);
			}
			throw InvalidType(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
		}

		public TRet UseNextValue<TRet>(params IMaybeFunc<TRet>[] uses)
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
	}
}