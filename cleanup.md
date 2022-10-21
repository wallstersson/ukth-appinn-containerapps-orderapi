# Cleanup
Deleting the Azure resource group should remove everything associated with this hackaton.


<details>
  <summary>Azure CLI using bash</summary>

```bash
az group delete -g $resourceGroup --no-wait -y

```

  </summary>
</details>

<details>
  <summary>PowerShell</summary>

```PowerShell
Remove-AzResourceGroup -Name $resourceGroup -Force -AsJob
```

  </summary>
</details>
<br>