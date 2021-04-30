using FluentAssertions;
using Irony.Parsing;
using Moq;
using NUnit.Framework;
using PartyDSL.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PartyDSL.Test
{
    public class PartyCommandTests
    {
        [Test]
        public void Create_WillTryToAddAParty()
        {
            var managerMock = new Mock<IPartyManager>();
            managerMock
                .Setup(x => x.Create("test"))
                .Returns(new Party("test"))
                .Verifiable();

            var evaluator = new PartyCommandEvaluator(null, managerMock.Object);

            var actual = evaluator.Evaluate("!party", "create test");

            actual.Value.Should().Be("Created.");

            managerMock.VerifyAll();
        }

        [Test]
        public void List_WillAskForAListOfParties()
        {
            var parties = new List<Party>(){
                new Party("Party1"),
                new Party("Party2"),
                new Party("Party3"),
            };
            
            var managerMock = new Mock<IPartyManager>();
            managerMock
                .Setup(x => x.GetAll())
                .Returns(parties)
                .Verifiable();

            var evaluator = new PartyCommandEvaluator(null, managerMock.Object);

            var actual = evaluator.Evaluate("!party", "list");

            actual.Value.Should().Be(string.Join(Environment.NewLine, parties.Select(x => x.Name)));

            managerMock.VerifyAll();
        }

        [Test]
        public void Create_WillTryToDeleteParty()
        {
            var managerMock = new Mock<IPartyManager>();
            managerMock
                .Setup(x => x.Delete("test"))
                .Returns(new Party("test"))
                .Verifiable();

            var evaluator = new PartyCommandEvaluator(null, managerMock.Object);

            var actual = evaluator.Evaluate("!party", "delete test");

            actual.Value.Should().Be("test deleted.");

            managerMock.VerifyAll();
        }

        [Test]
        public void AddMember_WillAddWithNoMaster()
        {
            var party = new Party("p");

            var managerMock = new Mock<IPartyManager>();
            managerMock
                .Setup(x => x.GetParty("p"))
                .Returns(party);

            var evaluator = new PartyCommandEvaluator(null, managerMock.Object);

            var actual = evaluator.Evaluate("!p", "add member");

            actual.Value.Should().Be("member added.");
            var member = party.GetMember("member");
            member.Should().NotBeNull();
            member.Name.Should().Be("member");
            member.Master.Should().BeNull();
        }

        [Test]
        public void AddMember_WillAddWithMaster()
        {
            var party = new Party("p");
            var master = party.AddMember("master");

            var managerMock = new Mock<IPartyManager>();
            managerMock
                .Setup(x => x.GetParty("p"))
                .Returns(party);

            var evaluator = new PartyCommandEvaluator(null, managerMock.Object);

            var actual = evaluator.Evaluate("!p", "add member as an ally of master");

            actual.Value.Should().Be("member added.");
            var member = party.GetMember("member");
            member.Should().NotBeNull();
            member.Name.Should().Be("member");
            member.Master.Should().Be(master);
        }

        [Test]
        public void RemoveMember_WillRemoveTheMember()
        {
            var party = new Party("p");
            var master = party.AddMember("member");

            var managerMock = new Mock<IPartyManager>();
            managerMock
                .Setup(x => x.GetParty("p"))
                .Returns(party);

            var evaluator = new PartyCommandEvaluator(null, managerMock.Object);

            var actual = evaluator.Evaluate("!p", "remove member");

            actual.Value.Should().Be("member removed.");
            var member = party.GetMember("member");
            member.Should().BeNull();
        }        

        [Test]
        public void AAA()
        {
            var sut = new PartyManager();

            var p1 = sut.Create("party1");
            var p1m1 = p1.AddMember("p1m1");
            p1.AddMember("p1m2");
            var p3 = p1.AddMember("p1m3");
            var p4 = p1.AddMember("p1m4", p1m1);

            p3.SetRoll("init", "d100!+35");
            p4.SetRoll("init", "d100!+15");
            
            var p2 = sut.Create("party2");
            var p2m1 = p2.AddMember("p2m1");
            p2.AddMember("p2m2");
            p2.AddMember("p2m3");
            p2.AddMember("p2m4", p2m1);
            
            var sv = sut.Serialize();

            var sut2 = new PartyManager();
            sut2.Hydrate(sv);
        }
    }
}