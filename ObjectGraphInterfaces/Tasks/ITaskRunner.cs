using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectGraph.Tasks
{
	public interface ITaskRunner
	{
		/// <summary>
		/// Registers the given <see cref="ITask"/> as a task to be subsequently performed
		/// </summary>
		/// <param name="task">The action to be performed</param>
		void RegisterTask(ITask task);
	}
}
