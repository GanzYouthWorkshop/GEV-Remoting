using GEV.Common;
using GEV.Remoting.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GEV.Remoting
{
    /// <summary>
    /// Can create an RPC-service via a TCP connection through a special interface without any additional code.
    /// </summary>
    public class RemoteService
    {
        private IRemoteService m_InternalService;
        private TcpCommunicator<BoxedObject> m_Communicator;
        private MethodInfo[] m_ServiceMethods;

        /// <summary>
        /// Opens a remote service that remote programs can connect (subscribe) to.
        /// </summary>
        /// <param name="service"></param>
        public RemoteService(IRemoteService service, int port)
        {
            //Starting network-layer
            this.m_InternalService = service;
            this.m_Communicator = new TcpCommunicator<BoxedObject>("127.0.0.1", port)
            {
                DispatchMessageInEvent = true,
            };
            this.m_Communicator.Open();
            this.m_Communicator.MessageReceived += M_Communicator_MessageReceived;

            //Getting all methodinfos in the start
            this.m_ServiceMethods = service.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(mi => mi.IsSpecialName == false).ToArray();
        }

        private void M_Communicator_MessageReceived(object sender, BoxedObject e)
        {
            //Checking incoming data
            if(e.DataType == typeof(MethodCall))
            {
                MethodCall call = (MethodCall)Convert.ChangeType(e.Value, e.DataType);

                //Check if method call request is valid
                MethodInfo method = this.m_ServiceMethods.FirstOrDefault(m => m.Name == call.MethodName);
                if(method != null)
                {
                    object returnValue = null;
                    try
                    {
                        //Calling local method
                        returnValue = method.Invoke(this.m_InternalService, call.Parameters.Values.ToArray());
                    }
                    catch(Exception ex)
                    {
                        //If exception is catched set it as return value
                        returnValue = ex;
                    }

                    if (method.ReturnType != typeof(void) && returnValue.GetType() != typeof(Exception))
                    {
                        //If called method is not a void and no exception occured send the return
                        this.m_Communicator.OutMessages.Enqueue(new BoxedObject(new MethodCall.Response()
                        {
                            MessageId = call.MessageId,
                            Value = returnValue
                        }));
                    }
                }
            }
        }

        /// <summary>
        /// Connects (subscribes) to a <see cref="RemoteService"/> that has been opened.
        /// </summary>
        /// <typeparam name="T">Type of the service's uderlying interface.</typeparam>
        /// <param name="remoteAddress">Address of the remote <see cref="RemoteService"/>.</param>
        /// <returns></returns>
        public static T SubscribeToRemoteService<T>(string host, int port) where T : IRemoteService
        {
            return RemoteInterfaceGenerator.GenerateService<T>(host, port);
        }
    }
}
