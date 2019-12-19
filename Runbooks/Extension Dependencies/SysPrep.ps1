####
#### Used to SysPrep a VM prior to creating an image
#### https://blogs.technet.microsoft.com/neales/2017/03/13/use-a-custom-script-extension-to-sysprep-an-azure-vm/
####


#!!!!! /shutdown 
#Allowing sysprep to shut down terminiates the VM.
#Wait for Extension to process the Deallocate VM from CreateImageFromVM.ps1  (Runbook)
Start-Process -FilePath C:\Windows\System32\Sysprep\Sysprep.exe -ArgumentList '/generalize /oobe /quiet /quit'  -Wait 
