# Общее описание

назначение модуля: PCI Complience, избежать хранения чувствительных данных в системе

# Настройка

## Настройка в Skyflow

### создание ролей
необходимо создать 2 системных роли
одна роль для работы на фронте (отправлять данные карточки в скайфлоу), используется роль Vault editor
вторая роль создается вместе с коннекшеном. то есть когда создается коннекшен, роль добавляется автоматически

### создание коннекшенов
модификация коннекшена для примера (перенести в разбор дефолтной реализации)

иногда просто создать дефолтный коннекшен не удалось, например формат номера карты не подходит. в таких случая можно модифицировать connection через RestAPI
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



## Настройка в админке

VaultId, VaultUrl, TableName

в админке настраиваются три параметра
Stores/<storeId>/PaymentMethods(widget)/Skyflow/Settings

VaultId и VaultUrl берутся из настроек в SkyflowStudio
TableName - это название таблицы, куда будут сохраняться чувствительные данные


## Настройка в конфиге

раздел Payments:Skyflow
tokenURI: string -  всегда должен быть `https://manage.skyflowapis.com/v1/auth/sa/oauth/token`
ClientSDK: SkyflowCredentials - три свойства из загруженного файла credentials (clientID, keyID, privateKey)
Connections[name: string]: SkyflowCredentials - в системе может быть зарегистрированно произвольное количество Connections


# Использование

используется класс SkyflowPaymentMethod
при вызове initializePayment (в GraphQL), возвращат token для работы с фронта
при вызове authorizePayment обращается к фабрике IPaymentClientFactory с входными параметрами, чтобы получить экхемпляр с IPaymentClient, который будет использовать необходимый Connection

## разбор дефолтной реализации
Дефолтная реализация использует Authorize.Net в формате xml.

Необходимые настройки прописаны в `appsettings.json` в разделе `Payemnts/Skyflow/DefaultConnection`.




