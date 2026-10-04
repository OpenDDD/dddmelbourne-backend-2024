using 'main.bicep'

param appName = 'dddmelb-2024-api'
param location = 'australiasoutheast'
param storageAccountName = 'dddmelb2024api'
param appInsightsName = 'dddmelb-2024202310202135'
param deployIdentityName = 'github-backend'

param allowedOrigins = [
  'https://portal.azure.com'
  'http://localhost:3000'
  'https://dddmelbourne.com'
  'https://www.dddmelbourne.com'
  'https://happy-river-0aceb3600-1.eastasia.3.azurestaticapps.net'
  'https://happy-river-0aceb3600-27.eastasia.3.azurestaticapps.net'
]

param timerFunctionNames = [
  'SessionizeAgendaSync'
  'SessionizeReadModelSync'
]

param enableTimers = true
param customDomain = ''
param customDomainCertificateIssued = false

param appSettings = json(readEnvironmentVariable('APP_SETTINGS_JSON', '{}'))
