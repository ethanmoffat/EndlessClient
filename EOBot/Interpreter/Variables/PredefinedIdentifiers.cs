﻿namespace EOBot.Interpreter.Variables
{
    public static class PredefinedIdentifiers
    {
        // variables
        public const string RESULT = "result";
        public const string HOST = "host";
        public const string PORT = "port";
        public const string USER = "user";
        public const string PASS = "pass";
        public const string VERSION = "version";
        public const string ARGS = "args";
        public const string BOTINDEX = "botindex";
        public const string RETCODE = "retcode";

        // state variables
        public const string ACCOUNT = "account";
        public const string CHARACTER = "character";
        public const string MAPSTATE = "mapstate";

        public const string NAME = "name";
        public const string CHARACTERS = "characters";

        // interpreter functions
        public const string PRINT_FUNC = "print";
        public const string LEN_FUNC = "len";
        public const string ARRAY_FUNC = "array";
        public const string OBJECT_FUNC = "object";
        public const string SLEEP_FUNC = "sleep";
        public const string TIME_FUNC = "time";
        public const string SETENV_FUNC = "setenv";
        public const string GETENV_FUNC = "getenv";
        public const string ERROR_FUNC = "error";
        public const string LOWER_FUNC = "lower";
        public const string UPPER_FUNC = "upper";

        // game functions
        public const string CONNECT_FUNC = "Connect";
        public const string DISCONNECT_FUNC = "Disconnect";
        public const string CREATE_ACCOUNT_FUNC = "CreateAccount";
        public const string LOGIN_FUNC = "Login";
        public const string CREATE_AND_LOGIN_FUNC = "CreateAndLogin";
        public const string CHANGE_PASS_FUNC = "ChangePassword";
        public const string CREATE_CHARACTER_FUNC = "CreateCharacter";
        public const string LOGIN_CHARACTER_FUNC = "LoginCharacter";
        public const string DELETE_CHARACTER_FUNC = "DeleteCharacter";

        public const string JOIN_PARTY = "JoinParty";
        public const string CHAT = "Chat";
    }
}
