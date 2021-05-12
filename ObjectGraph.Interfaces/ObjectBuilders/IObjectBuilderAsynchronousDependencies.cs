using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Optional interface that an instance of <see cref="IObjectBuilder{TAddress}"/> can implement to inform
	/// the framework that the dependencies should be performed on the thread pool
	/// </summary>
	public interface IObjectBuilderAsynchronousDependencies<TAddress>
	{
		/// <summary>
		/// Gets whether or not dependencies should be fetched in an asynchronous fashion for the given address
		/// </summary>
		/// <param name="address">The address in question</param>
		/// <returns>true if the dependencies should be sourced as a task otherwise false for standard mode</returns>
		bool GetDependenciesAsynchronously( TAddress address );
	}
}
