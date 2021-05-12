using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectGraph.Tasks
{
	/// <summary>
	/// Interface which tasks which are responsible for building objects will implement
	/// </summary>
	/// <remarks>This interface can be used by custom instances of <see cref="ITaskRunner"/> to decide how to run
	/// a particular task.
	/// 
	/// <para>This interface also allows for task runners to bypass the <see cref="ITask.Run"/> method and explicitly supply values. This functionality
	/// is useful in the rare circumstance whereby out of process object construction is required. To do this, <see cref="SetResult(object)"/> and <see cref="SetException(Exception)"/> can be used.</para>
	/// </remarks>
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

		/// <summary>
		/// Get the build object context to use whilst building the object
		/// </summary>
		ObjectBuilders.IObjectBuilderBuildObjectContext<TAddress> BuildObjectContext { get; }
		
		/// <summary>
		/// Get the dependencies which will be fed into the builder
		/// </summary>
		ObjectBuilders.IBuiltDependencies<TAddress> Dependencies { get; }

		/// <summary>
		/// Set the result to be used for this task
		/// </summary>
		/// <param name="result">The result object</param>
		void SetResult(object result);

		/// <summary>
		/// Set the exception to be used for this task
		/// </summary>
		/// <param name="ex">The exception</param>
		void SetException(Exception ex);
	}
}
