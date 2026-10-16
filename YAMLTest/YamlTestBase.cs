using System;
using System.Collections.Generic;
using System.IO;
using YAMLEdit;

namespace YAMLTest {
	public abstract class YamlTestBase : IDisposable {

		private readonly List<string> files = new List<string>();

		// Joins lines with "\n", so expectations don't depend on how the source file was checked out.
		protected static string Yaml(params string[] lines)
		{
			return string.Join("\n", lines) + "\n";
		}

		protected ConfigFile Load(string yaml)
		{
			string path = Path.GetTempFileName();
			files.Add(path);
			File.WriteAllText(path, yaml);
			return new ConfigFile(path);
		}

		// Saves the config and returns the written YAML with "\n" line endings and without the "..." document end marker.
		protected static string Saved(ConfigFile config)
		{
			config.Save();
			string text = File.ReadAllText(config.FILE_PATH).Replace("\r\n", "\n");
			return text.EndsWith("...\n") ? text.Substring(0, text.Length - "...\n".Length) : text;
		}

		public void Dispose()
		{
			foreach (string file in files)
				File.Delete(file);
		}
	}
}
