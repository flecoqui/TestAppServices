using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using AppServiceTask;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AppServiceServerApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        AppServiceConnection appServiceConnection;
        bool appServiceConnectionInitialized = false;
        public MainPage()
        {
            this.InitializeComponent();
            LogMessage("MainPage constructor");
            appServiceConnection = null;
            appServiceConnectionInitialized = false;
            UpdateControls();
        }
        void UpdateControls()
        {

                    if (appServiceConnection == null)
                    {
                        loadAppServiceButton.IsEnabled = true;
                        unloadAppServiceButton.IsEnabled = false;
                        initAppServiceButton.IsEnabled = false;
                        sendDataAppServiceButton.IsEnabled = false;
                    }
                    else
                    {
                        loadAppServiceButton.IsEnabled = false;
                        unloadAppServiceButton.IsEnabled = true;
                        if (appServiceConnectionInitialized == true)
                        {
                            initAppServiceButton.IsEnabled = false;
                            sendDataAppServiceButton.IsEnabled = true;
                        }
                        else
                        {
                            initAppServiceButton.IsEnabled = true;
                            sendDataAppServiceButton.IsEnabled = false;
                        }
                    }
        }
        protected  override void OnNavigatedTo(NavigationEventArgs e)
        {
            LogMessage("MainPage OnNavigatedTo");
            // Logs event to refresh the TextBox
            logs.TextChanged += Logs_TextChanged;
            UpdateControls();
        }
        private async void LoadAppService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (appServiceConnection == null)
                {
                    appServiceConnection = new AppServiceConnection();
                    appServiceConnection.AppServiceName = AppServiceTaskConstant.APPSERVICENAME;
                    //appServiceConnection.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
                    appServiceConnection.PackageFamilyName = AppServiceTaskConstant.APPSERVICEPACKAGEFAMILY;
                    var status = await appServiceConnection.OpenAsync();
                    if (status != AppServiceConnectionStatus.Success)
                    {
                        appServiceConnection = null;
                        LogMessage("Failed to load App Service");
                    }
                    else
                    {
                        appServiceConnection.RequestReceived += AppServiceConnection_RequestReceived;
                        appServiceConnection.ServiceClosed += AppServiceConnection_ServiceClosed;
                        LogMessage("Load App Service successful");
                    }
                }
                UpdateControls();
            }
            catch (Exception ex)
            {
                LogMessage("Failed to load App Service - Exception: " + ex.Message);
            }
        }
        private void UnloadAppService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (appServiceConnection != null)
                {
                    CloseAppServiceConnection();
                    LogMessage("Unload App Service successful");
                }
                UpdateControls();
            }
            catch (Exception ex)
            {
                LogMessage("Failed to unload App Service - Exception: " + ex.Message);
            }
        }
        void CloseAppServiceConnection()
        {
            appServiceConnectionInitialized = false;
            if (appServiceConnection!=null)
            {
                appServiceConnection.RequestReceived -= AppServiceConnection_RequestReceived;
                appServiceConnection.ServiceClosed -= AppServiceConnection_ServiceClosed;
                appServiceConnection = null;
            }
        }

        private async void AppServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            CloseAppServiceConnection();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            () =>
            {
                UpdateControls();
            });
        }

        private async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
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

                                LogMessage("Receive Message from " + source + " ip: " + ip + " message: " + data);
                                response = AppServiceTaskConstant.RESULT_FIELD_OK_VALUE;
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

        }

        private async void InitAppService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (appServiceConnection != null)
                {
                    var message = new ValueSet();
                    if (message != null)
                    {
                        message.Add(AppServiceTaskConstant.COMMAND_FIELD, AppServiceTaskConstant.COMMAND_FIELD_INIT_VALUE);
                        message.Add(AppServiceTaskConstant.SOURCE_FIELD, AppServiceTaskConstant.SOURCE_FIELD_LOCAL_VALUE);
                        message.Add(AppServiceTaskConstant.IPSOURCE_FIELD, AppServiceTaskConstant.SOURCE_FIELD_LOCAL_VALUE);
                        AppServiceResponse response = await appServiceConnection.SendMessageAsync(message);
                        if (response.Status != AppServiceResponseStatus.Success)
                        {
                            appServiceConnectionInitialized = false;
                            LogMessage("Failed to initialize App Service");
                        }
                        else
                        {
                            if (response.Message.ContainsKey(AppServiceTaskConstant.RESULT_FIELD))
                            {

                                string result = (string)response.Message[AppServiceTaskConstant.RESULT_FIELD];
                                if (string.Equals(result, AppServiceTaskConstant.RESULT_FIELD_OK_VALUE))
                                {
                                    appServiceConnectionInitialized = true;
                                    LogMessage("Initialize App Service successful");
                                }
                                else
                                {
                                    appServiceConnectionInitialized = false;
                                    LogMessage("Failed to Initialize App Service: " + result);
                                }
                            }
                        }
                    }
                }
                UpdateControls();
            }
            catch (Exception ex)
            {
                LogMessage("Failed to initialize communication with App Service: " + ex.Message);
            }
        }
        private async void SendDataAppService_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((appServiceConnection != null)&&(appServiceConnectionInitialized == true ))
                {
                    var message = new ValueSet();
                    if (message != null)
                    {
                        message.Add(AppServiceTaskConstant.COMMAND_FIELD, AppServiceTaskConstant.COMMAND_FIELD_DATA_VALUE);
                        message.Add(AppServiceTaskConstant.SOURCE_FIELD, AppServiceTaskConstant.SOURCE_FIELD_LOCAL_VALUE);
                        message.Add(AppServiceTaskConstant.IPSOURCE_FIELD, AppServiceTaskConstant.SOURCE_FIELD_LOCAL_VALUE);
                        message.Add(AppServiceTaskConstant.DATA_FIELD, "Hello from local app");
                        AppServiceResponse response = await appServiceConnection.SendMessageAsync(message);
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
                                    LogMessage("Send Data to App Service successful");
                                }
                                else
                                {
                                    LogMessage("Failed to send data to App Service: " + result);
                                }
                            }
                        }
                    }
                }
                UpdateControls();
            }
            catch (Exception ex)
            {
                LogMessage("Failed to send data to App Service: " + ex.Message);
            }
        }


        #region Logs
        void PushMessage(string Message)
        {
            App app = Windows.UI.Xaml.Application.Current as App;
            if (app != null)
                app.MessageList.Enqueue(Message);
        }
        bool PopMessage(out string Message)
        {
            Message = string.Empty;
            App app = Windows.UI.Xaml.Application.Current as App;
            if (app != null)
                return app.MessageList.TryDequeue(out Message);
            return false;
        }
        /// <summary>
        /// Display Message on the application page
        /// </summary>
        /// <param name="Message">String to display</param>
        async void LogMessage(string Message)
        {
            string Text = string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " " + Message + "\n";
            PushMessage(Text);
            System.Diagnostics.Debug.WriteLine(Text);
            await DisplayLogMessage();
        }
        /// <summary>
        /// Display Message on the application page
        /// </summary>
        /// <param name="Message">String to display</param>
        async System.Threading.Tasks.Task<bool> DisplayLogMessage()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {

                    string result;
                    while (PopMessage(out result))
                    {
                        logs.Text += result;
                        if (logs.Text.Length > 16000)
                        {
                            string LocalString = logs.Text;
                            while (LocalString.Length > 12000)
                            {
                                int pos = LocalString.IndexOf('\n');
                                if (pos == -1)
                                    pos = LocalString.IndexOf('\r');


                                if ((pos >= 0) && (pos < LocalString.Length))
                                {
                                    LocalString = LocalString.Substring(pos + 1);
                                }
                                else
                                    break;
                            }
                            logs.Text = LocalString;
                        }
                    }
                }
            );
            return true;
        }
        /// <summary>
        /// This method is called when the content of the Logs TextBox changed  
        /// The method scroll to the bottom of the TextBox
        /// </summary>
        void Logs_TextChanged(object sender, TextChangedEventArgs e)
        {
            //  logs.Focus(FocusState.Programmatic);
            // logs.Select(logs.Text.Length, 0);
            var tbsv = GetFirstDescendantScrollViewer(logs);
            tbsv.ChangeView(null, tbsv.ScrollableHeight, null, true);
        }
        /// <summary>
        /// Retrieve the ScrollViewer associated with a control  
        /// </summary>
        ScrollViewer GetFirstDescendantScrollViewer(DependencyObject parent)
        {
            var c = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < c; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var sv = child as ScrollViewer;
                if (sv != null)
                    return sv;
                sv = GetFirstDescendantScrollViewer(child);
                if (sv != null)
                    return sv;
            }

            return null;
        }
        #endregion
    }
}
