# AspNetStateService
Reimplementation of the ASP.Net State Server Protocol: https://msdn.microsoft.com/en-us/library/cc224028.aspx

Provides a dynamic backend implementation for state storage. Provides a dynamic front end implementation to change how the service is hosted. Current implementation uses ASP.Net Core Kestrel, with some hacks.

## Service Fabric
Provides a Service Fabric application using the Actor-model to persist session state using volatile persistence. This provides a `http://localhost:42424` endpoint on every machine within your Service Fabric cluster to which you can direct ASP.NET Session State. Be sure to configure your ActorService partition count appropriately.

## Entity Framework
Also provides an EF Core backend. Supports storing session state in SQL Server.

## Redis
Also provides a Redis backend. Supports storing session state in Redis.

## Azure Blob Storage
Also provides an Azure Blob Storage backend. Supports storing session state in Azure Blob Storage. Cleaning up old blobs is your responsibility.

## Azure Data Table
Also provides an Azure Data Table backend. Supports storing session state in Azure Data Tables. Cleaning up old records is your responsibility.
