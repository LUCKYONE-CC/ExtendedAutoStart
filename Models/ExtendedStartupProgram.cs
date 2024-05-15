using System.ComponentModel.DataAnnotations;

namespace ExtendedAutoStart.Models
{
    public class ExtendedStartupProgram
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public bool Activated { get; set; }
    }
}