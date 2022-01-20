using System.Collections.Generic;

namespace LabSolution.GovSync
{
    public class SyncResultDto
    {
        public SyncResultDto()
        {
            SynchedItems = new List<TestPushModel>();
            UnsynchedItems = new List<KeyValuePair<TestPushModel, string>>();
        }
        public List<TestPushModel> SynchedItems { get; set; }
        public List<KeyValuePair<TestPushModel, string>> UnsynchedItems { get; set; }
    }
}
