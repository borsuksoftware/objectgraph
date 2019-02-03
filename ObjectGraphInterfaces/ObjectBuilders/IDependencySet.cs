using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Interface representing the set of dependencies from an instance of <see cref="IObjectBuilder{TAddress}"/>
	/// </summary>
	/// <typeparam name="TAddress">The type of address</typeparam>
	public interface IDependencySet<TAddress>
	{
		/// <summary>
		/// Gets whether or not the object builder needs to be called again
		/// </summary>
		bool RecursiveMode { get;  }

		/// <summary>
		/// Gets the set of dependencies
		/// </summary>
		ICollection<IDependency<TAddress>> Dependencies { get; }
	}
}
