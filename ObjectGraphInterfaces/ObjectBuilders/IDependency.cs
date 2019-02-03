using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Interface representing a requested dependency within the framework
	/// </summary>
	/// <typeparam name="T">The type of the address</typeparam>
	public interface IDependency<T>
	{
		/// <summary>
		/// Gets the name of the dependency
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the object context that this dependency must come from (or null)
		/// </summary>
		IObjectContext<T> ObjectContext { get; }

		/// <summary>
		/// Gets the address of the dependency
		/// </summary>
		T Address { get; }
	}
}
