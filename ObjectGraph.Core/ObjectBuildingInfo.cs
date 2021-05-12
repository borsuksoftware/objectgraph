using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph
{
	/// <summary>
	/// Class representing the information around building a given address
	/// </summary>
	/// <typeparam name="TAddress">The type of the address</typeparam>
	public class ObjectBuildingInfo<TAddress> : IObjectBuildingInfo<TAddress>
	{
		#region Member variables

		/// <summary>
		/// The int which is used to control access to building the object
		/// </summary>
		/// <remarks>0 - not requested; 1 - requested</remarks>
		private int _buildObjectRequested = 0;

		/// <summary>
		/// The state of the component
		/// </summary>
		private volatile ObjectBuildingStates _objectBuildingState = ObjectBuildingStates.Starting;

		/// <summary>
		/// Gets the set of call backs which should be called when the object has finished building (or has failed to build)
		/// </summary>
		private LinkedList<Action<IObjectBuildingInfo<TAddress>>> _postBuildCallBacks = new LinkedList<Action<IObjectBuildingInfo<TAddress>>>();

		/// <summary>
		/// Gets the set of call backs which should be called when the object's dependencies are known (or there has been a failure)
		/// </summary>
		private LinkedList<Action<IObjectBuildingInfo<TAddress>>> _postDependenciesKnownCallBacks = new LinkedList<Action<IObjectBuildingInfo<TAddress>>>();

		#endregion

		#region Public Data Model

		System.Threading.WaitHandle IObjectBuildingInfo<TAddress>.DependenciesKnownOrFailureWaitHandle => this.DependenciesKnownOrFailureWaitHandle;
		System.Threading.WaitHandle IObjectBuildingInfo<TAddress>.ObjectBuiltOrFailureWaitHandle => this.ObjectBuiltOrFailureWaitHandle;

		/// <summary>
		/// Gets the object context that this instance represents
		/// </summary>
		public IObjectContext<TAddress> ObjectContext { get; private set; }

		/// <summary>
		/// Gets the address that this instance represents
		/// </summary>
		public TAddress Address { get; private set; }

		/// <summary>
		/// Gets the wait handle which will be set only when the object's dependencies are known
		/// </summary>
		public System.Threading.ManualResetEvent DependenciesKnownOrFailureWaitHandle { get; private set; }

		/// <summary>
		/// Gets the wait handle which will be set only when the object has been successfully built or it has failed
		/// </summary>
		public System.Threading.ManualResetEvent ObjectBuiltOrFailureWaitHandle { get; private set; }

		/// <summary>
		/// Gets the current state of the object
		/// </summary>
		public ObjectBuildingStates ObjectBuildingState
		{
			get => _objectBuildingState;
			private set => _objectBuildingState = value;
		}

		/// <summary>
		/// Gets the object builder for this instance (if available)
		/// </summary>
		public ObjectBuilders.IObjectBuilder<TAddress> ObjectBuilder { get; private set; }

		/// <summary>
		/// Gets the built object for this instance (if available)
		/// </summary>
		public object BuiltObject { get; private set; }

		/// <summary>
		/// Gets the exception for this instance (if relevant)
		/// </summary>
		public Exception Exception { get; private set; }

		/// <summary>
		/// Gets the set of dependencies which were requested for this instance (if available)
		/// </summary>
		public IRequestedDependencies<TAddress> RequestedDependencies { get; private set; }

		#endregion

		#region Construction Logic

		/// <summary>
		/// Creates a new instance of <see cref="ObjectBuildingInfo{TAddress}"/>
		/// </summary>
		/// <param name="objectContext">The object context that this building object is for</param>
		/// <param name="address">The address that this building object is for</param>
		/// <exception cref="ArgumentNullException">Thrown if either parameter is null</exception>
		public ObjectBuildingInfo(ObjectContext<TAddress> objectContext, TAddress address)
		{
			if (objectContext == null)
				throw new ArgumentNullException(nameof(objectContext));

			if (address == null)
				throw new ArgumentNullException(nameof(address));

			this.ObjectContext = objectContext;
			this.Address = address;

			this.DependenciesKnownOrFailureWaitHandle = new System.Threading.ManualResetEvent(false);
			this.ObjectBuiltOrFailureWaitHandle = new System.Threading.ManualResetEvent(false);
		}

		#endregion

		#region State Change functionality

		/// <summary>
		/// Set the object builder to be used
		/// </summary>
		/// <param name="objectBuilder">The object builder</param>
		public void SetObjectBuilder(ObjectBuilders.IObjectBuilder<TAddress> objectBuilder)
		{
			this.ObjectBuilder = objectBuilder;
		}

		/// <summary>
		/// Sets the set of dependencies that have been requested by this object
		/// </summary>
		/// <param name="requestedDependencies">The set of dependencies</param>
		public void SetRequestedDependencies(IRequestedDependencies<TAddress> requestedDependencies)
		{
			this.RequestedDependencies = requestedDependencies;
			this.ObjectBuildingState = ObjectBuildingStates.DependenciesKnown;
			this.DependenciesKnownOrFailureWaitHandle.Set();
			LaunchPostDependenciesKnownCallBacks();
		}

		/// <summary>
		/// Sets the built object for this address
		/// </summary>
		/// <param name="builtObject">The built object</param>
		public void SetObjectBuilt(object builtObject)
		{
			this.BuiltObject = builtObject;
			this.ObjectBuildingState = ObjectBuildingStates.ObjectBuilt;
			this.DependenciesKnownOrFailureWaitHandle.Set();
			this.ObjectBuiltOrFailureWaitHandle.Set();

			LaunchPostBuildCallBacks();
		}

		/// <summary>
		/// Sets the object has having failed during construction
		/// </summary>
		/// <param name="ex">The exception which was thrown</param>
		public void SetObjectFailed(Exception ex)
		{
			this.Exception = ex;
			this.ObjectBuildingState = ObjectBuildingStates.Failure;
			this.DependenciesKnownOrFailureWaitHandle.Set();
			this.ObjectBuiltOrFailureWaitHandle.Set();

			LaunchPostDependenciesKnownCallBacks();
			LaunchPostBuildCallBacks();
		}

		/// <summary>
		/// Sets that no builder could be found for this address
		/// </summary>
		public void SetNoBuilderFound()
		{
			this.ObjectBuildingState = ObjectBuildingStates.NoBuilderAvailable;
			this.DependenciesKnownOrFailureWaitHandle.Set();
			this.ObjectBuiltOrFailureWaitHandle.Set();

			LaunchPostDependenciesKnownCallBacks();
			LaunchPostBuildCallBacks();
		}

		#endregion

		#region Build Object functionality

		/// <summary>
		/// Requests that the object should be built
		/// </summary>
		/// <remarks>This call can be called as many times as desired, the object itself will only be built once</remarks>
		public void RequestBuildObject(Tasks.ITaskRunner taskRunner)
		{
			var originalValue = System.Threading.Interlocked.CompareExchange(ref _buildObjectRequested, 1, 0);
			if (originalValue != 0)
			{
				// The object building has already been requested and therefore we can simply return
				return;
			}

			BuildObject(taskRunner);
		}

		/// <summary>
		/// Method which is responsible for the orchestration of the actual building of this item
		/// </summary>
		/// <remarks>
		/// This should only be called once per instance and no internal checks are performed as to whether that that statement holds true or not
		/// </remarks>
		private void BuildObject(Tasks.ITaskRunner taskRunner)
		{
			// We now have responsibility for building the item / requesting that the item is built...

			var infoObj = this;
			this.RegisterPostDependenciesKnownCallBack((address) =>
		   {
			   if (this.ObjectBuildingState != ObjectBuildingStates.DependenciesKnown)
				   return;

			   try
			   {
				   {
					   var dependencyObjectsBuildingInfo = new List<Tuple<ObjectBuilders.IDependency<TAddress>, IObjectBuildingInfo<TAddress>>>();

					   foreach (var dependency in this.RequestedDependencies.Dependencies)
					   {
						   var dependencyObjectBuildingInfo = (dependency.ObjectContext ?? this.ObjectContext).BuildObject(dependency.Address);
						   dependencyObjectsBuildingInfo.Add(Tuple.Create(dependency, dependencyObjectBuildingInfo));
					   }

					   var callbackHandler = new ObjectBuilderCallBackHandler<TAddress>(taskRunner, infoObj, dependencyObjectsBuildingInfo);

					   foreach (var entry in dependencyObjectsBuildingInfo)
						   entry.Item2.RegisterPostBuildCallBack(callbackHandler.PostDependencyBuildCallBack);
				   }
			   }
			   catch (Exception ex)
			   {
				   this.SetObjectFailed(ex);
			   }
		   });
		}

		#endregion

		#region Call back functionality

		/// <summary>
		/// Causes each of the registered call backs to be called
		/// </summary>
		/// <remarks>This will be called whether the object suceeded or failed</remarks>
		private void LaunchPostBuildCallBacks()
		{
			lock (this._postBuildCallBacks)
			{
				foreach (var callBack in _postBuildCallBacks)
				{
					try
					{
						callBack(this);
					}
					catch/*( Exception ex )*/
					{

					}
				}

				_postBuildCallBacks.Clear();
			}
		}

		private void LaunchPostDependenciesKnownCallBacks()
		{
			lock (this._postDependenciesKnownCallBacks)
			{
				foreach (var callBack in _postDependenciesKnownCallBacks)
				{
					try
					{
						callBack(this);
					}
					catch/*( Exception ex )*/
					{

					}
				}

				_postDependenciesKnownCallBacks.Clear();
			}
		}

		/// <summary>
		/// Registers a call back to be performed after building has completed (or complete failure)
		/// </summary>
		/// <remarks>The supplied call backs should be trivial in nature, e.g. adding dependent object construction jobs
		/// to the thread pool.</remarks>
		/// <param name="postBuildCallBack">The action to perform</param>
		/// <exception cref="ArgumentNullException">If <paramref name="postBuildCallBack"/> is null</exception>
		public void RegisterPostBuildCallBack(Action<IObjectBuildingInfo<TAddress>> postBuildCallBack)
		{
			if (postBuildCallBack == null)
				throw new ArgumentNullException(nameof(postBuildCallBack));

			{
				var state = this.ObjectBuildingState;
				if (state == ObjectBuildingStates.ObjectBuilt || state == ObjectBuildingStates.Failure)
				{
					postBuildCallBack(this);
					return;
				}
			}

			lock (_postBuildCallBacks)
			{
				var state = this.ObjectBuildingState;
				if (state == ObjectBuildingStates.ObjectBuilt || state == ObjectBuildingStates.Failure)
				{
					postBuildCallBack(this);
					return;
				}

				_postBuildCallBacks.AddLast(postBuildCallBack);
			}
		}

		/// <summary>
		/// Registers a call back to be performed once the discovery phase has completed (or complete failure)
		/// </summary>
		/// <remarks>The supplied call backs should be trivial in nature, e.g. adding dependent object construction jobs
		/// to the thread pool.</remarks>
		/// <param name="postDependenciesKnownCallBack">The call back to be performed</param>
		public void RegisterPostDependenciesKnownCallBack(Action<IObjectBuildingInfo<TAddress>> postDependenciesKnownCallBack)
		{
			if (postDependenciesKnownCallBack == null)
				throw new ArgumentNullException(nameof(postDependenciesKnownCallBack));

			{
				var state = this.ObjectBuildingState;
				switch (state)
				{
					case ObjectBuildingStates.DependenciesKnown:
					case ObjectBuildingStates.Failure:
					case ObjectBuildingStates.NoBuilderAvailable:
					case ObjectBuildingStates.ObjectBuilt:
						postDependenciesKnownCallBack(this);
						return;
				}
			}

			lock (_postDependenciesKnownCallBacks)
			{
				var state = this.ObjectBuildingState;
				switch (state)
				{
					case ObjectBuildingStates.DependenciesKnown:
					case ObjectBuildingStates.Failure:
					case ObjectBuildingStates.NoBuilderAvailable:
					case ObjectBuildingStates.ObjectBuilt:
						postDependenciesKnownCallBack(this);
						return;
				}

				_postDependenciesKnownCallBacks.AddLast(postDependenciesKnownCallBack);
			}
		}
		#endregion
	}


}