# Integration with Skyflow
The module implements integration with [Skyflow](https://www.skyflow.com/): Data Privacy Vault service.

## Key Features:
* **Unified Credit Card User Experience:** Elevate your users' payment journey with a unified experience for credit card transactions across diverse payment providers.
* **PCI Compliance:** Skyflow ensures PCI compliance, streamlining adherence to industry standards for secure handling of payment data.
* **Integration with Virto Storefront:** Seamlessly integrated with [Virto Storefront](https://github.com/VirtoCommerce/vc-theme-b2b-vue) for a cohesive e-commerce experience.
* **Production-Ready Integration with Authorize.NET:** Fully functional integration with Authorize.NET payment providers, ready for production use.
* **Flexible Customization:** Utilize the power of Virto Commerce Native Extensibility Framework and Skyflow to integrate with a wide range of payment providers, tailored to your specific needs.
* **Optimized for Marketplaces:** Marketplaces can effortlessly connect with various Payment Service Providers preferred by individual suppliers, facilitating diverse payment options for customers.
* **Saved Credit Card:** Customers have the option to securely save their credit card information for future transactions, enhancing convenience and speed of checkout.

## Prerequisites:
1. **Skyflow Account:** Ensure you have created and configured your [Skyflow Account](https://www.skyflow.com/) to leverage the full capabilities of the module.
2. **Authorize.NET Account:** Similarly, create and configure your [Authorize.NET Account](https://www.authorize.net/) for seamless integration with the module.
3. **Installation:** Simply install the Skyflow Module to start harnessing its powerful features and functionalities.

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
  * The tokenized Credit Card information is seamlessly integrated with Payment processing through Skyflow.
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

#### Import vault schema
1. **Sign in to Skyflow Studio.**
2. On the home screen with the list of vaults. Select `Add vault`-> `upload vault schema`
3. Upload the default vault schema [vaultSchema.json](./docs/media/vaultSchema.json)



#### Roles Creation
The main security recommendation is to have two service accounts with different roles for saving and tokenizing card data in the vault and for executing outbound connections. In the next steps, create the two new roles:
1. 'Integrations invoker' role with the folowing policies
```
ALLOW READ ON credit_cards.* WITH REDACTION = DEFAULT
ALLOW TOKENIZATION ON credit_cards.*
ALLOW READ ON credit_cards.card_expiration WITH REDACTION = PLAIN_TEXT
```
1. 'Vault writer' role with the folowing policies
```
ALLOW CREATE ON credit_cards.*
ALLOW TOKENIZATION ON credit_cards.*
```
#### Service Accounts Creation
Create the two service accounts
Vault -> Service Accounts -> Add Service Account
1. Use name `Payment form account`  and assign the role `Vault writer`.
2. Use name `Integrations account` and assign the role `Integrations invoker`.
Download the `credentials.json` files for each service account and keep them on the secure place.


#### Creating Authorize.NET Skyflow Connection
1. In the Skyflow studio open `Vault -> Connections -> Add connection`
2. Use the   `https://apitest.authorize.net` as outbound base url
3. Use the `POST /xml/v1/request.api` endpoint for route
4. Select content type `XML`
5. For the request body set these fields and actions
   1. `createTransactionRequest.transactionRequest.payment.creditCard.cardNumber` - Detokenization
   2. `createTransactionRequest.transactionRequest.payment.creditCard.cardCode` - Detokenization
   3. `createTransactionRequest.transactionRequest.payment.creditCard.expirationDate` - Detokenization
6. Assign `Integrations account` service account to this connection

## Virto Commerce Configuration

### Appsettings.json Configuration

**Configure Skyflow Settings:** - Update the `appsettings.json` file with Skyflow configuration under `Payments:Skyflow` section:

Configuration example.
```json
{
  "Payments": {
    "Skyflow": {
      "tokenURI": "https://manage.skyflowapis-preview.com/v1/auth/sa/oauth/token",
      "vaultURI": "https://a370a9658141.vault.skyflowapis-preview.com",
      "gatewayURI": "https://a370a9658141.gateway.skyflowapis-preview.com",
      "vaultId": "ff9fc275bec848318361cc8928e094d1",
      "tableName": "credit_cards",
      "PaymentFormAccount": {
        "clientID": "j873500104e6439bbbeb8cec63a6d21",
        "keyID": "a70d977de5f24532810df376585031aa",
        "privateKey": "-----BEGIN PRIVATE KEY-----Base64-----END PRIVATE KEY-----"
      },
      "IntegrationsAccount": {
        "clientID": "b47bea9c61c74cf4aac3b26d09aaf825",
        "keyID": "c950c459157548f0817500288ec8ac96",
        "privateKey": "-----BEGIN PRIVATE KEY-----Base64-----END PRIVATE KEY-----"
      },
      "TargetPaymentMethod": "AuthorizeNetPaymentMethod",
      "TargetConnectionRoute": "b47bea9c61c74cf4aac3b26d09aaf825/xml/v1/request.api"

    }
  }
}
```

- `Payments:Skyflow:tokenURI` - url uses for receiving auth token to consume Skyflow api. (can be taken from credentials.json file downloaded from Skyflow dashboard)
- `Payments:Skyflow:vaultURI` - vault url can be taken from Skyflow studio click three dots on the vault and select `view details` actions
- `Payments:Skyflow:gatewayURI` - URI for invoke outbound connection rules. Just need to replace `vault` to `gateway` in the `vaultURI`
- `Payments.Skyflow.tableName` - the table name for store credit cards data. Default value `credit_cards`
- `Payments.Skyflow.PaymentFormAccount` and `Payments.Skyflow.IntegrationsAccount` - the two service account configuration sections with credentials taken from Skyflow service account configuration on the previous steps.
- `Payments.Skyflow.TargetPaymentMethod` -  The payment method being used for authorizing payment by Skyflow payment method. To execute the outbound Payment Service Provider API through Skyflow connection, create an HttpClient instance  `IHttpServiceFactory.CreateClient("Skyflow")` and execute the request to the Payment Service API with the tokenized card data. See code example:
```C#
 public override PostProcessPaymentRequestResult PostProcessPayment(PostProcessPaymentRequest request)
   {
....
       if (request.Parameters["CreditCard"] != null)
       {
           var tokenizedCard = JsonConvert.DeserializeObject<dynamic>(request.Parameters["CreditCard"]);
           creditCard = new AuthorizeNetCreditCard
           {
               CardCode = tokenizedCard.Cvv,
               CardNumber = tokenizedCard.CardNumber,
               ExpirationDate = tokenizedCard.CardExpiration,
               ProxyEndpointUrl = request.Parameters["ProxyEndpointUrl"],
               ProxyHttpClientName = request.Parameters["ProxyHttpClientName"]
           };

          using var stream = new MemoryStream();
          var proxyHttpClient = _httpClientFactory.CreateClient(request.CreditCard.ProxyHttpClientName);
          var xmlSerializer = new XmlSerializer(typeof(AuthorizeNetCreateTransactionRequest));
          using var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings
          {
              Encoding = new UTF8Encoding(false, true), //Exclude BOM
              Indent = true,
          });
          xmlSerializer.Serialize(xmlWriter, this);
          using var content = new StreamContent(stream);
          content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Xml);
          var proxyRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(request.CreditCard.ProxyEndpointUrl))
          {
              Content = content
          };
          var response = proxyHttpClient.Send(proxyRequest);
       }
```



## Customization
**Integration with Payment Providers:**
  * By default, the module supports Authorize.NET payment provider.
  * To integrate with multiple target  providers based on some customconditions  or implement custom orchestration:
    * Create a new Virto Commerce Module.
    * Create the new C# class derived from the  `SkyflowPaymentMethod`
    * Override virtual  `GetTargetPaymentMethod` with custom logic for target payment provider creation
    * Register custom payment method in the DI

`Module.cs`
  ```C#
   public void Initialize(IServiceCollection serviceCollection)
 {
  ...
   serviceCollection.AddTransient<SkyflowPaymentMethod, SkyflowPaymentMethod2>();
   ...
 }
  ```


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
