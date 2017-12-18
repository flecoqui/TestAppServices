using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace AppServiceTask
{
    public sealed class AppServiceTask : IBackgroundTask
    {
        BackgroundTaskDeferral serviceDeferral;
        static AppServiceConnection localConnection;
        static bool localConnectionInitialized = false;
        static string localIPAddress = string.Empty;
        static AppServiceConnection remoteConnection;
        static bool remoteConnectionInitialized = false;
        static string remoteIPAddress = string.Empty;
        static Dictionary<Guid, AppServiceConnection> connectionList;
        public void Run(IBackgroundTaskInstance taskInstance)
        {

            if (taskInstance != null)
            {
                System.Diagnostics.Debug.WriteLine("BackgroundTask Run for TaskID: " + taskInstance.InstanceId.ToString() + " Task name: " + taskInstance.Task.Name);
                //Take a service deferral so the service isn't terminated
                serviceDeferral = taskInstance.GetDeferral();

                if(connectionList==null)
                {
                    connectionList = new Dictionary<Guid, AppServiceConnection>();
                }
                taskInstance.Canceled += OnTaskCanceled;


                var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
                if (details != null)
                {
                    if ((details.IsRemoteSystemConnection == true)||(!string.Equals(details.CallerPackageFamilyName,AppServiceTaskConstant.APPSERVICEPACKAGEFAMILY)))
                    {
                        remoteConnection = details.AppServiceConnection;
                        remoteConnection.RequestReceived += OnRequestReceived;
                        remoteConnection.ServiceClosed += RemoteConnection_ServiceClosed;
                    }
                    else
                    {
                        localConnection = details.AppServiceConnection;
                        localConnection.RequestReceived += OnRequestReceived;
                        localConnection.ServiceClosed += LocalConnection_ServiceClosed;
                    }
                    if(!connectionList.ContainsKey(taskInstance.InstanceId))
                        connectionList.Add(taskInstance.InstanceId, details.AppServiceConnection);
                    else
                    {

                        System.Diagnostics.Debug.WriteLine("AppService Task loaded Error");
                    }
                }
                System.Diagnostics.Debug.WriteLine("AppService Task loaded");
            }
        }
        void CloseLocalConnection()
        {
            if (localConnection != null)
            {
                localConnection.RequestReceived -= OnRequestReceived;
                localConnection.ServiceClosed -= LocalConnection_ServiceClosed;
                localConnection = null;
                System.Diagnostics.Debug.WriteLine("AppService Task unloaded for local connectionx");
            }
        }
        void CloseRemoteConnection()
        {
            if (remoteConnection != null)
            {
                remoteConnection.RequestReceived -= OnRequestReceived;
                remoteConnection.ServiceClosed -= RemoteConnection_ServiceClosed;
                remoteConnection = null;
                System.Diagnostics.Debug.WriteLine("AppService Task unloaded for remote connection");
            }
        }
        private void LocalConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            CloseLocalConnection();
        }
        private void RemoteConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            CloseRemoteConnection();
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
                            (inputs.ContainsKey(AppServiceTaskConstant.SOURCE_FIELD)) &&
                            (inputs.ContainsKey(AppServiceTaskConstant.IPSOURCE_FIELD))) 
                            {
                                string data = (string)inputs[AppServiceTaskConstant.DATA_FIELD];
                                string source = (string)inputs[AppServiceTaskConstant.SOURCE_FIELD];
                                string ip = (string)inputs[AppServiceTaskConstant.IPSOURCE_FIELD];
                                if ((string.Equals(source, AppServiceTaskConstant.SOURCE_FIELD_LOCAL_VALUE)) &&
                                    (remoteConnection != null) &&
                                    (remoteConnectionInitialized == true))
                                {
                                    AppServiceResponse resp = await remoteConnection.SendMessageAsync(inputs);
                                    if (resp.Status == AppServiceResponseStatus.Success)
                                    {
                                        System.Diagnostics.Debug.WriteLine("AppService Task data sent");
                                        response = AppServiceTaskConstant.RESULT_FIELD_OK_VALUE;
                                    }
                                    else
                                        response = AppServiceTaskConstant.RESULT_FIELD_ERROR_VALUE;
                                }
                                else if ((string.Equals(source, AppServiceTaskConstant.SOURCE_FIELD_REMOTE_VALUE)) &&
                                    (localConnection != null)&&
                                    (localConnectionInitialized == true))
                                {
                                    AppServiceResponse resp = await localConnection.SendMessageAsync(inputs);
                                    if (resp.Status == AppServiceResponseStatus.Success)
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
                            if ((inputs.ContainsKey(AppServiceTaskConstant.SOURCE_FIELD)) && 
                                (inputs.ContainsKey(AppServiceTaskConstant.IPSOURCE_FIELD)))
                            {

                                string source = (string)inputs[AppServiceTaskConstant.SOURCE_FIELD];
                                string ip = (string)inputs[AppServiceTaskConstant.IPSOURCE_FIELD];
                                if( (string.Equals(source, AppServiceTaskConstant.SOURCE_FIELD_LOCAL_VALUE))&&
                                    (localConnection != null))
                                {
                                    localIPAddress = ip;
                                    localConnectionInitialized = true;
                                    response =  AppServiceTaskConstant.RESULT_FIELD_OK_VALUE;
                                    System.Diagnostics.Debug.WriteLine("AppService Task Initialized");

                                }
                                else if ((string.Equals(source, AppServiceTaskConstant.SOURCE_FIELD_REMOTE_VALUE)) &&
                                    (remoteConnection != null))
                                {
                                    remoteIPAddress = ip;
                                    remoteConnectionInitialized = true;
                                    response = AppServiceTaskConstant.RESULT_FIELD_OK_VALUE;
                                    System.Diagnostics.Debug.WriteLine("AppService Task Initialized");
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
                if (connectionList.ContainsKey(sender.InstanceId))
                {
                    AppServiceConnection loc = connectionList[sender.InstanceId];
                    if(loc == localConnection)
                        CloseLocalConnection();
                    if (loc == remoteConnection)
                        CloseRemoteConnection();
                }
                serviceDeferral.Complete();
                serviceDeferral = null;
            }
        }
    }
}
