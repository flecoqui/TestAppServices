using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using System.Collections.Concurrent;
using Windows.Foundation.Collections;

namespace AppServiceTask
{
    class ConnectionContext
    {
        public bool bConnected;
        public string CallerPackage;
        public AppServiceConnection Connection;
    }
    public sealed class AppServiceTask : IBackgroundTask
    {
        BackgroundTaskDeferral serviceDeferral;
        static ConcurrentDictionary<Guid, ConnectionContext> localConnectionList;

        static ConcurrentDictionary<Guid, ConnectionContext> remoteConnectionList;
        public void Run(IBackgroundTaskInstance taskInstance)
        {

            if (taskInstance != null)
            {
                System.Diagnostics.Debug.WriteLine("BackgroundTask Run for TaskID: " + taskInstance.InstanceId.ToString() );
                
                //Take a service deferral so the service isn't terminated
                serviceDeferral = taskInstance.GetDeferral();

                if(localConnectionList == null)
                {
                    localConnectionList = new ConcurrentDictionary<Guid, ConnectionContext>();
                }

                if (remoteConnectionList == null)
                {
                    remoteConnectionList = new ConcurrentDictionary<Guid, ConnectionContext>();
                }

                taskInstance.Canceled += OnTaskCanceled;


                var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
                if (details != null)
                {
                    if ((details.IsRemoteSystemConnection == true)||(!string.Equals(details.CallerPackageFamilyName,AppServiceTaskConstant.APPSERVICEPACKAGEFAMILY)))
                    {
                        ConnectionContext c = new ConnectionContext();
                        c.Connection = details.AppServiceConnection;
                        c.CallerPackage = details.CallerPackageFamilyName;
                        c.bConnected = false;
                        c.Connection.RequestReceived += OnRequestReceived;
                        c.Connection.ServiceClosed += RemoteConnection_ServiceClosed;
                        AddConnection(true, taskInstance.InstanceId, c);
                    }
                    else
                    {
                        ConnectionContext c = new ConnectionContext();
                        c.Connection = details.AppServiceConnection;
                        c.CallerPackage = details.CallerPackageFamilyName;
                        c.bConnected = false;
                        c.Connection.RequestReceived += OnRequestReceived;
                        c.Connection.ServiceClosed += LocalConnection_ServiceClosed;
                        AddConnection(false, taskInstance.InstanceId, c);
                    }

                }
                System.Diagnostics.Debug.WriteLine("AppService Task loaded");
            }
        }
        bool AddConnection(bool bRemote, Guid id, ConnectionContext c)
        {
            bool result = false;
            if(bRemote)
            {
                if (!remoteConnectionList.ContainsKey(id))
                    remoteConnectionList.TryAdd(id, c);
                else
                    remoteConnectionList[id] = c;
                result = true;
            }
            else
            {
                if(localConnectionList.Count>0)
                {
                    // Only one local connection is possible
                    CloseAllLocalConnection();
                }
                if (localConnectionList.Count == 0)
                {
                    if (!localConnectionList.ContainsKey(id))
                        localConnectionList.TryAdd(id, c);
                    else
                        localConnectionList[id] = c;
                    result = true;
                }
                else
                    result = false;
            }
            if (result == true)
                System.Diagnostics.Debug.WriteLine("CreateConnection for TaskID: " + id.ToString() + " successful");
            else
                System.Diagnostics.Debug.WriteLine("CreateConnection for TaskID: " + id.ToString() + " failed");

            return result; 
        }
        bool RemoveConnection(bool bRemote, Guid id)
        {
            bool result = false;
            if (bRemote)
            {
                if (remoteConnectionList.ContainsKey(id))
                {
                    ConnectionContext c;
                    if(remoteConnectionList.TryRemove(id, out c)==true)
                        result = true;

                }
            }
            else
            {
                if (localConnectionList.ContainsKey(id))
                {
                    ConnectionContext c;
                    if (localConnectionList.TryRemove(id, out c) == true)
                        result = true;
                }
            }
            return result;
        }
        bool CloseConnection(bool bRemote, AppServiceConnection c)
        {
            bool bResult = false;
            if(bRemote)
            {
                if (remoteConnectionList != null)
                {
                    foreach (var val in remoteConnectionList)
                    {
                        if (val.Value.Connection == c)
                        {
                            val.Value.Connection.RequestReceived -= OnRequestReceived;
                            val.Value.Connection.ServiceClosed -= RemoteConnection_ServiceClosed;
                            val.Value.Connection = null;
                            val.Value.bConnected = false;
                            if(RemoveConnection(true, val.Key)==true)
                                bResult = true;
                        }
                    }
                }
            }
            else
            {
                if (localConnectionList != null)
                {
                    foreach(var val in localConnectionList)
                    {
                        if(val.Value.Connection == c)
                        {
                            val.Value.Connection.RequestReceived -= OnRequestReceived;
                            val.Value.Connection.ServiceClosed -= LocalConnection_ServiceClosed;
                            val.Value.Connection = null;
                            val.Value.bConnected = false;
                            if(RemoveConnection(false, val.Key)==true)
                                bResult = true;
                        }
                    }
                }
            }
            return bResult;
        }
        bool CloseConnection(Guid id)
        {
            bool bResult = false;
            if (remoteConnectionList != null)
            {
                foreach (var val in remoteConnectionList)
                {
                    if (val.Key == id)
                    {
                        val.Value.Connection.RequestReceived -= OnRequestReceived;
                        val.Value.Connection.ServiceClosed -= RemoteConnection_ServiceClosed;
                        val.Value.Connection = null;
                        val.Value.bConnected = false;
                        if(RemoveConnection(true, val.Key)==true)
                            bResult = true;
                    }
                }
            }
            if (localConnectionList != null)
            {
                foreach (var val in localConnectionList)
                {
                    if (val.Key == id)
                    {
                        val.Value.Connection.RequestReceived -= OnRequestReceived;
                        val.Value.Connection.ServiceClosed -= LocalConnection_ServiceClosed;
                        val.Value.Connection = null;
                        val.Value.bConnected = false;
                        if(RemoveConnection(false, val.Key)==true)
                            bResult = true;
                    }
                }
            }
            if(bResult==true)
            System.Diagnostics.Debug.WriteLine("CloseConnection for TaskID: " + id.ToString() + " successful");
            else
            System.Diagnostics.Debug.WriteLine("CloseConnection for TaskID: " + id.ToString() + " failed");
            return bResult;
        }
        bool CloseAllLocalConnection()
        {
            bool bResult = false;
            if (localConnectionList != null)
            {
                foreach (var val in localConnectionList)
                {
                    val.Value.Connection.RequestReceived -= OnRequestReceived;
                    val.Value.Connection.ServiceClosed -= LocalConnection_ServiceClosed;
                    val.Value.Connection = null;
                    val.Value.bConnected = false;
                    if (RemoveConnection(false, val.Key) == true)
                        bResult = true;
                }
            }
            if (bResult == true)
                System.Diagnostics.Debug.WriteLine("CloseAllConnection: successful");
            else
                System.Diagnostics.Debug.WriteLine("CloseAllConnection: failed");
            return bResult;
        }
        private void LocalConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            CloseConnection(false, sender);
        }
        private void RemoteConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            CloseConnection(true, sender);
        }
        async System.Threading.Tasks.Task<bool> SendMessage(bool bRemote, ValueSet message)
        {
            bool bResult = false;
            int successCounter = 0;
            int errorCounter = 0;
            if(bRemote == true)
            {

                if (remoteConnectionList != null)
                {
                    foreach (var val in remoteConnectionList)
                    {
                        if ((val.Value.bConnected == true)&&(val.Value.Connection != null))
                        {
                            AppServiceResponse resp = await val.Value.Connection.SendMessageAsync(message);
                            if (resp.Status == AppServiceResponseStatus.Success)
                                successCounter++;
                            else
                                errorCounter++;
                        }
                    }
                }
            }
            else
            {
                if (localConnectionList != null)
                {
                    foreach (var val in localConnectionList)
                    {
                        if ((val.Value.bConnected == true) && (val.Value.Connection != null))
                        {
                            AppServiceResponse resp = await val.Value.Connection.SendMessageAsync(message);
                            if (resp.Status == AppServiceResponseStatus.Success)
                                successCounter++;
                            else
                                errorCounter++;
                        }
                    }
                }
            }

            if((successCounter>0)&&(errorCounter==0))
                bResult = true;
            return bResult;
        }
        bool SetConnected(bool bRemote, AppServiceConnection connection, bool bInitialized)
        {
            bool bResult = false;
            if (bRemote == true)
            {

                if (remoteConnectionList != null)
                {
                    foreach (var val in remoteConnectionList)
                    {
                        if ((val.Value.bConnected == false) && (val.Value.Connection == connection))
                        {
                            val.Value.bConnected = true;
                            bResult = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (localConnectionList != null)
                {
                    foreach (var val in localConnectionList)
                    {
                        if ((val.Value.bConnected == false) && (val.Value.Connection == connection))
                        {
                            val.Value.bConnected = true;
                            bResult = true;
                            break;
                        }
                    }
                }
            }

            return bResult;
        }
        async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            //Get a deferral so we can use an awaitable API to respond to the message
            var messageDeferral = args.GetDeferral();
            var result = new ValueSet();
            string response = AppServiceTaskConstant.RESULT_FIELD_ERROR_VALUE;
            try
            {
                if ((args.Request != null) && (args.Request.Message != null))
                {
                    var inputs = args.Request.Message;
                    if (inputs.ContainsKey(AppServiceTaskConstant.COMMAND_FIELD))
                    {
                        string s = (string)inputs[AppServiceTaskConstant.COMMAND_FIELD];
                        if (string.Equals(s, AppServiceTaskConstant.COMMAND_FIELD_DATA_VALUE))
                        {
                            if ((inputs.ContainsKey(AppServiceTaskConstant.DATA_FIELD)) &&
                            (inputs.ContainsKey(AppServiceTaskConstant.SOURCE_FIELD)) ) 
                            {
                                string data = (string)inputs[AppServiceTaskConstant.DATA_FIELD];
                                string source = (string)inputs[AppServiceTaskConstant.SOURCE_FIELD];
                                if ((string.Equals(source, AppServiceTaskConstant.SOURCE_FIELD_LOCAL_VALUE)) &&
                                    (remoteConnectionList != null) &&
                                    (remoteConnectionList.Count > 0))
                                {
                                    bool res = await SendMessage(true, inputs);
                                    if (res == true)
                                    {
                                        System.Diagnostics.Debug.WriteLine("AppService Task data sent");
                                        response = AppServiceTaskConstant.RESULT_FIELD_OK_VALUE;
                                    }
                                    else
                                        response = AppServiceTaskConstant.RESULT_FIELD_ERROR_VALUE;
                                }
                                else if ((string.Equals(source, AppServiceTaskConstant.SOURCE_FIELD_REMOTE_VALUE)) &&
                                    (localConnectionList != null)&&
                                    (localConnectionList.Count > 0))
                                {
                                    bool res = await SendMessage(false, inputs);
                                    if (res == true)
                                    {
                                        System.Diagnostics.Debug.WriteLine("AppService Task data sent");
                                        response = AppServiceTaskConstant.RESULT_FIELD_OK_VALUE;
                                    }
                                    else
                                        response = AppServiceTaskConstant.RESULT_FIELD_ERROR_VALUE;
                                }
                                else
                                    response = AppServiceTaskConstant.RESULT_FIELD_ERROR_VALUE;
                            }
                        }
                        else if (string.Equals(s, AppServiceTaskConstant.COMMAND_FIELD_INIT_VALUE))
                        {
                            // Background task started
                            if ((inputs.ContainsKey(AppServiceTaskConstant.SOURCE_FIELD)) )
                            {

                                string source = (string)inputs[AppServiceTaskConstant.SOURCE_FIELD];
                                if( (string.Equals(source, AppServiceTaskConstant.SOURCE_FIELD_LOCAL_VALUE))&&
                                    (localConnectionList != null))
                                {
                                    SetConnected(false, sender, true);
                                    response =  AppServiceTaskConstant.RESULT_FIELD_OK_VALUE;
                                    System.Diagnostics.Debug.WriteLine("AppService Connection established");
                                }
                                else if ((string.Equals(source, AppServiceTaskConstant.SOURCE_FIELD_REMOTE_VALUE)) &&
                                    (remoteConnectionList != null))
                                {
                                    SetConnected(true, sender, true);
                                    response = AppServiceTaskConstant.RESULT_FIELD_OK_VALUE;
                                    System.Diagnostics.Debug.WriteLine("AppService Connection established");
                                }
                                else
                                    response = AppServiceTaskConstant.RESULT_FIELD_ERROR_VALUE;


                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception while receiving message: " + e.Message);
            }
            finally
            {
                result.Add(AppServiceTaskConstant.RESULT_FIELD, response);
                await args.Request.SendResponseAsync(result);
                //Complete the message deferral so the platform knows we're done responding
                messageDeferral.Complete();
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (serviceDeferral != null)
            {
                CloseConnection(sender.InstanceId);
                serviceDeferral.Complete();
                serviceDeferral = null;
            }
        }
    }
}
