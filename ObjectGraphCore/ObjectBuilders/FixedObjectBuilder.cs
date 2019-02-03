using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Class to return a fixed object for all addresses
	/// </summary>
	/// <typeparam name="TAddress">The type of the address</typeparam>
	public sealed class FixedObjectBuilder<TAddress> : IObjectBuilder<TAddress>
	{
		#region Data Model

		public object FixedObject { get; private set; }

		#endregion

		#region Construction Logic

		public FixedObjectBuilder( object fixedObject )
		{
			this.FixedObject = fixedObject;
		}

		#endregion

		#region IObjectBuilder members

		public IDependencySet<TAddress> GetDependencies( IObjectBuilderGetDependenciesContext<TAddress> context, TAddress address)
		{
			return null;
		}

		public object BuildObject( IObjectBuilderBuildObjectContext<TAddress> context, TAddress address, IBuiltDependencies<TAddress> dependencies )
		{
			return this.FixedObject;
		}

		public IDependencySet<TAddress> GetAdditionalDependencies( IObjectBuilderGetAdditionalDependenciesContext<TAddress> context, TAddress address, IBuiltDependencies<TAddress> dependencies )
		{
			return null;
		}

		#endregion
	}
}
