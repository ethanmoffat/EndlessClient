namespace EOLib.Domain.Login
{
    public enum CharacterLoginReply : short
    {
        RequestGranted = 1, //response from welcome_request
        RequestCompleted = 2, //response from welcome_message
    }
}