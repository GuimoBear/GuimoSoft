# Monitorando exceções

> Qualquer dúvida acerca de algum termo desconhecido, acesse nosso [**_glossário_**](glossario.md)

Existem situações adversas ao se consumir produtos externos ou ao não tratar uma exceção em um middleware ou handler, para estas situações, existem formas de se capturar tais adversidades.

## Eventos de exceções

Existem dois tipos de exceções que podem ser capturadas

- **`BusExceptionEvent`**: Exceções que ocorrem antes de se receber uma mensagem de um endpoint, esta não possui informações como o tipo ou a instância da mensagem consumida.
- **`BusTypedExceptionEvent<TEvent>`**: Exceções capturadas após a leitura da mensagem de um endpoint, esta possui informações de tipo e da instância da mensagem consumida(A instância pode ser nula no caso de houver um erro na deserialização de uma mensagem).

### `BusExceptionEvent`

Esta classe possui as seguintes propriedades:

- `BusName Bus`: O nome do Bus de onde a exceção foi enviada.
- `Enum? Switch`: A __**posição**__ no __**interruptor**__ (**esta propriedade será nula caso não tenha sido usado os _switchs_**).
- `string Endpoint`: O __**endpoint**__ em que o consumidor está inscrito(**este valor será nulo no caso da exceção capturada ocorrer antes da inscrição no _endpoint_, como um erro na conexão com o _broker_, por exemplo**).
- `string Event`: O texto da exceção.
- `BusLogLevel Level`: O `BusLogLevel` da exceção.
- `Exception Exception`: A exceção capturada
- `IDictionary<string, object> Data`: Um dicionário contendo as informações adicionais.

### `BusTypedExceptionEvent<TEvent>`

Esta classe possui as seguintes propriedades:

- `BusName Bus`: O nome do Bus de onde a exceção foi enviada.
- `Enum? Switch`: A __**posição**__ no __**interruptor**__ (**esta propriedade será nula caso não tenha sido usado os _switchs_**).
- `string Endpoint`: O __**endpoint**__ em que o consumidor está inscrito(**este valor será nulo no caso da exceção capturada ocorrer antes da inscrição no _endpoint_, como um erro na conexão com o _broker_, por exemplo**).
- `TEvent EventObject`: A instância da mensagem(**esta propriedade pode ser nula no caso de haver um erro ao deserializar a mensagem recebida, por exemplo**).
- `string Event`: O texto da exceção.
- `BusLogLevel Level`: O `BusLogLevel` da exceção.
- `Exception Exception`: A exceção capturada
- `IDictionary<string, object> Data`: Um dicionário contendo as informações adicionais.

## Capturando exceções

Capturar uma exceção, assim como capturar uma mensagem consumida, só depende do `INotificationHandler` do [MediatR](https://github.com/jbogard/MediatR).

**1.** Exceção padrão:

```csharp
public class ExceptionEventHandler : INotificationHandler<BusExceptionEvent>
{
    public async Task Handle(BusExceptionEvent exceptionEvent, CancellationToken cancellationToken);
}
```

**2.** Exceção tipada:

```csharp
public class HelloExceptionEventHandler : INotificationHandler<BusTypedExceptionEvent<HelloEvent>>
{
    public async Task Handle(BusTypedExceptionEvent<HelloEvent> typedExceptionEvent, CancellationToken cancellationToken);
}
```

A ordem de chamadas dos handlers é a seguinte:

1. Caso, no momento da exceção, tenha sido informada a instância da mensagem recebida e **EXISTA** um handler registrado para aquele tipo, apenas o Handler tipado será chamado.
2. Caso a instância da mensagem tenha sido informada mas **NÃO EXISTA** um handler registrado para aquele tipo, apenas o handler padrão será chamado.
3. Caso não tenha sido informada a instância da mensagem apenas o handler padrão sejá chamado.

> **Não existes cenários onde ambos os handlers serão chamados.**
