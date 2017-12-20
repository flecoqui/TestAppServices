# TestAppServices: Universal Windows Platform (UWP) AppService Sample Application (client/server)

Overview
--------------
This Universal Windows Platform (UWP) Media Player Application can play video files, audio files and display pictures as well.
This application has been developped to test the playback of media assets on platforms running Windows 10.
Those media assets can be protected with DRM like PlayReady.
This sample application does support the following containers:<p/>
	-   **Video**: VMV, MP4, MPEG2-TS, MKV, HLS, MPEG-DASH, Smooth Streaming</p> 
	-   **Audio**: WMA, MP3, FLAC</p>
	-   **Picture**: JPG, PNG</p>


Installing the application
----------------------------
You can install the application on:<p/>
	- **Personal Computer Platform**: a desktop running Windows 10 Anniversary Update (RS1)</p>
	- **Windows 10 Mobile Platform**: a phone running Windows 10 Anniversary Update (RS1)</p>
	- **IOT Platform**: a IOT device running Windows 10 Anniversary Update (RS1)</p>
	- **XBOX One**: a XBOX One running Windows 10 Anniversary Update (RS1)</p>
	- **Hololens**: an Hololens running Windows 10 Anniversary Update (RS1)</p>

Using the application
----------------------------
Once the application is installed on your device, you can launch it and the main page will be displayed after few seconds.

### Step 1: Communication between 2 UWP Applications using AppServices

![](https://raw.githubusercontent.com/flecoqui/TestAppServices/master/Docs/Step1.png)


The application is used to play videos, audios and photos. By default you can select in the combo box `Select a stream` the asset you can to play.   


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



	
Building the application
----------------

**Prerequisite: Windows Smooth Streaming Client SDK**
This version is based on the latest UwpDesktop.10.0.14393.3 [Universal Smooth Streaming Client SDK](https://visualstudiogallery.msdn.microsoft.com/1e7d4700-7fa8-49b6-8a7b-8d8666685459)

1. If you download the samples ZIP, be sure to unzip the entire archive, not just the folder with the sample you want to build. 
2. Ensure the Creator Update (RS2) Windows 10 SDK is installed on your machine
3. Start Microsoft Visual Studio 2017 and select **File** \> **Open** \> **Project/Solution**.
3. Starting in the folder where you unzipped the samples, go to the Samples subfolder, then the subfolder for this specific sample, then the subfolder for your preferred language (C++, C#, or JavaScript). Double-click the Visual Studio 2015 Solution (.sln) file.
4. Press Ctrl+Shift+B, or select **Build** \> **Build Solution**.


**Deploying and running the sample**
1.  To debug the sample and then run it, press F5 or select **Debug** \> **Start Debugging**. To run the sample without debugging, press Ctrl+F5 or select **Debug** \> **Start Without Debugging**.



Next steps
--------------

The Universal Media Player C# Sample Applicaton could be improved to support the following features:</p>
1.  Support of Project Rome to launch remotely TestMediaApp from one device to another device</p>
2.  Support of several pages : Player Page, Companion Page, Playlist Page and Settings Page.</p>
3.  Support of a new JSON model to support music playlist (Artist, Album), TV channels  </p>

