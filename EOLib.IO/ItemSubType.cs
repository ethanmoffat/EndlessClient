namespace EOLib.IO
{
    public enum ItemSubType : byte
    {
        None,
        Ranged,
        Arrows,
        Wings,
        //The following 2 values require modded pubs in order to work properly.
        FaceMask, //ADDED: *this* client will interpret this value as a hat/mask, so all hair should be shown (ie frog head/dragon mask)
        HideHair //ADDED: *this* client will interpret this value as a hat/helmet, so no hair should be shown (ie helmy, H.O.D., etc)
    }
}