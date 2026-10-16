using System;
using System.Globalization;
using System.Reflection;
using Xunit.Sdk;

namespace YAMLTest {
	// Runs each test of the annotated class or method under the given culture.
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class UseCultureAttribute : BeforeAfterTestAttribute {

		private readonly string name;
		private CultureInfo originalCulture;
		private CultureInfo originalUICulture;

		public UseCultureAttribute(string name)
		{
			this.name = name;
		}

		public override void Before(MethodInfo methodUnderTest)
		{
			originalCulture = CultureInfo.CurrentCulture;
			originalUICulture = CultureInfo.CurrentUICulture;
			CultureInfo.CurrentCulture = new CultureInfo(name);
			CultureInfo.CurrentUICulture = new CultureInfo(name);
		}

		public override void After(MethodInfo methodUnderTest)
		{
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUICulture;
		}
	}
}
