using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Xunit;

namespace YAMLEdit.Tests
{
	public class ConfigFileTests : IDisposable
	{
		// The "Rooms" file from the README
		private const string Rooms =
			"Rooms:\n" +
			"- ROOM0\n" +
			"- Special:\n" +
			"  - NORMAL0\n" +
			"  - - DEEP0\n" +
			"    - DEEP1\n" +
			"  - NORMAL1\n" +
			"  - NORMAL2\n" +
			"  - - - DEEP2\n" +
			"      - DEEP3\n" +
			"- ROOM1\n" +
			"- ROOM2\n";

		private const string Person =
			"Person:\n" +
			"  Name: John Doe\n" +
			"  Age: 30\n" +
			"  Sex: Male\n";

		private readonly string dir = Path.Combine(Path.GetTempPath(), "YAMLEdit.Tests", Guid.NewGuid().ToString());

		public ConfigFileTests()
		{
			Directory.CreateDirectory(dir);
		}

		public void Dispose()
		{
			Directory.Delete(dir, true);
		}

		private ConfigFile Load(string content)
		{
			string path = Path.Combine(dir, "config.yaml");
			File.WriteAllText(path, content);
			return new ConfigFile(path);
		}

		// Saves and loads the file again, so tests check what really ends up on disk
		private static ConfigFile Reload(ConfigFile config)
		{
			config.Save();
			return new ConfigFile(config.FILE_PATH);
		}

		#region Reading

		[Theory]
		[InlineData("Rooms.ROOM0", "ROOM0")]
		[InlineData("Rooms.:0", "ROOM0")]
		[InlineData("Rooms.0", "ROOM0")]
		[InlineData("Rooms.Special.:0", "NORMAL0")]
		[InlineData("Rooms.Special.:1", "[ DEEP0, DEEP1 ]")]
		[InlineData("Rooms.Special.:2", "NORMAL1")]
		[InlineData("Rooms.Special.:0:.DEEP0", "DEEP0")]
		[InlineData("Rooms.Special.:0:.:0", "DEEP0")]
		[InlineData("Rooms.Special.:1:", "[ [ DEEP2, DEEP3 ] ]")]
		[InlineData("Rooms.Special.:1:.:0", "[ DEEP2, DEEP3 ]")]
		[InlineData("Rooms.Special.:1:.:0:.:1", "DEEP3")]
		public void GetNode_FollowsReadmeExamples(string path, string expected)
		{
			Assert.Equal(expected, Load(Rooms).GetNode(path).ToString());
		}

		[Theory]
		[InlineData("Rooms.Nope")]
		[InlineData("Rooms.:9")]
		[InlineData("Rooms.:-2")]
		[InlineData("Rooms.Special.:5:")]
		public void GetNode_ReturnsNullWhenNothingIsThere(string path)
		{
			Assert.Null(Load(Rooms).GetNode(path));
		}

		[Fact]
		public void Get_ReadsTypedValues()
		{
			ConfigFile config = Load(Person);

			Assert.Equal("John Doe", config.Get<string>("Person.Name"));
			Assert.Equal(30, config.Get<int>("Person.Age"));
			Assert.Equal("Male", config.Get<Dictionary<string, string>>("Person")["Sex"]);
		}

		[Fact]
		public void Get_ThrowsKeyNotFoundWhenNothingIsThere()
		{
			ConfigFile config = Load(Person);

			KeyNotFoundException error = Assert.Throws<KeyNotFoundException>(() => config.Get<int>("Person.Height"));
			Assert.Contains("Person.Height", error.Message);
		}

		#endregion

		#region Loading and saving

		[Fact]
		public void MissingFile_StartsEmpty_AndSaveCreatesIt()
		{
			string path = Path.Combine(dir, "new.yaml");

			ConfigFile config = new ConfigFile(path);
			Assert.False(config.Loaded);
			config.Set("Name", "KaNaDaAT");
			config.Save();

			Assert.True(File.Exists(path));
			Assert.Equal("KaNaDaAT", new ConfigFile(path).Get<string>("Name"));
		}

		[Fact]
		public void EmptyFile_StartsEmpty()
		{
			ConfigFile config = Load("");

			Assert.False(config.Loaded);
			config.Set("Name", "KaNaDaAT");
			Assert.Equal("KaNaDaAT", Reload(config).Get<string>("Name"));
		}

		[Fact]
		public void Save_CanWriteToANewPath()
		{
			string path = Path.Combine(dir, "copy.yaml");

			Load(Person).Save(path);

			Assert.Equal(30, new ConfigFile(path).Get<int>("Person.Age"));
		}

		[Fact]
		public void ReadmeGettingStarted_Works()
		{
			ConfigFile config = new ConfigFile(Path.Combine(dir, "persons.yaml"));
			config.Set("Persons.1.Name", "KaNaDaAT");
			config.Set("Persons.1.Age", 21);
			config.Set("Persons.1.Age", 22);

			config = Reload(config);

			Assert.Equal("KaNaDaAT", config.Get<string>("Persons.1.Name"));
			Assert.Equal(22, config.Get<int>("Persons.1.Age"));
		}

		[Fact]
		public void ReadmeExample2_UpdatesValues()
		{
			ConfigFile config = Load(Person);
			config.Set("Person.Name", "Jane Smith");
			config.Set("Person.Age", 32);

			config = Reload(config);

			Assert.Equal("Jane Smith", config.Get<string>("Person.Name"));
			Assert.Equal(32, config.Get<int>("Person.Age"));
			Assert.Equal("Male", config.Get<string>("Person.Sex"));
		}

		#endregion

		#region Set on mappings

		[Fact]
		public void Set_CreatesMissingParents()
		{
			ConfigFile config = Load(Person);
			config.Set("Person.Address.City", "Vienna");

			Assert.Equal("Vienna", Reload(config).Get<string>("Person.Address.City"));
		}

		[Fact]
		public void Set_CanReplaceAMappingWithAScalar()
		{
			ConfigFile config = Load(Person);
			config.Set("Person", "nobody");

			Assert.Equal("nobody", Reload(config).Get<string>("Person"));
		}

		[Fact]
		public void Set_TurnsAScalarParentIntoAMapping()
		{
			ConfigFile config = Load(Person);
			config.Set("Person.Name.First", "John");

			config = Reload(config);
			Assert.Equal("John", config.Get<string>("Person.Name.First"));
			Assert.Equal(30, config.Get<int>("Person.Age"));
		}

		[Fact]
		public void Set_KeepsTheQuotesOfAnExistingKey()
		{
			ConfigFile config = Load("\"Name\": John\n");
			config.Set("Name", "Jane");
			config.Save();

			Assert.Contains("\"Name\": Jane", File.ReadAllText(config.FILE_PATH));
		}

		[Fact]
		public void GetNode_FindsTaggedKeys()
		{
			ConfigFile config = Load("!!str 1: one\nplain: two\n");

			Assert.Equal("one", config.GetNode("1").ToString());
			Assert.Equal("two", config.GetNode("plain").ToString());
		}

		[Fact]
		public void Set_Null_RemovesAKey()
		{
			ConfigFile config = Load(Person);
			config.Set("Person.Sex", null);

			config = Reload(config);
			Assert.Null(config.GetNode("Person.Sex"));
			Assert.Equal(30, config.Get<int>("Person.Age"));
		}

		#endregion

		#region Set on sequences

		[Fact]
		public void Set_ReplacesASequenceItemByIndex()
		{
			ConfigFile config = Load(Rooms);
			config.Set("Rooms.:2", "LOBBY");

			Assert.Equal("LOBBY", Reload(config).GetNode("Rooms.:2").ToString());
		}

		[Fact]
		public void Set_ReplacesASequenceItemByName_InPlace()
		{
			ConfigFile config = Load("L:\n- x\n- y\n- z\n");
			config.Set("L.y", "w");

			Assert.Equal(new List<string> { "x", "w", "z" }, Reload(config).Get<List<string>>("L"));
		}

		[Fact]
		public void Set_ReplacesTheValueOfAKeyedSequenceItem()
		{
			ConfigFile config = Load(Rooms);
			config.Set("Rooms.Special", new List<string> { "A", "B" });

			config = Reload(config);
			Assert.Equal(new List<string> { "A", "B" }, config.Get<List<string>>("Rooms.Special"));
			Assert.Equal("ROOM1", config.GetNode("Rooms.:2").ToString());
		}

		[Fact]
		public void Set_Null_RemovesASequenceItemByIndex()
		{
			ConfigFile config = Load("L:\n- x\n- y\n- z\n");
			config.Set("L.:1", null);

			Assert.Equal(new List<string> { "x", "z" }, Reload(config).Get<List<string>>("L"));
		}

		[Fact]
		public void Set_Null_RemovesASequenceItemByName()
		{
			ConfigFile config = Load(Rooms);
			config.Set("Rooms.ROOM1", null);

			config = Reload(config);
			Assert.Null(config.GetNode("Rooms.ROOM1"));
			Assert.Equal("ROOM2", config.GetNode("Rooms.:2").ToString());
		}

		[Fact]
		public void Set_Null_RemovesAKeyedSequenceItem()
		{
			ConfigFile config = Load(Rooms);
			config.Set("Rooms.Special", null);

			Assert.Equal(new List<string> { "ROOM0", "ROOM1", "ROOM2" }, Reload(config).Get<List<string>>("Rooms"));
		}

		[Fact]
		public void Set_TurnsAScalarItemIntoAMapping_InPlace()
		{
			ConfigFile config = Load("L:\n- x\n- y\n");
			config.Set("L.:0.k", 1);

			config = Reload(config);
			Assert.Equal(1, config.Get<int>("L.:0.k"));
			Assert.Equal("y", config.GetNode("L.:1").ToString());
		}

		[Fact]
		public void Set_KeepsTheNameOfAScalarItemItWalksInto()
		{
			ConfigFile config = Load(Rooms);
			config.Set("Rooms.ROOM0.Size", 5);

			config = Reload(config);
			Assert.Equal(5, config.Get<int>("Rooms.ROOM0.Size"));
			Assert.Equal(5, config.Get<int>("Rooms.:0.ROOM0.Size"));
		}

		[Fact]
		public void Set_AddsAKeyedItemForANewName()
		{
			ConfigFile config = Load(Rooms);
			config.Set("Rooms.Garden.Size", 7);

			config = Reload(config);
			Assert.Equal(7, config.Get<int>("Rooms.Garden.Size"));
			Assert.Equal(5, config.Get<List<object>>("Rooms").Count);
		}

		[Fact]
		public void Set_IndexKey_CreatesASequence()
		{
			ConfigFile config = Load(Person);
			config.Set("Person.Pets.:0.Name", "Rex");

			config = Reload(config);
			Assert.Equal("Rex", config.Get<string>("Person.Pets.:0.Name"));
			Assert.Single(config.Get<List<object>>("Person.Pets"));
		}

		[Fact]
		public void Set_NestedSequenceKey_CreatesANestedSequence()
		{
			ConfigFile config = Load(Person);
			config.Set("Person.Grid.:0:.:0", "a");
			config.Set("Person.Grid.:0:.:1", "b");

			config = Reload(config);
			Assert.Equal("[ [ a, b ] ]", config.GetNode("Person.Grid").ToString());
			Assert.Equal("b", config.Get<string>("Person.Grid.:0:.:1"));
		}

		[Fact]
		public void Set_IndexAtTheEnd_Appends()
		{
			ConfigFile config = Load("L:\n- x\n");
			config.Set("L.:1", "y");

			Assert.Equal(new List<string> { "x", "y" }, Reload(config).Get<List<string>>("L"));
		}

		[Fact]
		public void Set_IndexPastTheEnd_Throws()
		{
			ConfigFile config = Load("L:\n- x\n");

			Assert.Throws<ArgumentOutOfRangeException>(() => config.Set("L.:5", "y"));
		}

		#endregion

		#region Values

		[Fact]
		public void Set_CanReplaceAScalarWithAList()
		{
			ConfigFile config = Load(Person);
			config.Set("Person.Age", new List<int> { 1, 2 });

			Assert.Equal(new List<int> { 1, 2 }, Reload(config).Get<List<int>>("Person.Age"));
		}

		[Fact]
		public void Set_WritesNumbersTheSameWayInEveryCulture()
		{
			CultureInfo culture = CultureInfo.CurrentCulture;
			try
			{
				CultureInfo.CurrentCulture = new CultureInfo("de-DE");
				ConfigFile config = Load(Person);
				config.Set("Person.Age", 3.5);
				config.Set("Person.Height", 1.8);

				config = Reload(config);
				Assert.Equal(3.5, config.Get<double>("Person.Age"));
				Assert.Equal(1.8, config.Get<double>("Person.Height"));
			}
			finally
			{
				CultureInfo.CurrentCulture = culture;
			}
		}

		#endregion
	}
}
