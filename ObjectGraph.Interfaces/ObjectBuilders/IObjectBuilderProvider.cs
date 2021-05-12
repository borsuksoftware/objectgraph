using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Interface representing a component which can provide <see cref="IObjectBuilder{TAddress}"/> instances per address
	/// </summary>
	/// <typeparam name="TAddress">The type of the address</typeparam>
	public interface IObjectBuilderProvider<TAddress>
	{
		/// <summary>
		/// Tries to get the <see cref="IObjectBuilder{TAddress}"/> to use for the given address
		/// </summary>
		/// <param name="address">The address of interest</param>
		/// <param name="objectBuilder">Contains the appropriate object for the given address</param>
		/// <returns>True if the object builder could be found, otherwise false</returns>
		bool TryGetObjectBuilder( TAddress address, out IObjectBuilder<TAddress> objectBuilder );
	}
}
