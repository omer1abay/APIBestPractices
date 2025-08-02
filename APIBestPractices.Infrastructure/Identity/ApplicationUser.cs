using Microsoft.AspNetCore.Identity;

namespace APIBestPractices.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public string FullName => $"{FirstName} {LastName}".Trim();
}