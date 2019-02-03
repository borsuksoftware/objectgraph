using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Interface representing an object builder within the hierarchy
	/// </summary>
	/// <typeparam name="TAddress">The type of the address</typeparam>
	public interface IObjectBuilder<TAddress>
	{
		/// <summary>
		/// Gets the set of required dependencies for the given address
		/// </summary>
		/// <param name="context">The context for this call</param>
		/// <param name="address">The address of the object to build</param>
		/// <returns>The set of required dependencies (may return null if appropriate)</returns>
		IDependencySet<TAddress> GetDependencies( IObjectBuilderGetDependenciesContext<TAddress> context, TAddress address );

		/// <summary>
		/// Gets any additional dependencies once the initial set have been sourced
		/// </summary>
		/// <param name="context">The context for this call</param>
		/// <param name="address">The address of the object to build</param>
		/// <param name="dependencies">The set of all previously requested dependencies</param>
		/// <returns>The set of additionally required dependencies (may return null if appropriate)</returns>
		IDependencySet<TAddress> GetAdditionalDependencies( IObjectBuilderGetAdditionalDependenciesContext<TAddress> context,
			TAddress address,
			IBuiltDependencies<TAddress> dependencies );

		/// <summary>
		/// Builds the object for the given address / dependencies pairing
		/// </summary>
		/// <param name="context">The context for this call</param>
		/// <param name="address">The address of the object to build</param>
		/// <param name="dependencies">The set of objects representing the previously requested dependencies</param>
		/// <returns>The built object</returns>
		object BuildObject( IObjectBuilderBuildObjectContext<TAddress> context, TAddress address, IBuiltDependencies<TAddress> dependencies );
	}
}
