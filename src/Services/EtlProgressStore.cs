using Interfaces.DTO;

namespace Services
{
    /// <summary>
    /// Thread-safe store for the latest ETL progress and cancellation. GET endpoint reads from here; ETL writes here.
    /// </summary>
    public interface IEtlProgressStore
    {
        void Set(EtlProgress? progress);
        EtlProgress? Get();
        void RequestCancel();
        bool IsCancellationRequested { get; }
        void ClearCancel();
    }

    public class EtlProgressStore : IEtlProgressStore
    {
        private readonly object _lock = new();
        private EtlProgress? _current;
        private bool _cancelRequested;

        public void Set(EtlProgress? progress)
        {
            lock (_lock)
            {
                _current = progress;
            }
        }

        public EtlProgress? Get()
        {
            lock (_lock)
            {
                return _current;
            }
        }

        public void RequestCancel()
        {
            lock (_lock)
            {
                _cancelRequested = true;
            }
        }

        public bool IsCancellationRequested
        {
            get { lock (_lock) return _cancelRequested; }
        }

        public void ClearCancel()
        {
            lock (_lock)
            {
                _cancelRequested = false;
            }
        }
    }
}
