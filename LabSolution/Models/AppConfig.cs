using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LabSolution.Models
{
    [Index(nameof(Key), Name = "IX_AppConfig_Key", IsUnique = true)]
    public class AppConfig
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}

