using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SEWilson.ScreenSaver.P2P
{
    [ServiceContract(Namespace = "http://mrshaunwilson/wass", CallbackContract = typeof(ICharacterFeed))]
    public interface ICharacterFeed
    {
        [OperationContract(IsOneWay=true)]
        void Advertise(string realmName, string characterName);
    }

    public interface ICharacterFeedChannel : ICharacterFeed, IClientChannel
    {
        //
    }
}
