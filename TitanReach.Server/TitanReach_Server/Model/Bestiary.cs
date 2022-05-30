using System;
using System.Collections.Generic;
using System.Linq;
using TRShared;
using TRShared.Data.Definitions;

namespace TitanReach_Server.Model {
    public class Bestiary {

        private Dictionary<int, BestiaryEntry> _entries;
        public float BASE_PET_THRESHOLD = 2000f;

        public Bestiary() {
            _entries = new Dictionary<int, BestiaryEntry>();
        }

        public void UpdateBestiary(int npcid, Player p) {
            UpdateSeen(npcid, p);
            UpdatePet(npcid, p);
            UpdateDrops(npcid, p);
            UpdateDescription(npcid, p);
            UpdateStats(npcid, p);
            UpdateLocations(npcid, p);
        }

        private void UpdateSeen(int npcid, Player p) {
            if (p.Bestiary[npcid].HasSeen) {
                throw new NotImplementedException();
                //send network update for seen
            }
        }
        private void UpdatePet(int npcid, Player p) {
            if (p.PetManager.HasPet(npcid)) {
                throw new NotImplementedException();
                //send network update for pet
            }
        }

        private void UpdateDrops(int npcid, Player p) {
            if(KillThresholdMet(_entries[npcid].DropsThreshold, npcid, p)) {
                DropDef drops = DataManager.DropTables[DataManager.NpcDefinitions[npcid].DropTableID];
                //send network update for drops
            }
        }

        private void UpdateDescription(int npcid, Player p) {
            if (KillThresholdMet(_entries[npcid].DropsThreshold, npcid, p)) {
                throw new NotImplementedException();
                //send network update for desc
            }
        }

        private void UpdateStats(int npcid, Player p) {
            if (KillThresholdMet(_entries[npcid].DropsThreshold, npcid, p)) {
                throw new NotImplementedException();
                //send network update for stats
            }
        }

        private void UpdateLocations(int npcid, Player p) {
            if (_entries[npcid].LocationsShown && p.Bestiary[npcid].Locations.Count > 0) {
                throw new NotImplementedException();
                //send network update for locations
            }
        }

        private bool KillThresholdMet(int threshold, int npcid, Player p) {
            return p.Bestiary[npcid].GetKills() > threshold;
        }
    }

    public class BestiaryEntry {
        public BestiaryEntry(int dropsThreshold, int descriptionThreshold, int statsThreshold, bool locationsShown) {
            DropsThreshold = dropsThreshold;
            DescriptionThreshold = descriptionThreshold;
            StatsThreshold = statsThreshold;
            LocationsShown = locationsShown;
        }

        // the threshold in kills before drop info is displayed, -1 is never displayed
        public int DropsThreshold { get; private set; }

        //the threshold in kills before description info is displayed, -1 is never displayed
        public int DescriptionThreshold { get; private set; }

        //the threshold in kills before stat info is displayed, -1 is never displayed
        public int StatsThreshold { get; private set; }

        //whether locations are ever shown for this npc
        public bool LocationsShown { get; private set; }
    }

    public class PlayerBestiary {
        private int _npcID;
        private Player _player;
        private int _killCount => killTimes.Count;
        private DateTime _lastSaved;
        private List<DateTime> killTimes;

        public PlayerBestiary(int npcID, Player player) {
            _npcID = npcID;
            _player = player;
            killTimes = new List<DateTime>();
            _lastSaved = DateTime.Now;
        }

        public int GetKills() {
            return _killCount;
        }

        public void AddKill() {
            killTimes.Add(DateTime.Now);
            RollPetUnlock();
            //Server.Instance.Bestiary.UpdateBestiary(_npcID, _player);
        }

        public List<DateTime> GetUnsavedKills() {
            return killTimes.Where(kt => kt > _lastSaved).ToList();
        }

        public void UpdateKillsSaved() {
            _lastSaved = DateTime.Now;
        }

        public void UpdateKillTimes(DateTime kill) {
            killTimes.Add(kill);
            UpdateKillsSaved();
        }

        private void RollPetUnlock() {

            //if (_player.PetManager.HasPet(_npcID))
            //    return;
            foreach(var petDef in DataManager.PetDefinitions)
            {
                if(petDef.NPCDropIds != null)
                {
                    if (petDef.NPCDropIds.Contains(_npcID))
                    {
                        Random random = new Random();
                        if(random.NextDouble() * petDef.Rarity < 1.0)
                        {
                            if (_player.PetManager.UnlockPet(petDef.PetID).Result)
                            {
                                Server.Instance.MessageAllPlayers("<color=yellow>" + _player.Name + " <color=white>has obtained the Pet: <color=yellow>" + petDef.Name + "<color=white> after <color=yellow>" + _killCount + "<color=white> kills!");
                                Server.DiscordChatMessages.Add( _player.Name + " has obtained the Pet: " + petDef.Name + " after " + _killCount + " kills!");
                            }
                        }
                       
                    }
                }
            }
            //Random random = new Random();
            //if (random.NextDouble() <  (1f / (Server.Instance.Bestiary.BASE_PET_THRESHOLD - _killCount))) {
            //    _player.PetManager.UnlockPet(_npcID);
            //}
        }

        public bool HasSeen;
        //TODO track seen

        public List<string> Locations;
        //TODO track locations
    }
}
