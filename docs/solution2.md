# Challenge 2: Solution

## Solution steps
We'll deploy the first version of the application to Azure and use the _curl_ tool to test the application. We will then use _Log Analytics_ to troubleshoot the application.

### Deploy a Container Apps environment with related resources
We'll deploy an initial version of the application to Azure. Review the [V1 Bicep template](..\v1_template.bicep) that contains IaC definitions for Azure Container Apps Environment and other related services such as Log Analytics and a Storage account for the queue. Notice the individual container app resources 


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

Let's see what happens if we call the URL of the store with curl.

```shell
curl $storeURL
```

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

```shell
curl -X POST $dataURL?message=item1
```
Verify that the store API returns the order

```shell
curl $storeURL
```

Still no orders are returned. 

Finally, check the queue length using the data API
```shell
curl $dataURL
```
You should see the following output indicating that the queue is not read correctly.
> `Queue 'demoqueue' has 1 message`

Let's do some troubleshooting.

### Troubleshoot application
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

Looks like we have configured the wrong name for the queue. That will be fixed in [Challenge 3](challenge3.md)

## The challenges

[Challenge 1: Setup the environment](challenge1.md)
[Challenge 2: Deploy and troubleshoot a Container Apps environment](challenge2.md)
[Challenge 3: Deploy Container App with traffic split](challenge3.md)
