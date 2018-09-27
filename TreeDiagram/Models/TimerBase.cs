using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Timers;

namespace TreeDiagram.Models
{
    public abstract class TimerBase : ITreeModel
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public ulong Id { get; internal set; }
        
        public ulong GuildId { get; set; }
        public ulong TextChannelId { get; set; }
        public DateTime TimerExpire { get; set; }

        [NotMapped] public Timer Timer { get; set; }
    }
}