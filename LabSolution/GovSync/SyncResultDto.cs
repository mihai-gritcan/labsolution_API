using System.Collections.Generic;

namespace LabSolution.GovSync
{
    public class SyncResultDto
    {
        public SyncResultDto()
        {
            SynchedItems = new List<TestPushModel>();
            UnsynchedItems = new Dictionary<TestPushModel, string>();
        }
        public List<TestPushModel> SynchedItems { get; set; }
        public Dictionary<TestPushModel, string> UnsynchedItems { get; set; }
    }
}
