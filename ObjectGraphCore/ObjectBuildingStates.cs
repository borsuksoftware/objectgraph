using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph
{
	/// <summary>
	/// Enum representing the states of an object builder context
	/// </summary>
	public enum ObjectBuildingStates
	{
		Starting = 0,

		NoBuilderAvailable = 1,

		// SourcingDependencies = 2,

		DependenciesKnown = 3,

		// ObjectBuilding = 4,

		ObjectBuilt = 5,

		Failure = 6,
	}
}
