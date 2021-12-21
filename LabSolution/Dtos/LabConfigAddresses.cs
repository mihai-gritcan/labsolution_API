namespace LabSolution.Dtos
{
    public class LabConfigAddresses
    {
        public string LabName { get; set; }
        public string LabAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string WebSiteAddress { get; set; }
        public string TestEquipmentAnalyzer { get; internal set; }
        public string DownloadPDFUrl { get; set; }
    }
}
