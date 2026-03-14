namespace Interfaces.DTO
{
    /// <summary>
    /// Progress of the ETL (load PGN → parse → persist). Used for progress bar / polling.
    /// </summary>
    public class EtlProgress
    {
        /// <summary>0-based index of the current PGN file.</summary>
        public int CurrentFileIndex { get; set; }

        /// <summary>Total number of PGN files to process.</summary>
        public int TotalFiles { get; set; }

        /// <summary>Name of the current PGN file.</summary>
        public string? CurrentFileName { get; set; }

        /// <summary>1-based index of the current game within the current file.</summary>
        public int CurrentGameIndex { get; set; }

        /// <summary>Total number of games in the current file.</summary>
        public int TotalGamesInCurrentFile { get; set; }

        /// <summary>Total games persisted so far in this run.</summary>
        public int TotalGamesProcessed { get; set; }

        /// <summary>Running | Completed | Failed</summary>
        public string Status { get; set; } = "Running";

        /// <summary>Optional message (e.g. error text when Failed).</summary>
        public string? Message { get; set; }

        /// <summary>Rough 0–100 percent when determinable (e.g. by file progress).</summary>
        public int? PercentComplete { get; set; }
    }
}
