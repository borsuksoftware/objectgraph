using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph
{
	/// <summary>
	/// Interface which represents a context within the graph hierarchy
	/// </summary>
	/// <typeparam name="TAddress">The type of the addressing objects</typeparam>
	public interface IObjectContext<TAddress>
	{
		/// <summary>
		/// Start the process of sourcing the dependencies for the given address
		/// </summary>
		/// <remarks>Note that control may be returned to the user prior to the actual dependencies have been sourced</remarks>
		/// <param name="address"></param>
		/// <returns></returns>
		IObjectBuildingInfo<TAddress> GetDependencies(TAddress address);

		/// <summary>
		/// Request that the given address is built
		/// </summary>
		/// <remarks>Note that control is returned to the user prior to the object actually having been built</remarks>
		/// <param name="address"></param>
		/// <returns></returns>
		IObjectBuildingInfo<TAddress> BuildObject(TAddress address);
	}
}
