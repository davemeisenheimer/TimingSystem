namespace TrailMeisterViewer.Windows.EventViewer
{
    public enum ResultSortField { TotalTime, BestLap, AverageLap, TotalLaps }

    public class ExportOptions
    {
        public ResultSortField SortBy { get; set; } = ResultSortField.TotalTime;
        public bool PruneEarlyLaps { get; set; }   // remove leading laps so all racers have equal count
        public bool PruneLastLaps { get; set; }    // remove trailing laps so all racers have equal count
        public bool IncludeRanking { get; set; } = true;
        public bool IncludeRawResults { get; set; } = true;
        public bool IncludeHandicapResults { get; set; }
    }
}
