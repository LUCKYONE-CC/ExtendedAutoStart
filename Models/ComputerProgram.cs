using ExtendedAutoStart.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExtendedAutoStart.Models
{
    public class ComputerProgram
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
        public bool IsInAutoStart { get; set; }
        public StartUpType StartUpType { get; set; } = StartUpType.Unknown;
    }
}