using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace OSCAL_SSP_Mapper
{
    public class CurrentTask
    {
        private static object syncRoot = new object();
        /// <summary>
        /// Gets or sets the process status.
        /// </summary>
        /// <value>The process status.</value>
        private static IDictionary<string, int> ProcessStatus { get; set; }

        public CurrentTask()
        {
            if (ProcessStatus == null)
            {
                ProcessStatus = new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Processes the long running action.
        /// </summary>
        /// <param name="id">The id.</param>
        public string ProcessLongRunningAction(string id)
        {
            for (int i = 1; i <= 100; i++)
            {
                Thread.Sleep(100);
                lock (syncRoot)
                {
                    ProcessStatus[id] = i;
                }
            }
            return id;
        }

        /// <summary>
        /// Adds the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        public void Add(string id)
        {
            lock (syncRoot)
            {
                ProcessStatus.Add(id, 0);
            }
        }

        /// <summary>
        /// Removes the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        public void Remove(string id)
        {
            lock (syncRoot)
            {
                ProcessStatus.Remove(id);
            }
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <param name="id">The id.</param>
        public int GetStatus(string id)
        {
            lock (syncRoot)
            {
                if (ProcessStatus.Keys.Count(x => x == id) == 1)
                {
                    return ProcessStatus[id];
                }
                else
                {
                    return 100;
                }
            }
        }
    }
}