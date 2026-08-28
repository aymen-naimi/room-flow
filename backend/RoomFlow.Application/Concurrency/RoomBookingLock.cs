using System.Collections.Concurrent;
using RoomFlow.Application.Abstractions.Concurrency;

namespace RoomFlow.Application.Concurrency;

public sealed class RoomBookingLock : IRoomBookingLock
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _gates = new();

    public async Task<IDisposable> AcquireAsync(Guid roomId, CancellationToken cancellationToken = default)
    {
        var gate = _gates.GetOrAdd(roomId, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new Releaser(gate);
    }

    private sealed class Releaser : IDisposable
    {
        private readonly SemaphoreSlim _gate;
        private int _disposed;

        public Releaser(SemaphoreSlim gate)
        {
            _gate = gate;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                _gate.Release();
            }
        }
    }
}
