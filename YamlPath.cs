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
			Slot slot = FindSlot(node, key);
			return slot == null ? null : slot.Value;
		}

		// Where a child lives in its parent: a mapping entry or a position in a sequence.
		// A sequence item found by its first key ("- Special: ...") lives in that item's mapping.
		private class Slot
		{
			public YamlSequenceNode Sequence;
			public int Index;
			public YamlMappingNode Mapping;
			public YamlNode Key;

			public YamlNode Value
			{
				get { return Mapping != null ? Mapping.Children[Key] : Sequence.Children[Index]; }
				set
				{
					if (Mapping != null)
						Mapping.Children[StoredKey()] = value;
					else
						Sequence.Children[Index] = value;
				}
			}

			// The key node as it is in the file, so replacing a value keeps the key's quotes
			private YamlNode StoredKey()
			{
				foreach (KeyValuePair<YamlNode, YamlNode> entry in Mapping.Children)
				{
					if (entry.Key.Equals(Key))
						return entry.Key;
				}
				return Key;
			}

			public void Remove()
			{
				if (Mapping == null)
				{
					Sequence.Children.RemoveAt(Index);
					return;
				}
				Mapping.Children.Remove(Key);
				// Drop the whole "- Key: ..." item once nothing is left in it
				if (Sequence != null && Mapping.Children.Count == 0)
					Sequence.Children.RemoveAt(Index);
			}
		}

		private static Slot FindSlot(YamlNode node, string key)
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
				// Plain keys are found with a hash lookup, which keeps big mappings fast.
				// Tagged or complex keys fall back to comparing every key's text.
				YamlScalarNode plainKey = new YamlScalarNode(key);
				if (mapping.Children.ContainsKey(plainKey))
					return new Slot { Mapping = mapping, Key = plainKey };

				foreach (KeyValuePair<YamlNode, YamlNode> entry in mapping.Children)
				{
					if (entry.Key.ToString().Equals(key))
					{
						return new Slot { Mapping = mapping, Key = entry.Key };
					}
				}
				return null;
			}

			YamlSequenceNode sequence = node as YamlSequenceNode;
			if (sequence != null)
			{
				IList<YamlNode> children = sequence.Children;
				int index = GetIndexByKey(key);
				if (IsSequenceInception(key))
				{
					index = IndexOfNestedSequence(sequence, index);
					return index == -1 ? null : new Slot { Sequence = sequence, Index = index };
				}
				else if (index != -1)
				{
					if (index < 0 || index >= children.Count)
						return null;
					return new Slot { Sequence = sequence, Index = index };
				}

				for (int i = 0; i < children.Count; i++)
				{
					YamlScalarNode keyNode = children[i] as YamlScalarNode;
					if (keyNode != null && string.Equals(keyNode.Value, key))
					{
						return new Slot { Sequence = sequence, Index = i };
					}

					YamlMappingNode mapNode = children[i] as YamlMappingNode;
					if (mapNode != null && mapNode.Children.Count > 0 && mapNode.Children[0].Key.ToString().Equals(key))
					{
						return new Slot { Sequence = sequence, Index = i, Mapping = mapNode, Key = mapNode.Children[0].Key };
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

		// ":N" and ":N:" always point into a sequence
		private static bool IsIndexKey(string key)
		{
			return key.StartsWith(":") && GetIndexByKey(key) != -1;
		}

		// Position of the Nth item that is itself a sequence, or -1
		private static int IndexOfNestedSequence(YamlSequenceNode sequence, int index)
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
			AddChild(node, key, nodeB);
			return nodeB;
		}

		// Returns the node at key so a path can continue into it. A missing node, or a scalar
		// (which can't hold keys), is replaced in place by an empty mapping or sequence.
		internal static YamlNode GetOrCreateContainer(this YamlNode node, string key, string nextKey)
		{
			Slot slot = FindSlot(node, key);
			if (slot != null && !(slot.Value is YamlScalarNode))
				return slot.Value;

			// ":N:" in a sequence is a nested sequence, and a ":N" index needs a sequence to point into
			YamlNode container = (node is YamlSequenceNode && IsSequenceInception(key)) || IsIndexKey(nextKey)
				? new YamlSequenceNode()
				: new YamlMappingNode();

			if (slot == null)
				AddChild(node, key, container);
			// A scalar item matched by name ("- ROOM0") becomes "- ROOM0: ..." so the path still finds it
			else if (slot.Mapping == null && GetIndexByKey(key) == -1)
				slot.Value = new YamlMappingNode(new YamlScalarNode(key), container);
			else
				slot.Value = container;
			return container;
		}

		// Adds a child under a key that doesn't exist yet. In a sequence an index
		// appends at the end, and a name appends "- key: child" so it can be found by name.
		private static void AddChild(YamlNode node, string key, YamlNode child)
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
				mapping.Add(new YamlScalarNode(key), child);
				return;
			}

			YamlSequenceNode sequence = node as YamlSequenceNode;
			if (sequence != null)
			{
				int index = GetIndexByKey(key);
				if (index == -1)
				{
					sequence.Add(new YamlMappingNode(new YamlScalarNode(key), child));
					return;
				}
				int count = IsSequenceInception(key)
					? sequence.Children.Count(c => c is YamlSequenceNode)
					: sequence.Children.Count;
				if (index != count)
					throw new ArgumentOutOfRangeException(nameof(key), string.Format("Index {0} is past the end of the sequence ({1} items)", index, count));
				sequence.Add(child);
				return;
			}
			throw new NotSupportedException(string.Format("Unsupported node type: {0}", node.GetType().Name));
		}


		public static void Remove(this YamlNode node, string key)
		{
			Slot slot = FindSlot(node, key);
			if (slot != null)
				slot.Remove();
		}

		public static void Put(this YamlNode node, string key, object value)
		{
			if (value == null)
				throw new ArgumentNullException(string.Format("Value is {0} use the 'Remove()' function instead", null));

			YamlNode valueNode = ConvertValueToYaml(value);
			Slot slot = FindSlot(node, key);
			if (slot != null)
				slot.Value = valueNode;
			else
				AddChild(node, key, valueNode);
		}


		// Building a serializer is expensive, so every call shares one
		private static readonly ISerializer serializer = new Serializer();

		public static YamlNode ConvertValueToYaml(object value)
		{
			TextReader reader = new StringReader(serializer.Serialize(value));
			YamlStream stream = new YamlStream();
			stream.Load(reader);
			YamlDocument yaml = stream.Documents[0];
			return yaml.RootNode;
		}

	}
}
