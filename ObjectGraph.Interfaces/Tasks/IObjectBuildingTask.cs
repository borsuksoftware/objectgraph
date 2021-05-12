using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectGraph.Tasks
{
	/// <summary>
	/// Interface which tasks which are responsible for building objects will implement
	/// </summary>
	/// <remarks>This interface can be used by custom instances of <see cref="ITaskRunner"/> to decide how to run
	/// a particular task.</remarks>
	/// <typeparam name="TAddress"></typeparam>
	public interface IObjectBuildingTask<TAddress>
	{
		/// <summary>
		/// Get the address of the object which will be built
		/// </summary>
		TAddress Address { get; }

		/// <summary>
		/// Get the object builder which will be used to build the object
		/// </summary>
		ObjectBuilders.IObjectBuilder<TAddress> ObjectBuilder { get; }
	}
}
