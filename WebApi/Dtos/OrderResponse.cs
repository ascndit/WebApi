namespace WebApi.Dtos
{
    public class OrderResponse
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public bool IsPaid { get; set; }

        public DateTime CreatedAt { get; set; }

        public CustomerInfoDto Customer { get; set; } = new();
    }

    public class CustomerInfoDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
