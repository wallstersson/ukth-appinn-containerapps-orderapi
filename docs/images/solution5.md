# Challenge 5: Solution

## Solution steps
Until now we have worked with docker images that others have created, now we are going to do a code change on our own code base and push the changes to Container Apps using GitHub Actions.
We are now going to use a Azure CLI command to create a GitHub Action that builds the queuereader C# project and pushes the image to Azure Container Registry and deploys it to our Container App.

First, we need to create a service principal that can be used from the GitHub action to deploy the changes we are introducing.

> **Note**<br> 
> If you are not able to create a service principal in the Azure Active Directory that is connected to your subscription, you will unfortunately not be able to do this part of the lab



### Create an Azure AD service principal
<details>
  <summary>Azure CLI using bash</summary>

```bash

az ad sp create-for-rbac \
  --name <SERVICE_PRINCIPAL_NAME> \
  --role "contributor" \
  --scopes /subscriptions/<SUBSCRIPTION_ID>/resourceGroups/<RESOURCE_GROUP_NAME> \
  --sdk-auth

```
The return value from this command is a JSON payload, which includes the service principal's `tenantId`, `clientId`, and `clientSecret`.
Set the variables in bash.

```bash
spClientid=[Replace with the clientId of the service principal]
spClientSecret=[Replace with the clientSecret of the service principal]
tenantid=[Replace with the tenantId of the service principal]
```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell
$sp = New-AzADServicePrincipal -DisplayName <SERVICE_PRINCIPAL_NAME>

New-AzRoleAssignment -ApplicationId $sp.ApplicationId -RoleDefinitionName 'Contributor' -ResourceGroupName <RESOURCE_GROUP_NAME>

$spClientId = $sp.ApplicationId
$spClientSecret = $sp.PasswordCredentials.SecretText
$tenantId = (Get-AzContext | Select-Object -ExpandProperty Tenant).Id

```

  </summary>
</details>
<br>

### Create a Personal Access Token (PAT) in GitHub
Now we need a GitHub Personal Access Token (PAT) so we can authenticate against GitHub from Azure CLI.

Go to github.com --> Settings --> Developer Settings --> Personal access tokens Click on ”Generate new token” button.
 
Password prompt might appear. Enter password.

In the “Note” textbox enter a name for the PAT, such as “ca-pat”.
Give the PAT the following scopes: 
-	repo (Full control of private repositories) 
-	workflows (Update GitHub Action workflows)

![pat](images/pat.png)


Click “Generate token”, copy the generated token and assign the variable. 

<details>
  <summary>Bash</summary>

```bash
ghToken=[Replace with the PAT]
```
Set the "repoUrl" variable, replace <OWNER> with the GitHub account name. 
```bash
repoUrl=https://github.com/<OWNER>/ukth-appinn-containerapps-orderapi

```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell
$ghToken=[Replace with the PAT]
```
Set the "repoUrl" variable, replace <OWNER> with the GitHub account name. 

```PowerShell
$repoUrl="https://github.com/<OWNER>/ukth-appinn-containerapps-orderapi"

```

  </summary>
</details>
<br>

### Add a GitHub Actions workflow to your repository to deploy a container app
Now all the variables are set so we can run the Azure CLI command, make sure you are located at the root of the repo and run the following command.

Now we need to get information about the Azure Container Registry that we created in the beginning.

<details>
  <summary>bash</summary>


```bash
acrUrl=$(az acr show -n $acr -g $resourceGroup --query 'loginServer' -o tsv)
acrUsername=$(az acr show -n $acr -g $resourceGroup --query 'name' -o tsv)
acrSecret=$(az acr credential show -n $acr -g $resourceGroup --query passwords[0].value -o tsv)
```


  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell
$acrUrl = Get-AzContainerRegistry -Name $acr -ResourceGroupName $resourceGroup | Select-Object -ExpandProperty LoginServer
$acrCreds = Get-AzContainerRegistryCredential -Name $acr -ResourceGroupName $resourceGroup
$acrUsername=$acrCreds.Username
$acrSecret=$acrCreds.Password

```

  </summary>
</details>
<br>

Now all the variables are set so we can run the Azure CLI command, make sure you are located at the root of the repo and run the following command.

<details>
  <summary>Azure CLI using bash</summary>


```bash
az containerapp github-action add \
  --repo-url $repoUrl \
  --context-path "./queuereaderapp/Dockerfile" \
  --branch main \
  --name queuereader \
  --resource-group $resourceGroup \
  --registry-url $acrUrl \
  --registry-username $acrUsername \
  --registry-password $acrSecret \
  --service-principal-client-id $spClientid \
  --service-principal-client-secret $spClientSecret \
  --service-principal-tenant-id $tenantid \
  --token $ghToken

```


  </summary>
</details>

<details>
  <summary>Azure CLI using PowerShell</summary>

> **Note**<br>
> Whilst most Azure tasks can be done using native Azure PowerShell commands this is an exception. 
> Hence we will be using AZ CLI tool for this.
> If you havent logged in to Azure CLI you can run the following commands
> ```PowerShell
> # Login into Azure CLI
> az login --use-device-code
>
> # Check you are logged into the right Azure subscription. Inspect the name field
> az account show
>
> # In case not the right subscription
> az account set -s <subscription-id>
>
>```

```PowerShell

az containerapp github-action add \
  --repo-url $repoUrl \
  --context-path "./queuereaderapp/Dockerfile" \
  --branch main \
  --name queuereader \
  --resource-group $resourceGroup \
  --registry-url $acrUrl \
  --registry-username $acrUsername \
  --registry-password $acrSecret \
  --service-principal-client-id $spClientid \
  --service-principal-client-secret $spClientSecret \
  --service-principal-tenant-id $tenantid \
  --token $ghToken
```

  </summary>
</details>
<br>

The command will create a GitHub Action and run it, it takes a couple of minutes, please check the status at github.com --> Actions and see the progress of the GitHub Action after it has been created by the Azure CLI command.

Dive into the logs and locate the “latestRevisionName”, then go to the Azure portal and verify that the revision name is the same for the “queuereader” Container App.

![ghaction1](images/ghaction1.png)

![ghaction2](images/ghaction2.png)

![revision](images/revision.png)

### Deploy updated Bicep template

One additional time, we'll now deploy the new configuration with scaling configured. We will also add a simple dashboard for monitoring the messages flow.




### Review application logs

Let's check the number of orders in the queue

```bash
curl $dataURL
```

As before, we can check the application log files in Log Analytics to see what messages are being received

```text
ContainerAppConsoleLogs_CL
| where ContainerAppName_s has "queuereader" and ContainerName_s has "queuereader"
| where Log_s has "Message"
| project TimeGenerated, Log_s
| order by TimeGenerated desc
```
### Run script to bulk add orders and monitor scaling
Now let's see scaling in action. To do this, we will generate a large amount of messages which should cause the applications to scale up to cope with the demand.

While the scaling script is running, you can also have this operations dashboard open to visually see the messages flowing through queue into the store. Get the dashboard URL and open in a browser.

<details>
  <summary>Bash</summary>

```bash
dashboardURL=https://dashboardapp.$(az containerapp env show -g $resourceGroup -n $containerAppEnv --query 'properties.defaultDomain' -o tsv)

```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell

$dashboardRL="https://dashboardapp$((Get-AzContainerAppManagedEnv -ResourceGroupName $resourceGroup -EnvName $containerAppEnv).Id)/"

```

  </summary>
</details>
<br>

To demonstrate this, a script that uses the `tmux` command is provided in the `scripts` folder of this repository. Run the following commands:

```bash
cd scripts
./appwatch.sh $resourceGroup $dataURL
```

> **Note**<br>
> When running the dev container locally it is a risk that the appwatch script has wrong line-endings. Run the following command to fix the issue.

> ```bash
> sed -i -e 's/\r$//' appwatch.sh
> ```

This will split your terminal into four separate views.

* On the left, you will see the output from the `hey` command. It's going to send 10,000 requests to the application, so there will be a short delay, around 20 to 30 seconds, whilst the requests are sent. Once the `hey` command finishes, it should report its results.
* On the right at the top, you will see a list of the container app versions (revisions) that we've deployed. One of these will be the latest version that we just deployed. As `hey` sends more and more messages, you should notice that one of these revisions of the app starts to increase its replica count
* Also on the right, in the middle, you should see the current count of messages in the queue. This will increase to 10,000 and then slowly decrease as the app works it way through the queue.

Once `hey` has finished generating messages, the number of instances of the HTTP API application should start to scale up and eventually max out at 10 replicas. After the number of messages in the queue reduces to zero, you should see the number of replicas scale down and return to 1.

> **Tip**<br> 
> To exit from tmux when you're finished, type `CTRL-b`, then `:` and then the command `kill-session`

That concludes the deployment of the application. In upcoming challenges we will look at improving the DevOps and security capabilities of our solution. Next up is CI/CD deployment as part of [Challenge 5](challenge5.md)

## The challenges

- [Challenge 1: Setup the environment](challenge1.md)
- [Challenge 2: Deploy Container Apps Environment and troubleshoot Container Apps](challenge2.md)
- [Challenge 3: Split traffic for controlled rollout](challenge3.md)
- [Challenge 4: Scale Container Apps](challenge4.md)
- [Challenge 5: Configure CI/CD for Container Apps](challenge5.md)
