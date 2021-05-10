using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectGraph
{
	/// <summary>
	/// Interface representing the main way of interacting with a given node in the graph
	/// </summary>
	/// <typeparam name="TAddress"></typeparam>
	public interface IObjectBuildingInfo<TAddress>
	{
		/// <summary>
		/// Gets the object context that this instance represents
		/// </summary>
		IObjectContext<TAddress> ObjectContext { get; }

		/// <summary>
		/// Gets the address that this instance represents
		/// </summary>
		TAddress Address { get; }

		/// <summary>
		/// Gets the wait handle which will be set only when the object's dependencies are known
		/// </summary>
		System.Threading.WaitHandle DependenciesKnownOrFailureWaitHandle { get; }

		/// <summary>
		/// Gets the wait handle which will be set only when the object has been successfully built or it has failed
		/// </summary>
		System.Threading.WaitHandle ObjectBuiltOrFailureWaitHandle { get; }

		/// <summary>
		/// Gets the current state of the object
		/// </summary>
		ObjectBuildingStates ObjectBuildingState { get; }

		/// <summary>
		/// Gets the object builder for this instance (if available)
		/// </summary>
		ObjectBuilders.IObjectBuilder<TAddress> ObjectBuilder { get; }

		/// <summary>
		/// Gets the built object for this instance (if available)
		/// </summary>
		object BuiltObject { get; }

		/// <summary>
		/// Gets the exception for this instance (if relevant)
		/// </summary>
		Exception Exception { get; }

		/// <summary>
		/// Gets the set of dependencies which were requested for this instance (if available)
		/// </summary>
		IRequestedDependencies<TAddress> RequestedDependencies { get; }

		/// <summary>
		/// Registers a call back function which should be called (on an arbitrary thread) once the object itself has been built (or failed to be built)
		/// </summary>
		/// <remarks>As this method is called on an arbitrary thread, its operations should be quick and explicitly should not involve doing a large amount of work. Instead
		/// it should request that the subsequent object construction is added to the job queue etc.</remarks>
		/// <param name="postBuildCallBack">The method which should be called.</param>
		void RegisterPostBuildCallBack(Action<IObjectBuildingInfo<TAddress>> postBuildCallBack);

		/// <summary>
		/// Registers a call back function which should be called (on an arbitrary thread) once the object's dependencies are known (or are guaranteed to not be known)
		/// </summary>
		/// <remarks>As this method is called on an arbitrary thread, its operations should be quick and explicitly should not involve doing a large amount of work. Instead
		/// if complex work needs to be performed, it should be added to the job queue for subsequent scheduling.</remarks>
		/// <param name="postDependenciesKnownCallBack">The call back</param>
		void RegisterPostDependenciesKnownCallBack(Action<IObjectBuildingInfo<TAddress>> postDependenciesKnownCallBack);
	}
}
