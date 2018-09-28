Imports System.IO
Imports System.Text
Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports MongoDB.Bson
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Music
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music
    
    <Group("music")>
    Partial Public Class Music
        Inherits ModuleBase
        
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _playerManager As PlayerManager
        Private ReadOnly _dbContext As TreeDiagramContext
        Private ReadOnly _musicService As MusicService

        Public Sub New(config As MasterConfig, playerManager As PlayerManager, dbContext As TreeDiagramContext, 
                       musicService As MusicService)
            _config = config
            _playerManager = playerManager
            _dbContext = dbContext
            _musicService = musicService
        End Sub
        
        <Command("join"), BotPerms(GuildPermission.Connect And GuildPermission.Speak)>
        Public Async Function JoinAsync() As Task
            if _playerManager.IsCreated(Context.Guild.Id)
                await ReplyAsync($"Sorry, I'm already in a voice channel. If you're experiencing problems, please do {Format.Code($"{_config.DiscordConfig.Prefix}music reset stream.")}")
                Return
            End If
            
            Dim user As IGuildUser = Context.User
            Dim vc As IVoiceChannel = user.VoiceChannel
            
            If vc Is Nothing
                await ReplyAsync("Please go into a voice channel before inviting me.")
                Return
            End If
            
            Await _playerManager.CreatePlayerAsync(user, vc, Context.Channel)
        End Function
        
        <Command("leave")>
        Public Async Function LeaveAsync() As Task
            If Not (_playerManager.IsCreated(Context.Guild.Id))
                await ReplyAsync("I'm not streaming any music at this time.")
                Return
            End If
            
            await ReplyAsync("Stopping Music Stream...")
            _playerManager.DisconnectPlayer(Context.Guild.Id)
        End Function
        
        <Command("playlist"), BotPerms(ChannelPermission.AttachFiles)>
        Public Async Function PlaylistAsync() As Task
            Dim data As ServerMusic = Await _dbContext.ServerMusics.GetAsync(Context.Guild.Id)
            
            If data Is Nothing OrElse data.PlaylistId = ObjectId.Empty
                await ReplyAsync("Server playlist is currently empty.")
                Return
            End If
            
            Dim playlist As Playlist = Await _musicService.GetPlaylistAsync(data.PlaylistId)
            
            If playlist Is Nothing OrElse playlist.Songs.Count < 1
                await ReplyAsync("Server playlist is currently empty.")
                Return
            End If
            
            Dim output As new StringBuilder
            
            output.AppendFormat("{0} Music Playlist!", Context.Guild.Name).AppendLine() _ 
                .AppendFormat("Total Songs : {0}", playlist.Songs.Count).AppendLine() _
                .AppendLine()
            
            For Each id As String in playlist.Songs
                Dim song As Song = Await _musicService.GetSongAsync(id)
                
                output.AppendFormat("--       Id =>", song.Id).AppendLine() _
                    .AppendFormat("--     Name => {0}", song.Metadata.Name).AppendLine() _
                    .AppendFormat("--   Length => {0}", song.Metadata.Length).AppendLine() _
                    .AppendFormat("--      Url => {0}", song.Metadata.Url).AppendLine() _
                    .AppendFormat("-- Uploader => {0}", song.Metadata.Uploader).AppendLine() _
                    .AppendLine()
            Next
            
            output.AppendLine("End of Playlist.")
            
            Dim filename As String = $"{Context.Guild.Name} Playlist.txt"
            
            await File.WriteAllTextAsync(filename, output.ToString())
            await Context.Channel.SendFileAsync(filename, $"{Context.Guild.Name} Music Playlist ({playlist.Songs.Count} songs)")
            File.Delete(filename)
        End Function
        
        <Command("repeat")>
        Public Async Function RepeatAsync() As Task
            Dim player As Player = _playerManager.GetPlayer(Context.Guild.Id)
            
            If player Is Nothing
                await ReplyAsync("I'm not playing anything at this time.")
                Return
            End If
            
            player.RepeatSong = True
            await ReplyAsync("Repeating song after finishing.")
        End Function
        
        <Command("np")>
        Public Async Function NowPlayingAsync() As Task
            Dim player As Player = _playerManager.GetPlayer(Context.Guild.Id)
            
            If player Is Nothing
                await ReplyAsync("I'm not playing anything at this time.")
                Return
            End If
            
            Dim meta As SongMetadata = (Await _musicService.GetSongAsync(player.GetFirstSongRequest())).Metadata
            Dim output As New StringBuilder
            
            output.AppendFormat("Currently playing {0} at the moment.", Format.Bold(meta.Name)).AppendLine() _
                .AppendFormat("Url: {0} || Length: {1}", Format.Bold($"<{meta.Url}>"), Format.Bold(meta.Length.ToString()))
            
            await ReplyAsync(output.ToString())
        End Function
        
        <Command("repo"), BotPerms(ChannelPermission.AttachFiles)>
        Public Async Function RepositoryAsync() As Task
            Dim repo = (Await _musicService.GetAllSongsAsync()).ToList()
            Dim output As New StringBuilder
            
            output.AppendLine("Railgun Music Repository!") _
                .AppendFormat("Total Songs : {0}", repo.Count()).AppendLine() _
                .AppendLine()
            
            For Each song As Song in repo
                output.AppendFormat("--       Id =>", song.Id).AppendLine() _
                    .AppendFormat("--     Name => {0}", song.Metadata.Name).AppendLine() _
                    .AppendFormat("--   Length => {0}", song.Metadata.Length).AppendLine() _
                    .AppendFormat("--      Url => {0}", song.Metadata.Url).AppendLine() _
                    .AppendFormat("-- Uploader => {0}", song.Metadata.Uploader).AppendLine() _
                    .AppendLine()
            Next
            
            output.AppendLine("End of Repository.")
            
            Const filename = "MusicRepo.txt"
            
            await File.WriteAllTextAsync(filename, output.ToString())
            await Context.Channel.SendFileAsync(filename, $"Music Repository ({repo.Count()} songs)")
            File.Delete(filename)
        End Function
        
        <Command("ping")>
        Public Async Function PingAsync() As Task
            Dim player As Player = _playerManager.GetPlayer(Context.Guild.Id)
            
            Await ReplyAsync(If(player Is Nothing, "Can not check ping due to not being in voice channel.", 
                                $"Ping to Discord Voice: {Format.Bold(player.Latency.ToString())}ms"))
        End Function
        
        <Command("queue")>
        Public Async Function QueueAsync() As Task
            Dim player As Player = _playerManager.GetPlayer(Context.Guild.Id)
            
            If player Is Nothing
                await ReplyAsync("I'm not playing anything at this time.")
                Return
            ElseIf player.Requests.Count < 2
                await ReplyAsync("There are currently no music requests in the queue.")
                Return
            End If
            
            Dim output As New StringBuilder
            
            output.AppendFormat(Format.Bold("Queued Music Requests ({0}) :"), (player.Requests.Count - 1)).AppendLine() _
                .AppendLine()
            
            Dim i = 0
            While player.Requests.Count > i
                Dim songId As String = player.Requests(i)
                Dim meta As SongMetadata = (Await _musicService.GetSongAsync(songId)).Metadata
                
                output.AppendFormat("{0} : {1} || Length : {2}", If(i = 0, "Next", Format.Code($"[{i}]")), 
                                    Format.Bold(meta.Name), Format.Bold(meta.Length.ToString())).AppendLine()
                
                i += 1
            End While
            
            await ReplyAsync(output.ToString())
        End Function
        
        <Command("show")>
        Public Async Function ShowAsync() As Task
            Dim data As ServerMusic = Await _dbContext.ServerMusics.GetAsync(Context.Guild.ID)
            Dim songCount = 0
            
            If data Is Nothing
                await ReplyAsync("There are no settings available for Music.")
                Return
            ElseIf data.PlaylistId <> ObjectId.Empty
                Dim playlist As Playlist = Await _musicService.GetPlaylistAsync(data.PlaylistId)
                songCount = playlist.Songs.Count
            End If
            
            Dim vc As IVoiceChannel = If(data.AutoVoiceChannel <> 0, 
                                         await Context.Guild.GetVoiceChannelAsync(data.AutoVoiceChannel), Nothing)
            Dim tc As ITextChannel = If(data.AutoTextChannel <> 0, 
                                        await Context.Guild.GetTextChannelAsync(data.AutoTextChannel), Nothing)
            Dim output As New StringBuilder
            
            output.AppendLine("Music Settings") _
                .AppendLine() _
                .AppendFormat("Number Of Songs : {0}", songCount).AppendLine() _
                .AppendLine() _
                .AppendFormat("      Auto-Join : {0} {1}", If(vc IsNot Nothing, vc.Name, "Disabled"), 
                              If(tc IsNot Nothing, $"(#{tc.Name})", "")).AppendLine() _
                .AppendFormat("  Auto-Download : {0}", If(data.AutoDownload, "Enabled", "Disabled")).AppendLine() _
                .AppendFormat("      Auto-Skip : {0}", If(data.AutoSkip, "Enabled", "Disabled")).AppendLine() _
                .AppendLine() _
                .AppendFormat(" Silent Running : {0}", If(data.SilentNowPlaying, "Enabled", "Disabled")).AppendLine() _
                .AppendFormat(" Silent Install : {0}", If(data.SilentSongProcessing, "Enabled", "Disabled")).AppendLine()
            
            await ReplyAsync(Format.Code(output.ToString()))
        End Function
        
    End Class
    
End NameSpace