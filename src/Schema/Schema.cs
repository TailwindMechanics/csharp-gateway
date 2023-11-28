//path: src\Schema\Schema.cs

namespace Neurocache.Schema
{
    public record StopOperationRequest(Guid OperationToken);
    public record Ship(string Name, int Port);
}
