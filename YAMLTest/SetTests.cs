using System.Collections.Generic;
using YamlDotNet.RepresentationModel;
using YAMLEdit;
using Xunit;

namespace YAMLTest {
	public class SetTests : YamlTestBase {

		private static readonly string Start = Yaml(
			"A:",
			"  B:",
			"    C: 1",
			"  L:",
			"  - x",
			"  - y");

		[Fact]
		public void ReplacesScalar()
		{
			ConfigFile config = Load(Start);
			config.Set("A.B.C", 2);
			Assert.Equal(2, config.Get<int>("A.B.C"));
		}

		[Fact]
		public void ReplacesMappingWithScalarInPlace()
		{
			ConfigFile config = Load(Start);
			config.Set("A.B", "str");
			Assert.Equal(Yaml(
				"A:",
				"  B: str",
				"  L:",
				"  - x",
				"  - y"), Saved(config));
		}

		[Fact]
		public void ReplacesSequenceWithScalarInPlace()
		{
			ConfigFile config = Load(Start);
			config.Set("A.L", "str");
			Assert.Equal(Yaml(
				"A:",
				"  B:",
				"    C: 1",
				"  L: str"), Saved(config));
		}

		[Theory]
		[InlineData("A.L.:0", "y")]
		[InlineData("A.L.0", "y")]
		[InlineData("A.L.x", "y")]
		[InlineData("A.L.:1", "x")]
		[InlineData("A.L.1", "x")]
		[InlineData("A.L.y", "x")]
		public void NullRemovesSequenceItem(string path, string remaining)
		{
			ConfigFile config = Load(Start);
			config.Set(path, null);
			Assert.Equal(Yaml(
				"A:",
				"  B:",
				"    C: 1",
				"  L:",
				"  - " + remaining), Saved(config));
		}

		[Fact]
		public void NullRemovesNestedSequenceByInceptionKey()
		{
			ConfigFile config = Load(Yaml(
				"L:",
				"- x",
				"- - a",
				"- - b"));
			config.Set("L.:1:", null);
			Assert.Equal(Yaml(
				"L:",
				"- x",
				"- - a"), Saved(config));
		}

		[Fact]
		public void NullRemovesListItemMatchedByFirstKey()
		{
			ConfigFile config = Load(Yaml(
				"L:",
				"- x",
				"- k: 1",
				"- m: 2",
				"  n: 3"));
			config.Set("L.k", null);
			config.Set("L.m", null);
			Assert.Equal(Yaml(
				"L:",
				"- x",
				"- n: 3"), Saved(config));
		}

		[Fact]
		public void NullRemovesMappingKey()
		{
			ConfigFile config = Load(Start);
			config.Set("A.B.C", null);
			Assert.Equal(Yaml(
				"A:",
				"  B: {}",
				"  L:",
				"  - x",
				"  - y"), Saved(config));
		}

		[Theory]
		[InlineData("A.Q")]
		[InlineData("A.Q.R")]
		[InlineData("A.L.z")]
		[InlineData("A.L.:5")]
		[InlineData("A.L.:0:")]
		[InlineData("A.L.x.k")]
		[InlineData("A.B.C.D")]
		public void NullOnMissingPathChangesNothing(string path)
		{
			ConfigFile config = Load(Start);
			config.Set(path, null);
			Assert.Equal(Start, Saved(config));
		}

		[Theory]
		[InlineData("A.L.y")]
		[InlineData("A.L.:1")]
		[InlineData("A.L.1")]
		public void ReplacesSequenceItemInPlace(string path)
		{
			ConfigFile config = Load(Start);
			config.Set(path, "w");
			Assert.Equal(Yaml(
				"A:",
				"  B:",
				"    C: 1",
				"  L:",
				"  - x",
				"  - w"), Saved(config));
		}

		[Fact]
		public void ReplacesValueOfListItemMatchedByFirstKey()
		{
			ConfigFile config = Load(Yaml(
				"L:",
				"- x",
				"- k: 1",
				"  m: 2"));
			config.Set("L.k", 3);
			Assert.Equal(Yaml(
				"L:",
				"- x",
				"- k: 3",
				"  m: 2"), Saved(config));
		}

		[Fact]
		public void AppendsSequenceItemForMissingName()
		{
			ConfigFile config = Load(Start);
			config.Set("A.L.z", 5);
			Assert.Equal(5, config.Get<int>("A.L.z"));
			Assert.Equal(Yaml(
				"A:",
				"  B:",
				"    C: 1",
				"  L:",
				"  - x",
				"  - y",
				"  - z: 5"), Saved(config));
		}

		[Fact]
		public void AppendsSequenceItemForMissingIndex()
		{
			ConfigFile config = Load(Start);
			config.Set("A.L.:2", "z");
			Assert.Equal(Yaml(
				"A:",
				"  B:",
				"    C: 1",
				"  L:",
				"  - x",
				"  - y",
				"  - z"), Saved(config));
		}

		[Fact]
		public void ReplacesScalarSequenceItemWithMappingInPlace()
		{
			ConfigFile config = Load(Start);
			config.Set("A.L.:0.k", 1);
			Assert.Equal(1, config.Get<int>("A.L.:0.k"));
			Assert.Equal(Yaml(
				"A:",
				"  B:",
				"    C: 1",
				"  L:",
				"  - k: 1",
				"  - y"), Saved(config));
		}

		[Fact]
		public void KeysScalarSequenceItemMatchedByNameWhenDescending()
		{
			ConfigFile config = Load(Start);
			config.Set("A.L.x.k", 1);
			Assert.Equal(1, config.Get<int>("A.L.x.k"));
			Assert.Equal(Yaml(
				"A:",
				"  B:",
				"    C: 1",
				"  L:",
				"  - x:",
				"      k: 1",
				"  - y"), Saved(config));
		}

		[Fact]
		public void ReplacesScalarWithMappingInPlace()
		{
			ConfigFile config = Load(Yaml(
				"A:",
				"  C: 1",
				"  E: 2"));
			config.Set("A.C.D", 3);
			Assert.Equal(Yaml(
				"A:",
				"  C:",
				"    D: 3",
				"  E: 2"), Saved(config));
		}

		[Fact]
		public void CreatesNestedSequenceForInceptionKey()
		{
			ConfigFile config = Load(Start);
			config.Set("A.S.:0:.v", 1);
			Assert.Equal(1, config.Get<int>("A.S.:0:.v"));
			Assert.Equal(Yaml(
				"A:",
				"  B:",
				"    C: 1",
				"  L:",
				"  - x",
				"  - y",
				"  S:",
				"  - - v: 1"), Saved(config));
		}

		[Fact]
		public void CreatesSequenceForIndexKey()
		{
			ConfigFile config = Load(Start);
			config.Set("A.S.:0.v", 1);
			Assert.Equal(1, config.Get<int>("A.S.:0.v"));
			Assert.Equal(Yaml(
				"A:",
				"  B:",
				"    C: 1",
				"  L:",
				"  - x",
				"  - y",
				"  S:",
				"  - v: 1"), Saved(config));
		}

		[Fact]
		public void CreatesMappingForNameKey()
		{
			ConfigFile config = Load(Start);
			config.Set("A.M.k.v", 1);
			Assert.Equal(1, config.Get<int>("A.M.k.v"));
			Assert.Equal(Yaml(
				"A:",
				"  B:",
				"    C: 1",
				"  L:",
				"  - x",
				"  - y",
				"  M:",
				"    k:",
				"      v: 1"), Saved(config));
		}

		[Fact]
		public void ReadmeExampleRoundTrips()
		{
			ConfigFile config = Load(Start);
			config.Set("Persons.1.Name", "KaNaDaAT");
			config.Set("Persons.1.Age", 21);
			config.Set("Persons.1.Age", 22);
			Assert.Equal("KaNaDaAT", config.Get<string>("Persons.1.Name"));
			Assert.Equal(22, config.Get<int>("Persons.1.Age"));
		}

		[Fact]
		public void SerializesListValue()
		{
			ConfigFile config = Load(Start);
			config.Set("A.B.C", new List<int> { 1, 2 });
			Assert.Equal(new List<int> { 1, 2 }, config.Get<List<int>>("A.B.C"));
			Assert.Equal(Yaml(
				"A:",
				"  B:",
				"    C:",
				"    - 1",
				"    - 2",
				"  L:",
				"  - x",
				"  - y"), Saved(config));
		}

		[Fact]
		public void SerializesNumbersCultureInvariant()
		{
			ConfigFile config = Load(Start);
			config.Set("A.B.C", 3.5);
			config.Set("A.B.D", 4.25);
			Assert.Equal(3.5, config.Get<double>("A.B.C"));
			Assert.Equal(4.25, config.Get<double>("A.B.D"));
			Assert.Equal(Yaml(
				"A:",
				"  B:",
				"    C: 3.5",
				"    D: 4.25",
				"  L:",
				"  - x",
				"  - y"), Saved(config));
		}

		[Fact]
		public void TrailingInceptionKeyAddressesNestedSequence()
		{
			ConfigFile config = Load(Yaml(
				"L:",
				"- x",
				"- - b",
				"  - c"));
			config.Set("L.:0:", new List<string> { "z" });
			Assert.Equal(Yaml(
				"L:",
				"- x",
				"- - z"), Saved(config));
		}

		[Fact]
		public void PutAddsKeyToEmptyMapping()
		{
			YamlMappingNode mapping = new YamlMappingNode();
			mapping.Put("k", 1);
			Assert.Equal("1", mapping.NodeByKey("k").ToString());
		}

		[Fact]
		public void CreateKeyMakesEmptySequenceForInceptionKey()
		{
			YamlMappingNode mapping = new YamlMappingNode();
			YamlNode created = mapping.CreateKey(":0:");
			Assert.Empty(Assert.IsType<YamlSequenceNode>(created).Children);
		}
	}
}
