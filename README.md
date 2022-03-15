
# A Minecraft Clone in OpenTK and C#

![Preview](./screenshot4.png)

# Setup
* Make sure to install OpenAL 
	* Windows google the openAL installer and run the installer
	* Linux install via package manager
		* TODO: add the DLL here for windows 

# How to Run

* Note: MonoDevelop is no longer working for this. Use VSCode or Mono on Linux

1. Visual Studio
	
	* Open Solution (.sln), F5.

2. VSCode + OmniSharp
	
	* This is for OmniSharp. There is also an extension that lets you debug with Mono on a server. 

	* Install Mono
	  sudo apt install mono-complete

	* Install steps for .NET runtime
	https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#2110-

	* I had to enable Omnisharp:UseGlobalMono = True to get this to work.

	* There is also a solution explorer extension for VSCode that makes this easier.


3. Manual Debug with Mono

	* Install mono-complete.

	* Install .net runtime https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#2110-

	* After installing you should be able to compile just by typing msbuild in the project (.sln) directory.

	* You can debug the .exe with mono using mono --debug myexe.exe



