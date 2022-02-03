namespace EOBot.Interpreter.Variables
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

        // state variables
        public const string ACCOUNT = "account";
        public const string CHARACTER = "character";
        public const string MAPSTATE = "mapstate";

        // interpreter functions
        public const string PRINT_FUNC = "print";
        public const string LEN_FUNC = "len";
        public const string ARRAY_FUNC = "array";
        public const string SLEEP = "sleep";
        public const string TIME = "time";

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
    }
}
