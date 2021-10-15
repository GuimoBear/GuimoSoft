using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Interfaces;
using GuimoSoft.Bus.Core.Internal;
using GuimoSoft.Bus.Tests.Fakes;
using Xunit;

namespace GuimoSoft.Bus.Tests.Consumer
{
    public class EventMiddlewareManagerTests
    {
        [Fact]
        public void Dado_UmFakeEventMiddleware_Se_RegistrarSemMetodoDeFabrica_Entao_TipoEhRegistradoNaCollection()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);

            sut.Register<FakeEvent, FakeEventMiddleware>(BusName.Kafka, ServerName.Default, ServiceLifetime.Singleton);

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeEventMiddleware>()
                    .Should().NotBeNull();
            }
        }

        [Fact]
        public void Dado_UmFakeEventMiddleware_Se_RegistrarComMetodoDeFabrica_Entao_TipoEhRegistradoNaCollection()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);

            bool factoryMethodExecuted = false;
            Func<IServiceProvider, FakeEventMiddleware> factory =
                prov =>
                {
                    factoryMethodExecuted = true;
                    return new FakeEventMiddleware();
                };

            sut.Register<FakeEvent, FakeEventMiddleware>(BusName.Kafka, ServerName.Default, factory, ServiceLifetime.Singleton);

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeEventMiddleware>()
                    .Should().NotBeNull();

                factoryMethodExecuted
                    .Should().BeTrue();
            }
        }

        [Fact]
        public void Dado_UmFakeEventMiddleware_Se_RegistrarComMetodoDeFabricaDefault_Entao_TipoEhRegistradoNaCollection()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);

            sut.Register<FakeEvent, FakeEventMiddleware>(BusName.Kafka, ServerName.Default, default(Func<IServiceProvider, FakeEventMiddleware>), ServiceLifetime.Singleton);

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeEventMiddleware>()
                    .Should().NotBeNull();
            }
        }

        [Fact]
        public void Dado_UmFakeEventMiddleware_Se_RegistrarSemMetodoDeFabricaETentarAdicionaloNovamente_Entao_EstouraExcecaoNaSegundaTentativa()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);

            sut.Register<FakeEvent, FakeEventMiddleware>(BusName.Kafka, ServerName.Default, ServiceLifetime.Singleton);

            Assert.Throws<InvalidOperationException>(() => sut.Register<FakeEvent, FakeEventMiddleware>(BusName.Kafka, ServerName.Default, ServiceLifetime.Singleton))
                .Message.Should().Be($"Não foi possível registrar o middleware do tipo '{typeof(FakeEventMiddleware).FullName}'");

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeEventMiddleware>()
                    .Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Dado_TresMiddlewares_Quando_GetPipeline_Entao_RetornaACriacaoComSucesso()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);

            serviceCollection.AddSingleton<IEventMiddlewareExecutorProvider>(sut);
            serviceCollection.AddSingleton(typeof(IConsumeContextAccessor<>), typeof(ConsumeContextAccessor<>));
            serviceCollection.AddSingleton(typeof(ConsumeContextAccessorInitializerMiddleware<>));

            sut.Register<FakePipelineEvent, FakePipelineEventMiddlewareOne>(BusName.Kafka, ServerName.Default, ServiceLifetime.Singleton);
            sut.Register<FakePipelineEvent, FakePipelineEventMiddlewareTwo>(BusName.Kafka, ServerName.Default, ServiceLifetime.Singleton);
            sut.Register<FakePipelineEvent, FakePipelineEventMiddlewareThree>(BusName.Kafka, ServerName.Default, ServiceLifetime.Singleton);

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakePipelineEventMiddlewareOne>()
                    .Should().NotBeNull();
                prov.GetService<FakePipelineEventMiddlewareTwo>()
                    .Should().NotBeNull();
                prov.GetService<FakePipelineEventMiddlewareThree>()
                    .Should().NotBeNull();

                var executorProvider = prov.GetService<IEventMiddlewareExecutorProvider>();

                executorProvider
                    .Should().NotBeNull();

                var pipeline = executorProvider.GetPipeline(BusName.Kafka, ServerName.Default, typeof(FakePipelineEvent));

                pipeline
                    .Should().NotBeNull();

                await pipeline.Execute(new FakePipelineEvent(FakePipelineEventMiddlewareOne.Name), prov, new ConsumeInformations(BusName.Kafka, ServerName.Default, "a"), CancellationToken.None);
            }
        }

        [Fact]
        public void Dado_UmFakeEventMiddleware_Quando_ForRegistradoMetodoDeFabricaESemFabrica_Entao_MetodoDeFabricaSeraRequisitadaApenasUmaVez()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);
            bool factoryMethodExecuted = false;
            Func<IServiceProvider, FakeEventMiddleware> factory =
                prov =>
                {
                    factoryMethodExecuted = true;
                    return new FakeEventMiddleware();
                };

            bool factoryTwoMethodExecuted = false;
            Func<IServiceProvider, FakeEventThrowExceptionMiddleware> factoryTwo =
                prov =>
                {
                    factoryTwoMethodExecuted = true;
                    return new FakeEventThrowExceptionMiddleware();
                };

            sut.Register<FakeEvent, FakeEventMiddleware>(BusName.Kafka, ServerName.Default, factory, ServiceLifetime.Singleton);

            sut.Register<FakeEvent, FakeEventThrowExceptionMiddleware>(BusName.Kafka, ServerName.Default, factoryTwo, ServiceLifetime.Singleton);

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeEventMiddleware>()
                    .Should().NotBeNull();

                factoryMethodExecuted
                    .Should().BeTrue();

                prov.GetService<FakeEventThrowExceptionMiddleware>()
                    .Should().NotBeNull();

                factoryTwoMethodExecuted
                    .Should().BeTrue();
            }
        }

        [Fact]
        public void Dado_UmFakeEventMiddleware_Se_RegistrarComMetodoDeFabricaETentarAdicionaloNovamente_Entao_EstouraExcecaoNaSegundaTentativa()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new EventMiddlewareManager(serviceCollection);
            bool factoryMethodExecuted = false;
            Func<IServiceProvider, FakeEventMiddleware> factory =
                prov =>
                {
                    factoryMethodExecuted = true;
                    return new FakeEventMiddleware();
                };

            bool factoryTwoMethodExecuted = false;
            Func<IServiceProvider, FakeEventMiddleware> factoryTwo =
                prov =>
                {
                    factoryTwoMethodExecuted = true;
                    return new FakeEventMiddleware();
                };

            sut.Register<FakeEvent, FakeEventMiddleware>(BusName.Kafka, ServerName.Default, factory, ServiceLifetime.Singleton);

            Assert.Throws<InvalidOperationException>(() => sut.Register<FakeEvent, FakeEventMiddleware>(BusName.Kafka, ServerName.Default, factoryTwo, ServiceLifetime.Singleton))
                .Message.Should().Be($"Não foi possível registrar o middleware do tipo '{typeof(FakeEventMiddleware).FullName}'");

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeEventMiddleware>()
                    .Should().NotBeNull();

                factoryMethodExecuted
                    .Should().BeTrue();

                factoryTwoMethodExecuted
                    .Should().BeFalse();
            }
        }
    }
}
