# Azure-Storage-Binding
A WCF binding enables communication via Azure Storage

## Usage
### In C#
Server Side
```csharp
 serviceHost.AddServiceEndpoint(
      typeof(IServiceContract),
      new TableTransportBinding()
      {
          ConnectionString = "azure-storage-connectionstring",
          TargetPartitionKey = "all"
      },
      "endpoint-address");
```

Client Side
```csharp
var client =
      new ServiceClient(new TableTransportBinding() 
      { 
          ConnectionString = "azure-storage-connectionstring", 
          TargetPartitionKey = "client-id" 
      },
      "endpoint-address");
```

### In XML
Server Side
```xml
 <system.serviceModel>
    <extensions>
      <bindingExtensions>
        <add name="azureTableTransportBinding"
             type="AzureStorageBinding.Table.Binding.TableTransportBindingCollectionElement, AzureStorageBinding" />
      </bindingExtensions>
    </extensions>
    <bindings>
      <azureTableTransportBinding>
        <binding name="EchoService"
                 connectionString="UseDevelopmentStorage=true" />
      </azureTableTransportBinding>
    </bindings>
    <services>
      <service name="AzureStorageBinding.TestingService.EchoService">
        <endpoint address="az.table://EchoTesting" binding="azureTableTransportBinding"
                  bindingConfiguration="EchoService" contract="AzureStorageBinding.TestingContract.IEcho" />
      </service>
    </services>
  </system.serviceModel>
  ```
  
 Client Side
 ```xml
  <system.serviceModel>
    <extensions>
      <bindingExtensions>
        <add name="azureTableTransportBinding"
             type="AzureStorageBinding.Table.Binding.TableTransportBindingCollectionElement, AzureStorageBinding" />
      </bindingExtensions>
    </extensions>
    <bindings>
      <azureTableTransportBinding>
        <binding name="EchoClient"
                 connectionString="UseDevelopmentStorage=true"
                 targetPartitionKey="client0" />
      </azureTableTransportBinding>
    </bindings>
    <client>
      <endpoint address="az.table:EchoTesting" binding="azureTableTransportBinding" bindingConfiguration="EchoClient"
                contract="AzureStorageBinding.TestingContract.IEchoClient" name="EchoClient" />
    </client>
  </system.serviceModel>
 ```
