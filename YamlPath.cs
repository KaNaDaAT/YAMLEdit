using System;
using System.IO;
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
				int index = IndexOfKey(mapping, key);
				return index != -1 ? mapping.Children[index].Value : null;
			}

			YamlSequenceNode sequence = node as YamlSequenceNode;
			if (sequence != null)
			{
				int index = IndexOfKey(sequence, key);
				if (index == -1)
					return null;
				// A name that matched a list-item mapping addresses the value of its first key.
				YamlMappingNode item = sequence.Children[index] as YamlMappingNode;
				if (item != null && IsName(key))
					return item.Children[0].Value;
				return sequence.Children[index];
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

		// Whether key addresses a sequence item by name rather than by position (N, :N or :N:).
		private static bool IsName(string key)
		{
			return !IsSequenceInception(key) && GetIndexByKey(key) == -1;
		}

		// The type of node to create at key so that nextKey can be resolved inside it:
		// a sequence when key is :N: or nextKey is :N or :N:, a mapping otherwise.
		internal static YamlNodeType ContainerTypeFor(string key, string nextKey)
		{
			if (IsSequenceInception(key) || (nextKey.StartsWith(":") && GetIndexByKey(nextKey) >= 0))
				return YamlNodeType.Sequence;
			return YamlNodeType.Mapping;
		}

		// Position of the entry whose key is key, or -1.
		private static int IndexOfKey(YamlMappingNode mapping, string key)
		{
			for (int i = 0; i < mapping.Children.Count; i++)
			{
				if (mapping.Children[i].Key.ToString().Equals(key))
					return i;
			}
			return -1;
		}

		// Position of the item key addresses: the Nth nested sequence for :N:, the Nth item for N or :N,
		// otherwise the first scalar equal to key or mapping whose first key is key. -1 if there is none.
		private static int IndexOfKey(YamlSequenceNode sequence, string key)
		{
			int index = GetIndexByKey(key);
			if (IsSequenceInception(key))
			{
				int sequenceIndex = 0;
				for (int i = 0; i < sequence.Children.Count; i++)
				{
					if (sequence.Children[i] is YamlSequenceNode)
					{
						if (index == sequenceIndex)
							return i;
						sequenceIndex++;
					}
				}
				return -1;
			}
			if (index != -1)
				return index >= 0 && index < sequence.Children.Count ? index : -1;
			for (int i = 0; i < sequence.Children.Count; i++)
			{
				if (NameOf(sequence.Children[i]) == key)
					return i;
			}
			return -1;
		}

		// The name a sequence item is matched by: a scalar's value, or a mapping's first key.
		private static string NameOf(YamlNode item)
		{
			YamlScalarNode scalar = item as YamlScalarNode;
			if (scalar != null)
				return scalar.Value;
			YamlMappingNode mapping = item as YamlMappingNode;
			if (mapping != null && mapping.Children.Count > 0)
				return (mapping.Children[0].Key as YamlScalarNode)?.Value;
			return null;
		}

		// Puts child where key addresses it in node, replacing what is there, or adds it if nothing is.
		// With keepName, a scalar item matched by name becomes "key: child", so that key still finds child.
		private static void Store(YamlNode node, string key, YamlNode child, bool keepName)
		{
			YamlMappingNode mapping = node as YamlMappingNode;
			if (mapping != null)
			{
				int index = IndexOfKey(mapping, key);
				// Assign through the key: OrderedDictionary's int setter leaves its lookup table stale.
				if (index != -1)
					mapping.Children[mapping.Children[index].Key] = child;
				else
					mapping.Add(new YamlScalarNode(key), child);
			}

			YamlSequenceNode sequence = node as YamlSequenceNode;
			if (sequence != null)
			{
				int index = IndexOfKey(sequence, key);
				bool name = IsName(key);
				YamlMappingNode item = index != -1 ? sequence.Children[index] as YamlMappingNode : null;
				if (index == -1)
					sequence.Add(name ? new YamlMappingNode(new YamlScalarNode(key), child) : child);
				else if (name && item != null)
					item.Children[item.Children[0].Key] = child;
				else if (name && keepName)
					sequence.Children[index] = new YamlMappingNode(new YamlScalarNode(key), child);
				else
					sequence.Children[index] = child;
			}
		}


		public static YamlNode CreateKey(this YamlNode node, string key)
		{
			if (IsSequenceInception(key))
			{
				return CreateKey(node, key, YamlNodeType.Sequence);
			}
			return CreateKey(node, key, YamlNodeType.Mapping);
		}

		// Puts a new empty node of type where key addresses it, replacing what is there, and returns it.
		public static YamlNode CreateKey(this YamlNode node, string key, YamlNodeType type)
		{
			if (node == null)
				throw new ArgumentNullException(string.Format("Unsupported node type: {0} for 'node'", null));

			YamlScalarNode scalar = node as YamlScalarNode;
			if (scalar != null)
			{
				throw new ArgumentException(string.Format("Node of type {0} cannot have a key", node.GetType().Name));
			}

			YamlNode nodeB;
			switch (type)
			{
				case YamlNodeType.Mapping:
					nodeB = new YamlMappingNode();
					break;
				case YamlNodeType.Scalar:
					nodeB = new YamlScalarNode();
					break;
				case YamlNodeType.Sequence:
					nodeB = new YamlSequenceNode();
					break;
				default:
					throw new NotSupportedException(string.Format("Unsupported node type: {0}", type.ToString()));
			}
			Store(node, key, nodeB, true);
			return nodeB;
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
				int index = IndexOfKey(mapping, key);
				if (index != -1)
					mapping.Children.RemoveAt(index);
			}

			YamlSequenceNode sequence = node as YamlSequenceNode;
			if (sequence != null)
			{
				int index = IndexOfKey(sequence, key);
				if (index == -1)
					return;
				// A name that matched a list-item mapping removes that first key, and the item once it is empty.
				YamlMappingNode item = sequence.Children[index] as YamlMappingNode;
				if (item != null && IsName(key))
				{
					item.Children.RemoveAt(0);
					if (item.Children.Count > 0)
						return;
				}
				sequence.Children.RemoveAt(index);
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

			Store(node, key, ConvertValueToYaml(value), false);
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
