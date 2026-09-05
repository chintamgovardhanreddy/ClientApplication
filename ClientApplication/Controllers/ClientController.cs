using ClientApplication.Data;
using ClientApplication.model;
using ClientApplication.model.Entity;
using ClientApplication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly SmsService _smsService;
        private readonly IWebHostEnvironment _environment;

        public ClientController(
            ApplicationDbContext context,
            IEmailService emailService,
            SmsService smsService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _emailService = emailService;
            _smsService = smsService;
            _environment = environment;
        }

        // ============================================================
        // GET: api/Client
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetAllClients()
        {
            var allClients = await _context.clients.ToListAsync();

            return Ok(allClients);
        }

        // ============================================================
        // GET: api/Client/{id}
        // ============================================================
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetClientById(Guid id)
        {
            var client = await _context.clients.FindAsync(id);

            if (client == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Client not found."
                });
            }

            return Ok(client);
        }

        // ============================================================
        // POST: api/Client
        // ============================================================
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddClient(
            [FromForm] AddClientDto request)
        {
            // ========================================================
            // VALIDATION
            // ========================================================

            if (string.IsNullOrWhiteSpace(request.ClientName))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Client name is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.ClientEmail))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Client email is required."
                });
            }

            // ========================================================
            // IMAGE UPLOAD - OPTIONAL
            // ========================================================

            string? imageUrl = null;

            if (request.Image != null && request.Image.Length > 0)
            {
                try
                {
                    imageUrl = await UploadImage(request.Image);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }
            }

            // ========================================================
            // CREATE CLIENT
            // ========================================================

            var client = new Client
            {
                ClientName = request.ClientName,
                ClientEmail = request.ClientEmail,
                PhoneNumber = request.PhoneNumber,

                // Languages
                Languages = request.Languages,

                // Gender
                Gender = request.Gender,

                // Image
                ImageUrl = imageUrl
            };

            _context.clients.Add(client);

            await _context.SaveChangesAsync();

            // ========================================================
            // EMAIL
            // ========================================================

            string body = $@"
Hello {client.ClientName},

Your registration has been completed successfully.

Name      : {client.ClientName}
Email     : {client.ClientEmail}
Phone     : {client.PhoneNumber}
Languages : {client.Languages}
Gender    : {client.Gender}

Thank you.

Regards,
Govardhanreddy Chintam";

            try
            {
                await _emailService.SendEmailAsync(
                    client.ClientEmail,
                    "Client Registration Successful",
                    body);
            }
            catch
            {
                // Ignore email errors
            }

            // ========================================================
            // SMS
            // ========================================================

            try
            {
                if (!string.IsNullOrWhiteSpace(client.PhoneNumber))
                {
                    await _smsService.SendSmsAsync(
                        client.PhoneNumber,
                        $"Hello {client.ClientName}, your registration has been completed successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SMS Error: " + ex.Message);
            }

            // ========================================================
            // RESPONSE
            // ========================================================

            return Ok(new
            {
                success = true,
                message = "Client saved successfully.",
                client
            });
        }

        // ============================================================
        // PUT: api/Client/{id}
        // ============================================================
        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateClient(
            Guid id,
            [FromForm] UpdateClientDto request)
        {
            try
            {
                // ====================================================
                // FIND CLIENT
                // ====================================================

                var client = await _context.clients.FindAsync(id);

                if (client == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Client not found."
                    });
                }

                // ====================================================
                // VALIDATION
                // ====================================================

                if (string.IsNullOrWhiteSpace(request.ClientName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Client name is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.ClientEmail))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Client email is required."
                    });
                }

                // ====================================================
                // UPDATE CLIENT DETAILS
                // ====================================================

                client.ClientName = request.ClientName;
                client.ClientEmail = request.ClientEmail;
                client.PhoneNumber = request.PhoneNumber;

                // Languages
                client.Languages = request.Languages;

                // Gender
                client.Gender = request.Gender;

                // ====================================================
                // IMAGE UPDATE - OPTIONAL
                // ====================================================

                if (request.Image != null && request.Image.Length > 0)
                {
                    // Upload new image first
                    string? newImageUrl =
                        await UploadImage(request.Image);

                    // Delete old image
                    DeleteImage(client.ImageUrl);

                    // Update database image URL
                    client.ImageUrl = newImageUrl;
                }

                // ====================================================
                // SAVE DATABASE
                // ====================================================

                await _context.SaveChangesAsync();

                // ====================================================
                // EMAIL
                // ====================================================

                string body = $@"
Hello {client.ClientName},

Your profile has been updated successfully.

Name      : {client.ClientName}
Email     : {client.ClientEmail}
Phone     : {client.PhoneNumber}
Languages : {client.Languages}
Gender    : {client.Gender}

Thank you.

Regards,
Govardhanreddy Chintam";

                try
                {
                    await _emailService.SendEmailAsync(
                        client.ClientEmail,
                        "Client Profile Updated",
                        body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        "Email Error: " + ex.Message);
                }

                // ====================================================
                // RESPONSE
                // ====================================================

                return Ok(new
                {
                    success = true,
                    message = "Client updated successfully.",
                    client = client
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("=================================");
                Console.WriteLine("UPDATE CLIENT ERROR");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("INNER ERROR:");
                Console.WriteLine(ex.InnerException?.ToString());
                Console.WriteLine("=================================");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while updating the client.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // ============================================================
        // DELETE: api/Client/{id}
        // ============================================================
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            var client = await _context.clients.FindAsync(id);

            if (client == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Client not found."
                });
            }

            // Delete image from wwwroot
            DeleteImage(client.ImageUrl);

            // Delete client from database
            _context.clients.Remove(client);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Client deleted successfully."
            });
        }

        // ============================================================
        // PATCH: api/Client/{id}
        // ============================================================
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> PatchClient(
            Guid id,
            [FromBody] Client request)
        {
            var client = await _context.clients.FindAsync(id);

            if (client == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Client not found."
                });
            }

            // ========================================================
            // PATCH CLIENT DETAILS
            // ========================================================

            client.ClientName =
                request.ClientName ?? client.ClientName;

            client.ClientEmail =
                request.ClientEmail ?? client.ClientEmail;

            client.PhoneNumber =
                request.PhoneNumber ?? client.PhoneNumber;

            // Languages
            if (request.Languages != null)
            {
                client.Languages = request.Languages;
            }

            // Gender
            if (!string.IsNullOrWhiteSpace(request.Gender))
            {
                client.Gender = request.Gender;
            }

            await _context.SaveChangesAsync();

            // ========================================================
            // EMAIL
            // ========================================================

            try
            {
                await _emailService.SendEmailAsync(
                    client.ClientEmail,
                    "Profile Updated",
                    $"Hi {client.ClientName},\n\nYour profile has been updated successfully.");
            }
            catch
            {
                // Ignore email errors
            }

            // ========================================================
            // RESPONSE
            // ========================================================

            return Ok(client);
        }

        // ============================================================
        // IMAGE UPLOAD
        // ============================================================
        private async Task<string?> UploadImage(IFormFile? image)
        {
            if (image == null || image.Length == 0)
            {
                return null;
            }

            // ========================================================
            // ALLOWED IMAGE EXTENSIONS
            // ========================================================

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".gif",
                ".webp"
            };

            string extension =
                Path.GetExtension(image.FileName)
                     .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException(
                    "Only JPG, JPEG, PNG, GIF and WEBP images are allowed.");
            }

            // ========================================================
            // SAFE WWWROOT PATH
            // ========================================================

            string webRootPath =
                _environment.WebRootPath
                ?? Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot");

            // ========================================================
            // UPLOAD FOLDER
            // ========================================================

            string uploadsFolder = Path.Combine(
                webRootPath,
                "uploads",
                "clients");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // ========================================================
            // UNIQUE FILE NAME
            // ========================================================

            string fileName =
                $"{Guid.NewGuid()}{extension}";

            string filePath =
                Path.Combine(
                    uploadsFolder,
                    fileName);

            // ========================================================
            // SAVE IMAGE
            // ========================================================

            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            // ========================================================
            // DATABASE IMAGE URL
            // ========================================================

            return $"/uploads/clients/{fileName}";
        }

        // ============================================================
        // DELETE IMAGE
        // ============================================================
        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            string fileName =
                Path.GetFileName(imageUrl);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            // ========================================================
            // SAFE WWWROOT PATH
            // ========================================================

            string webRootPath =
                _environment.WebRootPath
                ?? Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot");

            string filePath = Path.Combine(
                webRootPath,
                "uploads",
                "clients",
                fileName);

            // ========================================================
            // DELETE FILE
            // ========================================================

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}