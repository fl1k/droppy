using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Discord.WebSocket;
using Discord;
using Discord.Commands;
using Droppy_v1.Handlers;


namespace Droppy_v1
{
    class UserDataHandler
    {
        public static userDB UserDB;
        public static Declarations dec = new Declarations();
        static logHandler logHandler = new logHandler();

        public static string userRegister(ulong userID, string SCUsername)
        {
            if (IsRegistered(userID))
            {
                return "This discord account is already linked to a SC account.";
            }
            else
            {
                if (IsSCRegistered(SCUsername))
                    return "This social club account is already linked to a Discord account.";
                else
                {
                    UserDB = new userDB();
                    UserDB.userID = userID;
                    UserDB.SCUsername = SCUsername;
                    UserDB.inQueue = false;
                    UserDB.Lobby = 0;
                    UserDB.AfkCheckResult = false;
                    UserDB.DropPending = false;
                    UserDB.registerDate = DateTime.Now;
                    File.WriteAllText(dec.userdbPath + userID + ".json", JsonConvert.SerializeObject(UserDB, Formatting.Indented));
                    logHandler.WriteLine($"[{DateTime.Now}] [Info] User file created for {userID}, {SCUsername}.");
                    return $"Success. You've linked your discord account (`{userID}`) to your social club account [`{SCUsername}`].";
                }
            }
        }

        public static bool IsSCRegistered(string SCUsername)
        {
            foreach (string userFile in Directory.EnumerateFiles(dec.userdbPath))
            {
                UserDB = JsonConvert.DeserializeObject<userDB>(File.ReadAllText(userFile));
                if (UserDB.SCUsername == SCUsername)
                    return true;
            }
            return false;
        }

        public static DateTime GetRegisterDate(ulong userID)
        {
            LoadUser(userID);
            return UserDB.registerDate;
        }

        public static bool IsRegistered(ulong userID)
        {
            if (File.Exists($"{dec.userdbPath}{userID}.json"))
                return true;
            else
                return false;
        }

        public static void SetUserLobby(ulong userID, ulong lobby)
        {
            LoadUser(userID);
            UserDB.Lobby = lobby;
            SaveUser(userID);
        }

        public static bool IsInQueue(ulong userID)
        {
            LoadUser(userID);
            return UserDB.inQueue;
        }

        public static void LoadUser(ulong userID)
        {
            try
            {
                UserDB = JsonConvert.DeserializeObject<userDB>(File.ReadAllText(dec.userdbPath + userID + ".json"));
            }
            catch (Exception ex)
            {
                logHandler.WriteLine($"[{DateTime.Now}] [ERROR] {ex.Message}");
            }
        }

        public static void SaveUser(ulong userID)
        {
            try
            {
                File.WriteAllText(dec.userdbPath + userID + ".json", JsonConvert.SerializeObject(UserDB, Formatting.Indented));
            }
            catch (Exception ex)
            {
                logHandler.WriteLine($"[{DateTime.Now}] [ERROR] [{ex.Source}]: {ex.Message}");
            }
        }

        public static int GetQueueSize()
        {
            return File.ReadAllLines(dec.queuePath).Count();
        }

        public static int? GetQueuePosition(ulong userID)
        {
            List<string> queue = File.ReadAllLines(dec.queuePath).ToList();
            for (int i = 0; i < queue.Count(); i++)
            {
                if (queue[i] == userID.ToString())
                {
                    return i + 1;
                }
            }
            return null;
        }

        public static void RemoveFromQueue(ulong userID)
        {
            if (IsRegistered(userID))
            {
                List<string> queue = File.ReadAllLines(dec.queuePath).ToList();
                queue.Remove(userID.ToString());
                File.WriteAllLines(dec.queuePath, queue.ToArray());
                LoadUser(userID);
                UserDB.inQueue = false;
                SaveUser(userID);
            }
        }

        public static void Unregister(ulong userID)
        {
            File.Delete($@"{dec.userdbPath}\{userID}.json");
        }

        public static ulong[] GetOpenLobbies()
        {
            List<string> lobbies = Directory.EnumerateFiles(dec.lobbiesPath).ToList();
            ulong[] ids = new ulong[lobbies.Count()];
            for (int i = 0; i < lobbies.Count(); i++)
            {
                ids[i] = Convert.ToUInt64(Path.GetFileName(lobbies[i]));
            }
            return ids;
        }

        public static string GetSCUsername(ulong userID)
        {
            LoadUser(userID);
            return UserDB.SCUsername;
        }

        public static void RemoveFromLobby(ulong userID)
        {
            LoadUser(userID);
            ulong lobbyID = UserDB.Lobby;
            List<string> lobbyParticipants = File.ReadAllLines(dec.lobbiesPath + lobbyID).ToList();
            lobbyParticipants.Remove(userID.ToString());
            File.WriteAllLines(dec.lobbiesPath + lobbyID, lobbyParticipants);
            SetUserLobby(userID, 0);
        }

        public static int GetLobbySize(ulong userID)
        {
            if (File.Exists($"{dec.lobbiesPath}{userID}"))
            {
                string lobby = $"{dec.lobbiesPath}{userID}";
                return File.ReadAllLines($"{dec.lobbiesPath}{userID}").Count();
            }
            else
            {
                return 0;
            }
        }
    }

    public struct userDB
    {
        public ulong userID;
        public string SCUsername;
        public DateTime registerDate;
        public bool inQueue;
        public ulong Lobby;
        public bool AfkCheckResult;
        public bool DropPending;
    }
}
