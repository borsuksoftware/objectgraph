using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.Tasks
{
	/// <summary>
	/// Class to handle performing discovery on the thread pool
	/// </summary>
	/// <typeparam name="TAddress">The type of address</typeparam>
	internal class ObjectBuilderGetDependenciesTask<TAddress> : ITask
	{
		#region Data Model

		public ObjectContext<TAddress> ObjectContext { get; private set; }
		public ObjectBuildingInfo<TAddress> ObjectBuildingInfo { get; private set; }
		public TAddress Address { get; private set; }

		#endregion

		#region Construction Logic

		public ObjectBuilderGetDependenciesTask( ObjectContext<TAddress> objectContext,
			ObjectBuildingInfo<TAddress> objectBuildingInfo,
			TAddress address )
		{
			this.ObjectContext = objectContext;
			this.ObjectBuildingInfo = objectBuildingInfo;
			this.Address = address;
		}

		#endregion

		#region IJob Members

		public void Cancel()
		{
			this.ObjectBuildingInfo.SetObjectFailed( new OperationCanceledException( "The discovery job was cancelled" ) );
		}

		public void Run()
		{
			try
			{
				var context = new ObjectBuilders.ObjectBuilderGetDependenciesContext<TAddress>( this.ObjectContext );
				var dependencies = this.ObjectBuildingInfo.ObjectBuilder.GetDependencies( context, this.Address );
				var requestedDependencies = new RequestedDependencies<TAddress>(
					false,
					dependencies == null ? null : dependencies.Dependencies );

				this.ObjectBuildingInfo.SetRequestedDependencies( requestedDependencies );
			}
			catch( Exception ex )
			{
				this.ObjectBuildingInfo.SetObjectFailed( ex );
			}
		}

		#endregion
	}
}
