using Xunit;

namespace YAMLTest {
	// Pins the read behavior of the path rules, using the Rooms example from the README.
	public class GetNodeTests : YamlTestBase {

		private static readonly string Rooms = Yaml(
			"Rooms:",
			"- ROOM0",
			"- Special:",
			"  - NORMAL0",
			"  - - DEEP0",
			"    - DEEP1",
			"  - NORMAL1",
			"  - NORMAL2",
			"  - - - DEEP2",
			"      - DEEP3",
			"- ROOM1",
			"- ROOM2");

		[Theory]
		[InlineData("Rooms.ROOM0", "ROOM0")]
		[InlineData("Rooms.:0", "ROOM0")]
		[InlineData("Rooms.0", "ROOM0")]
		[InlineData("Rooms.:2", "ROOM1")]
		[InlineData("Rooms.Special", "[ NORMAL0, [ DEEP0, DEEP1 ], NORMAL1, NORMAL2, [ [ DEEP2, DEEP3 ] ] ]")]
		[InlineData("Rooms.:1", "{ { Special, [ NORMAL0, [ DEEP0, DEEP1 ], NORMAL1, NORMAL2, [ [ DEEP2, DEEP3 ] ] ] } }")]
		[InlineData("Rooms.Special.:0", "NORMAL0")]
		[InlineData("Rooms.Special.:1", "[ DEEP0, DEEP1 ]")]
		[InlineData("Rooms.Special.:2", "NORMAL1")]
		[InlineData("Rooms.Special.NORMAL2", "NORMAL2")]
		[InlineData("Rooms.Special.:0:.DEEP0", "DEEP0")]
		[InlineData("Rooms.Special.:0:.:0", "DEEP0")]
		[InlineData("Rooms.Special.:1:", "[ [ DEEP2, DEEP3 ] ]")]
		[InlineData("Rooms.Special.:1:.:0", "[ DEEP2, DEEP3 ]")]
		[InlineData("Rooms.Special.:1:.:0:.:1", "DEEP3")]
		public void ResolvesPath(string path, string expected)
		{
			Assert.Equal(expected, Load(Rooms).GetNode(path).ToString());
		}

		[Theory]
		[InlineData("Nope")]
		[InlineData("Rooms.Nope")]
		[InlineData("Rooms.:9")]
		[InlineData("Rooms.:-2")]
		[InlineData("Rooms.:2:")]
		[InlineData("Rooms.Special.:2:")]
		[InlineData("Rooms.Special.Nope.Deeper")]
		public void ReturnsNullForMissingPath(string path)
		{
			Assert.Null(Load(Rooms).GetNode(path));
		}
	}
}
