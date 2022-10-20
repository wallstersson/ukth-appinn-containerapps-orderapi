# Challenge 2: Solution

## Solution steps
We'll deploy the first version of the application to Azure and use the _curl_ tool to test the application. We will then use _Log Analytics_ to troubleshoot the application.

### Deploy a Container Apps environment with related resources
We'll deploy an initial version of the application to Azure. Review the [V1 Bicep template](v1_template.bicep) that contains IaC definitions for Azure Container Apps Environment and other related services such as Log Analytics and a Storage account for the queue. Notice the individual container app resources 


Let's start by setting some variables that we will use for creating Azure resources in this lab.

Use the `name` and `resourceGroup`  variables you created in [challenge 1](challenge1.md). 

Make sure you navigate in your shell to the root folder of the lab files (example _c:\repos\mylab\ukth-appinn-containerapps-orderapi_)

<details>
  <summary>Bash</summary>

```bash
# Reuse the random name created in previous challenge
name = <your random name>

# Set variables for the rest of the lab
resourceGroup=${name}-rg
location=northeurope
containerAppEnv=${name}-env
logAnalytics=${name}-la
appInsights=${name}-ai
acr=${name}acr

# Deploy Bicep template.
az deployment group create \
  -g $resourceGroup \
  --template-file v1_template.bicep \
  --parameters @v1_parametersbicep.json \
  --parameters \
    ContainerApps_Environment_Name=$containerAppEnv \
    LogAnalytics_Workspace_Name=$logAnalytics \
    AppInsights_Name=$appInsights \
    Container_Registry_Name=$acr 

```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell
# Reuse the random name created in previous challenge
$name = <your random name>

# Set variables for the rest of the lab
$resourceGroup="$name-rg"
$location="northeurope"
$containerAppEnv="$name-env"
$logAnalytics="$name-la"
$appInsights="$name-ai"
$acr="$($name)acr"

New-AzResourceGroupDeployment -ResourceGroup $resourceGroup -Name 'v1_deployment' -TemplateFile .\v1_template.bicep -TemplateParameterFile v1_template.bicep -Location $location -ContainerApps_Environment_Name $containerAppEnv -LogAnalytics_Workspace_Name $logAnalytics -AppInsights_Name $appInsights -Container_Registry_Name $acr

```

  </summary>
</details>
<br>

The deployment typically takes around 3 to 5 minutes to complete.

Once deployment has completed, verify the resources created by navigating to [Azure Portal](https://portal.azure.com). The following resources should be created. Note that the resource name prefix will differ in your environment.
![](images/resources.png)

Select the Container Apps Environment and review the apps listed in the Apps blade
![](images/containerapps.png)


### Test APIs deployed as Container Apps
Now the application is deployed, let's verify that it works correctly. First determine the URL we'll use to access the store application and save that in a variable for convenience

<details>
  <summary>Bash</summary>

```bash
storeURL=https://storeapp.$(az containerapp env show -g $resourceGroup -n $containerAppEnv --query 'properties.defaultDomain' -o tsv)/store

```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell

$storeURL="https://storeapp.$((Get-AzContainerAppManagedEnv -ResourceGroupName $resourceGroup -EnvName $containerAppEnv).Id)/store"

```

  </summary>
</details>
<br>

Let's see what happens if we call the URL
<details>
  <summary>Bash</summary>
  
```bash
curl $storeURL

```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell

Invoke-RestMethod -Url $storeUrl

```

  </summary>
</details>
<br>


The response you should see is `[]` which means no data was returned. Either there has been no orders submitted or something's not working correctly.

Try adding a new order to the order data API and verify that it is stored correctly. First grab the order data API URL using a similar approach as in previous step.

<details>
  <summary>Bash</summary>
  
```bash
dataURL=https://httpapi.$(az containerapp env show -g $resourceGroup -n $containerAppEnv --query 'properties.defaultDomain' -o tsv)/data

```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell

$dataURL="https://storeapp.$((Get-AzContainerAppManagedEnv -ResourceGroupName $resourceGroup -EnvName $containerAppEnv).Id)/data"

```

  </summary>
</details>
<br>

Add a new order test item using a HTTP Post

<details>
  <summary>Bash</summary>
  
```bash
curl -X POST $dataURL?message=item1
```
Verify that the store API returns the order

```bash
curl $storeURL
```

Still no orders are returned. 

Finally, check the queue length using the data API
```bash
curl $dataURL
```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell

Invoke-RestMethod -Url "$dataURL?message=item1" -Method Post

```
Verify that the store API returns the order

```PowerShell

Invoke-RestMethod $storeURL

```

Still no orders are returned. 

Finally, check the queue length using the data API
```PowerShell

Invoke-RestMethod $dataURL

```

  </summary>
</details>
<br>


You should see the following output indicating that the queue is not read correctly.
> `Queue 'demoqueue' has 1 message`

Let's do some troubleshooting.

### Troubleshoot and redeploy application
ContainerApps integrates with Application Insights and Log Analytics. In the Azure Portal, go to the Log Analytics workspace in the resource group we're using for this demo and run the following query to view the logs for the `queuereader` application.

```text
ContainerAppConsoleLogs_CL
| where ContainerAppName_s has "queuereader" and ContainerName_s has "queuereader"
| top 100 by TimeGenerated
```
> If you don't see any results from the query you need to wait a couple of minutes for logs to be populated.


You should see a some log entries that will likely contain the same information about the error. Drill down on one of them to reveal more. You should see something like the following:
![](images/loganalytics-queue-error.png)
> "Log_s": "      Queue 'foo' does not exist. Waiting..",

Looks like we have configured the wrong name for the queue. 

Go through the [V1 Bicep template](v1_template.bicep) and find where the wrong queue configuration is located.
You will find it in the _queuereader_ container app configuration section.

```bicep
        name: 'queuereader'
          env: [
            {
              name: 'QueueName'
              value: 'foo'
            }
            {
              name: 'QueueConnectionString'
              secretRef: 'queueconnection'
            }
            {
              name: 'TargetApp'
              value: 'storeapp'
            }
```

The needed changes to the Bicep code is already made for you in [V2 Bicep template](v2_template.bicep).

Go ahead and deploy that version of the solution by repeating the same command from earlier but with the version 2 of the configuration


<details>
  <summary>Bash</summary>

```bash

# Deploy Bicep template.
az deployment group create \
  -g $resourceGroup \
  --template-file v2_template.bicep \
  --parameters @v2_parametersbicep.json \
  --parameters \
    ContainerApps_Environment_Name=$containerAppEnv \
    LogAnalytics_Workspace_Name=$logAnalytics \
    AppInsights_Name=$appInsights \
    Container_Registry_Name=$acr 

```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell

New-AzResourceGroupDeployment -ResourceGroup $resourceGroup -Name 'v2_deployment' -TemplateFile .\v2_template.bicep -TemplateParameterFile v2_template.bicep -Location $location -ContainerApps_Environment_Name $containerAppEnv -LogAnalytics_Workspace_Name $logAnalytics -AppInsights_Name $appInsights -Container_Registry_Name $acr

```

  </summary>
</details>
<br>

Let's see what happens when we access the queue application using the data URL

> As before, you can type `echo $dataURL` to get the URL of the HTTP API and then open it in a browser if you prefer

<details>
  <summary>Bash</summary>
  
```bash
curl $dataURL

```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell

Invoke-RestMethod -Url $dataUrl

```

  </summary>
</details>
<br>

The result tells us that `demoqueue` has no messages:

> `Queue 'demoqueue' has 0 messages`

This indicates that the messages are now processed. Now add another test message.

<details>
  <summary>Bash</summary>
  
```bash
curl -X POST $dataURL?message=item2
```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell

Invoke-RestMethod -Url "$dataURL?message=item2" -Method Post

```


  </summary>
</details>
<br>



Ok, let's check our Store URL and see what happens this time

<details>
  <summary>Bash</summary>
  
```bash
curl $storeURL

```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell

Invoke-RestMethod -Url $storeUrl

```

  </summary>
</details>
<br>

> `[{"id":"a85b038a-a01f-4f25-b468-238d0c8a3676","message":"24a1f5ed-2407-4f9d-a6f9-5664436f1c28"},{"id":"f2b4c93a-63e5-4a4d-8a66-1fa4d4b958fe","message":"5940cf24-8c55-4b38-938a-10d9351d5d2b"}]`

Ok, that's some progress but not the messages we sent in the query string. 

Let's take a look at the application code


[MessageQueueClient.cs](httpapiapp\MessageQueueClient.cs)

```c#
...
  public async Task<bool> SendMessage(string message) => await SendMessageToQueue(Guid.NewGuid().ToString());
...
```

It looks like the code is set to send a GUID, not the message itself. Must have been something the developer left in to test things out. The correct code should look like this:

[MessageQueueClient.cs](httpapiapp\MessageQueueClient.cs) (version 2)

```c#
    public async Task<bool> SendMessageV2(string message) => await SendMessageToQueue($"{Guid.NewGuid()}--{message}");
 
```
We've now fixed the code for you so that the message received is now actually being sent and we've packaged this up into a new container ready to be redeployed. 

But maybe we should be cautious and make sure this new change is working as expected and therefore perform a controlled rollout of the new version so only a subset of the incoming requests hit the new version.

That will be done as part of [Challenge 3](challenge3.md)

## The challenges

- [Challenge 1: Setup the environment](challenge1.md)
- [Challenge 2: Deploy Container Apps Environment and troubleshoot Container Apps](challenge2.md)
- [Challenge 3: Split traffic for controlled rollout](challenge3.md)
- [Challenge 4: Scale Container Apps](challenge4.md)
- [Challenge 5: Configure CI/CD for Container Apps](challenge5.md)
- [Challenge 6: Protect Container App with API Management](challenge6.md)
- [Challenge 7: Enable Container App authentication](challenge7.md)
