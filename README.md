# TestAppServices: AppService sample for communication between UWP and Win32 Applications (client/server)

Overview
--------------
This repository contains demonstrations of UWP AppService implementations to allow communication:<p/>
	-   **Step 1: UWP App to UWP App**: Communication between two UWP Applications</p> 
	-   **Step 2: UWP App to Win32 App**: Communication between one UWP Application and one Win32 Application using Desktop Bridge</p> 


Installing the application
----------------------------
You can install the application on:<p/>
	- **Personal Computer Platform**: a desktop running Windows 10 Fall Creator Update (RS3) - Step 1 and Step 2</p>
	- **Windows 10 Mobile Platform**: a phone running Windows 10 Fall Creator Update (RS3) - Step 1 only</p>
	- **XBOX One**: a XBOX One running Windows 10 Fall Creator Update (RS3) - Step 1 only</p>


Building the application
----------------

**Prerequisite for Step 2: UwpDesktop package**
This version is based on the latest UwpDesktop nuget package (version 10.0.14393.3)  [UwpDesktop](https://www.nuget.org/packages/UwpDesktop/)

1. If you download the samples ZIP, be sure to unzip the entire archive, not just the folder with the sample you want to build. 
2. Ensure the Fall Creator Update (RS3) Windows 10 SDK is installed on your machine
3. Start Microsoft Visual Studio 2017 and select **File** \> **Open** \> **Project/Solution**.
3. Starting in the folder where you unzipped the samples, go to the Step1 or Step2 subfolder, then the subfolder TestAppServices, open the file TestAppServices.sln, the Visual Studio 2017 Solution (.sln) file.
4. Press Ctrl+Shift+B, or select **Build** \> **Build Solution**.


**Deploying and running the sample**
1.  To debug the Client Application select project AppServiceClientApp and then run it, press F5 or select **Debug** \> **Start Debugging**. To run the sample without debugging, press Ctrl+F5 or select **Debug** \> **Start Without Debugging**.
2.  To debug the Server Application select project AppServiceServerApp and then run it, press F5 or select **Debug** \> **Start Debugging**. To run the sample without debugging, press Ctrl+F5 or select **Debug** \> **Start Without Debugging**.


Using the application
----------------------------
Once the applications for each step are installed on your device, you can launch it and the main page will be displayed after few seconds.

### Step 1: Communication between 2 UWP Applications using AppServices

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1.png)

For Step 1, the 2 UWP applications will use an AppService channel to exchange messages. Each application needs to book a communication channel with the AppService.
By default, the AppService is embedded in the AppServiceServerApp, the communication between the AppServiceServerApp and the AppService use a local channel. On the otherhand, the communication between the AppServiceClientApp and the AppService uses a remote channel.
Once the AppServiceServerApp and the AppServiceClientApp have initialized the communication with the AppService, the AppService will forward:
- the data messages received on the local channel to the remote channel,  
- the data messages received on the remote channel to the local channel.

1. Launch the AppServiceClientApp Application, if the Server Application is not installed on the same machine, click on the button **Install AppService Server App** to install the server application from Windows Store.
Once the Server Application is installed, you can launch the Server Application using LaunchUriAsync using the Uri associated with the Server Application **appserviceserverapp:\\\\**.
Clicking on button **Connect to AppService Server App** you establish a communication channel with the AppService running with the Application Server.

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceClientApp.png)

2. On the server side, it's almost the same initialization. Click on button **Connect to AppService** to establish a communication channel with the AppService.

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceServerApp.png)

3. Once the communication channel is established with the AppService, you can send an Init command to the AppService, in this sample application, this command is simple, you can use this initialization process to authentified the client and server application.

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceClientApp_1.png)

4. On the server side, click on button **Send Init Command to AppService** to send an Init command to the AppService.

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceServerApp_1.png)


5. Once the initialization phase is done, you can send the message to the Server App through the AppService. If on Server side, the Server Application is not synchronized with the AppService, this step will fail.

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceClientApp_2.png)

6. You can check if the message has been received by the Server App reading the logs.

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceClientApp_3.png)


7. On the server side, once the communication with the AppService is initialized, you can send the message to the Client App through the AppService. If on Client side, the Client Application is not synchronized with the AppService, this step will fail.

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceServerApp_2.png)

8. On the server side, You can check if the message has been received by the Client App reading the logs.

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceServerApp_3.png)


### Step 2: Communication between UWP Application and Win32 Application using AppServices

For Step 2, one UWP application will use an AppService channel to exchange messages with a Win32 Application. Like for Step 1, each application needs to book a communication channel with the AppService.
By default, the AppService is embedded in the AppServiceServerApp which is a UWP Application with a Win32 component implementing the AppSerivce Client on the Server side. The communication between the Win32 component and the AppService use a local channel. On the otherhand, the communication between the AppServiceClientApp and the AppService uses a remote channel.
Once the AppServiceServerApp and the AppServiceClientApp have initialized the communication with the AppService, the AppService will forward:
- the data messages received on the local channel to the remote channel,  
- the data messages received on the remote channel to the local channel.

1. On the client side, the use case is similar to the Step1 use case. The Win32 component can be used to communicate with devices connected to your PC through USB cable for instance.

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step2.png)

2. Launch the AppServiceClientApp Application, if the Server Application is not installed on the same machine, click on the button **Install AppService Server App** to install the server application from Windows Store.
Once the Server Application is installed, you can launch the Server Application using LaunchUriAsync using the Uri associated with the Server Application **appserviceserverapp:\\**.
Clicking on button **Connect to AppService Server App** you establish a communication channel with the AppService running with the Application Server.
Then you can send an Init Command to confirm the connection wiht the AppService. You are now ready to send data to the Server Application.

3. On the server side, click on button **Launch Win32Proc** to launch the Win32 component (process). This process will automatically establish a connection with the AppService.

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step2AppServiceServerApp.png)

4. This process will automatically establish a connection with the AppService. From the client application you can send message to the Win32 Server Application, for each message you should receive a message from the Win32 process. By default the Win32 process will respond to each message by sending another message to the Client Application.

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step2AppServiceServerApp_1.png)

5. On the server side, the application is configured to store the logs in a file under: C:\temp\win32proc.txt

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step2AppServiceServerApp_2.png)

6. Reading the win32proc.txt file, you can check whether your Win32 process did receive the messages from the Client Application.

	


Next steps
--------------

This communication model between UWP and Win32 applications can be extended using the RemoteSystem API (aka project Rome) to enable communication between applications not installed on the same machine.

