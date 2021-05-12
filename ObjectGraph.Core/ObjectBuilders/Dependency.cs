using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Standard implementation of <see cref="IDependency{T}"/>
	/// </summary>
	/// <typeparam name="TAddress">The type of the address</typeparam>
	public sealed class Dependency<TAddress> : IDependency<TAddress>
	{
		#region IDependency Members

		public string Name { get; private set; }

		public IObjectContext<TAddress> ObjectContext { get; private set; }

		public TAddress Address { get; private set; }

		#endregion

		#region Construction Logic

		/// <summary>
		/// Creates a new instance of <see cref="Dependency{TAddress}"/>
		/// </summary>
		/// <param name="name">The name of the dependency</param>
		/// <param name="address">The address of the dependency</param>
		/// <param name="objectContext">The object context that the item must be sourced from (optional)</param>
		/// <exception cref="ArgumentNullException">If no address is specified</exception>
		public Dependency( string name, TAddress address, IObjectContext<TAddress> objectContext )
		{
			if( address == null )
				throw new ArgumentNullException( nameof( address ) );

			this.Name = name;
			this.Address = address;
			this.ObjectContext = objectContext;
		}

		#endregion
	}
}
