namespace Equilobe.DailyReport.Models.BitBucket
{
    public class SourceOrDestination
    {
        public Commit Commit { get; set; }
        public Repository Repository { get; set; }
        public Branch Branch { get; set; }
    }
}
