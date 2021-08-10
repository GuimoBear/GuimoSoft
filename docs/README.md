# GuimoSoft.Core

Projetado com o intuito de centralizar, dentro de uma lib, a implementação de algumas funcionalidades que são agnósticas ao contexto em que serão utilizadas, o projeto **GuimoSoft.Core** é dividido em:

1. **Bus**: Projeto de integração da api com diferentes brokers(atualmente, apenas o kafka está implementado), acesse sua documentação [*aqui*](bus/README.md).
2. **Cache**: Projeto de integração da api com diferentes formas de cache(atualmente, existe apenas um cache em memória).
3. **Core**: Projeto contendo objetos de valor importantes para a api como Tenant, CorrelationId, notificações, etc, e sua integração dom o Asp.NET Core, utilizado os provedores de Tenant e de CorrelationId, existe também uma referência ao projeto de log.
4. **Cryptography**: Projeto contendo a implementação de algumas criptografias(atualmente, apenas o RSA 2048 está implementada) e sua integração com HttpClient e com o Asp.NET Core.
5. **Logger**: Projeto contendo a implementação de um logger _descritivo_ utilizando o padrão builder, este log é escrito em forma de json no console.
6. **Notifications**: A implementação do Notification pattern para validação de valores utilizando FluentValidation, tal como a integração com o Asp.NET Core(utilizando um filtro para validar um request antes que ele chegue na action no controller).
