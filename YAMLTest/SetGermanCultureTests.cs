using System.Globalization;
using Xunit;

namespace YAMLTest {
	// Runs every SetTests case again under a culture that writes "3,5" for 3.5.
	[UseCulture("de-DE")]
	public class SetGermanCultureTests : SetTests {

		[Fact]
		public void RunsUnderGermanCulture()
		{
			Assert.Equal("de-DE", CultureInfo.CurrentCulture.Name);
			Assert.Equal(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
		}
	}
}
