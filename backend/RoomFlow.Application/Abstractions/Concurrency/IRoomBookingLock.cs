namespace RoomFlow.Application.Abstractions.Concurrency;

public interface IRoomBookingLock
{
    Task<IDisposable> AcquireAsync(Guid roomId, CancellationToken cancellationToken = default);
}
