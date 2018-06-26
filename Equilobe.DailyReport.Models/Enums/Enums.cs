
namespace Equilobe.DailyReport.Models.Enums
{
    public enum SourceControlType { None, GitHub, SVN, BitBucket };
    public enum ErrorType { HasRemaining, HasNoTimeSpent, HasNoRemaining, NotConfirmed, NotFromSprint };
    public enum Health { Bad, Weak, Good, None };
    public enum SendScope { SendIndividualDraft, SendFinalDraft, SendReport };
}
