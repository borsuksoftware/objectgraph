namespace BorsukSoftware.ObjectGraph.Tasks
{
	/// <summary>
	/// Class containing the settings for <see cref="TaskRunner"/>
	/// </summary>
	public class TaskRunnerSettings
	{
		/// <summary>
		/// Gets the number of threads to spawn (if appropriate)
		/// </summary>
		public int? ThreadCount { get; set; }
	}
}