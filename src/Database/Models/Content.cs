using System.ComponentModel.DataAnnotations;

namespace VRtask.Database.Models
{
    public sealed class Content
    {
        [Key]
        public string PoNumber { get; set; }

        [Required]
        public string Isbn { get; set; }

        public int Quantity { get; set; }
    }
}