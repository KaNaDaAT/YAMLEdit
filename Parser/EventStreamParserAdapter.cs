using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YAMLEdit.Parser {
	public class EventStreamParserAdapter : IParser {

		private readonly IEnumerator<ParsingEvent> enumerator;


		public EventStreamParserAdapter(IEnumerable<ParsingEvent> events) {
			enumerator = events.GetEnumerator();
		}

		public ParsingEvent Current {
			get {
				return enumerator.Current;
			}
		}

		public bool MoveNext() {
			return enumerator.MoveNext();
		}

	}
}
