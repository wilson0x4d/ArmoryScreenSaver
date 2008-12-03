using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ServiceModel;

namespace SEWilson.ScreenSaver.P2P
{
    internal class CharacterFeedService : ICharacterFeed
    {
        public class CharacterFeedEventArgs : EventArgs
        {
            public string RealmName { get; set; }
            public string CharacterName { get; set; }
        }

        public event EventHandler<CharacterFeedEventArgs> CharacterAdvertised;

        #region ICharacterFeed Members

        public void Advertise(string realmName, string characterName)
        {
            if (CharacterAdvertised != null)
            {
                try
                {
                    CharacterAdvertised(this, new CharacterFeedEventArgs
                    {
                        RealmName = realmName,
                        CharacterName = characterName
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
                }
            }
        }

        #endregion
        #region Character Feed Peer Channel

        public ICharacterFeedChannel CreateChannel()
        {
            try
            {
                DuplexChannelFactory<ICharacterFeedChannel> factory = new DuplexChannelFactory<ICharacterFeedChannel>(
                    new InstanceContext(this), 
                    "CharacterFeedEndpoint");
                factory.Open();
                return factory.CreateChannel();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
            }
            return null;
        }

        #endregion
    }
}
