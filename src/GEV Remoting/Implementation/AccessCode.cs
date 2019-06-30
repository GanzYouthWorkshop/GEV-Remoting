using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEV.Remoting.Implementation
{
    /// <summary>
    /// Impelemnts an auto-increment mechanism.
    /// </summary>
    internal class AccessCode
    {
        public static int Id { get { return m_Id++; } }
        private static int m_Id = 0;
    }
}
