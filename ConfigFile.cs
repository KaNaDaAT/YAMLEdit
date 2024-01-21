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

		public void Set(string path, object value)
		{
			path = path.TrimEnd(':');
			YamlNode rootNode = yaml.RootNode;
			if (rootNode == null)
				return;
			string[] keys = path.Split('.');
			int index = 0;
			for (index = 0; index < keys.Length - 1; index++)
			{
				YamlNode newNode = rootNode.NodeByKey(keys[index]);
				if (newNode == null)
				{
					rootNode = rootNode.CreateKey(keys[index]);
				}
				else if (newNode is YamlScalarNode)
				{
					YamlScalarNode scalarNode = newNode as YamlScalarNode;
					rootNode.Remove(keys[index]);
					rootNode = rootNode.CreateKey(keys[index]);
				}
				else
				{
					rootNode = newNode;
				}
			}
			YamlNode tempNode = rootNode.NodeByKey(keys[keys.Length - 1]);
			if (value == null)
			{
				rootNode.Remove(keys[keys.Length - 1]);
				return;
			}
			if (tempNode != null)
			{
				if (rootNode is YamlSequenceNode tempSequenceNode)
				{
					tempSequenceNode.Put(YamlPath.GetIndexByKey(keys[keys.Length - 1]).ToString(), value);
					return;
				}
				if (tempNode is YamlScalarNode tempScalardNode)
				{
					tempScalardNode.Value = value.ToString();
					return;
				}
				else if (tempNode is YamlSequenceNode)
				{
					rootNode.Put(keys[keys.Length - 1], value);
					return;
				}
				else
				{
					rootNode.CreateKey(keys[keys.Length - 1]);
					rootNode.Put(keys[keys.Length - 1], value);
					return;
				}
			}
			else
			{
				rootNode.CreateKey(keys[keys.Length - 1]);
				rootNode.Put(keys[keys.Length - 1], value);
			}
		}
	}
}
