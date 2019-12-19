# AzureVMToImage
Create an Azure Image from a VM without destroying a VM. 
This is accomplished by Cloning the target and generating the image from the clone.
Used to Re-Image Scalesets from a baseline Image.
Intended for use stand-alone or as Azure RunBooks.

The goal of this project is to make using Scalesets with COTS packages manageable.

## Getting Started

### Local Operation
 * The local folder conatins scripts to be run locally and for debugging purposes. 
 * Invoke <b>local_Create Image from VM.ps1</b> to get started. 
 * Values to be configured are in all caps, ie 'YOUR SUBSCRIPTION ID' and are found in **local_*.ps1**'  or **RB_*.ps1** files.

### Runbooks
 * Runbooks can be published directly to the portal using the **Publish Runbook(s).ps**1 scripts.
 * Files prefixed with **RB-** are intended as the Runbook to invoke.
 * Authentication can be tricky. Use **RB-AzureRMAuthenticate.ps1** to validate your Automation account configuration.
 * **RB-CloneVMandCreateImage.ps1** will create an image from an existing VM without destroying it. 
 * **RB-CloneVMandCreateImage.ps1** has the list of required Modules. Make sure these modules are imported into your AutomationAccount. Runbooks tend to fail without providing clues when the failure isdue to configuration.

## Important Notes
 * Encrypted config files such as ConnectionStrings in web.config or app.config, must be decrypted prior to imaging. This is because DAPI uses encryption keys are wiped out during imaging.
 
## Scale Set Process Diagram
A significant limitation of Azure Scalesets when using COTS packages is the inability to base the Scaleset on a running VM. The recommendation is to create a reference image from a configured VM. Once the image is made, the image is static and the VM is destroyed. Patches require a new VM to be created and configured. If a scaleset is spun from that image one year later, that machine is One year out of date in maintaining updates and security posture. This process is an attempt to mitigate this limitation.

<img src="https://github.com/jlongo62/AzureVMToImage/blob/master/reference%20vm%20to%20vmss.jpg" />

### Prerequisites

 * AzureRM Powershell Modules

 ### Built With

* Visual Studio
* Azure AZ Powershell Modules
* Powershell Tools for Visual Studio Extension

### Authors

* **Joseph Longo** - *Initial work* 

ReadMe based on template:
https://gist.github.com/PurpleBooth/109311bb0361f32d87a2#file-readme-template-md

### License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details. No warrantly is expressed or implied. Use at your own risk.

