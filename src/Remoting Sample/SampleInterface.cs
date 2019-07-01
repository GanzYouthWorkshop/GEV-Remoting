using GEV.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public interface SampleInterface : IRemoteService
    {
        int Add(int a, int b);
        void LogOnServer();
    }
}
