param location string = resourceGroup().location

param name string

param sku string = 'free'

resource swam 'Microsoft.Web/staticSites@2022-03-01' = {
  name: name
  location: location
  sku: {
    name: sku
    tier: sku
  }
  properties: {

  }
}
