using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using GuimoSoft.MessageBroker.Kafka.Consumer;
using GuimoSoft.MessageBroker.Kafka.Tests.Fakes;
using Xunit;

namespace GuimoSoft.MessageBroker.Kafka.Tests.Consumer
{
    public class MessageMiddlewareManagerTests
    {
        [Fact]
        public void Dado_UmFakeMessageMiddleware_Se_RegistrarSemMetodoDeFabrica_Entao_TipoEhRegistradoNaCollection()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new MessageMiddlereManager(serviceCollection);

            sut.Register<FakeMessage, FakeMessageMiddleware>();

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();
            }
        }

        [Fact]
        public void Dado_UmFakeMessageMiddleware_Se_RegistrarComMetodoDeFabrica_Entao_TipoEhRegistradoNaCollection()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new MessageMiddlereManager(serviceCollection);

            bool factoryMethodExecuted = false;
            Func<IServiceProvider, FakeMessageMiddleware> factory =
                prov =>
                {
                    factoryMethodExecuted = true;
                    return new FakeMessageMiddleware();
                };

            sut.Register<FakeMessage, FakeMessageMiddleware>(factory);

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();

                factoryMethodExecuted
                    .Should().BeTrue();
            }
        }

        [Fact]
        public void Dado_UmFakeMessageMiddleware_Se_RegistrarComMetodoDeFabricaDefault_Entao_TipoEhRegistradoNaCollection()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new MessageMiddlereManager(serviceCollection);

            sut.Register<FakeMessage, FakeMessageMiddleware>(default(Func<IServiceProvider, FakeMessageMiddleware>));

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();
            }
        }

        [Fact]
        public void Dado_UmFakeMessageMiddleware_Se_RegistrarSemMetodoDeFabricaETentarAdicionaloNovamente_Entao_EstouraExcecaoNaSegundaTentativa()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new MessageMiddlereManager(serviceCollection);

            sut.Register<FakeMessage, FakeMessageMiddleware>();

            Assert.Throws<InvalidOperationException>(() => sut.Register<FakeMessage, FakeMessageMiddleware>())
                .Message.Should().Be($"Não foi possível registrar o middleware do tipo '{typeof(FakeMessageMiddleware).FullName}'");

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();
            }
        }

        [Fact]
        public void Dado_TresMiddlewares_Se_GetPipeline_Entao_ExecutaACriacaoERetornaComSucesso()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new MessageMiddlereManager(serviceCollection);

            serviceCollection.AddSingleton<IMessageMiddlereExecutorProvider>(sut);

            sut.Register<FakePipelineMessage, FakePipelineMessageMiddlewareOne>();
            sut.Register<FakePipelineMessage, FakePipelineMessageMiddlewareTwo>();
            sut.Register<FakePipelineMessage, FakePipelineMessageMiddlewareThree>();

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakePipelineMessageMiddlewareOne>()
                    .Should().NotBeNull();
                prov.GetService<FakePipelineMessageMiddlewareTwo>()
                    .Should().NotBeNull();
                prov.GetService<FakePipelineMessageMiddlewareThree>()
                    .Should().NotBeNull();

                var executorProvider = prov.GetService<IMessageMiddlereExecutorProvider>();

                executorProvider
                    .Should().NotBeNull();

                var pipeline = executorProvider.GetPipeline(typeof(FakePipelineMessage));

                pipeline
                    .Should().NotBeNull();
            }
        }

        [Fact]
        public void Dado_UmFakeMessageMiddleware_Se_RegistrarComMetodoDeFabricaEAdicionaOutroMiddlewareSemFabrica_Entao_TipoEhRegistradoNaCollectionApenasUmaVez()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new MessageMiddlereManager(serviceCollection);
            bool factoryMethodExecuted = false;
            Func<IServiceProvider, FakeMessageMiddleware> factory =
                prov =>
                {
                    factoryMethodExecuted = true;
                    return new FakeMessageMiddleware();
                };

            bool factoryTwoMethodExecuted = false;
            Func<IServiceProvider, FakeMessageThrowExceptionMiddleware> factoryTwo =
                prov =>
                {
                    factoryTwoMethodExecuted = true;
                    return new FakeMessageThrowExceptionMiddleware();
                };

            sut.Register<FakeMessage, FakeMessageMiddleware>(factory);

            sut.Register<FakeMessage, FakeMessageThrowExceptionMiddleware>(factoryTwo);

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();

                factoryMethodExecuted
                    .Should().BeTrue();

                prov.GetService<FakeMessageThrowExceptionMiddleware>()
                    .Should().NotBeNull();

                factoryTwoMethodExecuted
                    .Should().BeTrue();
            }
        }

        [Fact]
        public void Dado_UmFakeMessageMiddleware_Se_RegistrarComMetodoDeFabricaETentarAdicionaloNovamente_Entao_EstouraExcecaoNaSegundaTentativa()
        {
            var serviceCollection = new ServiceCollection();

            var sut = new MessageMiddlereManager(serviceCollection);
            bool factoryMethodExecuted = false;
            Func<IServiceProvider, FakeMessageMiddleware> factory =
                prov =>
                {
                    factoryMethodExecuted = true;
                    return new FakeMessageMiddleware();
                };

            bool factoryTwoMethodExecuted = false;
            Func<IServiceProvider, FakeMessageMiddleware> factoryTwo =
                prov =>
                {
                    factoryTwoMethodExecuted = true;
                    return new FakeMessageMiddleware();
                };

            sut.Register<FakeMessage, FakeMessageMiddleware>(factory);

            Assert.Throws<InvalidOperationException>(() => sut.Register<FakeMessage, FakeMessageMiddleware>(factoryTwo))
                .Message.Should().Be($"Não foi possível registrar o middleware do tipo '{typeof(FakeMessageMiddleware).FullName}'");

            using (var prov = serviceCollection.BuildServiceProvider())
            {
                prov.GetService<FakeMessageMiddleware>()
                    .Should().NotBeNull();

                factoryMethodExecuted
                    .Should().BeTrue();

                factoryTwoMethodExecuted
                    .Should().BeFalse();
            }
        }
    }
}
