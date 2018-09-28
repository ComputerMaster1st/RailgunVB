Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports MongoDB.Bson
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Music
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music

    Partial Public Class Music
    
        <Group("remove"), UserPerms(GuildPermission.ManageGuild)>
        Public Class MusicRemove
            Inherits ModuleBase
            
            Private ReadOnly _playerManager As PlayerManager
            Private ReadOnly _dbContext As TreeDiagramContext
            Private ReadOnly _musicService As MusicService

            Public Sub New(playerManager As PlayerManager, dbContext As TreeDiagramContext, 
                           musicService As MusicService)
                _playerManager = playerManager
                _dbContext = dbContext
                _musicService = musicService
            End Sub
            
            <Command, Priority(0)>
            Public Async Function RemoveAsync(id As String) As Task
                Dim data As ServerMusic = Await _dbContext.ServerMusics.GetAsync(Context.Guild.Id)
                
                If data Is Nothing OrElse data.PlaylistId = ObjectId.Empty
                    await ReplyAsync("Unknown Music Id Given!")
                    Return
                End If
                
                Dim playlist As Playlist = Await _musicService.GetPlaylistAsync(data.PlaylistId)
                
                If Not (playlist.Songs.Contains(id))
                    Await ReplyAsync("Unknown Music Id Given!")
                    Return
                End If
                
                playlist.Songs.Remove(id)
                
                Await playlist.SaveAsync()
                await ReplyAsync("Music removed from playlist.")
            End Function
            
            <Command("current"), Priority(1)>
            Public Async Function CurrentAsync() As Task
                Dim player As Player = _playerManager.GetPlayer(Context.Guild.Id)
                Dim data As ServerMusic = Await _dbContext.ServerMusics.GetAsync(Context.Guild.Id)
                
                If data Is Nothing OrElse data.PlaylistId = ObjectId.Empty OrElse player Is Nothing
                    Await ReplyAsync("Can not remove current song because I am not in voice channel.")
                    Return
                End If
                
                Dim playlist As Playlist = Await _musicService.GetPlaylistAsync(data.PlaylistId)
                
                playlist.Songs.Remove(player.GetFirstSongRequest())
                
                Await playlist.SaveAsync()
                await ReplyAsync("Removed from playlist. Skipping to next song...")
                
                player.CancelMusic()
            End Function
            
        End Class
        
    End Class
    
End NameSpace