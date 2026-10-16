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
			deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
			if (autoLoad)
				Load();
		}

		public void Load()
		{
			using (StreamReader reader = new StreamReader(FILE_PATH))
			{
				YamlStream stream = new YamlStream();
				stream.Load(reader);
				if (stream != null && stream.Documents.Count != 0)
				{
					yaml = stream.Documents[0];
					Loaded = true;
				}
				else
				{
					yaml = null;
					Loaded = false;
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
			if (!File.Exists(FILE_PATH))
				File.Create(FILE_PATH);
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
			YamlStream stream = new YamlStream(new YamlDocument(node));
			return deserializer.Deserialize<T>(
				new EventStreamParserAdapter(
					YamlNodeToEventStreamConverter.ConvertToEventStream(stream)
				)
			);
		}

		// Sets the node at path to value, or removes it when value is null. Keys resolve as in GetNode.
		public void Set(string path, object value)
		{
			YamlNode rootNode = yaml.RootNode;
			if (rootNode == null)
				return;
			string[] keys = path.Split('.');
			for (int index = 0; index < keys.Length - 1; index++)
			{
				YamlNode newNode = rootNode.NodeByKey(keys[index]);
				if (newNode == null || newNode is YamlScalarNode)
				{
					// Nothing to remove below a missing or scalar node.
					if (value == null)
						return;
					// Create the container in place of a missing node or scalar, typed for the next key.
					newNode = rootNode.CreateKey(keys[index], YamlPath.ContainerTypeFor(keys[index], keys[index + 1]));
				}
				rootNode = newNode;
			}
			if (value == null)
				rootNode.Remove(keys[keys.Length - 1]);
			else
				rootNode.Put(keys[keys.Length - 1], value);
		}
	}
}
