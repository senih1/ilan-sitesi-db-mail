using System.ComponentModel.DataAnnotations;

namespace ilan_sitesi.Models
{
    public class Ad
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public string Detail { get; set; }
        [Required]
        public IFormFile Image { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool isApproved { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string sellerName { get; set; }
        public string Email { get; set; }
    }
}
