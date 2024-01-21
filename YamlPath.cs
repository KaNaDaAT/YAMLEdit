using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace YAMLEdit {
	public static class YamlPath {

		public static YamlNode GetValue(this YamlNode node)
		{
			YamlScalarNode scalar = node as YamlScalarNode;
			if (scalar != null)
			{
				return scalar;
			}

			YamlMappingNode mapping = node as YamlMappingNode;
			if (mapping != null && mapping.Children.Count == 1)
			{
				return mapping.Children[0].Value;
			}

			YamlSequenceNode sequence = node as YamlSequenceNode;
			if (sequence != null)
			{
				return sequence;
			}

			return null;
		}


		public static YamlNode NodeByKey(this YamlNode node, string key)
		{
			if (node == null)
				throw new ArgumentNullException(string.Format("Unsupported node type: {0} for 'node'", null));

			YamlScalarNode scalar = node as YamlScalarNode;
			if (scalar != null)
			{
				throw new ArgumentException(string.Format("Node of type {0} cannot have a key", node.GetType().Name));
			}

			YamlMappingNode mapping = node as YamlMappingNode;
			if (mapping != null)
			{
				foreach (KeyValuePair<YamlNode, YamlNode> entry in mapping.Children)
				{
					if (entry.Key.ToString().Equals(key))
					{
						return entry.Value;
					}
				}
				return null;
			}

			YamlSequenceNode sequence = node as YamlSequenceNode;
			if (sequence != null)
			{
				int index = GetIndexByKey(key);

				IEnumerator<YamlNode> enumerator = sequence.GetEnumerator();
				if (IsSequenceInception(key))
				{
					return GetSequenceInSequence(index, enumerator);
				}
				else if (index != -1)
				{
					return GetSequence(index, enumerator);
				}
				else
				{
					for (int i = 0; enumerator.MoveNext(); i++)
					{
						YamlNode current = enumerator.Current;
						if (current == null)
							break;

						YamlScalarNode keyNode = current as YamlScalarNode;
						if (keyNode != null && keyNode.Value.ToString().Equals(key))
						{
							return current;
						}

						YamlMappingNode mapNode = current as YamlMappingNode;
						if (mapNode != null && mapNode.Children.Count > 0)
						{
							string mapKey = (mapNode.Children[0].Key as YamlScalarNode).Value.ToString();
							if (mapKey.Equals(key))
							{
								current = mapNode.Children[0].Value;
								return current;
							}
						}
					}

				}
				return null;
			}
			throw new NotSupportedException(string.Format("Unsupported node type: {0}", node.GetType().Name));
		}

		public static int GetIndexByKey(string key)
		{
			try
			{
				if (key.StartsWith(":"))
				{
					return int.Parse(key.TrimEnd(':').TrimStart(':'));
				}
				else
				{
					return int.Parse(key.TrimEnd(':'));
				}
			}
			catch (Exception)
			{
				return -1;
			}
		}

		private static bool IsSequenceInception(string key)
		{
			return key.StartsWith(":") && key.EndsWith(":");
		}

		private static YamlNode GetSequenceInSequence(int index, IEnumerator<YamlNode> enumerator)
		{
			int sequenceIndex = 0;
			for (int i = 0; enumerator.MoveNext(); i++)
			{
				YamlNode current = enumerator.Current;
				if (current == null)
					break;
				YamlSequenceNode sequence = current as YamlSequenceNode;
				if (sequence != null)
				{
					if (index == sequenceIndex)
						return current;
					sequenceIndex++;
				}
			}
			return null;
		}

		private static YamlNode GetSequence(int index, IEnumerator<YamlNode> enumerator)
		{
			for (int i = 0; enumerator.MoveNext(); i++)
			{
				YamlNode current = enumerator.Current;
				if (current == null)
					break;
				if (index == i)
				{
					return current;
				}
			}
			return null;
		}


		public static YamlNode CreateKey(this YamlNode node, string key)
		{
			if (IsSequenceInception(key))
			{
				return CreateKey(node, key, YamlNodeType.Sequence);
			}
			return CreateKey(node, key, YamlNodeType.Mapping);
		}

		public static YamlNode CreateKey(this YamlNode node, string key, YamlNodeType type)
		{
			if (node == null)
				throw new ArgumentNullException(string.Format("Unsupported node type: {0} for 'node'", null));

			YamlScalarNode scalar = node as YamlScalarNode;
			if (scalar != null)
			{
				throw new ArgumentException(string.Format("Node of type {0} cannot have a key", node.GetType().Name));
			}

			YamlMappingNode mapping = node as YamlMappingNode;
			if (mapping != null)
			{
				YamlNode nodeB;
				switch (type)
				{
					case YamlNodeType.Mapping:
						nodeB = new YamlMappingNode();
						mapping.Add(new YamlScalarNode(key), nodeB);
						break;
					case YamlNodeType.Scalar:
						nodeB = new YamlScalarNode();
						mapping.Add(new YamlScalarNode(key), nodeB);
						break;
					case YamlNodeType.Sequence:
						nodeB = new YamlSequenceNode(key);
						mapping.Add(new YamlScalarNode(key), nodeB);
						break;
					default:
						throw new NotSupportedException(string.Format("Unsupported node type: {0}", type.ToString()));
				}
				return nodeB;
			}

			YamlSequenceNode sequence = node as YamlSequenceNode;
			if (sequence != null)
			{
				YamlNode nodeB;
				switch (type)
				{
					case YamlNodeType.Mapping:
						nodeB = new YamlMappingNode();
						sequence.Add(nodeB);
						break;
					case YamlNodeType.Scalar:
						nodeB = new YamlScalarNode();
						sequence.Add(nodeB);
						break;
					case YamlNodeType.Sequence:
						nodeB = new YamlSequenceNode(key);
						sequence.Add(nodeB);
						break;
					default:
						throw new NotSupportedException(string.Format("Unsupported node type: {0}", type.ToString()));
				}
				return nodeB;
			}

			return null;
		}


		public static void Remove(this YamlNode node, string key)
		{
			if (node == null)
				throw new ArgumentNullException(string.Format("Unsupported node type: {0} for 'node'", null));

			YamlScalarNode scalar = node as YamlScalarNode;
			if (scalar != null)
			{
				throw new ArgumentException(string.Format("Node of type {0} cannot have a key", node.GetType().Name));
			}

			YamlMappingNode mapping = node as YamlMappingNode;
			if (mapping != null)
			{
				mapping.Children.Remove(key);
			}

			YamlSequenceNode sequence = node as YamlSequenceNode;
			if (sequence != null)
			{
				sequence.Children.Remove(key);
			}
		}

		public static void Put(this YamlNode node, string key, object value)
		{
			if (node == null)
				throw new ArgumentNullException(string.Format("Unsupported node type: {0} for 'node'", null));
			if (value == null)
				throw new ArgumentNullException(string.Format("Value is {0} use the 'Remove()' function instead", null));

			YamlScalarNode scalar = node as YamlScalarNode;
			if (scalar != null)
			{
				throw new ArgumentException(string.Format("Node of type {0} cannot have a key", node.GetType().Name));
			}

			YamlNode valueNode = node.NodeByKey(key);

			YamlMappingNode mapping = node as YamlMappingNode;
			if (mapping != null && mapping.Children.Count > 0)
			{
				if (valueNode == null)
				{
					mapping.Children.Add(
						new YamlScalarNode(key),
						ConvertValueToYaml(value)
					);
				}
				else
				{
					mapping.Children[key] = ConvertValueToYaml(value);
				}
			}

			YamlSequenceNode sequence = node as YamlSequenceNode;
			if (sequence != null)
			{
				if (valueNode == null)
				{
					sequence.Children.Add(
						ConvertValueToYaml(value)
					);
				}
				else
				{
					int index = sequence.Children.ToList().FindIndex(y => ReferenceEquals(y, valueNode));
					sequence.Children[index] = ConvertValueToYaml(value);
				}
			}
		}


		public static YamlNode ConvertValueToYaml(object value)
		{
			TextReader reader = new StringReader(new Serializer().Serialize(value));
			YamlStream stream = new YamlStream();
			stream.Load(reader);
			YamlDocument yaml = stream.Documents[0];
			return yaml.RootNode;
		}

	}
}
