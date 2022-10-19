# Challenge 4: Scale Container Apps
Azure Container Apps manages automatic horizontal scaling through a set of declarative scaling rules. As a container app scales out, new instances of the container app are created on-demand. These instances are known as replicas.

In our scenario our app is about to go live so we will change things so that the new app is receiving all of the traffic, plus we'll also setup some scaling rules. This will allow the container apps to scale up when things are busy, and scale to zero when things are quiet. We will also deploy and additional Dashboard App to monitor the orders currently in queue and store.

The following image illustrates the steps in this challenge

![](images/challenge-4-overview.png)


## Main objectives
- Add support for scaling Container App based on load.
- Deploy new version of Container App
- Run load testing tool to examine scaling behavior


## Activities

- Examine scaling rules for HTTP API and QueueReader App by revieing existing [Bicep template v4](..\v4_template.bicep)
- Deploy the updated Bicep template
- Review QueueReader application logs in Log Analytics.
- Run script to bulk add orders via HTTP API and watch revisions
- View order count in store and queue using Dashboard App



## Definition of done
- Reviewed and understood scaling rules for HTTP API and QueueReader App by examine existing [Bicep template v4](..\v4_template.bicep)
- Deployed [Bicep template v4](..\v4_template.bicep) including scaling rules
- Run [script](..\scripts\appwatch.sh) to bulk add 10000 orders via HTTP API and watch replica scaling using `tmux`
- Verified logs in Log Analytics for QueueReader application that all orders are correctly using newest revision.
- Added 10000 orders and observed replica scaling using [script](..\scripts\appwatch.sh).
- Examined order count in queue and store using Dashboard App (https://dashboardapp.[your container app environment domain])
 

## Helpful links
- [Set scaling rules in Azure Container Apps (learn.microsoft.com)](https://learn.microsoft.com/en-us/azure/container-apps/scale-app)
- [tmux (github.com)](https://github.com/tmux/tmux)
- [Monitor logs in Azure Container Apps with Log Analytics (learn.microsoft.com)](https://learn.microsoft.com/en-us/azure/container-apps/log-monitoring?tabs=bash)

## Solution
- View the solution here: [Challenge 4 - Solution](solution-4.md)

## The challenges
- [Challenge 1: Setup the environment](challenge1.md)
- [Challenge 2: Deploy and troubleshoot a Container Apps environment](challenge2.md)
- [Challenge 3: Perform blue-green deployment with Container App traffic splitting](challenge3.md)
- [Challenge 4: Scale Container Apps](challenge4.md)

