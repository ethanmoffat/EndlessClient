// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.API;

namespace EOBot
{
    delegate void DisplayMessageFunc(string message = "");

    class BotHelper
    {
        private readonly PacketAPI _api;
        private readonly DisplayMessageFunc _outputFunc;
        private readonly DisplayMessageFunc _errorMessage;

        public BotHelper(PacketAPI api, DisplayMessageFunc outputFunc, DisplayMessageFunc errorMessageFunc)
        {
            _api = api;
            _outputFunc = outputFunc;
            _errorMessage = errorMessageFunc;
        }

        public bool CreateAccountIfNeeded(string name, string password)
        {
            //AccountReply accReply;
            //bool res = _api.AccountCheckName(name, out accReply);
            //if (res && accReply != AccountReply.Exists)
            //{
            //    if (_api.AccountCreate(name, password, name + " " + name, "COMPY-" + name, name + "@BOT.COM",
            //        new HDSerialNumberService().GetHDSerialNumber(), out accReply))
            //    {
            //        _outputFunc(string.Format("Created account {0}", name));
            //    }
            //    else
            //    {
            //        _errorMessage();
            //        return false;
            //    }
            //}
            //else if (!res)
            //{
            //    _errorMessage();
            //    return false;
            //}

            return true;
        }

        public bool LoginToAccount(string name, string password, out CharacterLoginData[] loginData)
        {
            //LoginReply loginReply;
            //var res = _api.LoginRequest(name, password, out loginReply, out loginData);
            //if (!res)
            //{
            //    _errorMessage();
            //    return false;
            //}
            //if (loginReply != LoginReply.Ok)
            //{
            //    _errorMessage("Login reply was invalid");
            //    return false;
            //}
            loginData = new CharacterLoginData[0];
            return true;
        }

        public bool CreateCharacterIfNeeded(string name, ref CharacterLoginData[] loginData)
        {
            //if (loginData == null || loginData.Length == 0)
            //{
            //    CharacterReply charReply;
            //    var res = _api.CharacterRequest(out charReply);

            //    if (!res || charReply != CharacterReply.Ok)
            //    {
            //        _errorMessage("Character create request failed");
            //        return false;
            //    }

            //    var rand = new Random();

            //    res = _api.CharacterCreate((byte)rand.Next(1), (byte)rand.Next(0, 20), (byte)rand.Next(0, 9), (byte)rand.Next(0, 5),
            //        name, out charReply, out loginData);
            //    if (!res || charReply != CharacterReply.Ok || loginData == null || loginData.Length == 0)
            //    {
            //        _errorMessage("Character create failed");
            //        return false;
            //    }

            //    _outputFunc(string.Format("Created character {0}", name));

            //    Thread.Sleep(500);
            //}
            return true;
        }

        //public bool DoWelcomePacketsForFirstCharacter(CharacterLoginData[] loginData, out WelcomeRequestData welcomeReqData, out WelcomeMessageData welcomeMsgData)
        //{
        //    welcomeMsgData = null;
        //    welcomeReqData = null;

        //    var res = _api.SelectCharacter(loginData[0].ID, out welcomeReqData);
        //    if (!res)
        //    {
        //        _errorMessage();
        //        return false;
        //    }

        //    Thread.Sleep(500);

        //    res = _api.WelcomeMessage(welcomeReqData.ActiveCharacterID, out welcomeMsgData);
        //    if (!res)
        //    {
        //        _errorMessage();
        //        return false;
        //    }
        //    return true;
        //}
    }
}
