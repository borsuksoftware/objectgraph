using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	/// <summary>
	/// Interface representing the context for calls to <see cref="IObjectBuilder{TAddress}.GetAdditionalDependencies(IObjectBuilderGetAdditionalDependenciesContext{TAddress}, TAddress, IBuiltDependencies{TAddress})"/>
	/// </summary>
	public interface IObjectBuilderGetAdditionalDependenciesContext<TAddress>
	{
		/// <summary>
		/// Gets the object context for this item
		/// </summary>
		IObjectContext<TAddress> ObjectContext { get; }
	}
}
