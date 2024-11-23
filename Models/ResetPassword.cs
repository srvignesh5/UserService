namespace UserService.Models
{
    public class ResetPassword
    {
        public required string Email { get; set; }
        public required string NewPassword { get; set; }
    }
}
