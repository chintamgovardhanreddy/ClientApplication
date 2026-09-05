namespace ClientApplication.model.Entity
{
    public class Client
    {
        public Guid ClientId { get; set; }

        public string? ClientName { get; set; }

        public string? ClientEmail { get; set; }

        public string? PhoneNumber { get; set; }

        public string? ImageUrl { get; set; }

        public string? Languages { get; set; }

        public string? Gender { get; set; }
    }
}