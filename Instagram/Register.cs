using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Instagram_MVC.Models
{
    public class Register
    {
       
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        [DisplayName("Image")]
        [Required(ErrorMessage = "Please select an image.")]
        [DataType(DataType.Upload)]
        public IFormFile ImageFile { get; set; }
        public byte[] ImageData { get; set; }
    }
}
