namespace LabSolution.Infrastructure
{
    public class GovSyncConfiguration
    {
        public bool IsSyncToGovEnabled { get; set; }
        public string LaboratoryId { get; set; }
        public string LaboratoryOfficeId { get; set; }
        public string ApiUrl { get; set; }
        public string LaboratoryAntigenDeviceIdentifier { get; set; }
    }
}
