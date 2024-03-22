using System.ComponentModel.DataAnnotations.Schema;


namespace hushazvillany_backend.Models
{
    public class Messages
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? MessageContent { get; set; }
        public Boolean isReplied { get; set; } = false;
        public string? ReplyContent { get; set; }    
    }
}
