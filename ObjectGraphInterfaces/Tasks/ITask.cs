using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.Tasks
{
	/// <summary>
	/// Interface representing a job within the job framework
	/// </summary>
	public interface ITask
	{
		/// <summary>
		/// Called when the job has been cancelled
		/// </summary>
		void Cancel();

		/// <summary>
		/// Called when the job is to be run
		/// </summary>
		void Run();
	}
}
