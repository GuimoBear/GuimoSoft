# Monitorando logs

> Qualquer dúvida acerca de algum termo desconhecido, acesse nosso [**_glossário_**](glossario.md)

Os logs são eventos gerais enviado pelo Bus, indicando algumas ações, monitorando tempos de execução, etc.

Atualmente, apenas os logs enviados pelo próprio __broker__ estão sendo capturados, não havendo logs da implementação do Bus.

## Eventos de logs

Existem dois tipos de logs que podem ser capturados

- **`BusLogEvent`**: Logs que ocorrem antes de se receber uma mensagem de um endpoint, esta não possui informações como o tipo ou a instância da mensagem consumida.
- **`BusTypedLogEvent<TEvent>`**: Logs capturados após a leitura da mensagem de um endpoint, este possui informações de tipo e da instância da mensagem consumida(A instância pode ser nula no caso de houver um erro na deserialização de uma mensagem).

### `BusLogEvent`

Esta classe possui as seguintes propriedades:

- `BusName Bus`: O nome do Bus de onde o log foi enviado.
- `Enum? Switch`: A __**posição**__ no __**interruptor**__ (**esta propriedade será nula caso não tenha sido usado os _switchs_**).
- `string Endpoint`: O __**endpoint**__ em que o consumidor está inscrito(**este valor será nulo no caso do log capturada ocorrer antes da inscrição no _endpoint_, como um erro na conexão com o _broker_, por exemplo**).
- `string Event`: O texto do log.
- `BusLogLevel Level`: O `BusLogLevel` do log.
- `IDictionary<string, object> Data`: Um dicionário contendo as informações adicionais.

### `BusTypedLogEvent<TEvent>`

Esta classe possui as seguintes propriedades:

- `BusName Bus`: O nome do Bus de onde o log foi enviado.
- `Enum? Switch`: A __**posição**__ no __**interruptor**__ (**esta propriedade será nula caso não tenha sido usado os _switchs_**).
- `string Endpoint`: O __**endpoint**__ em que o consumidor está inscrito(**este valor será nulo no caso do log capturada ocorrer antes da inscrição no _endpoint_, como um erro na conexão com o _broker_, por exemplo**).
- `TEvent EventObject`: A instância da mensagem(**esta propriedade pode ser nula no caso de haver um erro ao deserializar a mensagem recebida, por exemplo**).
- `string Event`: O texto do log.
- `BusLogLevel Level`: O `BusLogLevel` do log.
- `IDictionary<string, object> Data`: Um dicionário contendo as informações adicionais.

## Capturando logs

Capturar um log, assim como capturar uma mensagem consumida, só depende do `IEventHandler`.

**1.** Log padrão:

```csharp
public class LogEventHandler : IEventHandler<BusLogEvent>
{
    public async Task Handle(BusLogEvent logEvent, CancellationToken cancellationToken);
}
```

**2.** Log tipado:

```csharp
public class HelloLogEventHandler : IEventHandler<BusTypedLogEvent<HelloEvent>>
{
    public async Task Handle(BusTypedLogEvent<HelloEvent> typedLogEvent, CancellationToken cancellationToken);
}
```

A ordem de chamadas dos handlers é a seguinte:

1. Caso, no momento do log, tenha sido informada a instância da mensagem recebida e **EXISTA** um handler registrado para aquele tipo, apenas o Handler tipado será chamado.
2. Caso a instância da mensagem tenha sido informada mas **NÃO EXISTA** um handler registrado para aquele tipo, apenas o handler padrão será chamado.
3. Caso não tenha sido informada a instância da mensagem apenas o handler padrão sejá chamado.

> **Não existes cenários onde ambos os handlers serão chamados.**
