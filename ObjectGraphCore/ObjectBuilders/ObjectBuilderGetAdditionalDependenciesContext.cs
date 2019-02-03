using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Standard implementation of <see cref="IObjectBuilderGetDependenciesContext{TAddress}"/>
	/// </summary>
	/// <typeparam name="TAddress">The type of the address objects</typeparam>
	public class ObjectBuilderGetAdditionalDependenciesContext<TAddress> : IObjectBuilderGetAdditionalDependenciesContext<TAddress>
	{
		#region Data Model

		public IObjectContext<TAddress> ObjectContext { get; private set; }

		#endregion

		public ObjectBuilderGetAdditionalDependenciesContext( IObjectContext<TAddress> objectContext )
		{
			if( objectContext == null )
				throw new ArgumentNullException( nameof( objectContext ) );

			this.ObjectContext = objectContext;
		}
	}
}
