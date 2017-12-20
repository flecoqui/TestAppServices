using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppServiceTask;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using System.Threading;
using System.Globalization;

namespace Win32Proc
{

    class Program
    {
        static bool appServiceConnectionInitialized = false;
        static AppServiceConnection connection = null;
        static AutoResetEvent appServiceExit;
        static System.Threading.Tasks.TaskFactory taskFactory;
        static string filePath = "C:\\temp\\win32proc.txt";
        static void ClearLog()
        {
            string Text = string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + "Win32Proc Starting\r\n";
            System.IO.File.WriteAllText(filePath, Text);
        }
        static void LogMessage(string message)
        {
            string Text = string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " " + message + "\r\n";
            System.Diagnostics.Debug.WriteLine(Text);
            System.IO.File.AppendAllText(filePath, Text + "\n\r");
        }
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            var cultureUi = CultureInfo.CurrentUICulture;
            var culture = CultureInfo.CurrentCulture;
            return taskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            }).Unwrap().GetAwaiter().GetResult();
        }
        static  void Main(string[] args)
        {
            // connect to app service and wait until the connection gets closed
            taskFactory = new TaskFactory();
            LogMessage("Win32Proc: Main");
            appServiceExit = new AutoResetEvent(false);
            LogMessage("Win32Proc: Calling ConnectToAppService");
            bool result = RunSync(() => ConnectToAppService());
            if (result == true)
            {
                LogMessage("Win32Proc: Calling SendIntializeCommandToAppService");
                result = RunSync(() => SendIntializeCommandToAppService());
                if (result == true)
                {
                    LogMessage("Win32Proc: Win32Proc Connected to AppService");
                }
            }
            appServiceExit.WaitOne();
        }

        static async System.Threading.Tasks.Task<bool> ConnectToAppService()
        {
            bool bResult = false;
            LogMessage("Win32Proc: Connect To AppService");
            connection = new AppServiceConnection();
            connection.AppServiceName = AppServiceTaskConstant.APPSERVICENAME; 
            connection.PackageFamilyName = AppServiceTaskConstant.APPSERVICEPACKAGEFAMILY;
            connection.RequestReceived += Connection_RequestReceived;
            connection.ServiceClosed += Connection_ServiceClosed;

            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status == AppServiceConnectionStatus.Success)
            {
                bResult = true;
                // TODO: error handling
                LogMessage("Win32Proc: Connect To AppService Successful");
            }
            else
                LogMessage("Win32Proc: Failed to Connect To AppService - Status: " + status.ToString());
            return bResult;
        }
        static async System.Threading.Tasks.Task<bool> SendIntializeCommandToAppService()
        {
            bool bResult = false;
            try
            {
                LogMessage("Win32Proc: Send Initialize Command to App Service");
                if (connection != null)
                {
                    var message = new ValueSet();
                    if (message != null)
                    {
                        message.Add(AppServiceTaskConstant.COMMAND_FIELD, AppServiceTaskConstant.COMMAND_FIELD_INIT_VALUE);
                        message.Add(AppServiceTaskConstant.SOURCE_FIELD, AppServiceTaskConstant.SOURCE_FIELD_LOCAL_VALUE);
                        AppServiceResponse response = await connection.SendMessageAsync(message);
                        if (response.Status != AppServiceResponseStatus.Success)
                        {
                            appServiceConnectionInitialized = false;
                            LogMessage("Win32Proc: Failed to Send Initialize Command to App Service");
                        }
                        else
                        {
                            if (response.Message.ContainsKey(AppServiceTaskConstant.RESULT_FIELD))
                            {

                                string result = (string)response.Message[AppServiceTaskConstant.RESULT_FIELD];
                                if (string.Equals(result, AppServiceTaskConstant.RESULT_FIELD_OK_VALUE))
                                {
                                    appServiceConnectionInitialized = true;
                                    bResult = true;
                                    LogMessage("Send Initialize Command to App Service successful");
                                }
                                else
                                {
                                    appServiceConnectionInitialized = false;
                                    LogMessage("Win32Proc: Failed to Send Initialize Command to Initialize App Service: " + result);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("Win32Proc: Failed to Send Initialize Command to communication with App Service: " + ex.Message);
            }

            return bResult;
        }

        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            // signal the event so the process can shut down
            appServiceExit.Set();
        }
        private static async System.Threading.Tasks.Task<bool> SendMessage(string payload)
        {
            bool bResult = false;
            var message = new ValueSet();
            if (message != null)
            {
                message.Add(AppServiceTaskConstant.COMMAND_FIELD, AppServiceTaskConstant.COMMAND_FIELD_DATA_VALUE);
                message.Add(AppServiceTaskConstant.SOURCE_FIELD, AppServiceTaskConstant.SOURCE_FIELD_LOCAL_VALUE);
                message.Add(AppServiceTaskConstant.DATA_FIELD, payload);
                AppServiceResponse response = await connection.SendMessageAsync(message);
                if (response.Status != AppServiceResponseStatus.Success)
                {
                    LogMessage("Failed to send data to App Service");
                }
                else
                {
                    if (response.Message.ContainsKey(AppServiceTaskConstant.RESULT_FIELD))
                    {

                        string result = (string)response.Message[AppServiceTaskConstant.RESULT_FIELD];
                        if (string.Equals(result, AppServiceTaskConstant.RESULT_FIELD_OK_VALUE))
                        {
                            bResult = true;
                            LogMessage("Send Data to App Service successful");
                        }
                        else
                        {
                            LogMessage("Failed to send data to App Service: " + result);
                        }
                    }
                }
            }
            return bResult;
        }
        private async static void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var result = new ValueSet();
            string data = string.Empty;
            string source = string.Empty;
            string response = AppServiceTaskConstant.RESULT_FIELD_ERROR_VALUE;
            try
            {
                if (appServiceConnectionInitialized)
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
                                (inputs.ContainsKey(AppServiceTaskConstant.SOURCE_FIELD)))
                                {
                                    data = (string)inputs[AppServiceTaskConstant.DATA_FIELD];
                                    source = (string)inputs[AppServiceTaskConstant.SOURCE_FIELD];

                                    LogMessage("Receive Message from " + source + " message: " + data);
                                    response = AppServiceTaskConstant.RESULT_FIELD_OK_VALUE;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception while receiving message: " + e.Message);
            }
            finally
            {
                result.Add(AppServiceTaskConstant.RESULT_FIELD, response);
                await args.Request.SendResponseAsync(result);
            }
            if(!string.IsNullOrEmpty(data))
            {
                await SendMessage("response from Win32Proc: " + data);
            }
        }

    }

}
