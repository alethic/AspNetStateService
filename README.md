# AspNetStateService
Reimplementation of the ASP.Net State Server Protocol: https://msdn.microsoft.com/en-us/library/cc224028.aspx

Provides a dynamic backend implementation for state storage. Provides a dynamic front end implementation to change how the service is hosted. Current implementation uses ASP.Net Core Kestrel, with some hacks.

Provides a Service Fabric application using the Actor-model to persist session state using volatile persistence.

