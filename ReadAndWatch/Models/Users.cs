using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReadAndWatch.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Required]
        [Column("Username")]
        public string Username { get; set; }


        [Required]
        [Column("Surname")]
        public string Surname { get; set; }

        [Required]
        [Column("Email")]
        public string Email { get; set; }

        [Required]
        [Column("Password")]
        public string Password { get; set; }

        public ICollection<FavoriteBook> FavoriteBooks { get; set; }
        public ICollection<FavoriteMovie> FavoriteMovies { get; set; }


    }
}
