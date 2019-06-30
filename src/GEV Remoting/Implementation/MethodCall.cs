using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEV.Remoting.Implementation
{
    /// <summary>
    /// Describes a remote rocedure call.
    /// </summary>
    [Serializable]
    internal class MethodCall
    {
        /// <summary>
        /// A response to a remote procedure call.
        /// </summary>
        [Serializable]
        public class Response
        {
            public int MessageId { get; set; }
            public object Value { get; set; }
        }

        public int MessageId { get; } = AccessCode.Id;
        public string MethodName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
