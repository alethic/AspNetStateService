set
set AspNetStateService.Samples.Web.Host:Protocol=http
set AspNetStateService.Samples.Web.Host:BindingInformation=*:%Fabric_Endpoint_ServiceEndpoint%:
%~dp0\AspNetStateService.Samples.Web.Host.exe
IF %ERRORLEVEL% NEQ 0 (
  EXIT /B 1
)
