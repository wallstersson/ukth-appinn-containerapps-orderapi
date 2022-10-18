# Challenge 1: Solution

## Solution steps
To setup the environment you will fork the respository and then use Azure CLI or Azure PowerShell to create a resource group. You will also install some tools used in later challenges.

### Fork the repository
1. Log in to [GitHub](https://github.com) with your GitHub account
2. Fork this repo by selecting the *Fork* menu in the GitHub top right corner
![](images/fork.png)

> **Note**<br>
> If your GitHub account is part of an organization there is a limitation that only one fork is possible in the same organization. The workaround is to clone this repo, create a new repository and then push the code from the cloned working copy similar to this:
>
>  ``` bash
>  git clone https://github.com/pewill-msft/ukth-appinn-containerapps-orderapi
>  cd ukth-appinn-containerapps-orderapi
>  git remote set-url origin <url of your new repo>
>  git push origin master
>  
>  ```

### Clone the respository
Depending on development option the steps are different
<details>
  <summary>Using Codespaces</summary>

Open **your** repository in GitHub Codespaces by selecting menu _Code->Create codespace on main_

![](images/codespaces.png)

This command will take 5-10 minutes to set up the development container and clone the source code.

![](images/codespaces-progress.png)

Once the Codespaces finished deployment you will have a browser based VSCode instance available with a cloned repository. Take a few minutes to familarize yourself with the source code and starter files. 

![](images/codespaces-done.png)

> **Note**<br>
> By default Codespaces time out after 30 minutes of inactivity. After that time any shell variables you defined will be gone and needs to be added again. All changes on files will be persisted between restarts though. An option can be to add variables and commands to a script file to be used between timeout restarts.

</details>


<details>
  <summary>Using git CLI from bash or PowerShell</summary>
Open the shell and run the following git CLI commands

```shell
git clone <your GitHub repository url>/ukth-appinn-containerapps-orderapi
cd ukth-appinn-containerapps-orderapi
```
</details>
<br>

### Install hey load testing tool
We will be using the `hey` load testing tool later on.

<details>
  <summary>Using Homebrew (included in Codespaces)</summary>
  If you are using Codespaces, the container includes Homebrew, so you can install `hey` like this:

```bash
brew install hey
```
</details>
<details>
  <summary>Manual installation</summary>

  If you are using an environment other than Codespaces, you can find installation instructions for `hey` here - [https://github.com/rakyll/hey](https://github.com/rakyll/hey)

</details>
<br>

## Install Azure CLI extension for Container Apps
You will need to install an Azure CLI extension to work with Container Apps.


Run the following command in a shell:

```bash
az extension add --name containerapp
```

## Log in to Azure 
<details>
  <summary>Azure CLI</summary>

```bash
# Login into Azure CLI
az login --use-device-code

# Check you are logged into the right Azure subscription. Inspect the name field
az account show

# In case not the right subscription
az account set -s <subscription-id>

```

  </details>

  <br>

<details>
  <summary>Azure PowerShell</summary>

```PowerShell
# Login into Azure PowerShell
Connect-AzAccount -UseDeviceAuthentication

# Check you are logged into the right Azure subscription. Inspect the SubscriptionName field
Get-AzContext

# In case not the right subscription
Select-AzSubscription -SubscriptionId <subscription-id>

```
  </details>

  <br>

## Create a resource group
Let's start by setting a unique name variable that we will use for creating Azure resources in this demo, and a resource group for those resources to reside in. Select a location for the resource group.

<details>
  <summary>Azure CLI</summary>

```shell
# Generate a random name
name=ca$(cat /dev/urandom | tr -dc '[:lower:]' | fold -w ${1:-5} | head -n 1)

# Set variable for resource group
resourceGroup=${name}-rg

# Set a variable for location
location=northeurope

# Create Resource Group
az group create --name $resourceGroup --location $location -o table

```
</details>
<br>

<details>
  <summary>Azure PowerShell</summary>

```PowerShell
# Generate a random name
$name = -join ((97..122) | Get-Random -Count 6 | % {[char]$_})

# Set variable for resource group
$resourceGroup = "$name-rg"

# Set a variable for location
$location="northeurope"

# Create Resource Group
New-AzResourceGroup -Name $resourceGroup -Location $location

```
</details>
<br>

## The challenges
[Challenge 1: Setup the environment](challenge1.md)
[Challenge 2: Deploy and troubleshoot a Container Apps environment](challenge2.md)
[Challenge 3: Deploy Container App with traffic split](challenge3.md)
