using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MarketQADataProcessorApp
{
	internal static class Startup
	{
		public static readonly bool Is64Bit;

		internal static void UpdatePathEnvironmentVariable()
		{
			const string envPath = "Path";
			var sb = new StringBuilder();
			foreach (string item in Startup.GetAssemblyLocations())
			{
				sb.Append(item).Append(';');
			}

			string path = Environment.GetEnvironmentVariable(envPath);
			if (path != null)
			{
				sb.Append(path);
			}

			Environment.SetEnvironmentVariable(envPath, sb.ToString());
		}

		public static IList<string> GetAssemblyLocations()
		{
			string subfolder = (Is64Bit ? "64" : "32");
			var list = new List<string>();
			IList<string> locations = GetLocations(Settings.AssemblyPath, @".;lib;..\lib");
			foreach (var location in locations)
			{
				list.Add(location);
				list.Add(Path.Combine(location, subfolder));
			}
			return list;
		}

		public static IList<string> GetLocations(Settings name, string defaultValue)
		{
			return FileHelper.GetLocations(GetValue(name, defaultValue));
		}

		public static string GetValue(Settings name, string defaultValue)
		{
			string value = GetParameterVariable(name)
				?? (GetEnvironmentVariable(name)
				?? ConfigurationManager.AppSettings[name.ToString()]);
			return (value ?? defaultValue);
		}

		private static string GetParameterVariable(Settings name)
		{
			string prefix = name + "=";
			return (from value in Environment.GetCommandLineArgs()
					where value.StartsWith(prefix)
					select value.Substring(prefix.Length)).FirstOrDefault();
		}

		public static string GetEnvironmentVariable(Settings name)
		{
			return Environment.GetEnvironmentVariable(ToEnvironmentVariableName(name));
		}

		private static string ToEnvironmentVariableName(Settings name)
		{
			return "MarketQA" + name;
		}

		#region Internal classes

		internal enum Settings
		{
			ApplicationName,
			ApplicationTitle,
			Version,
			ReleaseDate,
			ShowErrors,
			AssemblyPath,
			AssemblyCache,
			ExtensionList,
			ExtensionPath,
			ConfigurationPath,
			ConfigurationFile,
			MaxConfigurationCount,
			AutoLogin,
		}

		internal static class FileHelper
		{
			public static readonly string StartupPath;

			static FileHelper()
			{
				StartupPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			}

			public static IList<string> GetLocations(string path)
			{
				List<string> list = new List<string>();
				if (path != null)
				{
					path = ExpandEnvironmentVariables(path);
					string[] locations = path.Split(';');
					foreach (string location in locations)
					{
						string item = location.Trim();
						if (item.Length > 0)
						{
							list.Add(GetFullPath(item));
						}
					}
				}
				return list.ToArray();
			}

			private static string ExpandEnvironmentVariables(string value)
			{
				return TextHelper.ExpandEnvironmentVariables(value);
			}

			public static string GetFullPath(string path)
			{
				if (path != null && !string.IsNullOrEmpty(path = path.Trim()))
				{
					path = ExpandEnvironmentVariables(path);
					if (Path.IsPathRooted(path))
					{
						return path;
					}
					if (path == ".")
					{
						return StartupPath;
					}
					if (path.StartsWith(@".\"))
					{
						path = path.Substring(2);
					}
					return Path.Combine(StartupPath, path);
				}
				return null;
			}
		}

		internal static class TextHelper
		{
			private static readonly Regex VariablePattern = new Regex(@"(\${\s*((?:\w|\.|-|_)+)\s*})");

			public static string ExpandEnvironmentVariables(string value)
			{
				return ExpandVariables(value, Environment.GetEnvironmentVariables());
			}

			public static string ExpandVariables(string value, IDictionary variables)
			{
				Match match;
				int pos = 0;
				StringBuilder sb = new StringBuilder();

				while ((match = VariablePattern.Match(value, pos)).Success)
				{
					if (pos < match.Index)
					{
						sb.Append(value.Substring(pos, match.Index - pos));
					}

					string varName = match.Groups[2].Value;

					string varValue = GetVariableValue(varName, variables);

					if (varValue != null)
					{
						sb.Append(varValue);
					}

					pos = match.Index + match.Length;
				}

				if (pos > 0)
				{
					if (pos < value.Length)
					{
						sb.Append(value.Substring(pos, value.Length - pos));
					}
					return sb.ToString();
				}

				return value;
			}

			private static string GetVariableValue(string name, IDictionary variables)
			{
				foreach (DictionaryEntry entry in variables)
				{
					string n = entry.Key as string;
					if (string.Compare(n, name, true) == 0)
					{
						return entry.Value as string;
					}
				}
				return null;
			}
		}

		#endregion

	}
}
