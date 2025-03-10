namespace Domain.News
{
    public class SimpleNews
    {
        public string title { get; set; } = "Hello World";
        public string image { get; set; } = "";
        public string description { get; set; } = "Lorem ipsum";

        public SimpleNews(string title, string image, string description)
        {
            this.title = title;
            this.image = image;
            this.description = description;
        }

        public static SimpleNews GetEmpty()
        {
            return new SimpleNews("Hello World", "", "Lorem ipsum");
        }
    }
}
