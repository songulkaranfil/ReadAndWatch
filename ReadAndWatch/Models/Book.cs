using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadAndWatch.Models
{
    [Table("books")]
    public class Book
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("subtitle")]
        public string? Subtitle { get; set; }

        [Column("authors")]
        public string? Authors { get; set; }

        [Column("categories")]
        public string? Categories { get; set; }

        [Column("thumbnail")]
        public string? Thumbnail { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("published_year")]
        public double? PublishedYear { get; set; }

        [Column("average_rating")]
        public double? AverageRating { get; set; }

        [Column("num_pages")]
        public double? NumPages { get; set; }

        [Column("ratings_count")]
        public double? RatingsCount { get; set; }


        public ICollection<FavoriteBook> FavoriteBooks { get; set; }



    }
}
