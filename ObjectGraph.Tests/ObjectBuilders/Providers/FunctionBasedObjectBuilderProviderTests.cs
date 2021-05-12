using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders.Providers
{
	/// <summary>
	/// Summary description for FunctionBasedObjectBuilderProviderTests
	/// </summary>
	[TestClass]
	public class FunctionBasedObjectBuilderProviderTests
	{
		[TestMethod]
		public void CheckProviderBehaviour()
		{
			var provider = new FunctionBasedObjectBuilderProvider<int>(
				( address ) =>
				{
					switch( address )
					{
						case 0:
							return new FixedObjectBuilder<int>( 0 );

						default:
							return null;
					}
				} );

			Assert.IsTrue( provider.TryGetObjectBuilder( 0, out var objectBuilder ) );
			Assert.IsNotNull( objectBuilder );

			Assert.IsFalse( provider.TryGetObjectBuilder( 1, out objectBuilder ) );
		}
	}
}
