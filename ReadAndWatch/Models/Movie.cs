using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadAndWatch.Models
{
    [Table("movies")]
    public class Movie
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("genres")]
        public string? Genres { get; set; }

        [Column("original_language")]
        public string? OriginalLanguage { get; set; }

        [Column("overview")]
        public string? Overview { get; set; }

        [Column("popularity")]
        public double? Popularity { get; set; }

        [Column("release_date")]
        public DateTime? ReleaseDate { get; set; }

        [Column("revenue")]
        public long? Revenue { get; set; }

        [Column("runtime")]
        public double? Runtime { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("vote_average")]
        public double? VoteAverage { get; set; }

        [Column("vote_count")]
        public int? VoteCount { get; set; }

        [Column("director")]
        public string? Director { get; set; }

        public ICollection<FavoriteMovie> FavoriteMovies { get; set; }
    }
}
