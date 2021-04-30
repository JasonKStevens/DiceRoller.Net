using NUnit.Framework;

namespace PartyDSL.Test
{
    public class PartyBuilder
    {
        [Test]
        public void BuildImmortalsParty()
        {
            const string pName = "Lewts";

            var man = new PartyManager();
            var p = man.Create(pName);

            var naimon = p.AddMember("Naimon");
            p.AddMember("Glaukus", naimon);
            p.AddMember("Histion", naimon);

            var logan = p.AddMember("Logan-Kai");
            p.AddMember("Luke", logan);

            var muse = p.AddMember("Muse");
            p.AddMember("Evening", muse);

            var max = p.AddMember("Gautama");


        }
    }
}