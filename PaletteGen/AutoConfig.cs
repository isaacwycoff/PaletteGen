using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace DaggerLevelEditor
{
	public class AutoConfig
	{
		private readonly Dictionary<Type, Func<string, Object>> ParserLookup =
			new Dictionary<Type, Func<string, Object>>()
		{
			{ typeof(string), s => s },
			{ typeof(bool), s => bool.Parse(s) },
			{ typeof(Int32), s => Int32.Parse(s) },
			{ typeof(Int64), s => Int64.Parse(s) },
			{ typeof(UInt32), s => UInt32.Parse(s) },
			{ typeof(UInt64), s => UInt64.Parse(s) },
			{ typeof(float), s => Double.Parse(s) },
			{ typeof(double), s => Double.Parse(s) },
			{ typeof(decimal), s => Decimal.Parse(s) },
			{ typeof(DateTime), s => DateTime.Parse(s) },
		};

		private bool IsValidType(Type type)
		{
			return ParserLookup.Keys.Contains(type);
		}

		private List<string> Errors = new List<string>();
		public string[] GetErrors()
		{
			return Errors.ToArray();
		}

		private void AddError(Exception ex, string message)
		{
			Errors.Add($"{message}: {ex.Message}");
		}

		private void ParseMember(Type memberType, string name, Action<object> setFunc)
		{
			if (!IsValidType(memberType))
			{
				AddError(new Exception("Invalid type"), $"Invalid type on {name}");
				return;
			}

			try
			{
				var rawValue = ConfigurationManager.AppSettings[name];

				if (rawValue == null) throw new Exception("Couldn't find setting with that name.");

				var parsedValue = ParserLookup[memberType](rawValue);
				setFunc(parsedValue);
			}
			catch (Exception ex)
			{
				AddError(ex, $"Couldn't parse {name}");
			}
		}

		protected AutoConfig()
		{
			var type = this.GetType();

			foreach (var field in type.GetFields())
			{
				ParseMember(
					field.FieldType,
					field.Name,
					v => field.SetValue(this, v)
				);
			}

			foreach (var prop in type.GetProperties())
			{
				ParseMember(
					prop.PropertyType,
					prop.Name,
					v => prop.SetMethod.Invoke(this, new object[] { v })
				);
			}
		}
	}
}
