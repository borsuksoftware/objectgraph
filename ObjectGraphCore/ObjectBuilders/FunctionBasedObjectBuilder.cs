using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Object builder which bases its behaviour off the supplied functions
	/// </summary>
	/// <typeparam name="TAddress">The type of the address</typeparam>
	public class FunctionBasedObjectBuilder<TAddress> : IObjectBuilder<TAddress>
	{
		#region Data Model

		public Func<IObjectBuilderGetDependenciesContext<TAddress>, TAddress, IDependencySet<TAddress>> GetDependenciesFunc { get; private set; }

		public Func<IObjectBuilderGetAdditionalDependenciesContext<TAddress>, TAddress, IBuiltDependencies<TAddress>, IDependencySet<TAddress>> GetAdditionalDependenciesFunc { get; private set; }

		public Func<IObjectBuilderBuildObjectContext<TAddress>, TAddress, IBuiltDependencies<TAddress>, object> BuildObjectFunc { get; private set; }

		#endregion

		#region Construction Logic

		public FunctionBasedObjectBuilder(
			Func<IObjectBuilderGetDependenciesContext<TAddress>, TAddress, IDependencySet<TAddress>> getDependenciesFunc,
			Func<IObjectBuilderGetAdditionalDependenciesContext<TAddress>, TAddress, IBuiltDependencies<TAddress>, IDependencySet<TAddress>> getAdditionalDependenciesFunc,
			Func<IObjectBuilderBuildObjectContext<TAddress>, TAddress, IBuiltDependencies<TAddress>, object> buildObjectFunc )
		{
			if( getDependenciesFunc == null )
				throw new ArgumentNullException( nameof( getDependenciesFunc ) );

			if( getAdditionalDependenciesFunc == null )
				throw new ArgumentNullException( nameof( getAdditionalDependenciesFunc ) );

			if( buildObjectFunc == null )
				throw new ArgumentNullException( nameof( buildObjectFunc ) );

			this.GetDependenciesFunc = getDependenciesFunc;
			this.GetAdditionalDependenciesFunc = getAdditionalDependenciesFunc;
			this.BuildObjectFunc = buildObjectFunc;
		}

		#endregion

		#region IObjectBuilder<TAddress> Members

		public IDependencySet<TAddress> GetDependencies( IObjectBuilderGetDependenciesContext<TAddress> context, TAddress address )
		{
			return this.GetDependenciesFunc( context, address );
		}

		public object BuildObject( IObjectBuilderBuildObjectContext<TAddress> context, TAddress address, IBuiltDependencies<TAddress> dependencies )
		{
			return this.BuildObjectFunc( context, address, dependencies );
		}

		public IDependencySet<TAddress> GetAdditionalDependencies( IObjectBuilderGetAdditionalDependenciesContext<TAddress> context, TAddress address, IBuiltDependencies<TAddress> dependencies )
		{
			return this.GetAdditionalDependenciesFunc( context, address, dependencies );
		}

		#endregion
	}
}
