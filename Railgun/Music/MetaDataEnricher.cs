﻿using AudioChord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Railgun.Music
{
    public class MetaDataEnricher : IAudioMetadataEnricher
    {
        private Dictionary<SongId, (string Username, string Title)> _mapping = new Dictionary<SongId, (string, string)>();

        public void AddMapping(string user, SongId id, string title)
        {
            if (!_mapping.ContainsKey(id))
                _mapping.Add(id, (user, title));
        }

        public Task<ISong> EnrichAsync(ISong song)
        {
            song.Metadata.Uploader = _mapping.GetValueOrDefault(song.Metadata.Id).Username;
            song.Metadata.Title = _mapping.GetValueOrDefault(song.Metadata.Id).Title;
            _mapping.Remove(song.Metadata.Id);
            return Task.FromResult(song);
        }
    }
}
