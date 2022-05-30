using System;
using TitanReach_Server.Model;
using TitanReach_Server.Network.Assets.Core.Network;
using TitanReach_Server.Utilities;
using TRShared.Data.Enums;

namespace TitanReach_Server.Network.Incoming
{
    class OBJECT_INTERACT : IncomingPacketHandler
    {


        public static int LastPull = Environment.TickCount;
        public int GetID()
        {
            return Packets.OBJECT;
        }

        public void OnPacketReceivedAsync(Player p, int Len, MessageBuffer packet)
        {
            int subType = packet.ReadByte();
            if (p.Dead)
                return;


            if (subType == 5) // active gathering
            {
                 if (p.Busy)
                  return;
                uint num = packet.ReadUInt32();
                Obj obj = p.GetObjectByUID(num);
                int interactionID = (int)packet.ReadByte();
                int secondaryVal = packet.ReadInt32();
                if (obj != null)
                {

                    if (!obj.Definition.Interactable || !Formula.InRadius(p.transform.position, obj.transform.position, obj.Definition.InteractableRadius + 1))
                    {
                        p.Msg("Interaction is too far!");
                        return;
                    }
                    EventManager.RaiseObjectInteractionEvent(p, obj, interactionID, -1, true);
                }
            }
            if (subType == 4) {
               // if (p.Busy)
                  //  return;
                p.StopInteracting();
            } else if (subType == 3) {

                uint num = packet.ReadUInt32();
                Obj obj = p.GetObjectByUID(num);
                int interactionID = (int)packet.ReadByte();
                int craftingItemID = (int)packet.ReadUInt32();
                int craftIDX = packet.ReadInt32();
                Events.OnItemAdd.Event += OnItemAdd_Event;
          
                if (obj != null)
                    if (obj.ID == 128)
                    {
                        if (p.Map.LandID == 0)   //if in overworld
                            p.ChangeMap(1, Locations.PVP_ISLAND_SPAWN);
                        else  //if in PvP island
                            p.ChangeMap(0, Locations.PVP_ISLAND_EXIT_ALYSSIA_SPAWN);
                        return;
                    }


                if (obj != null || obj == null && interactionID == 6) { // needle leatherworking
                 
                    if (interactionID != 6) {
                        if(!obj.Definition.Interactable || !Formula.InRadius(p.transform.position, obj.transform.position, obj.Definition.InteractableRadius + 1)) {
                            p.Msg("You cannot interact with that object");
                            return;
                        }
                    }
                  //  Server.Log("Interaction ID: " + interactionID + " CraftingItemID " + craftingItemID + " craftIDX " + craftIDX);
                    if (!Skill.CraftItem((ObjectInteractTypes)interactionID, p, craftingItemID, craftIDX)) {
                   
                        EventManager.RaiseObjectInteractionEvent(p, obj, interactionID, craftingItemID);
                        p.PlayerTracking.TrackInteraction(obj.Definition.Name + " (" + obj.transform.position.X + "," + obj.transform.position.Y + "," + obj.transform.position.X + ")");
                    }
                }
            }
        }

        private void OnItemAdd_Event(object sender, EventManager.ItemAdd e)
        {
            Console.WriteLine("item " + e.item.ID + "changed for user " + e.player.Name);
        }
    }
}

