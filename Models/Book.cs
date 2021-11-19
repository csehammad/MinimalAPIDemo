

namespace APIDemo.Models
{
    public class Book
    {
        
        [Key]
        public int BookID { get; set; }
        public string Title { get; set; }

        public int Year { get; set; }
        public long ISBN { get; set; }
        public DateTime PublishedDate { get; set; }
        public short Price { get; set; }

        public int AuthorID { get; set; }
    }
    class BooksDB : DbContext
    {
        public BooksDB(DbContextOptions<BooksDB> options) : base(options) { }
        public DbSet<Book> Books => Set<Book>();
    }
}
