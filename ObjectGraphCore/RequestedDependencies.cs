using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph
{
	public class RequestedDependencies<TAddress>
	{
		/// <summary>
		/// Gets whether or not dependencies are recursive
		/// </summary>
		public bool RecursiveDependencies { get; private set; }

		/// <summary>
		/// Gets the set of dependencies for this collection
		/// </summary>
		public ICollection<ObjectBuilders.IDependency<TAddress>> Dependencies { get; private set; }

		public RequestedDependencies( bool recursive, IEnumerable<ObjectBuilders.IDependency<TAddress>> dependencies )
		{
			this.RecursiveDependencies = recursive;
			this.Dependencies =
				dependencies == null ?
					new ObjectBuilders.IDependency<TAddress> [ 0 ] :
					dependencies.ToArray();
		}
	}
}
