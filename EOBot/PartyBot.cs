// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Threading;
using EOLib.Net.API;

namespace EOBot
{
    /// <summary>
    /// Represents a bot that connects to a server and attempts to join the party of someone on the map
    /// </summary>
    class PartyBot : BotBase
    {
        private readonly string _name;
        private readonly Random _rand;
        private readonly Timer _speechTimer;

        public PartyBot(int botIndex, string host, int port, string name)
            : base(botIndex, host, port)
        {
            _name = name;

            _rand = new Random();
            _speechTimer = new Timer(SpeakYourMind, null, _rand.Next(15000, 30000), _rand.Next(15000, 30000));
        }

        protected override void DoWork(CancellationToken ct)
        {
            CharacterLoginData[] loginData;
            //WelcomeRequestData welcomeReqData;
            //WelcomeMessageData welcomeMsgData;

            var h = new BotHelper(_api, Console.WriteLine, _errorMessage);

            if (!h.CreateAccountIfNeeded(_name, _name) || ct.IsCancellationRequested) return;
            Thread.Sleep(500);

            if (!h.LoginToAccount(_name, _name, out loginData) || ct.IsCancellationRequested) return;
            Thread.Sleep(500);

            if (!h.CreateCharacterIfNeeded(_name, ref loginData) || ct.IsCancellationRequested) return;

            //if (!h.DoWelcomePacketsForFirstCharacter(loginData, out welcomeReqData, out welcomeMsgData) || ct.IsCancellationRequested) return;

            Console.WriteLine("{0} logged in and executing.", _name);

            //var charlist = welcomeMsgData.CharacterData.ToList();
            //int testuserNdx = charlist.FindIndex(_data => _data.Name.ToLower() == "testuser");
            //if (testuserNdx >= 0)
            //{
            //    PartyRequest(_api, charlist[testuserNdx].ID);
            //}
        }

        private void SpeakYourMind(object state)
        {
            //_api.Speak(ChatType.Local, _name + " standing by!");

            if (_speechTimer != null)
            {
                _speechTimer.Change(_rand.Next(15000, 30000), _rand.Next(15000, 30000));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            { //note: partybot will dispose of the event after the first one is disposed so it better not be used after that!
                //yes, this implementation sucks.
                if (s_partyEvent != null)
                {
                    s_partyEvent.Dispose();
                    s_partyEvent = null;
                }
                
                _speechTimer.Dispose();
            }

            base.Dispose(disposing);
        }

        private void _errorMessage(string msg = null)
        {
            if (msg == null)
            {
                Console.WriteLine("Error contacting server - server did not respond. Quitting {0}", _index);
                return;
            }

            Console.WriteLine("Error - {0}. Quitting {1}.", msg, _index);
        }

        private static AutoResetEvent s_partyEvent = new AutoResetEvent(true);
        private static void PartyRequest(PacketAPI api, short id)
        {
            s_partyEvent.WaitOne();
            Action<List<PartyMember>> action = _list => s_partyEvent.Set();

            api.OnPartyDataRefresh += action;
            api.OnPartyDataRefresh += t => api.OnPartyDataRefresh -= action;

            api.PartyRequest(PartyRequestType.Join, id);
        }
    }
}
