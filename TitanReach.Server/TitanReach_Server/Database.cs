using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TitanReach_Server.Model;
using TitanReach_Server.Quests;
using TitanReach_Server.Utilities;

namespace TitanReach_Server
{
    public class Database
    {

        private static SqlConnectionStringBuilder builder;

        public static string API_URL = "https://titanreach.com/api/";
        public static string TOKEN = "null";



        public class DbNft
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string TokenId { get; set; }
            public string ImageUrl { get; set; }
            // public int ItemId { get; set; }
            // public DbItem Item { get; set; }
        }



        public class PlayerCheckin
        {
            public int characterUptime { get; set; }
            public int characterId { get; set; }
        }

        public class ServerCheckin
        {

            public int worldOnline { get; set; }
            public int worldPort { get; set; }
            public string worldIPAddress { get; set; }
            public int serverUptime { get; set; }
            public string worldName { get; set; }
            public string worldRegion { get; set; }
            public int worldNumber { get; set; }
            public int worldMaxPlayers { get; set; }

            public string consoleLog { get; set; }

            public int cumulativePlayers { get; set; }

            public int cumulativeConnections { get; set; }

            public int averageTickRate { get; set; }

            public int outPacketsPerSecond { get; set; }

            public int inPacketsPerSecond { get; set; }
            public List<PlayerCheckin> players { get; set; }
        }
        public class AppearanceData
        {

            public int appearanceId { get; set; }
            public string slotType { get; set; }
            public int slotId { get; set; }
            public int slotColor1 { get; set; }
            public int? slotColor2 { get; set; }
            public int? slotColor3 { get; set; }
            public int? clothingId { get; set; }
        }

        public class Equipment
        {
            public int equipmentItemId { get; set; }
            public int equipmentItemAmount { get; set; }
            public int slotId { get; set; }
        }

        public class Bank
        {
            public int bankId { get; set; }
            public int bankItemId { get; set; }
            public int bankItemAmount { get; set; }
        }

        public class Inventory
        {
            public int inventoryId { get; set; }
            public int inventoryItemId { get; set; }
            public int inventoryItemAmount { get; set; }
        }

        public class Location
        {
            public int locationId { get; set; }
            public double posX { get; set; }
            public double posY { get; set; }
            public double posZ { get; set; }
            public double rotX { get; set; }
            public double rotY { get; set; }
            public double rotZ { get; set; }
            public int map { get; set; }
        }

        public class Property
        {
            public int propertyId { get; set; }
            public string propertyName { get; set; }
            public string propertyValue { get; set; }
        }

        public class Skill
        {
            public int skillsId { get; set; }
            public int skillExperience { get; set; }
            public int skillLevel { get; set; }
            public string skillName { get; set; }
        }

        public class Root
        {
            public string username { get; set; }
            public int? clan { get; set; }
            public int? title { get; set; }
            public int characterId { get; set; }
            public int pId { get; set; }
            public int? activePet { get; set; }
            public List<AppearanceData> appearance { get; set; }
            public List<Bank> bank { get; set; }
            public List<Inventory> inventory { get; set; }
            public List<Location> location { get; set; }
            public List<Property> properties { get; set; }
            public List<Equipment> equipment { get; set; }
            public List<Skill> skills { get; set; }
            public List<FriendData> friendsList { get; set; }
            public List<TitleData> unlockedTitles { get; set; }
            public List<PetData> unlockedPets { get; set; }
            public List<KillData> characterKills { get; set; }
            public List<QuestData> quests { get; set; }
        }

        public class CurrentSlayerTask //merge into root
        {
            public int killsRequired { get; set; }
            public int killsComplete { get; set; }
            public int npcCategory { get; set; }

        }

        public class KillData {
            public int npcId { get; set; }
            public string createdTimestamp { get; set; }
        }


        public class AccountAccessLevel
        {
            public string accessLevel { get; set; }
        }

        public class AccountCharacterData
        {
            public int? clan { get; set; }
            public int? title { get; set; }
            public int characterId { get; set; }
            public string username { get; set; }
            public int combatLevel { get; set; }
            public int totalLevel { get; set; }
            public int? map { get; set; }
            public List<Equipment> equipment { get; set; }
            public List<AppearanceData> appearance { get; set; }

        }

        public class QuestData {

            public int questId { get; set; }
            public string questName { get; set; }
            public string questDescription { get; set; }
            public int completedState { get; set; }
            public List<QuestProgressData> progress { get; set; }
        }

        public class QuestData2 {
            public int characterId { get; set; }
            public int questId { get; set; }
            public string questName { get; set; }
            public string questDescription { get; set; }
            public int completedState { get; set; }
            public List<QuestProgressData> progress { get; set; }
        }

        public class QuestProgressData {
            public int? questProgressId { get; set; }
            public int? characterId { get; set; }
            public int? currentState { get; set; }
            public List<QuestStepData> steps { get; set; }
        }

        public class QuestStepData {
            public int step { get; set; }
            public int isStepComplete { get; set; }
            public int nextState { get; set; }
            public string stepDescription { get; set; }
        }

        public class FriendData {
            public int friendsListId { get; set; }
            public int characterId { get; set; }
            public int friendId { get; set; }
            public string status { get; set; }
            public string friendType { get; set; }
            public string friendName { get; set; }
            public int initiatedBy { get; set; }
        }

        public class TitleData {
            public int titleId { get; set; }
            public string title { get; set; }
            public int titleType { get; set; }
        }

        public class PetData {
            public int petId { get; set; }
            public string pet { get; set; }
        }

        public class AccountData
        {
            public int pId { get; set; }
            public int isMuted { get; set; }
            public int isBanned { get; set; }
            public List<AccountCharacterData> characters { get; set; }

            public List<AccountAccessLevel> accountAccessLevels { get; set; }
        }


        public class TokenRequest
        {
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string access_token { get; set; }
            public string scope { get; set; }
        }


        public class ShopResult
        {
            public string itemName { get; set; }
            public int purchaseId { get; set; }
            public int itemId { get; set; }
            public int purchaseQuantity { get; set; }
        }

        public class SaveConfirmation
        {
            public string message { get; set; }
        }

        public class BanResult
        {
            public string message { get; set; }
        }


        bool firstRefresh = true;
        public async Task<bool> RefreshAccessToken()
        {

            firstRefresh = false;
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("client_id", "0oaesu7kfWhTJr7jq5d5");
            values.Add("client_secret", "XkwjER5v2tO2J0DKupaFs1Jko0o-jCqJ2H3XceFZ");
            values.Add("grant_type", "client_credentials");
            values.Add("scope", "client-login");

            var response = await client.PostAsync("https://dev-7562863.okta.com/oauth2/TitanReachClientAuthServer/v1/token", new FormUrlEncodedContent(values));
            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync<TokenRequest>();
                TOKEN = token.access_token;
                Console.WriteLine("Token Fetched: " + token.access_token);
                return true;
            }
            return false;

        }
        public class Error
        {
            public string internalError { get; set; }
            public string error { get; set; }
            public int errorCode { get; set; }

        }

        public class BugReportResponse
        {
            public string message { get; set; }

        }

        public struct Response
        {
            public HttpResponseMessage response;
            public bool Error;
            public Error ErrorObject;
            public int StatusCode;
            public object value;
        }

        public async Task<Response> FetchAPI(string api, Dictionary<string, string> values, HttpClient client, bool print = false)
        {
            var test = new FormUrlEncodedContent(values);
            string query = "API: " + api;
            foreach(string s in values.Keys)
            {
                query += " " + s + "=" + values[s];
            }

           HttpResponseMessage response = null;
            try
            {
                response = await client.PostAsync(API_URL + api, test);
            } catch(Exception e)
            {
                Server.ErrorDB("FetchAPI Post response error " + e.Message);
                response = null;
            }

            Response res = new Response();
            if (response == null)
            {
                res.Error = true;
                return res;
            }
          
            res.Error = !response.IsSuccessStatusCode;
            res.StatusCode = (int)response.StatusCode;
            res.response = response;

            if (res.Error)
            {
                string content = "";
                try { 


                    content = await response.Content.ReadAsStringAsync();
                    res.ErrorObject = JsonSerializer.Deserialize<Error>(content);
                } catch(Exception e)
                {
                    Server.ErrorDB("Error @ FetchAPI [StatusCode: " + res.StatusCode + "] SENT=" + query + " RECV: " + content, e);
                    return res;
                } 
                if (res.ErrorObject.errorCode == 1) // expired token
                {
                    bool refreshed = await RefreshAccessToken();
                    bool dontcheck = false;

                    if (!refreshed)
                    {
                        for (int i = 0; i < 5; i++)
                            Console.WriteLine("ERROR - Could not refresh token, you dun goofed - this will break teh server");

                        Server.ErrorDB("SERIOUS ERROR - TOKEN FAILED TO REFRESH - NOTHING WILL LOAD/SAVE FROM HERE");
                        return res;
                    }
                    else
                    {
                        if (values.ContainsKey("access_token"))
                            values["access_token"] = TOKEN;
                        try
                        {
                            res.response = await client.PostAsync(API_URL + api, new FormUrlEncodedContent(values));
                            res.Error = !res.response.IsSuccessStatusCode;
                            res.StatusCode = (int)response.StatusCode;
                        } catch(Exception e)
                        {
                            Server.Log("Error @ FetchAPI: " + e.Message + " - " + e.StackTrace);
                            res.Error = true;
                            res.StatusCode = 1337;
                            dontcheck = true;
                        }
                        if (res.Error && !dontcheck)
                        {
                            try
                            {
                                res.ErrorObject = await res.response.Content.ReadFromJsonAsync<Error>();
                            } catch(Exception e)
                            {
                                Server.ErrorDB("Error @ FetchAPI [StatusCode: " + res.StatusCode + "]", e);
                                return res;
                            }
                        }
                        return res;
                    }
                }

            }
            return res;
        }

        public async Task<Response> GetCharacter(int cid)
        {

            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("character_id", cid.ToString());

            var response = await FetchAPI("get-character", values, client);
            if (!response.Error)
            {
                try
                {
                    response = await ReadJson(response, values, typeof(Root));
                }
                catch (Exception e)
                {
                    Server.Log(e.Message + " \n " + e.StackTrace);
                    Console.In.Read();
                }
            }
            return response;
        }


        public async Task<Response> SaveCharacter(Root root)
        {
            string json = JsonSerializer.Serialize(root);
            //Server.Log(json);
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("character", json);
            var response = await FetchAPI("save-character", values, client);
            //  Server.Log("Print: " + await response.response.Content.ReadAsStringAsync());
            if (!response.Error)
            {
                try
                {
                    response = await ReadJson(response, values, typeof(SaveConfirmation));
                } catch(Exception e)
                {
                    Server.ErrorDB("User " + root.characterId + " probably failed to save, DB error: " + e.Message);
                    response.Error = true;
                }
            }
            return response;
        }

        public async Task<Response> CreateCharacter(int aid, string name)
        {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("player_id", aid.ToString());
            values.Add("username", name);
            var response = await FetchAPI("create-character", values, client);
            //  Server.Log("Print: " + await response.response.Content.ReadAsStringAsync());
            if (!response.Error)
                response = await ReadJson(response, values, typeof(AccountData));
            return response;
        }
        public async Task<Response> GetAccount(string email, string password, string ip)
        {
            using var client = new HttpClient();
           
            
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("ip_address", ip);
            values.Add("email", email);
            values.Add("password", password);

            var response = await FetchAPI("login", values, client);
            if (!response.Error)
                response = await ReadJson(response, values, typeof(AccountData));
            return response;
        }

        public async Task<Response> SetOnline(int cid, int world)
        {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("character_id", cid.ToString());
            values.Add("world", world.ToString());
            var response = await FetchAPI("update-player-world", values, client);
          //  if (!response.Error)
              //  response.value = await response.response.Content.ReadFromJsonAsync<AccountData>();
            return response;
        }

        public async Task<Response> WorldCheckIn(ServerCheckin sdu)
        {
            string json = JsonSerializer.Serialize(sdu);
          //  Server.Log(json);
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("world_object", json);
            var response = await FetchAPI("world-checkin", values, client);
            //  if (!response.Error)
            //  response.value = await response.response.Content.ReadFromJsonAsync<AccountData>();
            return response;
        }

        #region FRIENDS

        public async Task<Response> AcceptFriendRequest(int friendListID) {
            return await ActionFriendRequest(friendListID, "Accept");
        }

        public async Task<Response> DeclineFriendRequest(int friendListID) {
            return await ActionFriendRequest(friendListID, "Decline");
        }

        private async Task<Response> ActionFriendRequest(int friendsListID, string actionType) {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("friends_list_id", friendsListID.ToString());
            values.Add("action", actionType);
            var response = await FetchAPI("action-friend-request", values, client);
            if (!response.Error)
                response = await ReadJson(response, values, typeof(FriendData));
            return response;
        }

        public async Task<Response> SendFriendRequest(int cid, string friendName, FriendType friendType) {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("character_id", cid.ToString());
            values.Add("friend_username", friendName);
            values.Add("friend_type", friendType == FriendType.FRIEND ? "Friend" : "Ignore");
            var response = await FetchAPI("add-to-friends-list", values, client);
            if (!response.Error)
                response = await ReadJson(response, values, typeof(FriendData));
            return response;
        }

        public async Task<Response> RemoveFriend(int friendsListID) {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("friends_list_id", friendsListID.ToString());
            var response = await FetchAPI("remove-from-friends-list", values, client);
            if (!response.Error)
                response = await ReadJson(response, values, typeof(FriendData));
            return response;
        }

        public async Task<Response> GetFriendsList(int id) {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("character_id", id.ToString());
            var response = await FetchAPI("get-friends-list", values, client);
            if (!response.Error)
                response = await ReadJson(response, values, typeof(List<FriendData>));
            return response;
        }

        #endregion

        #region TITLES
        public async Task<Response> UnlockTitle(int accountID, int titleID) {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("player_id", accountID.ToString());
            values.Add("title_id", titleID.ToString());
            var response = await FetchAPI("unlock-player-title", values, client);
            return response;
        }

        public async Task GetAllTitles(Dictionary<int, Title> allTitles) {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            Response response = await FetchAPI("get-all-titles", values, client);
            if (response.Error) {
                Server.Log(response.ErrorObject.error);
            } else {
                response = await ReadJson(response, values, typeof(List<TitleData>));
                if (!response.Error)
                {
                    List<TitleData> titleDatas = (List<TitleData>)response.value;
                    foreach (TitleData titleData in titleDatas)
                    {
                        allTitles.Add(titleData.titleId, new Title(titleData.titleId, titleData.title, (TitleType)titleData.titleType));
                    }
                }
            }
        }

        #endregion

        #region Slayer
        public async Task<Response> GetSlayerTask(int accountID)
        {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("character_id", accountID.ToString());
            var response = await FetchAPI("get-character-slayer-tasks", values, client);
            return response;
        }

        public async Task<Response> UpdateSlayerTask(int accountID, int killsReq, int killsCopleted, int npcCategory)
        {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("character_id", accountID.ToString());
            values.Add("kills_required", killsReq.ToString());
            values.Add("kills_complete", killsCopleted.ToString());
            values.Add("npc_category", npcCategory.ToString());
            var response = await FetchAPI("update-character-slayer-tasks", values, client);
            return response;
        }
        #endregion

        #region PETS
        public async Task<Response> UnlockPet(int accountID, int petID) {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("player_id", accountID.ToString());
            values.Add("pet_id", petID.ToString());
            var response = await FetchAPI("unlock-player-pet", values, client);
            return response;
        }

        //public async Task GetAllPets(Dictionary<int, Pet> allPets) {
        //    using var client = new HttpClient();
        //    var values = new Dictionary<string, string>();
        //    values.Add("access_token", TOKEN);
        //    Response response = await FetchAPI("get-all-pets", values, client);
        //    if (response.Error) {
        //        Server.Log(response.ErrorObject.error);
        //    } else {
        //        List<PetData> petDatas = (List<PetData>)await response.response.Content.ReadFromJsonAsync<List<PetData>>();
        //        foreach (PetData petData in petDatas) {
        //            allPets.Add(petData.petId, new Pet(petData.petId, petData.pet));
        //        }
        //    }
        //}

        #endregion

        #region BUG REPORTS

        public async Task<Response> SendBugReport(Player player, string category, string description, string outcome, string reproduce)
        {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("category", category);
            values.Add("description", description);
            values.Add("expected_outcome", outcome);
            values.Add("steps_to_reproduce", reproduce);
            values.Add("reported_by", player.AccountID.ToString());
            values.Add("reported_from", "Game");
            var response = await FetchAPI("save-bug-report", values, client);
              if (!response.Error)
                response = await ReadJson(response, values, typeof(BugReportResponse));
            return response;
        }

        #endregion

        public async Task<Response> SaveQuest(List<QuestData2> questData2s) {
            string json = JsonSerializer.Serialize(questData2s);
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("quest_object", json);
            var response = await FetchAPI("save-quest-progress", values, client);
            if (!response.Error)
                response = await ReadJson(response, values, typeof(List<QuestData>));
            return response;
        }

        public async Task<Response> ReadJson(Response response, Dictionary<string, string> dict, Type type)
        {
            var content = "";
            try
            {
                content = await response.response.Content.ReadAsStringAsync();
                response.value = JsonSerializer.Deserialize(content, type);
            } catch(Exception e)
            {
                response.Error = true;
                string query = "";
                foreach (string s in dict.Keys)
                {
                    if (s.Equals("password")) {
                        query += " " + s + "=******";
                    } else {
                        query += " " + s + "=" + dict[s];
                    }
                }
                Server.ErrorDB("ReadJson ERROR: " + query + " Content=" + content, e);
            }
            return response;
        }

        public async Task<Response> BanPlayer(string name, bool unban)
        {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("character", name.Trim());
            values.Add("ban_status", unban ? "0" : "1");
            var response = await FetchAPI("toggle-player-ban", values, client);
            if (!response.Error)
                response = await ReadJson(response, values, typeof(BanResult));
            return response;
        }

        public async Task UpdateAppearance(int cid, List<AppearanceData> data)
        {
            string json = JsonSerializer.Serialize(data);
           // Server.Log("JSON for List<AppearanceData>: " + json);
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("character_id", cid.ToString());
            values.Add("appearance", json);
            var response = await FetchAPI("update-character-appearance", values, client);
           // if (!response.Error)
             //   response = await ReadJson(response, values, typeof(BanResult));
  
        }



        public async Task<Response> DeleteCharacter(int cid)
        {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("character_id", cid.ToString());
            var response = await FetchAPI("delete-character", values, client);
            return response;
        }

        public async Task<Response> MutePlayer(string name, bool mute) {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("character", name.Trim());
            values.Add("mute_status", mute ? "1" : "0");
            var response = await FetchAPI("toggle-player-mute", values, client);
            if (!response.Error)
                response.value = await response.response.Content.ReadFromJsonAsync<BanResult>();
            return response;
        }


        public async Task<Response> GetPurchases(Player p)
        {
            using var client = new HttpClient();
            var values = new Dictionary<string, string>();
            values.Add("access_token", TOKEN);
            values.Add("player_id", p.AccountID.ToString());
            var response = await FetchAPI("get-player-purchases", values, client);
            if (!response.Error)
                response = await ReadJson(response, values, typeof(List<ShopResult>));
            return response;
        }

        public void UnlockPlayer(string name)
        {
            //  Execute("UPDATE accounts SET online = 0 WHERE username LIKE '" + name + "'");
        }

        public async Task SaveAllQuestProgress(Player player) {
            if (player == null)
                return;
            List<QuestData2> questDatas = new List<QuestData2>();
            for (int i = 0; i < player.QuestManager.questStates.Length; i++) {
                Quest quest = QuestManager.quests[i];

                if (quest == null)
                    continue;

                QuestState state = player.QuestManager.questStates[i];
                AddQuestDatas(questDatas, player, quest, state);
            }

            if (questDatas.Count > 0) {
                await SaveQuestProgress(player, questDatas);
            }
        }

        private void AddQuestDatas(List<QuestData2> questDatas, Player player, Quest quest, QuestState state) {
            List<QuestStepData> questStepDatas = new List<QuestStepData>();
            foreach (KeyValuePair<int, List<QuestStep>> keyValuePair in state.Options) {
                foreach (QuestStep step in keyValuePair.Value) {
                    QuestStepData stepData = new QuestStepData {
                        step = step.ID,
                        isStepComplete = step.complete ? 1 : 0,
                        nextState = keyValuePair.Key,
                        stepDescription = step.Text
                    };

                    questStepDatas.Add(stepData);
                }
            }

            QuestProgressData questProgressData = new QuestProgressData {
                questProgressId = player.QuestManager.ID,
                characterId = (int)player.UID,
                currentState = state.Number,
                steps = questStepDatas
            };

            QuestData2 questData = new QuestData2 {
                characterId = (int)player.UID,
                questId = quest.ID,
                questName = quest.Name,
                questDescription = quest.Description,
                completedState = quest.CompletedState,
                progress = new List<QuestProgressData> { questProgressData }
            };

            questDatas.Add(questData);
        }

        public async Task SaveQuestProgress(Player player, List<QuestData2> questDatas) {

            Response res = await SaveQuest(questDatas);

            if (res.Error) {
                string error;
                if (res.ErrorObject == null)
                    error = "Unknown Error";
                else
                    error = res.ErrorObject.errorCode + " - " + res.ErrorObject.error + " - " + res.ErrorObject.internalError;

                Server.ErrorDB("Error from AID/CID: " + player.AccountID + "/" + player.UID + ": " + error);
            } else {
                List<QuestData> receivedQuestDatas = res.value as List<QuestData>;
                player.QuestManager.LoadQuestProgress(receivedQuestDatas);
            }
        }

        public async void SaveCharacter(Player player)
        {

            if (player == null)
                return;

            await SaveAllQuestProgress(player);

            Root root = new Root();
            root.username = player.Name;
            root.clan = null;
            root.title = player.TitleManager.CurrentTitle?.ID;
            root.characterId = (int)player.UID;
            root.pId = (int)player.AccountID;
            root.activePet = null;
            root.appearance = player.appearanceData;
            
            root.bank = new List<Bank>();
            foreach (var i in player.Vault.items)
                if (i != null)
                    root.bank.Add(new Bank() { bankItemId = i.ID, bankItemAmount = i.Amount });

            root.equipment = new List<Equipment>();
            for (int il = 0; il < player.Equipment.EquippedItems.Length; il++)
            {
                Item i = player.Equipment.EquippedItems[il];
                if (i != null)
                    root.equipment.Add(new Equipment() { equipmentItemId = i.ID, equipmentItemAmount = i.Amount, slotId = il });
            }

            root.inventory = new List<Inventory>();
            foreach (var i in player.Inventory.items)
                if (i != null)
                    root.inventory.Add(new Inventory() { inventoryItemId = i.Item.ID, inventoryItemAmount = i.Item.Amount });

            var pos = player.transform.position;
            var rot = player.transform.rotation;
            if(Math.Abs(pos.X) > 100000 || Math.Abs(pos.Y) > 100000 || Math.Abs(pos.Z) > 100000) {
                pos = Locations.PLAYER_SPAWN_POINT;
                player.MapID = 0;
            }
            root.location = new List<Location>() { new Location() { posX = pos.X, posY = pos.Y, posZ = pos.Z, rotX = rot.X, rotY = rot.Y, rotZ = rot.Z, map = player.MapID } };

            root.skills = new List<Skill>();
            var sk = player.Skills;
            for (int i = 0; i < Enum.GetNames(typeof(Stats.SKILLS)).Length; i++)
                root.skills.Add(new Skill { skillExperience = sk.GetExp(i), skillLevel = sk.GetCurLevel(i), skillName = Enum.GetName(typeof(Stats.SKILLS), i) });

            root.characterKills = new List<KillData>();
            foreach (KeyValuePair<int, PlayerBestiary> keyValuePair in player.Bestiary) {
                foreach (DateTime killTime in keyValuePair.Value.GetUnsavedKills()) {
                    KillData killData = new KillData {
                        npcId = keyValuePair.Key,
                        createdTimestamp = killTime.ToString("yyyy-MM-dd hh:mm:ss" + ".000")
                    };
                    root.characterKills.Add(killData);
                }
                keyValuePair.Value.UpdateKillsSaved();
            }



            root.properties = new List<Property>();
            root.properties.Add(new Property() { propertyName = "Health", propertyValue = player.CurrentHealth.ToString() });
            root.properties.Add(new Property() { propertyName = "Stance", propertyValue = player.Stance.ToString() });

            Response res = await SaveCharacter(root);
            var thing = res.value as SaveConfirmation;
            string error = null;

            if (res.Error)
            {
                if (res.ErrorObject == null)
                    error = "Unknown Error";
                else
                    error = res.ErrorObject.errorCode + " - " + res.ErrorObject.error + " - " + res.ErrorObject.internalError;

                Server.ErrorDB("Error from AID/CID: " + player.AccountID + "/" + player.UID + ": " + error);
            }
        }

        public async void SetOnline(Player player, bool online)
        {
            //Server.Log(player.Describe() + " set API to " + (online ? "online" : "OFFLINE"));
            Response res = await SetOnline((int)player.UID, online ? int.Parse(Server.SERVER_World) : 0);
           
            //  Server.Log("saved online");
        }

  


    }
}

