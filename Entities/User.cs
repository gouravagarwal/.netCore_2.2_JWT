using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Gender { get; set; }
        public string Password { get; set; }

        [NotMapped]
        public string Token { get; set; }
    }
}