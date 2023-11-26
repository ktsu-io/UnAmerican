﻿using CommandLine;
using ktsu.io.CaseConverter;
using WeCantSpell.Hunspell;

namespace ktsu.io.unamerican
{

	/// <summary>
	/// 
	/// </summary>
	public sealed class Program
	{
		/// <summary>
		/// 
		/// </summary>
		public sealed class Options
		{
			/// <summary>
			/// 
			/// </summary>
			[Option('f', "filepath", Required = false, HelpText = "The path of the file to validate.", Default = "")]
			public string FilePath { get; set; } = string.Empty;
		}

		private static int Main(string[] args)
		{
			var americanDictionary = WordList.CreateFromFiles(@"English (American).dic", @"English (American).aff");
			var unamericanDictionaries = new[]
			{
				WordList.CreateFromFiles(@"English (British).dic", @"English (British).aff"),
				WordList.CreateFromFiles(@"English (Australian).dic", @"English (Australian).aff"),
				WordList.CreateFromFiles(@"English (Canadian).dic", @"English (Canadian).aff"),
				WordList.CreateFromFiles(@"English (South African).dic", @"English (South African).aff"),
			};

			int returnCode = 0;

			Parser.Default.ParseArguments<Options>(args)
			.WithParsed(o =>
			{
				var linesToCheck = new List<string>
				{
					Path.GetFileNameWithoutExtension(o.FilePath),
				};

				if (!string.IsNullOrEmpty(o.FilePath))
				{
					try
					{
						linesToCheck.AddRange(File.ReadAllLines(o.FilePath));
					}
					catch (FileNotFoundException)
					{
					}
				}

				for (int i = 0; i < linesToCheck.Count; i++)
				{
					var line = linesToCheck[i].Trim();

					//extract words from line
					var words = line.ToMacroCase().Split('_');

					foreach (var word in words)
					{
						if (!americanDictionary.Check(word))
						{
							foreach (var dict in unamericanDictionaries)
							{
								if (dict.Check(word))
								{
									var suggestion = americanDictionary.Suggest(word).FirstOrDefault() ?? string.Empty;
									suggestion = suggestion.Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();

									if (suggestion == word)
									{
										break;
									}

									Console.ForegroundColor = ConsoleColor.Red;
									Console.Write(word);
									Console.ForegroundColor = ConsoleColor.White;
									Console.Write(suggestion.Length != 0 ? " did you mean " : "");
									Console.ForegroundColor = ConsoleColor.Green;
									Console.Write(suggestion);
									Console.ForegroundColor = ConsoleColor.White;

									Console.Write($" in {(i > 0 ? $"{Path.GetFileName(o.FilePath)} line {i}" : "filename")}: ");
									Console.WriteLine($"{line}");

									break;
								}
							}
						}
					}
				}

			});

			return returnCode;
		}
	}
}