using System.ComponentModel.DataAnnotations;

namespace VRtask.Database.Models
{
    public sealed class Box
    {
        [Required]
        public string SupplierIdentifier { get; set; }

        [Key]
        public string Identifier { get; set; }

        public List<Content> Contents { get; set; }
    }
}