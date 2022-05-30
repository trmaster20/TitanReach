using System.Collections.Generic;
using System.Linq;
using TitanReach_Server.Model;
using static TitanReach_Server.Database;

namespace TitanReach_Server {
    public class FriendsManager {
        private Player _player;
        public List<Friend> Friends;

        public FriendsManager(Player player) {
            _player = player;
            Friends = new List<Friend>();
        }

        public async void InviteFriend(string name) {
            Response response = await Server.Instance.DB.SendFriendRequest((int)_player.UID, name, FriendType.FRIEND);
            if (response.Error) {
                _player.NetworkActions.SendMessage(response.ErrorObject.error);
                return;
            } else {
                FriendData friendData = (FriendData) response.value;
                if(friendData == null) {
                    _player.Msg("Issue fetching data from server");
                    return;
                }
                Friend friend = new Friend(friendData);
                friend.InitiatedBy = _player.UID;
                Friends.Add(friend);
                Player other = Server.GetPlayerByName(name);
                if (other == null)
                    return;
                _player.NetworkActions.SyncFriend(friend, other != null);
                _player.Msg("Sent a friend request to " + name);

                if (other != null) {
                    Friend thisFriend = new Friend(friendData, _player.UID, _player.Name);
                    thisFriend.InitiatedBy = _player.UID;
                    other.FriendsManager.Friends.Add(thisFriend);
                    if (friend.InviteStatus == InviteStatus.PENDING) {
                        other.NetworkActions.SendFriendInvite(thisFriend);
                    } else {
                        other.NetworkActions.AddFriend(thisFriend, true);
                    }
                }
            }
        }

        public async void AcceptInvite(uint friendID) {
            Friend friend = GetFriend(friendID);
            if (friend == null) {
                _player.NetworkActions.SendMessage("That invite no longer exists");
                return;
            }

            Response response = await Server.Instance.DB.AcceptFriendRequest((int)friend.FriendListID);
            if (response.Error) {
                _player.NetworkActions.SendMessage(response.ErrorObject.error);
                return;
            }

            friend.AcceptInvite();
            Player other = Server.GetPlayerByUID(friendID);
            if (other == null)
                return;
            _player.NetworkActions.AddFriend(friend, other != null);

            if (other != null) {
                Friend thisFriend = other.FriendsManager.GetFriend(_player.UID);
                if (thisFriend != null)
                {
                    thisFriend.AcceptInvite();
                    other.NetworkActions.AddFriend(thisFriend, true);
                }
            }
        }

        public async void RemoveFriend(uint UID) {
            Friend friend = GetFriend(UID);
            if (friend == null) {
                _player.NetworkActions.SendMessage("That friend does not exist");
                return;
            }

            Response response = await Server.Instance.DB.RemoveFriend((int)friend.FriendListID);
            if (response.Error) {
                _player.NetworkActions.SendMessage(response.ErrorObject.error);
                return;
            }

            Friends.Remove(friend);
            _player.NetworkActions.FriendRemoved(friend);

            Player other = Server.GetPlayerByUID(UID);
            if(other != null) {
                Friend thisFriend = other.FriendsManager.GetFriend(_player.UID);
                if (thisFriend != null) {
                    other.NetworkActions.FriendRemoved(thisFriend);
                    other.FriendsManager.Friends.Remove(thisFriend);
                }
            }
        }

        public async void IgnorePlayer(string name) {
            Response response = await Server.Instance.DB.SendFriendRequest((int)_player.UID, name, FriendType.IGNORE);
            if (response.Error) {
                _player.NetworkActions.SendMessage(response.ErrorObject.error);
                return;
            } else {
                FriendData friendData = response.value as FriendData;
                Friend friend = new Friend(friendData);
                Friends.Add(friend);
                _player.NetworkActions.SyncFriend(friend, false);
            }
        }

        public async void Login(List<FriendData> friendDatas) {
            foreach (FriendData friendData in friendDatas) {
                Friend friend = new Friend(friendData);
                Friends.Add(friend);
                
                Player other = Server.GetPlayerByUID(friend.UID);
                if (other == null) {
                    //sync offline
                    _player.NetworkActions.SyncFriend(friend, false);
                } else {
                    //sync online
                    _player.NetworkActions.SyncFriend(friend, true);

                    Response response = await Server.Instance.DB.GetFriendsList((int)other.UID);
                    if (response.Error) {
                        if(response.ErrorObject != null && response.ErrorObject.error != null)
                             Server.Log(response.ErrorObject.error);
                        return;
                    }

                    List<FriendData> otherDatas = (List <FriendData>) response.value;
                    FriendData data = otherDatas.Where(f => f.friendId == _player.UID).SingleOrDefault();
                    if (data == null) {
                        Server.Log("could not find friend");
                        return;
                    }

                    Friend thisFriend = new Friend(data);

                    if(thisFriend == null) {
                        Server.Log("thisfriend was null");
                        return;
                    }

                    other.FriendsManager.Friends.Add(thisFriend);
                    other.NetworkActions.SyncFriend(thisFriend, true);
                }
            }
        }

        public bool IsIgnored(Player other) {
            Friend otherFriend = GetFriend(other.UID);
            Friend thisFriend = other.FriendsManager.GetFriend(_player.UID);

            bool ignored = false;
            if(otherFriend != null) {
                ignored = otherFriend.FriendType == FriendType.IGNORE;
            }
            if (thisFriend != null) {
                ignored = ignored || thisFriend.FriendType == FriendType.IGNORE;
            }
            return ignored;
        }

        public void Logout() {
            List<Friend> friends = GetFriends();
            foreach (Friend friend in friends) {
                Player other = Server.GetPlayerByUID(friend.UID);
                if (other != null) {
                    Friend thisFriend = other.FriendsManager.GetFriend(_player.UID);
                    if(thisFriend != null)
                        other.NetworkActions.SyncFriend(thisFriend, false);
                }
            }
        }

        public Friend GetFriend(uint UID) {
            return Friends.Where(f => f.UID == UID).FirstOrDefault();
        }

        public List<Friend> GetFriends() {
            return Friends.Where(f => f.InviteStatus == InviteStatus.ACCEPTED && f.FriendType == FriendType.FRIEND).ToList();
        }
    }

    public class Friend {
        public uint FriendListID { get; private set; }
        public uint UID { get; private set; }
        public string Name { get; private set; }
        public InviteStatus InviteStatus { get; private set; }
        public FriendType FriendType { get; private set; }
        public uint InitiatedBy;

        public Friend(FriendData friendData) {
            FriendListID = (uint)friendData.friendsListId;
            UID = (uint) friendData.friendId;
            Name = friendData.friendName;
            FriendType = friendData.friendType.Equals("Friend") ? FriendType.FRIEND : FriendType.IGNORE;
            InviteStatus = friendData.status.Equals("Accepted") ? InviteStatus.ACCEPTED : InviteStatus.PENDING;
            InitiatedBy = (uint)friendData.initiatedBy;
        }

        public Friend(FriendData friendData, uint friendID, string friendName) {
            FriendListID = (uint)friendData.friendsListId;
            UID = friendID;
            Name = friendName;
            FriendType = friendData.friendType == "Friend" ? FriendType.FRIEND : FriendType.IGNORE;
            InviteStatus = friendData.status == "Accepted" ? InviteStatus.ACCEPTED : InviteStatus.PENDING;
            InitiatedBy = (uint)friendData.initiatedBy;
        }

        public void AcceptInvite() {
            InviteStatus = InviteStatus.ACCEPTED;
        }
    }

    public enum FriendType {
        FRIEND = 0,
        IGNORE = 1
    }

    public enum InviteStatus {
        PENDING = 0,
        ACCEPTED = 1
    }
}
