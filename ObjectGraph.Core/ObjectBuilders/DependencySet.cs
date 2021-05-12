using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Standard implementation of <see cref="IDependencySet{TAddress}"/>
	/// </summary>
	/// <typeparam name="TAddress">The type of the address</typeparam>
	public sealed class DependencySet<TAddress> : IDependencySet<TAddress>
	{
		#region IDependencySet<TAddress> Members

		public bool RecursiveMode { get; set; }

		public ICollection<IDependency<TAddress>> Dependencies { get; private set; }

		#endregion

		#region Construction Logic

		public DependencySet()
		{
			this.Dependencies = new List<IDependency<TAddress>>();
		}

		#endregion

		#region Class functionality

		/// <summary>
		/// Adds a new dependency
		/// </summary>
		/// <param name="name">The name of the dependency (required)</param>
		/// <param name="address">The address of the dependency (required)</param>
		/// <param name="objectContext">Optional object context</param>
		public void AddDependency( string name, TAddress address, IObjectContext<TAddress> objectContext = null )
		{
			if( name == null )
				throw new ArgumentNullException( nameof( name ) );

			if( address == null )
				throw new ArgumentNullException( nameof( address ) );

			if( this.Dependencies.Any( dep => dep.Name == name ) )
				throw new InvalidOperationException( string.Format( "A dependency with name '{0}' has already been added", name ) );

			var dependency = new Dependency<TAddress>( name, address, objectContext );
			this.Dependencies.Add( dependency );
		}

		#endregion
	}
}
