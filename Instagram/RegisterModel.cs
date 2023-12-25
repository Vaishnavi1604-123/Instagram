using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Instagram_MVC.Models
{
    public class RegisterModel
    {
       
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
     
        public byte[] ImageData { get; set; }
    }
}
