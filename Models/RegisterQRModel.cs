namespace OtpExample.Models
{
    public class RegisterQRModel
    {
        public string Email { get; set; }
        public string Key { get; set; }
        public string QRImageUrl { get; set; }
        public string ManualEntryKey { get; set; }
    }
}