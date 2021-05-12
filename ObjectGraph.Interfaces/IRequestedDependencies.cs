using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectGraph
{
	public interface IRequestedDependencies<TAddress>
	{
		/// <summary>
		/// Gets whether or not dependencies are recursive
		/// </summary>
		bool RecursiveDependencies { get; }

		/// <summary>
		/// Gets the set of dependencies for this collection
		/// </summary>
		IReadOnlyCollection<ObjectBuilders.IDependency<TAddress>> Dependencies { get; }
	}
}
