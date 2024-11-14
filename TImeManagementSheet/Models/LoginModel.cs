namespace TImeManagementSheet.Models
{
    public class User
    {
        public int Slno {  get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public long MobileNo { get; set; }
        public DateTime LoginDate { get; set; }
    }
}
