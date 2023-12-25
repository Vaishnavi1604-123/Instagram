using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace Instagram_MVC.Models
{
    public class PostModel
    {
        public int userId { get; set; }
        public int PostId { get; set; }
        public string Text { get; set; }
        [DisplayName("Image")]
        [Required(ErrorMessage = "Please select an image.")]
        [DataType(DataType.Upload)]
        public IFormFile ImageFile { get; set; }
        public byte[] ImageData { get; set; }
        public DateTime PostDate { get; set; }

    }
}
