using System;
using System.ComponentModel.DataAnnotations;

namespace FGA.Models
{
    public class Message
    {
        [Key()]
        public int Id { get; set; }

        public string Content { get; set; } 

        public DateTime Time { get; set; } 

        public string FromUser { get; set; }

        public string ToUser { get; set; } 
    }
}
