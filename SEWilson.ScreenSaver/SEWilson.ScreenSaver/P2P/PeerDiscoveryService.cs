using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel;
using System.Diagnostics;

namespace SEWilson.ScreenSaver.P2P
{
    public class PeerDiscoveryService : CustomPeerResolverService
    {
        #region Local Resolver Instance

        private static P2P.PeerDiscoveryService peerDiscoveryService = null;
        private static ServiceHost peerDiscoveryServiceHost = null;

        internal static void Start()
        {
            // NOTE using PNRP instead of a custom resolver
            //try
            //{
            //    peerDiscoveryService = new P2P.PeerDiscoveryService();
            //    peerDiscoveryService.ControlShape = false;
            //    peerDiscoveryServiceHost = new ServiceHost(peerDiscoveryService);
            //    peerDiscoveryService.Open();
            //    peerDiscoveryServiceHost.Open();
            //    // TODO proper closure/shutdown of service
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message + "|" + ex.StackTrace);
            //    try
            //    {
            //        peerDiscoveryServiceHost.Close();
            //    }
            //    catch { }
            //    try
            //    {
            //        peerDiscoveryService.Close();
            //    }
            //    catch { }
            //}
        }

        #endregion
    }
}
