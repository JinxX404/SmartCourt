using Microsoft.AspNetCore.Identity;

namespace SmartCourt.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<UserVerificationDocument> VerificationDocuments { get; set; }
            = new List<UserVerificationDocument>();
    }
}
