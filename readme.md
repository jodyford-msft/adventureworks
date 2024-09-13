```bash

# Set variables

SUFFIX=$(( RANDOM ))$(date +%s)
MI_NAME="mi$SUFFIX"
LOCATION="eastus2"
APP_NAME="app$SUFFIX"
RESOURCE_GROUP_NAME="msdocs-dab$SUFFIX"
SQL_SERVER_NAME="srvr$SUFFIX"
PLAN_NAME="plan$SUFFIX"
CURRENT_USER_PRINCIPAL_ID=$(az ad signed-in-user show --query "id" --output "tsv")

# Echo variables
echo "SUFFIX: $SUFFIX"
echo "LOCATION: $LOCATION"
echo "MI_NAME: $MI_NAME"
echo "APP_NAME: $APP_NAME"
echo "RESOURCE_GROUP_NAME: $RESOURCE_GROUP_NAME"
echo "SQL_SERVER_NAME: $SQL_SERVER_NAME"
echo "PLAN_NAME: $PLAN_NAME"
echo "CURRENT_USER_PRINCIPAL_ID: $CURRENT_USER_PRINCIPAL_ID"

# Create Resource Group
az group create --name $RESOURCE_GROUP_NAME --location $LOCATION

# Create SQL Server
az sql server create \
  --resource-group $RESOURCE_GROUP_NAME \
  --name $SQL_SERVER_NAME \
  --location $LOCATION \
  --enable-ad-only-auth \
  --external-admin-principal-type "User" \
  --external-admin-name $CURRENT_USER_PRINCIPAL_ID \
  --external-admin-sid $CURRENT_USER_PRINCIPAL_ID

# Create SQL Server Firewall Rule
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP_NAME \
  --server $SQL_SERVER_NAME \
  --name "AllowAzure" \
  --start-ip-address "0.0.0.0" \
  --end-ip-address "0.0.0.0"

# Create SQL Database with AdventureWorksLT sample data
az sql db create \
  --resource-group $RESOURCE_GROUP_NAME \
  --server $SQL_SERVER_NAME \
  --name "adventureworks" \
  --sample-name "AdventureWorksLT"

SQL_SERVER_ENDPOINT=$(az sql server show --resource-group $RESOURCE_GROUP_NAME --name $SQL_SERVER_NAME --query "fullyQualifiedDomainName" --output tsv)

SQL_CONNECTION_STRING=$(echo "Server=$SQL_SERVER_ENDPOINT;Database=adventureworks;Encrypt=true;Authentication=Active Directory Default;" | sed 's/"//g')

# Print the connection string without quotes
echo $SQL_CONNECTION_STRING

# Create App Service Plan
az appservice plan create \
  --name $PLAN_NAME \
  --resource-group $RESOURCE_GROUP_NAME \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP_NAME \
  --plan $PLAN_NAME 

# Set Web App settings
az webapp config appsettings set --resource-group $RESOURCE_GROUP_NAME --name $APP_NAME --settings SQL_CONNECTION_STRING=$SQL_CONNECTION_STRING

# Create Managed Identity
az identity create --name $MI_NAME --resource-group $RESOURCE_GROUP_NAME

# Show Managed Identity details
az identity show --name $MI_NAME --resource-group $RESOURCE_GROUP_NAME

# Assign Managed Identity to Web App
az webapp identity assign --name $APP_NAME --resource-group $RESOURCE_GROUP_NAME --identities $MI_NAME

```

