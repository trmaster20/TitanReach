namespace TitanReach_Server.Network
{
    public static class Packets
    {

        public static ushort LOGIN = 1;
        public static ushort NULL_LENGTH = 2;
        public static ushort LOGIN_OK = 3;
        public static ushort SPAWN_PLAYER = 4;
        public static ushort MOVE = 5;
        public static ushort PING = 6;
        public static ushort CHAT_MESSAGE = 7;
        public static ushort NPC = 8;
        public static ushort SYNC_POS = 9;
        public static ushort INVENTORY_SYNC = 11;
        public static ushort ITEM = 12;
        public static ushort EQUIP_UPDATE = 14;
        public static ushort UNEQUIP_ITEM = 15;
        public static ushort ANIMATION = 16;

        public static ushort OBJECT = 19;
        public static ushort GROUND_ITEM = 21;
        public static ushort LOCATION_UPDATE = 22;

        public static ushort PLAYER_ATTACK_NPC = 26;
        public static ushort GAME_READY = 33;
        public static ushort SYNC_EXP = 34;
        public static ushort PROJECTILE = 35;
        public static ushort PLAY_SOUND = 36;
        public static ushort SEND_DEATH = 37;
        public static ushort USE_ITEM_ON_ITEM = 38;
        public static ushort VAULT = 39;

        public static ushort PLAYER_DAMAGE_PLAYER = 42;
        public static ushort DAMAGE = 43;
        public static ushort SPELL_TARGET_GROUND = 45;
        public static ushort BUSY_FLAG = 46;

        public static ushort DIALOG_ACTION = 51;

        public static ushort ADMIN_MODE = 54;
        public static ushort SKILL_ACTION_TRIGGER = 55;
        public static ushort SHOP = 56;
        public static ushort SYNC_PLAYER = 58;
        public static ushort INDICATOR_UPDATE = 60;
        public static ushort QUEST = 61;
        public static ushort PLAYER_CUSTOM_UPDATE = 62;
        public static ushort APPEARANCE_CREATED = 63;
        public static ushort ADD_ITEM_TO_HOTBAR = 64;
        public static ushort OPEN_CRAFTING_MENU = 65;
        public static ushort UPDATE_BUFF = 66;  
        public static ushort LOGOUT_REQUEST = 68;

        public static ushort PLAYER_RANK = 72;
        public static ushort COMBAT_STANCE = 74;
        public static ushort CHARACTER_CREATOR = 75;
        public static ushort TRADE = 76;
        public static ushort PARTY = 77;
        public static ushort FOLLOW = 78;

        public static ushort FRIENDS = 79;
        public static ushort BUG_REPORT = 80;
        public static ushort PET = 81;
        public static ushort TITLE = 82;
        public static ushort EMOTE = 83;
        public static ushort STATS = 84;
        public static ushort MAP_CHANGE = 85;
    }
}
