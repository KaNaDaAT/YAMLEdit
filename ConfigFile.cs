using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YAMLEdit.Parser;

namespace YAMLEdit {
	public class ConfigFile {

		public string FILE_PATH = "";
		protected YamlDocument yaml;
		protected IDeserializer deserializer;

		public bool Loaded
		{
			get; protected set;
		}

		public ConfigFile(string path, bool autoLoad = true)
		{
			FILE_PATH = path;
			Loaded = false;
			yaml = new YamlDocument(new YamlMappingNode());
			deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
			if (autoLoad)
				Load();
		}

		public void Load()
		{
			// A missing or empty file starts as an empty document, so Set() and Save() can create it
			yaml = new YamlDocument(new YamlMappingNode());
			Loaded = false;
			if (!File.Exists(FILE_PATH))
				return;
			using (StreamReader reader = new StreamReader(FILE_PATH))
			{
				YamlStream stream = new YamlStream();
				stream.Load(reader);
				if (stream.Documents.Count != 0)
				{
					yaml = stream.Documents[0];
					Loaded = true;
				}
			}
		}

		public void Load(string path)
		{
			FILE_PATH = path;
			Load();
		}

		public void Save()
		{
			using (TextWriter writer = File.CreateText(FILE_PATH))
				new YamlStream(yaml).Save(writer, false);
		}

		public void Save(string path)
		{
			FILE_PATH = path;
			Save();
		}

		public YamlNode GetNode(string path)
		{
			YamlNode rootNode = yaml.RootNode;
			string[] keys = path.Split('.');
			foreach (string key in keys)
			{
				if (rootNode == null)
				{
					return null;
				}
				rootNode = rootNode.NodeByKey(key);
			}
			rootNode.GetValue();
			return rootNode;
		}

		public T Get<T>(string path)
		{
			YamlNode node = GetNode(path);
			if (node == null)
				throw new KeyNotFoundException(string.Format("Nothing found at '{0}'", path));
			YamlStream stream = new YamlStream(new YamlDocument(node));
			return deserializer.Deserialize<T>(
				new EventStreamParserAdapter(
					YamlNodeToEventStreamConverter.ConvertToEventStream(stream)
				)
			);
		}

		public void Set(string path, object value)
		{
			YamlNode rootNode = yaml.RootNode;
			string[] keys = path.Split('.');
			for (int index = 0; index < keys.Length - 1; index++)
			{
				rootNode = rootNode.GetOrCreateContainer(keys[index], keys[index + 1]);
			}
			// Replaces whatever GetNode(path) finds, in place, or adds it if nothing is there
			if (value == null)
				rootNode.Remove(keys[keys.Length - 1]);
			else
				rootNode.Put(keys[keys.Length - 1], value);
		}
	}
}
