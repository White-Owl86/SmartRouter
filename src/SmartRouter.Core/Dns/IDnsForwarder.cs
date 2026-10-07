namespace SmartRouter.Core.Dns;

public interface IDnsForwarder
{
    Task<byte[]?> ForwardAsync(byte[] queryBuffer, CancellationToken ct = default);
    string Name { get; }
    string Endpoint { get; }
}
