using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph
{
	/// <summary>
	/// Standard implementation of <see cref="IObjectContext{TAddress}"/>
	/// </summary>
	/// <remarks>This class forms the basis of the object graph and is the primary externally referenced component</remarks>
	/// <typeparam name="TAddress">The type of the address keys</typeparam>
	public class ObjectContext<TAddress> : IObjectContext<TAddress>
	{
		#region Member variables

		private IDictionary<TAddress, ObjectBuildingInfo<TAddress>> _objectBuildingInfo = new Dictionary<TAddress, ObjectBuildingInfo<TAddress>>();

		#endregion

		#region Data Model

		public IObjectContext<TAddress> ParentContext { get; private set; }

		public ObjectBuilders.IObjectBuilderProvider<TAddress> ObjectBuilderProvider { get; private set; }

		public Tasks.ITaskRunner TaskRunner { get; private set; }

		#endregion

		#region Construction Logic

		/// <summary>
		/// Creates a new instance of the <see cref="ObjectContext{TAddress}"/> with the given parameters
		/// </summary>
		/// <param name="parentContext">Optional parent context</param>
		/// <param name="objectBuilderProvider">The object builder provider to use for this context</param>
		/// <param name="taskRunner">The task runner for jobs for this context</param>
		public ObjectContext( IObjectContext<TAddress> parentContext,
			ObjectBuilders.IObjectBuilderProvider<TAddress> objectBuilderProvider,
			Tasks.ITaskRunner taskRunner )
		{
			if( objectBuilderProvider == null )
				throw new ArgumentNullException( nameof( objectBuilderProvider ) );

			if( taskRunner == null )
				throw new ArgumentNullException( nameof( taskRunner ) );

			this.ParentContext = parentContext;
			this.ObjectBuilderProvider = objectBuilderProvider;
			this.TaskRunner = taskRunner;
		}

		#endregion

		#region IObjectContext Members

		public IObjectBuildingInfo<TAddress> GetDependencies(TAddress address)
		{
			return this.GetDependenciesInt(address);
		}

		/// <summary>
		/// Tries to build the specified object
		/// </summary>
		/// <remarks>Control will be returned to the caller as soon as possible with any actual building
		/// being done in the background.</remarks>
		/// <param name="address">The address of the object to build</param>
		/// <returns>An info object for the given address</returns>
		public IObjectBuildingInfo<TAddress> BuildObject(TAddress address)
		{
			var info = this.GetDependenciesInt(address);

			info.RequestBuildObject(this.TaskRunner);
			return info;
		}

		#endregion

		#region Building business logic

		private ObjectBuildingInfo<TAddress> GetDependenciesInt( TAddress address )
		{
			if( address == null )
				throw new ArgumentNullException( nameof( address ) );

			// Check to see if we've been asked to build this before, if so, simply return
			ObjectBuildingInfo<TAddress> objectBuildingInfo = null;
			lock( this._objectBuildingInfo )
			{
				if( _objectBuildingInfo.TryGetValue( address, out objectBuildingInfo ) )
					return objectBuildingInfo;

				objectBuildingInfo = new ObjectBuildingInfo<TAddress>( this, address );
				_objectBuildingInfo [ address ] = objectBuildingInfo;
			}

			// At this stage, we're now responsible for the population of this chap...
			if( this.ObjectBuilderProvider.TryGetObjectBuilder( address, out var objectBuilder ) )
			{
				try
				{
					objectBuildingInfo.SetObjectBuilder( objectBuilder );

					if( objectBuilder is ObjectBuilders.IObjectBuilderAsynchronousDependencies<TAddress> &&
						( (ObjectBuilders.IObjectBuilderAsynchronousDependencies<TAddress>) objectBuilder ).GetDependenciesAsynchronously( address ) )
					{
						// Create a task to do discovery and then to perform the 
						var job = new Tasks.ObjectBuilderGetDependenciesTask<TAddress>( this,
							objectBuildingInfo,
							address );

						this.TaskRunner.RegisterTask( job );
					}
					else
					{
						var context = new ObjectBuilders.ObjectBuilderGetDependenciesContext<TAddress>( this );
						var dependencies = objectBuilder.GetDependencies( context, address );

						if( dependencies != null && dependencies.RecursiveMode )
						{
							var dependencyTuples = new List<Tuple<ObjectBuilders.IDependency<TAddress>, IObjectBuildingInfo<TAddress>>>();
							// Recursive mode
							foreach( var dependency in dependencies.Dependencies )
							{
								var dependencyObjectBuildingInfo = this.BuildObject( dependency.Address );
								var tuple = Tuple.Create( dependency, dependencyObjectBuildingInfo );
								dependencyTuples.Add( tuple );
							}

							var callBackHandler = new ObjectBuilderRecursiveDependenciesCallBackHandler<TAddress>(
								objectBuildingInfo,
								dependencyTuples );

							foreach( var tuple in dependencyTuples )
								tuple.Item2.RegisterPostBuildCallBack( callBackHandler.PostDependencyBuildCallBack );
						}
						else
						{
							var requestedDependencies = new RequestedDependencies<TAddress>(
								false,
								dependencies == null ? null : dependencies.Dependencies );

							objectBuildingInfo.SetRequestedDependencies( requestedDependencies );
						}
					}
				}
				catch( Exception ex )
				{
					objectBuildingInfo.SetObjectFailed( ex );
				}
				return objectBuildingInfo;
			}
			else
			{
				// This needs to be extended to ask for items from the child context...
				var parentContext = (ObjectContext<TAddress>) this.ParentContext;
				if( parentContext == null )
				{
					objectBuildingInfo.SetNoBuilderFound();
					return objectBuildingInfo;
				}

				var parentContextObjectBuildingInfo = parentContext.GetDependencies( address );

				parentContextObjectBuildingInfo.RegisterPostDependenciesKnownCallBack( ( x ) =>
				{
					if( parentContextObjectBuildingInfo.ObjectBuildingState == ObjectBuildingStates.Failure )
					{
						objectBuildingInfo.SetObjectFailed( new Exception( "Parent dependencies failed" ) );
						return;
					}

					objectBuildingInfo.SetObjectBuilder( parentContextObjectBuildingInfo.ObjectBuilder );
					objectBuildingInfo.SetRequestedDependencies( parentContextObjectBuildingInfo.RequestedDependencies );
				} );
				return objectBuildingInfo;
			}
		}

		#endregion
	}
}
