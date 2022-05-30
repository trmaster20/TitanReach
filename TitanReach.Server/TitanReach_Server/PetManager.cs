using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanReach_Server.Model;
using static TitanReach_Server.Database;

namespace TitanReach_Server {
    public class PetManager {
        private Player _player;
        public int CurrentPet { get; private set; }
        public List<int> UnlockedPets { get; private set; }

        public PetManager(Player player) {
            _player = player;
            UnlockedPets = new List<int>();
            CurrentPet = -1;
        }

        public void SetActivePet(int petID) {
            if (UnlockedPets.Contains(petID))
            {
                CurrentPet = petID;
                foreach (Player p in _player.Map.Players)
                {
                    p.NetworkActions.SetPet(CurrentPet, _player);
                }
            }
            //int pet = UnlockedPets.Where(p => p.ID == petID).SingleOrDefault();
            //if (pet != null) {
            //    CurrentPet = pet;
            //    foreach (Player p in _player.Map.Players) {
            //        p.NetworkActions.SetPet(CurrentPet, _player);
            //    }
            //}
        }

        public void SetupPets(List<PetData> pets, int? currentPetID) {
            foreach (PetData pet in pets) {
                int id = pet.petId;
                UnlockedPets.Add(pet.petId);// Server.Instance.AllPets[id]);
                if(pet.petId == currentPetID) {
                    CurrentPet = pet.petId;// Server.Instance.AllPets[id];
                }
            }
        }

        public void SyncPets() {
            _player.NetworkActions.SyncAllPets();
            _player.NetworkActions.SyncPlayerPets();

            foreach (Player other in _player.Map.Players) {
                if (CurrentPet != null) {

                    other.NetworkActions.SetPet(CurrentPet, _player);
                }
                if (_player.UID != other.UID && other.PetManager.CurrentPet != null) {

                    _player.NetworkActions.SetPet(other.PetManager.CurrentPet, other);
                }
            }
        }

        public void ClearPets()
        {
            UnlockedPets.Clear();
        }

        public async Task<bool> UnlockPet(int petID) {
            if(UnlockedPets.Contains(petID))
            {
                _player.NetworkActions.SendMessage("You would have received a pet " + TRShared.DataManager.PetDefinitions[petID].Name + ", but you already had that one");
                return false;
            }

            Response response = await Server.Instance.DB.UnlockPet((int)_player.AccountID, petID);
            if (response.Error) {
                Server.Log("Error unlocking Pet");
                Server.Log(response.ErrorObject.errorCode + " " + response.ErrorObject.error);
                return false;
            }
            //Pet pet = Server.Instance.AllPets[petID];
            UnlockedPets.Add(petID);
            _player.NetworkActions.SendMessage("CONGRATULATIONS! You have unlocked the " + TRShared.DataManager.PetDefinitions[petID].Name + " pet");
            _player.NetworkActions.SyncPlayerPets();
            return true;
        }

        public async void Unlock1Pet(int petID)
        {
            await UnlockPet(petID);
        }


        public async void UnlockAllPets() {
            Server.Log("Unlocking all pets for: " + _player.Name);

            foreach (var petDef in TRShared.DataManager.PetDefinitions)
            {
                if (UnlockedPets.Contains(petDef.PetID))
                {
                    continue;
                }
                await UnlockPet(petDef.PetID);
            }

            //foreach(int petID in Server.Instance.AllPets.Keys) {
            //    UnlockPet(petID);
            //}
        }

        public bool HasPet(int petID) {
            return UnlockedPets.Contains(petID);
            //return UnlockedPets.Where(p => p.ID == petID).Any();
        }

        public int TotalPets() {
            return UnlockedPets.Count;
        }
    }
}
