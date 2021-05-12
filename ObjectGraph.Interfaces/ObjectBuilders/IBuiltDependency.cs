using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Interface encapsulating a built dependency's properties
	/// </summary>
	/// <typeparam name="TAddress"></typeparam>
	public interface IBuiltDependency<TAddress>
	{
		/// <summary>
		/// Gets the name of the dependency
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the address for the given dependency
		/// </summary>
		TAddress Address { get; }

		/// <summary>
		/// Gets the built object
		/// </summary>
		object BuiltObject { get; }
	}
}
