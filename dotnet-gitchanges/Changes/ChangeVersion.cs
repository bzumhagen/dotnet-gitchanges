using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gitchanges.Changes
{
	public class ChangeVersion : IComparable
	{
		private readonly string _value;
		private readonly List<int> _numbers;

		public ChangeVersion(string value)
		{
			if (string.IsNullOrEmpty(value)) throw new ArgumentException("Value cannot be null or empty.");
			
			_value = value;
			_numbers = GetNumbers(value);
		}

		private static List<int> GetNumbers(string value)
		{
			var numbers = new List<int>();
			var currentNumber = new StringBuilder();
			foreach (var character in value)
			{
				if (char.IsDigit(character))
				{
					currentNumber.Append(character);
				}
				else
				{
					if (currentNumber.Length > 0)
					{
						var currentInt = int.Parse(currentNumber.ToString()); 
						numbers.Add(currentInt);
						currentNumber.Clear();
					}
				}
			}
			
			if (currentNumber.Length > 0)
			{
				var currentInt = int.Parse(currentNumber.ToString()); 
				numbers.Add(currentInt);
				currentNumber.Clear();
			}

			return numbers;
		}

		protected bool Equals(ChangeVersion other)
		{
			return _value == other._value;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((ChangeVersion) obj);
		}

		public override int GetHashCode()
		{
			return (_value != null ? _value.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return _value;
		}

		public int CompareTo(object obj)
		{
			var otherVersion = (ChangeVersion) obj;
			
			if (_numbers.Count > 0 && otherVersion._numbers.Count > 0)
			{
				foreach (var (thisNum, otherNum) in _numbers.Zip(otherVersion._numbers, (thisNum, otherNum) => (thisNum, otherNum)))
				{
					var comparison = thisNum.CompareTo(otherNum);
					if (comparison != 0)
					{
						return comparison;
					}
				}
				return 0;
			}

			return _numbers.Count switch
			{
				0 when otherVersion._numbers.Count == 0 => (otherVersion._numbers.Count == 0
					? string.Compare(_value, otherVersion._value, StringComparison.Ordinal)
					: 1),
				0 => 1,
				_ => -1
			};
		}
	}
}