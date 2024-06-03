namespace LibraryManagementAPI.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime PublicationDate { get; set; }
        public string ISBN { get; set; }
        public int AuthorId { get; set; }
        public Author AuthorDetails { get; set; }
    }
}
