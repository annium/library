namespace Backuper.Api.Config
{
    public class PlanConfiguration
    {
        public string Storage { get; set; }

        public string Interval { get; set; }

        public string[] Notifications { get; set; }
    }
}