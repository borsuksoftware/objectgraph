using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Standard implementation of <see cref="IBuiltDependencies{T}"/>
	/// </summary>
	/// <typeparam name="TAddress"></typeparam>
	public class BuiltDependencies<TAddress> : IBuiltDependencies<TAddress>
	{
		#region Member variables

		private IDictionary<string, IBuiltDependency<TAddress>> _dependenciesByName = new Dictionary<string, IBuiltDependency<TAddress>>();

		#endregion

		#region Class Functionality

		public void AddDependency( string name, TAddress address, object dependencyObject )
		{
			if( _dependenciesByName.ContainsKey( name ) )
				throw new ArgumentException( string.Format( "Duplicate dependency specified for '{0}'", name ), nameof( name ) );

			var builtDependency = new BuiltDependency<TAddress>( name, address, dependencyObject );

			_dependenciesByName.Add( name, builtDependency );
		}

		#endregion

		#region IBuiltDependencies<TAddress> Members

		public bool TryGetDependency( string name, out IBuiltDependency<TAddress> dependency )
		{
			return this._dependenciesByName.TryGetValue( name, out dependency );
		}

		#endregion
	}
}
