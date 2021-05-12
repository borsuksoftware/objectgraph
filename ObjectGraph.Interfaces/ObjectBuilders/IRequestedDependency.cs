using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Interface representing a requested dependency
	/// </summary>
	/// <typeparam name="TAddress"></typeparam>
	public interface IRequestedDependency<TAddress>
	{
		/// <summary>
		/// Gets the address for the dependency
		/// </summary>
		TAddress Address { get; }

		/// <summary>
		/// Gets the name of the dependency
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the optional object context that this dependency must come from (handles dependency 'locking')
		/// </summary>
		IObjectContext<TAddress> ObjectContext { get; }
	}
}
