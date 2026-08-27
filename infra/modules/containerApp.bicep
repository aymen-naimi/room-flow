@description('Azure region')
param location string

@description('Container App name')
param name string

@description('Container Apps environment resource ID')
param environmentId string

@description('ACR login server, e.g. myregistry.azurecr.io')
param acrLoginServer string

@description('User-assigned identity resource ID with AcrPull')
param identityId string

@description('Full image reference including tag')
param apiImage string

@description('JWT signing key (>= 32 bytes)')
@secure()
param jwtSigningKey string

@description('SQL connection string')
@secure()
param sqlConnectionString string

@description('Allowed CORS origin (Static Web App HTTPS origin)')
param corsOrigin string

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: name
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: environmentId
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      registries: [
        {
          server: acrLoginServer
          identity: identityId
        }
      ]
      secrets: [
        {
          name: 'jwt-signing-key'
          value: jwtSigningKey
        }
        {
          name: 'sql-connection'
          value: sqlConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'ASPNETCORE_HTTPS_REDIRECT'
              value: 'false'
            }
            {
              name: 'HttpsRedirection'
              value: 'false'
            }
            {
              name: 'APPLY_MIGRATIONS'
              value: 'true'
            }
            {
              name: 'ENABLE_SWAGGER'
              value: 'false'
            }
            {
              name: 'Jwt__Issuer'
              value: 'RoomFlow'
            }
            {
              name: 'Jwt__Audience'
              value: 'RoomFlow'
            }
            {
              name: 'Jwt__SigningKey'
              secretRef: 'jwt-signing-key'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'sql-connection'
            }
            {
              name: 'Cors__AllowedOrigins__0'
              value: corsOrigin
            }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 20
              periodSeconds: 30
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 10
              periodSeconds: 10
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
output name string = containerApp.name
