using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Standard implementation of <see cref="IRequestedDependency{TAddress}"/>
	/// </summary>
	/// <typeparam name="TAddress">The type of the address</typeparam>
	public sealed class RequestedDependency<TAddress> : IRequestedDependency<TAddress>
	{
		#region Construction Logic

		public RequestedDependency( string name,
			TAddress address,
			IObjectContext<TAddress> objectContext = null )
		{
			this.Name = name;
			this.Address = address;
			this.ObjectContext = objectContext;
		}

		#endregion

		#region IRequestedDependency<TAddress> Members

		public TAddress Address { get; private set; }

		public string Name { get; private set; }

		public IObjectContext<TAddress> ObjectContext { get; private set; }

		#endregion
	}
}
