using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders.Providers
{
	/// <summary>
	/// Function based implementation of <see cref="IObjectBuilderProvider{TAddress}"/> to allow for use 
	/// of delegate based programming
	/// </summary>
	/// <typeparam name="TAddress">The type of the address</typeparam>
	public class FunctionBasedObjectBuilderProvider<TAddress> : IObjectBuilderProvider<TAddress>
	{
		#region Data Model

		/// <summary>
		/// Gets the function to supply the appropriate <see cref="IObjectBuilder{TAddress}"/>
		/// </summary>
		/// <remarks>If the delegate returns null, then this is assumed to be an unsupported signature</remarks>
		public Func<TAddress, IObjectBuilder<TAddress>> ObjectBuilderProviderFunc { get; private set; }

		#endregion

		#region Construction Logic

		/// <summary>
		/// Creates a new instance of <see cref="FunctionBasedObjectBuilderProvider{TAddress}"/> based off the supplied delegate
		/// </summary>
		/// <param name="objectBuilderProviderFunc">The delegate to use to supply the <see cref="IObjectBuilder{TAddress}"/></param>
		public FunctionBasedObjectBuilderProvider( Func<TAddress, IObjectBuilder<TAddress>> objectBuilderProviderFunc)
		{
			if( objectBuilderProviderFunc == null )
				throw new ArgumentNullException( nameof( objectBuilderProviderFunc ) );

			this.ObjectBuilderProviderFunc = objectBuilderProviderFunc;
		}

		#endregion

		#region IObjectBuilderProvider<TAddress> Members

		public bool TryGetObjectBuilder( TAddress address, out IObjectBuilder<TAddress> objectBuilder )
		{
			objectBuilder = this.ObjectBuilderProviderFunc( address );
			return objectBuilder != null;
		}

		#endregion
	}
}
