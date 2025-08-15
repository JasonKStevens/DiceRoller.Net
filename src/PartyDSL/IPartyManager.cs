using System.Collections.Generic;

namespace PartyDSL
{
    public interface IPartyManager
    {
        Party Create(string partyName);
        Party GetParty(string partyName);
        IEnumerable<Party> GetAll();

        Party Delete(string partyName);


        string Serialize();
        void Hydrate(string json);
    }
}