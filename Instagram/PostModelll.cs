using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Instagram_MVC.Models
{
    public class PostModelll
    {
        public int userId { get; set; }
        public string Text { get; set; }
        public byte[] ImageData { get; set; }
        public DateTime PostDate { get; set; }

    }
}
