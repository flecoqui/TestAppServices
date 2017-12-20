# TestAppServices: Universal Windows Platform (UWP) AppService Sample Application (client/server)

Overview
--------------
This repository contains demonstrations of UWP AppService implementations to allow communication:<p/>
	-   **Step 1: UWP App to UWP App**: Communication between two UWP Application</p> 
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

For Step 1, the 2 UWP applications will use a AppService channel to exchange messages. Each application needs to book a communication channel with the AppService.
By default, the AppService is embedded in the AppServiceServerApp, the communication between the AppServiceServerApp and the AppService use a local channel. On the otherhand, the communication between the AppServiceClientApp and the AppService uses a remote channel.
Once the AppServiceServerApp and the AppServiceClientApp have initialized the communication with the AppService, the AppService will forward:
- the data messages received on the local channel to the remote channel,  
- the data messages received on the remote channel to the local channel.



![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceClientApp.png)


![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceClientApp_1.png)



![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceClientApp_2.png)


![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceClientApp_3.png)


![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceServerApp.png)


![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceServerApp_1.png)



![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceServerApp_2.png)


![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1AppServiceServerApp_3.png)


### Step 2: Communication between UWP Application and Win32 Application using AppServices

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step2.png)

The application is used to play videos, audios and photos. By default you can select in the combo box `Select a stream` the asset you can to play.   

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step2AppServiceServerApp.png)


![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step2AppServiceServerApp_1.png)



![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step2AppServiceServerApp_2.png)



	


Next steps
--------------

The Universal Media Player C# Sample Applicaton could be improved to support the following features:</p>
1.  Support of Project Rome to launch remotely TestMediaApp from one device to another device</p>
2.  Support of several pages : Player Page, Companion Page, Playlist Page and Settings Page.</p>
3.  Support of a new JSON model to support music playlist (Artist, Album), TV channels  </p>

