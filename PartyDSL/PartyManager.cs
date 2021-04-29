using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PartyDSL
{
    public class PartyManager : IPartyManager
    {
        private Dictionary<string, Party> _parties = new Dictionary<string, Party>();

        public Party Create(string partyName)
        {
            if (_parties.ContainsKey(partyName.ToLower()))
                throw new InvalidOperationException($"{partyName} already exists");
            
            _parties.Add(partyName.ToLower(), new Party(partyName));
            return _parties[partyName.ToLower()];
        }

        public IEnumerable<Party> GetAll()
        {
            return _parties.Values;
        }

        public Party GetParty(string partyName)
        {
            if (!_parties.ContainsKey(partyName.ToLower()))
                return null;
            
            return _parties[partyName.ToLower()];
        }

        public Party Delete(string partyName)
        {
            if (!_parties.ContainsKey(partyName.ToLower()))
                return null;
            
            var result = _parties[partyName.ToLower()];
            _parties.Remove(partyName.ToLower());

            return result;
        }        

        public string Serialize()
        {
            return JsonSerializer.Serialize(_parties, new JsonSerializerOptions(){
                IncludeFields = true,
                ReferenceHandler = ReferenceHandler.Preserve
            });
        }

        public void Hydrate(string json)
        {
            _parties = JsonSerializer.Deserialize<Dictionary<string, Party>>(json, new JsonSerializerOptions() {
                ReferenceHandler = ReferenceHandler.Preserve,
                IncludeFields = true
            });
        }
    }
}