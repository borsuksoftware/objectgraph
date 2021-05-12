using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders.Providers
{
	/// <summary>
	/// Standard implementation of <see cref="IObjectBuilderProvider{TAddress}"/> based off being able to 
	/// specify builders per address
	/// </summary>
	/// <typeparam name="TAddress">The type of the address</typeparam>
	public class FixedObjectBuilderProvider<TAddress> : IObjectBuilderProvider<TAddress>
	{
		#region Data Model

		/// <summary>
		/// Gets the set of object builders per address
		/// </summary>
		/// <remarks>Note that if an entry is specified in both <see cref="ObjectBuilders"/> and <see cref="FixedObjects"/> then the value in 
		/// <see cref="FixedObjects"/> will take precedence</remarks>
		public IDictionary<TAddress, IObjectBuilder<TAddress>> ObjectBuilders { get; private set; }

		/// <summary>
		/// Gets the set of fixed objects to use per address
		/// </summary>
		/// <remarks>Note that if an entry is specified in both <see cref="ObjectBuilders"/> and <see cref="FixedObjects"/> then the value in 
		/// <see cref="FixedObjects"/> will take precedence</remarks>
		public IDictionary<TAddress, object> FixedObjects { get; private set; }

		#endregion

		#region Construction Logic

		/// <summary>
		/// Create a new instance of <see cref="FixedObjectBuilderProvider{TAddress}"/> with the supplied
		/// initial state
		/// </summary>
		/// <param name="objectBuilders">Optional set of builders</param>
		/// <param name="fixedObjects">Optional set of fixed objects</param>
		public FixedObjectBuilderProvider(
			IDictionary<TAddress, IObjectBuilder<TAddress>> objectBuilders,
			IDictionary<TAddress, object> fixedObjects )
		{
			this.ObjectBuilders = new Dictionary<TAddress, IObjectBuilder<TAddress>>();
			this.FixedObjects = new Dictionary<TAddress, object>();

			if( objectBuilders != null )
				foreach( var pair in objectBuilders )
					this.ObjectBuilders.Add( pair );

			if( fixedObjects != null )
				foreach( var pair in fixedObjects )
					this.FixedObjects.Add( pair );
		}

		#endregion

		#region IObjectBuilderProvider<TAddress> Members

		public bool TryGetObjectBuilder( TAddress address, out IObjectBuilder<TAddress> objectBuilder )
		{
			if( this.FixedObjects.TryGetValue( address, out object value ) )
			{
				objectBuilder = new FixedObjectBuilder<TAddress>( value );
				return true;
			}

			return this.ObjectBuilders.TryGetValue( address, out objectBuilder );
		}

		#endregion
	}
}
