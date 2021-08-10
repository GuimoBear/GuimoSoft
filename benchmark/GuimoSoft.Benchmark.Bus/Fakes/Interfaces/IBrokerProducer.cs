namespace GuimoSoft.Benchmark.Bus.Fakes.Interfaces
{
    public interface IBrokerProducer
    {
        void Enqueue(string topic, byte[] message);
    }
}
