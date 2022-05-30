using ENet;
using System;
using System.Collections.Concurrent;
using System.Linq;
using TitanReach_Server.Model;

namespace TitanReach_Server.Network
{
    public class NetworkEngine
    {

        public Host server;

        public static int ReadAttempts = 0;

        public void Poll()
        {
            try
            {
                CheckData();
            }
            catch (Exception e)
            {
                Server.Error(e.Message + " - " + e.StackTrace);
            }
            try
            {
                ProcessOutgoingPackets();

            }
            catch (Exception e)
            {
                Server.Error(e.Message + " - " + e.StackTrace);
            }
        }
        public void CheckData()
        {
            Event netEvent;


            bool polled = false;

            while (!polled)
            {
                if (server.CheckEvents(out netEvent) <= 0)
                {
                    if (server.Service(0, out netEvent) <= 0)
                        break;

                    polled = true;
                }
                try
                {
                    switch (netEvent.Type)
                    {
                        case EventType.None:
                            {
                                Player pl = Server.Instance.AllPlayers.FirstOrDefault(x => x.UID == netEvent.Peer.ID);
                                if (pl == null)
                                    Server.Error("EventType.None");
                                else
                                    Server.Error("EventType.None: " + pl.Describe());

                                break;
                            }


                        case EventType.Connect:
                            if (Server.SHUTDOWN)
                            {
                                netEvent.Peer.DisconnectNow(0);
                                return;
                            }
                            netEvent.Peer.Timeout(75000, 70000, 90000);
                            Server.CONNECTIONS_SINCE_START++;
                            Server.Log("Client connected - ID: " + netEvent.Peer.ID + " Hash: " + netEvent.Peer.GetHashCode() + ", IP: " + netEvent.Peer.IP);

                            Player player = new Player();
                            //player.UID = netEvent.Peer.ID;
                            player.peer = netEvent.Peer;
                            player.NetworkActions = new NetworkActions(player.peer, player);
                            player.Waiting = true;
                            lock (Server.Instance.WaitingPool)
                                Server.Instance.WaitingPool.Add(player);

                            break;

                        case EventType.Disconnect:
                             Server.Log("Client disconnected - ID: " + netEvent.Peer.ID + " Hash: " + netEvent.Peer.GetHashCode() + ", IP: " + netEvent.Peer.IP);
                            Server.Instance.RemovePlayer(netEvent.Peer.ID, "Enet - Disconnected");
                            break;

                        case EventType.Timeout:
                            Server.Log("Client timeout - ID: " + netEvent.Peer.ID + " Hash: " + netEvent.Peer.GetHashCode() + ", IP: " + netEvent.Peer.IP);
                            Server.Instance.RemovePlayer(netEvent.Peer.ID, "Enet - Timed Out");
                            break;

                        case EventType.Receive:
                            // Console.WriteLine("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                            // Server.Log("Channel: " + netEvent.ChannelID);

                            Player p = null;
                            //netEvent.Peer.
                            if (Server.Instance.playerRefs.ContainsKey(netEvent.Peer.GetHashCode()))
                            {
                                p = Server.Instance.playerRefs[netEvent.Peer.GetHashCode()];//players.FirstOrDefault(x => x.peer.ID == netEvent.Peer.ID);
                            }
                            else
                            {
                                lock (Server.Instance.WaitingPool)
                                {
                                    foreach (var pl in Server.Instance.WaitingPool)
                                    {
                                        if (pl != null && pl.peer.ID == netEvent.Peer.ID)
                                        {
                                            //  Server.Log("Found in waiting pool");
                                            p = pl;
                                            break;
                                        }
                                    }
                                    //  Server.Log("not found");
                                }
                            }

                            Server.IncomingPacketsPerSecond++;
                            p?.QueueIncomingPacket(netEvent);
                            if (p != null) {
                                p.PlayerTracking.PacketThreshold++;
                            }

                            break;
                    }
                }
                catch (Exception e)
                {
                    Server.Error("CheckData PollingLoop Exception: " + e.Message + " Stack: " + e.StackTrace);
                }
            }
        }

        public void ProcessWaitingPackets() // monitor waiting pool size
        {

            for (int i = 0; i < Server.Instance.WaitingPool.Count; i++)
            {
                Player p = Server.Instance.WaitingPool[i];
                if (p == null)
                    continue;

                while (p.incomingPackets.TryDequeue(out Event evt))
                {
                    try
                    {
                        Server.Instance.HandleIncomingPacket(p, evt);
                    }
                    catch (Exception e)
                    {
                        Server.Error(p.Describe() + " Exception @ ProcessWaitingPackets: " + e.Message + " Stack: " + e.StackTrace);
                    }
                }

            }
        }

        public void ProcessIncomingPackets()
        {
            ProcessWaitingPackets();
            for (int i = 0; i < Server.Instance.AllPlayers.Count; i++)
            {
                Player p = Server.Instance.AllPlayers[i];
                if (p == null)
                    continue;

                while (p.incomingPackets.TryDequeue(out Event evt))
                {
                    try
                    {
                        Server.Instance.HandleIncomingPacket(p, evt);
                    }
                    catch (Exception e)
                    {
                        Server.Error(p.Describe() + " Exception @ ProcessIncomingPackets: " + e.Message + " Stack: " + e.StackTrace);
                    }
                }
            }
        }
        public ConcurrentQueue<NetworkActions.Message> OutgoingPackets = new ConcurrentQueue<NetworkActions.Message>();
        public void ProcessOutgoingPackets()
        {
            while (OutgoingPackets.TryDequeue(out NetworkActions.Message buf))
            {
                try
                {

                    NetworkActions.WritePacket(buf);
                    Server.OutgoingPacketsPerSecond++;

                }
                catch (Exception e)
                {
                    Server.Error(e.Message + " - " + e.StackTrace);
                }
            }

        }

    }
}
