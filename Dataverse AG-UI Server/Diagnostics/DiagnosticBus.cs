namespace Dataverse_AG_UI_Server.Diagnostics
{
    using System.Runtime.CompilerServices;
    using System.Threading.Channels;

    public class DiagnosticBus : IDiagnosticBus
    {
        private readonly Channel<AgentDiagnosticEvent> _channel =
            Channel.CreateUnbounded<AgentDiagnosticEvent>();

        public void Publish(AgentDiagnosticEvent evt)
        {
            Console.WriteLine($"{evt.SourceAgent}-{evt.Target}: {evt.Payload})");
            _channel.Writer.TryWrite(evt);
            
        }

        public async IAsyncEnumerable<AgentDiagnosticEvent> Stream(
            [EnumeratorCancellation] CancellationToken ct)
        {
            while (await _channel.Reader.WaitToReadAsync(ct))
            {
                while (_channel.Reader.TryRead(out var evt))
                {
                    yield return evt;
                }
            }
        }
    }

}
