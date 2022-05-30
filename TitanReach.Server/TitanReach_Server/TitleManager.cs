using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TitanReach_Server.Model;
using static TitanReach_Server.Database;

namespace TitanReach_Server {
    public class TitleManager {
        private Player _player;
        public Title CurrentTitle { get; private set; }
        public List<Title> UnlockedTitles { get; private set; }

        public TitleManager(Player player) {
            _player = player;
            UnlockedTitles = new List<Title>();
        }

        public void SetActiveTitle(uint titleID) {
            Title title = UnlockedTitles.Where(t => t.ID == titleID).SingleOrDefault();
            if (title != null) {
                CurrentTitle = title;
                foreach (Player p in _player.Map.Players) {
                    p.NetworkActions.SetTitle(CurrentTitle, _player);
                }
            }
        }

        public void DeselectTitle()
        {
            CurrentTitle = null;
            foreach (Player p in _player.Map.Players)
            {
                p.NetworkActions.DeselectTitle(_player);
            }
        }

        public void SetupTitles(List<TitleData> titles, int? curentTitleID) {

            foreach (TitleData title in titles) {
                int id = title.titleId;
                UnlockedTitles.Add(Server.Instance.AllTitles[id]);
                if(title.titleId == curentTitleID) {
                    CurrentTitle = Server.Instance.AllTitles[id];
                }
            }            
        }

        public void SyncTitles() {
            _player.NetworkActions.SyncAllTitles();
            _player.NetworkActions.SyncPlayerTitles();

            foreach (Player other in _player.Map.Players) {
                if (CurrentTitle != null) {
                    other.NetworkActions.SetTitle(CurrentTitle, _player);
                }
                if (_player.UID != other.UID && other.TitleManager.CurrentTitle != null) {
                    _player.NetworkActions.SetTitle(other.TitleManager.CurrentTitle, other);
                }
            }
        }

        public async void UnlockTitle(Title title) {
            Response response = await Server.Instance.DB.UnlockTitle((int)_player.AccountID, (int)title.ID);
            if (response.Error) {
                _player.NetworkActions.SendMessage(response.ErrorObject.error);
                return;
            }
            UnlockedTitles.Add(Server.Instance.AllTitles[title.ID]);
            _player.NetworkActions.SendMessage("Title unlocked: " + title.Display);
            _player.NetworkActions.SyncPlayerTitles();
        }

        public void UnlockAllTitles() {
            foreach (Title title in Server.Instance.AllTitles.Values) {
                UnlockTitle(title);
            }
        }

        public int TotalTitles() {
            return UnlockedTitles.Count;
        }
    }
}
