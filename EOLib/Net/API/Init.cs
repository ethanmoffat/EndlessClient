using EOLib.Net.Handlers;
using System;
using System.IO;
using System.Threading;

namespace EOLib.Net.API
{
    public enum InitReply : byte
    {
        INIT_OUT_OF_DATE = 1,
        INIT_OK = 2,
        INIT_BANNED = 3,
        INIT_FILE_MAP = 4,
        INIT_FILE_EIF = 5,
        INIT_FILE_ENF = 6,
        INIT_FILE_ESF = 7,
        INIT_PLAYERS = 8,
        INIT_MAP_MUTATION = 9,
        INIT_FRIEND_LIST_PLAYERS = 10,
        INIT_FILE_ECF = 11,
        THIS_IS_WRONG = 0
    }

    public enum PaperdollIconType
    {
        Normal = 0,
        GM = 4,
        HGM = 5,
        Party = 6,
        GMParty = 9,
        HGMParty = 10,
        SLNBot = 20
    }

    public partial class PacketAPI
    {
        private AutoResetEvent m_init_responseEvent;

        //shared between API calls and response handler
        private int m_init_requestedMap;

        public event Action OnMapMutation;

        private void _createInitMembers()
        {
            m_init_responseEvent = new AutoResetEvent(false);

            m_init_requestedMap = 0;

            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Init, PacketAction.Init), _handleInitInit, false);
        }

        private void _disposeInitMembers()
        {
            if (m_init_responseEvent != null)
            {
                m_init_responseEvent.Dispose();
                m_init_responseEvent = null;
            }
        }

        private void _handleInitInit(OldPacket pkt)
        {
            InitReply response = (InitReply)pkt.GetByte();
            switch (response)
            {
                case InitReply.INIT_MAP_MUTATION:
                {
                    string localDir = response == InitReply.INIT_FILE_MAP || response == InitReply.INIT_MAP_MUTATION ? "maps" : "pub";

                    if (response == InitReply.INIT_MAP_MUTATION)
                        m_init_requestedMap = 0;

                    if (!Directory.Exists(localDir))
                        Directory.CreateDirectory(localDir);

                    string filename;
                    if (response == InitReply.INIT_FILE_EIF)
                        filename = "dat001.eif";
                    else if (response == InitReply.INIT_FILE_ENF)
                        filename = "dtn001.enf";
                    else if (response == InitReply.INIT_FILE_ESF)
                        filename = "dsl001.esf";
                    else if (response == InitReply.INIT_FILE_ECF)
                        filename = "dat001.ecf";
                    else
                        filename = $"{m_init_requestedMap,5:D5}.emf";

                    using (FileStream fs = File.Create(Path.Combine(localDir, filename)))
                    {
                        int dataLen = pkt.Length - 3;
                        if (dataLen == 0)
                            return; //trigger error by not setting response event
                        fs.Write(pkt.GetBytes(dataLen), 0, dataLen);
                    }

                    if (response == InitReply.INIT_MAP_MUTATION && OnMapMutation != null)
                    {
                        OnMapMutation();
                    }
                }
                    break;
            }

            m_client.ExpectingFile = false;
            m_init_responseEvent.Set(); //packet was handled
        }
    }
}
