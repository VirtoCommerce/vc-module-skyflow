# General Description

module purpose: PCI Complience, avoid storing sensitive data on the system

## Customization

## Customization in Skyflow

### role creation
you need to create 2 system roles
one role to work on the front end (send card data to skyflow), the Vault editor role is used
The second role is created together with the connection. i.e. when the connection is created, the role is added automatically.

### creating connectors
modifying a connector for an example (move it to the default implementation parsing)

Sometimes it is not possible to create a default connection, for example, the format of the card number does not fit. in such cases you can modify the connection via RestAPI.
`Update Outbound Connection`
```
{
    "ID": "...",
	...
    "routes": [
        {
            "path": "/xml/v1/request.api",
            "method": "POST",
            "contentType": "XML",
            "url": [],
            "requestBody": [
                {
                    "action": "DETOKENIZATION",
                    "fieldName": "createTransactionRequest.transactionRequest.payment.creditCard.cardNumber",
					...
                    "sourceRegex": "(?P<s1>[0-9]{4}?)[ =]?(?P<s2>[0-9]{4}?)[ =]?(?P<s3>[0-9]{4}?)[ =]?(?P<s4>[0-9]{4}?)",
                    "transformedRegex": "{s1}{s2}{s3}{s4}"
                },
				...
			]
			...
		}
		...
	]
	....
}
```

## Customization in admin

VaultId, VaultUrl, TableName

three parameters are configured in the admin
Stores/<storeId>/PaymentMethods(widget)/Skyflow/Settings

VaultId and VaultUrl are taken from the settings in SkyflowStudio.
TableName is the name of the table where sensitive data will be saved.


## Configure in config

Payments:Skyflow section
tokenURI: string - must always be `https://manage.skyflowapis.com/v1/auth/sa/oauth/token`
ClientSDK: SkyflowCredentials - three properties from the loaded credentials file (clientID, keyID, privateKey)
Connections[name: string]: SkyflowCredentials - any number of Connections can be registered in the system.


# Usage

SkyflowPaymentMethod class is used
when calling initializePayment (in GraphQL), returns token to work from the front end
when calling authorizePayment, calls IPaymentClientFactory with input parameters to get an echemplate with an IPaymentClient that will use the required Connection.

## parsing the default implementation
The default implementation uses Authorize.Net in xml format.

The required settings are written in `appsettings.json` under `Payemnts/Skyflow/DefaultConnection`.




