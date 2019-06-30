using GEV.Common;
using GEV.Remoting.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GEV.Remoting
{
    /// <summary>
    /// Handles network-layer functionality for a <see cref="RemoteService"/> subscribtion.
    /// </summary>
    public class ServiceSubscriber
    {
        private class ReturnHandler
        {
            public ManualResetEvent ResetEvent { get; set; }
            public object ReturnValue { get; set; }
        }

        private TcpCommunicator<BoxedObject> m_Communicator;

        private object returnLock = false;
        private Dictionary<int, ReturnHandler> returnValues = new Dictionary<int, ReturnHandler>();

        public ServiceSubscriber(string host, int port)
        {
            this.m_Communicator = new TcpCommunicator<BoxedObject>(host, port);
            this.m_Communicator.DispatchMessageInEvent = true;
            this.m_Communicator.MessageReceived += M_Communicator_MessageReceived;
            this.m_Communicator.Connect();
        }

        private void M_Communicator_MessageReceived(object sender, BoxedObject e)
        {
            //Checking incoming data
            if(e.DataType == typeof(MethodCall.Response))
            {
                //Response is a method return
                MethodCall.Response response = (MethodCall.Response)Convert.ChangeType(e.Value, e.DataType);

                lock(this.returnLock)
                {
                    //Setting the ResetEvent and the return value
                    if(this.returnValues.ContainsKey(response.MessageId))
                    {
                        this.returnValues[response.MessageId].ReturnValue = response.Value;
                        this.returnValues[response.MessageId].ResetEvent.Set();
                    }
                }
            }
        }

        public void CallVoid(string name, Dictionary<string, object> parameters)
        {
            //Calling the remote method
            this.m_Communicator.OutMessages.Enqueue(new BoxedObject(new MethodCall()
            {
                MethodName = name,
                Parameters = parameters,
            }));
        }

        public object CallMethod(string name, Dictionary<string, object> parameters)
        {
            //Setting up threading
            ManualResetEvent eventer = new ManualResetEvent(false);

            //Calling the remote method
            MethodCall call = new MethodCall()
            {
                MethodName = name,
                Parameters = parameters,
            };

            this.m_Communicator.OutMessages.Enqueue(new BoxedObject(call));
            this.returnValues.Add(call.MessageId, new ReturnHandler()
            {
                ResetEvent = eventer,
                ReturnValue = null,
            });

            //Waiting for a return
            eventer.WaitOne();

            //Remote method call returned, setting return value
            object result = null;
            lock(this.returnLock)
            {
                result = this.returnValues[call.MessageId].ReturnValue;
                this.returnValues.Remove(call.MessageId);
            }

            //Returning raw object - strong-typed returning is handled by the proxy itself
            return result;
        }
    }
}
