namespace Instagram_MVC.Models
{
    public class ViewPostsModel
    {
        public int id { get; set; }
        public string Username { get; set; }
        public byte[] ImageData { get; set; }
        public DateTime DateTime { get; set; }
        public byte[] Profilepic { get; set; }

        public string Text { get; set; } // For image posts
        public int Likes { get; set; }

    }
}
