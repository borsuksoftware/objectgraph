using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorsukSoftware.ObjectGraph.Tasks
{
	/// <summary>
	/// Multi-threaded task runner for use within the object graph framework
	/// </summary>
	public class TaskRunner : ITaskRunner
	{
		#region Member variables

		private volatile bool _quit = false;

		private System.Threading.ManualResetEvent _resetEvent = new System.Threading.ManualResetEvent( false );

		private Queue<ITask> _tasks = new Queue<ITask>();

		private List<System.Threading.Thread> _threads;

		#endregion

		#region Construction Logic

		public TaskRunner( TaskRunnerSettings settings )
		{
			if( settings == null )
				settings = new TaskRunnerSettings();

			if( settings.ThreadCount.HasValue &&
				settings.ThreadCount.Value <= 0 )
				throw new InvalidOperationException( string.Format( "A valid number (null | >= 1) of threads must be specified " ) );

			int threadCount = settings.ThreadCount.HasValue ?
				settings.ThreadCount.Value :
				System.Environment.ProcessorCount;

			Console.Out.WriteLine( "Spawning {0} threads", threadCount );
			this._threads = new List<System.Threading.Thread>( threadCount );

			for( int i = 0; i < threadCount; i++ )
			{
				var thread = new System.Threading.Thread( new System.Threading.ParameterizedThreadStart( ( val ) => ThreadStart( (int) val ) ) )
				{
					Name = $"Thread Pool #{i}",
					IsBackground = true
				};

				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
					thread.SetApartmentState(System.Threading.ApartmentState.MTA); thread.Start( i );

				_threads.Add( thread );
			}
		}

		#endregion

		#region Class functionality 

		/// <summary>
		/// Registers the given <see cref="Action"/> as a task to be subsequently performed
		/// </summary>
		/// <param name="action">The action to be performed</param>
		public void RegisterTask( ITask action )
		{
			if( action == null )
				throw new ArgumentNullException( nameof( action ) );

			lock( this._tasks )
			{
				_tasks.Enqueue( action );
				_resetEvent.Set();
			}
		}

		/// <summary>
		/// Stops all threads - note that any outstanding items in the work queue will be left incomplete
		/// </summary>
		public void StopThreads()
		{
			_quit = true;
			lock( _tasks )
			{
				ITask job;
				while( _tasks.Any() )
				{
					job = _tasks.Dequeue();
					try
					{
						job.Cancel();
					}
					catch
					{
					}
				}
				_resetEvent.Set();
			}
		}

		#endregion

		#region Thread Functionality

		private void ThreadStart( int id )
		{
			while( true )
			{
				_resetEvent.WaitOne();

				if( _quit )
					return;

				ITask job = null;
				lock( _tasks )
				{
					if( _tasks.Count > 0 )
						job = _tasks.Dequeue();
					else
						_resetEvent.Reset();
				}

				if( job == null )
					continue;

				// Console.Out.WriteLine( "Processing action on {0}", System.Threading.Thread.CurrentThread.ManagedThreadId );

				try
				{
					job.Run();
				}
				catch( Exception ex )
				{
					Console.Out.WriteLine( "Exception caught: {0}", ex );
				}
			}
		}

		#endregion
	}
}
