using System;
using System.Collections.Generic;

namespace CmdArgsParser
{
	public class Parser
	{
		private readonly Dictionary<string, IParserRule> RuleLookup;
		public Parser()
		{
			RuleLookup = new Dictionary<string, IParserRule>();
		}

		public void AddRule<T>(string name, Action<T> action, params string[] commands)
		{
			var rule = new ParserRule<T>(name, action);

			foreach(var command in commands)
			{
				RuleLookup[command] = rule;
			}
		}

		public void Parse(string[] args)
		{
			Console.WriteLine("Parsing command line");

			var length = args.Length;

			for (int i = 0; i < length - 1; ++i)
			{
				var arg = args[i];
				Console.WriteLine(arg);

				if (RuleLookup.ContainsKey(arg))
				{
					var rule = RuleLookup[arg];
					var value = args[i + 1];
					rule.Parse(value);
					i++;
				}
			}
		}


	}

	public interface IParserRule
	{
		void Parse(string value);
	}

	public class ParserRule<T> : IParserRule
	{
		public ParserRule(string name, 
			Action<T> parseFunc)
		{
			Name = name;
			ParseImpl = parseFunc;
		}

		public void Parse(string value)
		{
			if(typeof(T) == typeof(System.String))
			{
				ParseImpl((T)Convert.ChangeType(value, typeof(T)));
				return;
			}

			throw new ParserException(Name, value);
		}

		private readonly string Name;
		private readonly Action<T> ParseImpl;
	}

	public class ParserException : Exception
	{
		public ParserException(string name, string value)
			: base($"Couldn't parse {value} for command {name}")
		{

		}
	}
}
