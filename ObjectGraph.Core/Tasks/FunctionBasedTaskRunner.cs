using System;
using System.Collections.Generic;
using System.Text;

namespace BorsukSoftware.ObjectGraph.Tasks
{
    /// <summary>
    /// Standard function based implementation of <see cref="ITaskRunner"/>
    /// </summary>
    /// <remarks>This class can be useful where a consumer wants to have fine grained control over what happens with a given task</remarks>
    public class FunctionBasedTaskRunner : ITaskRunner
    {
        #region Data Model

        /// <summary>
        /// Get the function which should be called when a task is registered
        /// </summary>
        public Action<ITask> RegisterTaskFunc { get; private set; }

        #endregion

        /// <summary>
        /// Construct a new instance based off the supply function
        /// </summary>
        /// <param name="registerTaskFunc">The function which will process each registered task</param>
        public FunctionBasedTaskRunner( Action<ITask> registerTaskFunc)
        {
            if (registerTaskFunc == null)
                throw new ArgumentNullException(nameof(registerTaskFunc));

            this.RegisterTaskFunc = registerTaskFunc;
        }

        #region ITaskRunner Members

        public void RegisterTask(ITask task)
        {
            this.RegisterTaskFunc(task);
        }

        #endregion
    }
}
