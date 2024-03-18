# Integration with SkyFlow
The module implements integration with [SkyFlow](https://www.skyflow.com/): Data Privacy Vault service. 

## Key Features:
* **Unified Credit Card User Experience:** Elevate your users' payment journey with a unified experience for credit card transactions across diverse payment providers.
* **PCI Compliance:** SkyFlow ensures PCI compliance, streamlining adherence to industry standards for secure handling of payment data.
* **Integration with Virto Storefront:** Seamlessly integrated with [Virto Storefront](https://github.com/VirtoCommerce/vc-theme-b2b-vue) for a cohesive e-commerce experience.
* **Production-Ready Integration with Authorize.NET:** Fully functional integration with Authorize.NET payment providers, ready for production use.
* **Flexible Customization:** Utilize the power of Virto Commerce Native Extensibility Framework and SkyFlow to integrate with a wide range of payment providers, tailored to your specific needs.
* **Optimized for Marketplaces:** Marketplaces can effortlessly connect with various Payment Service Providers preferred by individual suppliers, facilitating diverse payment options for customers.
* **Saved Credit Card:** Customers have the option to securely save their credit card information for future transactions, enhancing convenience and speed of checkout.

## Prerequisites:
1. **SkyFlow Account:** Ensure you have created and configured your [SkyFlow Account](https://www.skyflow.com/) to leverage the full capabilities of the module.
2. **Authorize.NET Account:** Similarly, create and configure your [Authorize.NET Account](https://www.authorize.net/) for seamless integration with the module.
3. **Installation:** Simply install the SkyFlow Module to start harnessing its powerful features and functionalities.

## Architecture

![image-20240124-131429](https://github.com/VirtoCommerce/vc-module-skyflow/assets/7639413/7bcd8edc-68fc-47ac-a13f-d38089064bdc)

1. **Order Placement:**
  * As a customer, after adding items to my cart, I proceed to checkout.
  * Upon reaching the payment method selection page, I notice a new option: "Credit Card."
2. **Credit Card Payment Form:**
  * I select the "Credit Card" payment method.
  * The frontend displays a Credit Card form built with PCI compliance and Skyflow integration, maintaining the current design of the platform.
  * I enter my Credit Card details securely into the form.
3. **Tokenization Process:**
  * Upon submission of the form, the payment process utilizes Skyflow for PCI-compliant Credit Card storage.
  * Skyflow tokenizes my Credit Card information securely.
4. **Transaction Processing:**
  * The tokenized Credit Card information is seamlessly integrated with Payment processing through SkyFlow.
  * The transaction is created successfully via Authorize.NET.
5. **Payment Document Creation:**
  * A Payment document is automatically created within Virto Commerce Payment Processing for the Credit Card transaction.
  * This Payment document includes all necessary details for tracking and processing the transaction within the Virto Commerce platform.
6. **Saved Credit Card:**
  * The Skyflow-generated token for Credit Card is stored within the Virto Commerce Payment document securely.
  * Virto Commerce can save the token and use later as Saved Credit Card.


## Setup

This setup documentation provides step-by-step instructions for secure payment processing by integrating Skyflow with Virto Commerce.

### Skyflow Configuration

#### Role Creation
1. **Sign in to Skyflow Portal.**
1. **Vault Editor Role Creation:**
  * Create a system role named "Vault Editor" for frontend operations. This role will be responsible for sending card data to Skyflow.
2. **Automated Role Creation with Connection:**
  * When creating a connection, a second system role is automatically generated and associated with the connection.

#### Creating Authorize.NET Connectors
1. Create a new connector.
2. Modify Connector via RestAPI:
  * If default connections are not suitable (e.g., mismatched card number format), modify the connector via RestAPI.
  * Use the "Update Outbound Connection" endpoint to adjust the connector's configuration according to your requirements.
3. Generate and save credentials file.

## Virto Commerce Configuration

### Appsettings.json Configuration

1. **Configure Skyflow Settings:** - Update the `appsettings.json` file with Skyflow configuration under `Payments:Skyflow` section:
  * `tokenURI`: Ensure it is always set to `https://manage.skyflowapis.com/v1/auth/sa/oauth/token`.
  * `ClientSDK`: Provide SkyflowCredentials including `clientID`, `keyID`, and `privateKey` from the loaded credentials file.
  * `Connections`: Register any number of Connections with their respective SkyflowCredentials.
2. Mandatory settings should be configured in the `appsettings.json` under `Payments/Skyflow/DefaultConnection`.

```json
{
  "Payments": {
    "Skyflow": {
      "tokenURI": "https://manage.skyflowapis.com/v1/auth/sa/oauth/token",
      "clientSDK": {
        "clientID": "b7eeb4df0007492cbef5bd1000000000",
        "keyID": "i24bb5b53c114f1c9531db69000000000",
        "privateKey": "-----BEGIN PRIVATE KEY---TODO---END PRIVATE KEY-----"
      },
      "Connections": {
        "Default": {
          "clientID": "ca2836c68afa4546b6e09b000000000",
          "keyID": "hd75811c6f4b4ed4835eda00000000",
          "privateKey": "-----BEGIN PRIVATE KEY---TODO---END PRIVATE KEY-----"
        }
      },
      "DefaultConnection": {
        "connectionUrl": "https://ebfc00000000.gateway.skyflowapis.com/v1/gateway/outboundRoutes/gfb5ce07e91340efac348a2df00000000/xml/v1/request.api",
        "name": "TODO:YOURID",
        "transactionKey": "TODO:YOUR_TRANSACTION_KEY"
      }
    }
  }
}
```

### Virto Commerce Back Office Setup
1. Open the Virto Commerce Back Office, select Store and navigate to PaymentMethods widget.
2. Select SkyFlow Payment provider.
3. Configure the following properties:
    * `VaultId` and `VaultUrl`: Retrieve from the settings in Skyflow.
    * `TableName`: Specify the name of the table for storing Credit Card data.
4. Activate/Enable SkyFlow for the Store.

## Customization
1. **Integration with Payment Providers:**
  * By default, the module supports Authorize.NET payment provider.
  * To integrate with another provider or implement custom orchestration:
    * Create a new Virto Commerce Module.
    * Implement [IPaymentClient](https://github.com/VirtoCommerce/vc-module-skyflow/blob/dev/src/VirtoCommerce.Skyflow.Core/Services/IPaymentClient.cs#L6) interface.
    * Register IPaymentClient in IPaymentClientFactory.
2. **SkyflowPaymentMethod Class Usage:**
  * `SkyflowPaymentMethod` class is utilized for:
    * `initializePayment` (in GraphQL): Returns a token for frontend operations.
    * `authorizePayment`: Invokes `IPaymentClientFactory` to obtain an instance of `IPaymentClient` for processing transactions using the required Connection.

## References
* Documenation: https://docs.virtocommerce.org
* Home: https://virtocommerce.com
* Community: https://www.virtocommerce.org

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.









