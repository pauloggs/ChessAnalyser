using Interfaces.DTO;

namespace Services;

/// <summary>Thread-safe store for maintenance job progress (FIDE seed, metadata link).</summary>
public interface IMaintenanceProgressStore
{
    void Set(MaintenanceProgress? progress);

    MaintenanceProgress? Get();

    /// <summary>Starts a new cancellable maintenance operation; cancels any prior token.</summary>
    CancellationToken BeginOperation();

    void RequestCancel();

    bool IsCancellationRequested { get; }

    void ClearOperation();
}

public sealed class MaintenanceProgressStore : IMaintenanceProgressStore
{
    private readonly object _lock = new();
    private MaintenanceProgress? _current;
    private CancellationTokenSource? _cts;

    public void Set(MaintenanceProgress? progress)
    {
        lock (_lock)
        {
            _current = progress;
        }
    }

    public MaintenanceProgress? Get()
    {
        lock (_lock)
        {
            return _current;
        }
    }

    public CancellationToken BeginOperation()
    {
        lock (_lock)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            return _cts.Token;
        }
    }

    public void RequestCancel()
    {
        lock (_lock)
        {
            _cts?.Cancel();
        }
    }

    public bool IsCancellationRequested
    {
        get
        {
            lock (_lock)
            {
                return _cts?.IsCancellationRequested ?? false;
            }
        }
    }

    public void ClearOperation()
    {
        lock (_lock)
        {
            _cts?.Dispose();
            _cts = null;
        }
    }
}
