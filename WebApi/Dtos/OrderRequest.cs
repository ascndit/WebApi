using System.ComponentModel.DataAnnotations;

namespace WebApi.Dtos
{
    public class OrderRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be greater than 0.")]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public int Amount { get; set; }

        public bool IsPaid { get; set; }
        }
}
