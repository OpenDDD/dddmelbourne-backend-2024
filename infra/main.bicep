targetScope = 'resourceGroup'

@description('Name of the Function App. The Flex Consumption plan uses the same name with an `-plan` suffix.')
param appName string

param location string = resourceGroup().location

@description('Storage account for the Functions host (AzureWebJobsStorage) and for deployment packages.')
@minLength(3)
@maxLength(24)
param storageAccountName string

@description('Existing Application Insights component in this resource group.')
param appInsightsName string

@description('Existing user-assigned managed identity that GitHub Actions uses to deploy the app.')
param deployIdentityName string

param allowedOrigins string[]

@description('Names of the timer functions. They stay disabled until `enableTimers` is true, so two apps do not run the same sync jobs.')
param timerFunctionNames string[]

param enableTimers bool = false

@description('Custom host name, for example api.dddmelbourne.com. Leave empty until the DNS records exist.')
param customDomain string = ''

param maximumInstanceCount int = 40

@allowed([512, 2048, 4096])
param instanceMemoryMB int = 2048

@description('App settings that are not managed here (secrets and feature settings). `deploy.sh` reads them from the live app.')
@secure()
param appSettings object

var deploymentContainerName = 'app-package'

var roleIds = {
  storageBlobDataOwner: 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
  websiteContributor: 'de139f84-1756-47ae-9be6-808fbbe84772'
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

resource deployIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: deployIdentityName
}

resource storage 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  tags: {
    app: appName
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    defaultToOAuthAuthentication: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2024-01-01' = {
  parent: storage
  name: 'default'
}

resource deploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2024-01-01' = {
  parent: blobService
  name: deploymentContainerName
}

resource plan 'Microsoft.Web/serverfarms@2024-11-01' = {
  name: '${appName}-plan'
  location: location
  kind: 'functionapp'
  sku: {
    tier: 'FlexConsumption'
    name: 'FC1'
  }
  properties: {
    reserved: true
  }
}

resource site 'Microsoft.Web/sites@2024-11-01' = {
  name: appName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storage.properties.primaryEndpoints.blob}${deploymentContainerName}'
          authentication: {
            type: 'SystemAssignedIdentity'
          }
        }
      }
      scaleAndConcurrency: {
        maximumInstanceCount: maximumInstanceCount
        instanceMemoryMB: instanceMemoryMB
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '10.0'
      }
    }
    siteConfig: {
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      ftpsState: 'Disabled'
      http20Enabled: true
      cors: {
        allowedOrigins: allowedOrigins
        supportCredentials: true
      }
    }
  }
}

resource appSettingsConfig 'Microsoft.Web/sites/config@2024-11-01' = {
  parent: site
  name: 'appsettings'
  properties: union(
    appSettings,
    {
      AzureWebJobsStorage__accountName: storage.name
      APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.properties.ConnectionString
    },
    toObject(timerFunctionNames, name => 'AzureWebJobs.${name}.Disabled', _ => string(!enableTimers))
  )
  dependsOn: [
    storageBlobDataOwner
  ]
}

resource ftpPublishing 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-11-01' = {
  parent: site
  name: 'ftp'
  properties: {
    allow: false
  }
}

resource scmPublishing 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-11-01' = {
  parent: site
  name: 'scm'
  properties: {
    allow: false
  }
}

// The Functions host needs Blob Data Owner for identity-based AzureWebJobsStorage. It also covers the deployment container.
resource storageBlobDataOwner 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storage
  name: guid(storage.id, site.id, roleIds.storageBlobDataOwner)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleIds.storageBlobDataOwner)
    principalId: site.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource deployIdentityWebsiteContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: site
  name: guid(site.id, deployIdentity.id, roleIds.websiteContributor)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleIds.websiteContributor)
    principalId: deployIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource hostNameBinding 'Microsoft.Web/sites/hostNameBindings@2024-11-01' = if (!empty(customDomain)) {
  parent: site
  name: empty(customDomain) ? 'unused' : customDomain
  properties: {
    siteName: site.name
    hostNameType: 'Verified'
    sslState: 'Disabled'
  }
}

resource managedCertificate 'Microsoft.Web/certificates@2024-11-01' = if (!empty(customDomain)) {
  name: '${appName}-${replace(customDomain, '.', '-')}'
  location: location
  properties: {
    serverFarmId: plan.id
    canonicalName: customDomain
  }
  dependsOn: [
    hostNameBinding
  ]
}

// ARM cannot declare the same binding twice in one template, so a module adds the certificate to the binding.
module hostNameSsl 'modules/hostNameSsl.bicep' = if (!empty(customDomain)) {
  params: {
    siteName: site.name
    hostName: customDomain
    thumbprint: managedCertificate.?properties.thumbprint ?? ''
  }
}

output appName string = site.name
output defaultHostName string = site.properties.defaultHostName
output customDomainVerificationId string = site.properties.customDomainVerificationId
output principalId string = site.identity.principalId
