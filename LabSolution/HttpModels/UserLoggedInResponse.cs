namespace LabSolution.HttpModels
{
    public class UserLoggedInResponse
    {
        public string Username { get; set; }
        public string Token { get; set; }
        public string Firstname { get; internal set; }
        public string Lastname { get; internal set; }
        public bool IsSuperUser { get; internal set; }
        public bool IsIpRestricted { get; internal set; }
    }
}
