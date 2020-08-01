using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TreeDiagram.Models.SubModels
{
    public class MusicAutoJoinConfig
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong VoiceChannelId { get; private set; }
        public ulong TextChannelId { get; set; }

        public MusicAutoJoinConfig(ulong voiceChannelId, ulong textChannelId)
        {
            VoiceChannelId = voiceChannelId;
            TextChannelId = textChannelId;
        }
    }
}