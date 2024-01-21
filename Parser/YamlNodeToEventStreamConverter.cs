using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace YAMLEdit.Parser {
	public static class YamlNodeToEventStreamConverter {

		public static IEnumerable<ParsingEvent> ConvertToEventStream(YamlStream stream) {
			yield return new StreamStart();
			foreach(YamlDocument document in stream.Documents) {
				foreach(ParsingEvent evt in ConvertToEventStream(document)) {
					yield return evt;
				}
			}
			yield return new StreamEnd();
		}

		public static IEnumerable<ParsingEvent> ConvertToEventStream(YamlDocument document) {
			yield return new DocumentStart();
			foreach(ParsingEvent evt in ConvertToEventStream(document.RootNode)) {
				yield return evt;
			}
			yield return new DocumentEnd(false);
		}

		public static IEnumerable<ParsingEvent> ConvertToEventStream(YamlNode node) {
			YamlScalarNode scalar = node as YamlScalarNode;
			if(scalar != null) {
				return ConvertToEventStream(scalar);
			}
			YamlSequenceNode sequence = node as YamlSequenceNode;
			if(sequence != null) {
				return ConvertToEventStream(sequence);
			}
			YamlMappingNode mapping = node as YamlMappingNode;
			if(mapping != null) {
				return ConvertToEventStream(mapping);
			}
			throw new NotSupportedException(string.Format("Unsupported node type: {0}", node.GetType().Name));
		}

		private static IEnumerable<ParsingEvent> ConvertToEventStream(YamlScalarNode scalar) {
			yield return new Scalar(scalar.Anchor, scalar.Tag, scalar.Value, scalar.Style, false, false);
		}

		private static IEnumerable<ParsingEvent> ConvertToEventStream(YamlSequenceNode sequence) {
			yield return new SequenceStart(sequence.Anchor, sequence.Tag, false, sequence.Style);
			foreach(YamlNode node in sequence.Children) {
				foreach(ParsingEvent evt in ConvertToEventStream(node)) {
					yield return evt;
				}
			}
			yield return new SequenceEnd();
		}

		private static IEnumerable<ParsingEvent> ConvertToEventStream(YamlMappingNode mapping) {
			yield return new MappingStart(mapping.Anchor, mapping.Tag, false, mapping.Style);
			foreach(KeyValuePair<YamlNode, YamlNode> pair in mapping.Children) {
				foreach(ParsingEvent evt in ConvertToEventStream(pair.Key)) {
					yield return evt;
				}
				foreach(ParsingEvent evt in ConvertToEventStream(pair.Value)) {
					yield return evt;
				}
			}
			yield return new MappingEnd();
		}
	}
}
