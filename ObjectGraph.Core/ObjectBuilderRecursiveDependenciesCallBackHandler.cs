using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph
{
	public class ObjectBuilderRecursiveDependenciesCallBackHandler<TAddress>
	{
		#region Member variables

		private volatile int _outstandingDependencies;

		#endregion

		#region Data Model

		/// <summary>
		/// Gets the <see cref="ObjectBuildingInfo{TAddress}"/> object that this call back is for
		/// </summary>
		public ObjectBuildingInfo<TAddress> ObjectBuildingInfo { get; private set; }

		/// <summary>
		/// The set of dependencies which are already 
		/// </summary>
		public ICollection<Tuple<ObjectBuilders.IDependency<TAddress>, IObjectBuildingInfo<TAddress>>> Dependencies { get; private set; }

		#endregion

		public ObjectBuilderRecursiveDependenciesCallBackHandler( ObjectBuildingInfo<TAddress> objectBuildingInfo,
			IEnumerable<Tuple<ObjectBuilders.IDependency<TAddress>, IObjectBuildingInfo<TAddress>>> dependencies )
		{
			if( dependencies == null )
				dependencies = new Tuple<ObjectBuilders.IDependency<TAddress>, IObjectBuildingInfo<TAddress>> [ 0 ];

			this.Dependencies = new List<Tuple<ObjectBuilders.IDependency<TAddress>, IObjectBuildingInfo<TAddress>>>( dependencies );
			this.ObjectBuildingInfo = objectBuildingInfo;

			this._outstandingDependencies = this.Dependencies.Count;
			if( _outstandingDependencies == 0 )
				this.RunGetAdditionalDependencies();
		}

		#region Class functionality

		/// <summary>
		/// Functionality which provides the post dependency building call back
		/// </summary>
		/// <remarks>This functionality uses <see cref="System.Threading.Interlocked.Decrement(ref int)"/> in order to avoid
		/// multiple locks / wait handles etc.</remarks>
		/// <param name="dependency">The dependency which has just completed</param>
		public void PostDependencyBuildCallBack( IObjectBuildingInfo<TAddress> dependency )
		{
			var outstanding = System.Threading.Interlocked.Decrement( ref _outstandingDependencies );
			if( outstanding != 0 )
				return;

			this.RunGetAdditionalDependencies();
		}

		#endregion

		#region Class functionality (private)

		private void RunGetAdditionalDependencies()
		{
			try
			{
				var dependencySet = new ObjectBuilders.BuiltDependencies<TAddress>();
				if( this.Dependencies.Any( tuple => tuple.Item2.ObjectBuildingState != ObjectBuildingStates.ObjectBuilt ) )
				{
					this.ObjectBuildingInfo.SetObjectFailed( new InvalidOperationException( "Dependency Failed" ) );
					return;
				}

				foreach( var entry in this.Dependencies )
					dependencySet.AddDependency( entry.Item1.Name,
						entry.Item1.Address,
						entry.Item2.BuiltObject );

				var context = new ObjectBuilders.ObjectBuilderGetAdditionalDependenciesContext<TAddress>( this.ObjectBuildingInfo.ObjectContext );

				var additionalDependencies = this.ObjectBuildingInfo.ObjectBuilder.GetAdditionalDependencies( context, this.ObjectBuildingInfo.Address, dependencySet );

				IEnumerable<ObjectBuilders.IDependency<TAddress>> combinedDependencies = this.Dependencies.Select( tuple => tuple.Item1 );
				if( additionalDependencies != null )
					combinedDependencies = combinedDependencies.Concat( additionalDependencies.Dependencies );

				if( additionalDependencies == null || !additionalDependencies.RecursiveMode )
				{
					// Say that the dependencies are now know...
					var requestedDependencies = new RequestedDependencies<TAddress>(
						false,
						combinedDependencies );

					this.ObjectBuildingInfo.SetRequestedDependencies( requestedDependencies );
				}
				else if( additionalDependencies.RecursiveMode )
				{
					var dependencyTuples = new List<Tuple<ObjectBuilders.IDependency<TAddress>, IObjectBuildingInfo<TAddress>>>();
					// Recursive mode
					foreach( var dependency in combinedDependencies )
					{
						var dependencyObjectBuildingInfo = this.ObjectBuildingInfo.ObjectContext.BuildObject( dependency.Address );
						var tuple = Tuple.Create( dependency, dependencyObjectBuildingInfo );
						dependencyTuples.Add( tuple );
					}

					var callBackHandler = new ObjectBuilderRecursiveDependenciesCallBackHandler<TAddress>(
						this.ObjectBuildingInfo,
						dependencyTuples );

					foreach( var tuple in dependencyTuples )
						tuple.Item2.RegisterPostBuildCallBack( callBackHandler.PostDependencyBuildCallBack );
				}
			}
			catch( Exception ex )
			{
				this.ObjectBuildingInfo.SetObjectFailed( ex );
			}
		}

		#endregion
	}
}
