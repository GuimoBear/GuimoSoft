# Nuget / GuimoSoft

Este é o GuimoSoft. É um conjunto de libs, que fornecem as capacidades comuns entre APIs no geral.
Atualmente esta solução contém:

## Projetos:
		
###	[Bus]
	GuimoSoft.Bus.Abstractions: Abstrações para contratos de tópicos, producers e consumos de mensagens;		
	GuimoSoft.Bus.Kafka: Contratos para implementação de mensageria em Kafka.

###	[Core]
	GuimoSoft.Core: Voltado para objetos com utilização comum entre as APIs
		- Constants;
		- CorrelationId;
		- Notification;
		- Tenant.
	
###	[Cache]
	GuimoSoft.Cache: Voltado para cache realizado pelas APIs 
							   com Abstrações do ICache e Factory Proxy;
	GuimoSoft.Cache.InMemory: Extension e contrato do Cache InMemory;			
	GuimoSoft.Cache.Resilience: Extension e contrato da Resiliência utilizando o Polly para o Cache.			

###	[Cryptography]
	GuimoSoft.Cryptography: Abstrações para encriptação de dados, payloads JSONs, etc...
		
	GuimoSoft.Cryptography.RSA: Contrato da encriptação em padrão RSA.
	
###	[Logger]
	GuimoSoft.Logger: Contrato do Builder, abstrações e extensões do Ilogger;			
	GuimoSoft.Logger.AspNetCore: Middleware de inicialização do contexto.

			
###	[Notifications]		
	GuimoSoft.Notifications: Implantação do Notification Pattern;
	GuimoSoft.Notifications.AspNetCore: Abstração do Notification.

###	[Tests]
	GuimoSoft.Cache.Tests: Biblioteca de testes do projeto de Cache;
	GuimoSoft.Cryptography.Tests: Biblioteca de testes do projeto de Cryptography;
	GuimoSoft.Logger.Tests: Biblioteca de testes do projeto de Logger;
	GuimoSoft.Bus.Tests: Biblioteca de testes do projeto de EventBroker;
	GuimoSoft.Notifications.Tests: Biblioteca de testes do projeto de Notifications;
	GuimoSoft.Core.Tests: Biblioteca de testes do projeto Core.


## Leia nossa [documentação](./docs/README.md)!
Nesta página existem as definições de:

	Arquitetura;
	Qual foi a motivação para sua criação;
	As tecnologias utilizadas;
	Listagem das APIs existentes;
	Monitoramento e;
	Muito mais!


## Requisitos

### Instalação do .NET Core SDK >= 5.0

Esta API depende do [.NET Core SDK](https://dotnet.microsoft.com/download/dotnet/5.0), com versão igual ou superior ao 5.0.

## Instalação dos pacotes

### GuimoSoft.Core
```
Install-Package GuimoSoft.Core
```

### GuimoSoft.Cache
```
Install-Package GuimoSoft.Cache
```

### GuimoSoft.Cryptography
```
Install-Package GuimoSoft.Cryptography
```

### GuimoSoft.Cryptography.RSA
```
Install-Package GuimoSoft.Cryptography
```

### GuimoSoft.Logger
```
Install-Package GuimoSoft.Logger
```

### GuimoSoft.Logger.AspNetCore
```
Install-Package GuimoSoft.Logger.AspNetCore
```

### GuimoSoft.Bus.Abstractions
```
Install-Package GuimoSoft.Bus.Abstractions
```

### GuimoSoft.Bus.Kafka
```
Install-Package GuimoSoft.Bus.Kafka
```

### GuimoSoft.Notifications
```
Install-Package GuimoSoft.Notifications
```

### GuimoSoft.Notifications.AspNetCore
```
Install-Package GuimoSoft.Notifications.AspNetCore
```

## O que eu devo fazer ao abrir um PR para este repositório?
- O PR deve ser aberto com base no arquivo [pull_request_template.md](pull_request_template.md). O conteúdo deve ser copiado para a *description* da página de Pull Request, e editado conforme a necessidade de cada cenário;
- Garantir que o arquivo [changelog.md](changelog.md) tenha sido atualizado de acordo com as últimas alterações;
- Garantir que o seu Tech Lead revise o código submetido no PR, antes de cobrar a aprovação do time de Core Apps.