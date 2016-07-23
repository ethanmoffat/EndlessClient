// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading;
using EOLib.Domain.Account;
using EOLib.Localization;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        private AutoResetEvent m_account_responseEvent;

        private AccountReply m_account_reply;

        private void _createAccountMembers()
        {
            m_account_responseEvent = new AutoResetEvent(false);
            m_account_reply = AccountReply.THIS_IS_WRONG;
            
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Account, PacketAction.Reply),
                pkt =>
                {
                    m_account_reply = (AccountReply) pkt.GetShort();
                    m_account_responseEvent.Set();
                }, false);
        }

        private void _disposeAccountMembers()
        {
            if (m_account_responseEvent != null)
            {
                m_account_responseEvent.Dispose();
                m_account_responseEvent = null;
            }
        }

        public bool AccountChangePassword(string username, string old_password, string new_password, out AccountReply result)
        {
            result = AccountReply.THIS_IS_WRONG;
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket builder = new OldPacket(PacketFamily.Account, PacketAction.Agree);
            builder.AddBreakString(username);
            builder.AddBreakString(old_password);
            builder.AddBreakString(new_password);
            
            if (!m_client.SendPacket(builder) || !m_account_responseEvent.WaitOne(Constants.ResponseTimeout))
                return false;

            result = m_account_reply;

            return true;
        }

        public bool AccountCheckName(string username, out AccountReply result)
        {
            result = AccountReply.THIS_IS_WRONG;
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket builder = new OldPacket(PacketFamily.Account, PacketAction.Request);
            builder.AddString(username);

            if (!m_client.SendPacket(builder) || !m_account_responseEvent.WaitOne(Constants.ResponseTimeout))
                return false;

            result = m_account_reply;

            return true;
        }

        public bool AccountCreate(string uName, string pass, string realName, string location, string email, string HDDSerial, out AccountReply result)
        {
            result = AccountReply.THIS_IS_WRONG;
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket builder = new OldPacket(PacketFamily.Account, PacketAction.Create);
            //eoserv doesn't care...
            builder.AddShort(1337);
            builder.AddByte(42);

            builder.AddBreakString(uName);
            builder.AddBreakString(pass);
            builder.AddBreakString(realName);
            builder.AddBreakString(location);
            builder.AddBreakString(email);
            builder.AddBreakString(System.Net.Dns.GetHostName());
            builder.AddBreakString(HDDSerial);

            if (!m_client.SendPacket(builder) || !m_account_responseEvent.WaitOne(Constants.ResponseTimeout))
                return false;

            result = m_account_reply;

            return true;
        }

        public DialogResourceID AccountResponseMessage()
        {
            DialogResourceID ret = DialogResourceID.NICE_TRY_HAXOR;
            switch (m_account_reply)
            {
                case AccountReply.Exists:
                    ret = DialogResourceID.ACCOUNT_CREATE_NAME_EXISTS;
                    break;
                case AccountReply.NotApproved:
                    ret = DialogResourceID.ACCOUNT_CREATE_NAME_NOT_APPROVED;
                    break;
                case AccountReply.Created:
                    ret = DialogResourceID.ACCOUNT_CREATE_SUCCESS_WELCOME;
                    break;
                case AccountReply.ChangeFailed:
                    ret = DialogResourceID.CHANGE_PASSWORD_MISMATCH;
                    break;
                case AccountReply.ChangeSuccess:
                    ret = DialogResourceID.CHANGE_PASSWORD_SUCCESS;
                    break;
            }
            return ret;
        }
    }
}
