using Microsoft.AspNetCore.Http;

namespace ClientApplication.model
{
    public class AddClientDto
    {
        public string? ClientName { get; set; }

        public string? ClientEmail { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Languages { get; set; }

        public string? Gender { get; set; }

        public IFormFile? Image { get; set; }
    }
}