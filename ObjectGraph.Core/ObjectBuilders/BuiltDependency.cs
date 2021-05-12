using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.ObjectBuilders
{
	public class BuiltDependency<TAddress> : IBuiltDependency<TAddress>
	{
		public string Name { get; private set; }

		public TAddress Address { get; private set; }

		public object BuiltObject { get; private set; }

		public BuiltDependency( string name, TAddress address, object builtObject )
		{
			this.Name = name;
			this.Address = address;
			this.BuiltObject = builtObject;
		}
	}
}
