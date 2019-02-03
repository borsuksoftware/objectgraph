using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Interface representing a set of built dependencies for passing into <see cref="IObjectBuilder{T}"/>
	/// </summary>
	/// <typeparam name="TAddress">The type of the address</typeparam>
	public interface IBuiltDependencies<TAddress>
	{
		/// <summary>
		/// Tries to get the named dependency from the set
		/// </summary>
		/// <param name="name">The name of the dependency</param>
		/// <param name="dependency">The dependency object</param>
		/// <returns>True if the dependency was found, otherwise false</returns>
		bool TryGetDependency( string name, out IBuiltDependency<TAddress>  dependency );
	}
}
