# Middlewares

> Qualquer dúvida acerca de algum termo desconhecido, acesse nosso [**_glossário_**](glossario.md)

Assim como um middleware no [ASP.NET Core](https://github.com/dotnet/aspnetcore), um middleware no Bus é chamado entre o consumo da mensagem na fila e seu envio para o handler.

Utilizando middleware, é possível:  

1. Ajustar valores dentro do escopo da chamada(para aplicações multiinquilinos, por exemplo).

2. Capturar os erros que ocorrerão nos middlewares subjacentes.

3. Interromper o fluxo da pipeline, fazendo com que os middlewares subjacentes e o handler não sejam executados.

## Implementando um middleware

Para a criação de um middleware, é necessário implementarmos a interface `IEventMiddleware<TEvent>`:

```csharp
public class HelloEventMiddleware : IEventMiddleware<HelloEvent>
{
    private readonly ILogger<HelloEventMiddleware> _logger;

    public HelloEventMiddleware(ILogger<HelloEventMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(ConsumeContext<HelloEvent> context, Func<Task> next)
    {
        //Código executado antes dos pipelines subjacentes e do handler
        _logger.Debug($"Middleware {nameof(HelloEventMiddleware)} iniciado");
        await next();
        _logger.Debug($"Middleware {nameof(HelloEventMiddleware)} finalizado");
        //Código executado após os pipelines subjacentes e do handler
    }
}
```

> 1. Caso o método `next()` não seja chamado, o fluxo da pipeline será interrompido.
> 2. Se houver qualquer exceção não tratada nos middlewares subjacentes, esta chegará até o middleware atual.

## Registrando um middleware

O registro dos middlewares é feito junto ao registro das mensagens.

### Forma padrão

```csharp
.Consume()
    .OfType<HelloEvent>()
    // Middleware criado sempre que é requisitado
    .WithMiddleware<FirstHelloEventMiddleware>(ServiceLifetime.Transient)
    // Middleware criado usando um método favctory uma vez durante o escopo da requisição
    .WithMiddleware(prov => new SecondHelloEventMiddleware(), ServiceLifetime.Scoped) 
    // Middleweare criado uma vez no lifetime da aplicação.
    .WithMiddleware<ThirdHelloEventMiddleware>(ServiceLifetime.Singleton)
    // Middleware criado como Singleton(assim como o anterior)
    .WithMiddleware<FourthHelloEventMiddleware>() 
    .FromEndpoint(HelloEvent.TOPIC_NAME)
```

> 1. Assim como os middlewares do Asp.NET Core, a ordem de registro de um middleware afeta a ordem de sua execução, não se atentar nesta ordem pode ocasionar quebras no fluxo de execução.
> 2. O Lifetime Scoped ou transient, no caso do middleware, terão o mesmo comportamento, visto que a criação de um middleware é requerida apenas uma vez durante o escomo do recebimento de uma mensagem.

### Utilizando interruptores

Para o caso da implementação de __**interruptores**__, Os middlewares são separados por tipo de mensagem e por __**posição**__ no __**interruptor**__:

```csharp
public enum ServerName
{
    Host1,
    Host2
}

[...]
switcher
    .When(ServerName.Host1)
        .Consume()
            .OfType<HelloEvent>()
            .WithMiddleware<FirstHelloEventMiddleware>(ServiceLifetime.Transient)
            .WithMiddleware(prov => new SecondHelloEventMiddleware(), ServiceLifetime.Scoped) 
            .FromEndpoint(HelloEvent.TOPIC_NAME)
        .FromServer(options =>
        {
            options.BootstrapServers = "localhost:9093";
            options.GroupId = "test";
        });

switcher
    .When(ServerName.Host2)
        .Consume()
            .OfType<HelloEvent>()
            .WithMiddleware<ThirdHelloEventMiddleware>(ServiceLifetime.Singleton)
            .WithMiddleware<FourthHelloEventMiddleware>() 
            .FromEndpoint(HelloEvent.TOPIC_NAME)
        .FromServer(options =>
        {
            options.BootstrapServers = "google.com:9093";
            options.GroupId = "test";
        });
```

- As mensagens recebidas no `ServerName.Host1` executarão apenas os middlewares `FirstHelloEventMiddleware` e `SecondHelloEventMiddleware`, necessariamente nesta ordem.
- As mensagens recebidas no `ServerName.Host2` executarão apenas os middlewares `ThirdHelloEventMiddleware` e `FourthHelloEventMiddleware`, necessariamente nesta ordem.
