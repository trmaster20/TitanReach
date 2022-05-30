using TitanReach_Server.Model;

namespace TitanReach_Server.Utilities
{
    public static class Locations
    {

        public static Vector3 PLAYER_RESPAWN_POINT
        {
            get
            {
                return new Vector3(-196f, 88.8f, 832);
            }
        }

        public static Vector3 PVP_ISLAND_SPAWN = new Vector3(134, -80f, 284);
        public static Vector3 PVP_ISLAND_EXIT_ALYSSIA_SPAWN = new Vector3(-151f, 109f, 768);
        public static Vector3 DEV_WORLD_SPAWN = new Vector3(56.5f, 104, 67.1f);
        public static Vector3 OASIS_SPAWN = new Vector3(2661f, 39f, 1177f);
        public static Vector3 DROMHEIM_SPAWN = new Vector3(624.83f, 101.75f, 598.26f);
        public static Vector3 PLAYER_SPAWN_POINT
        {
            get
            {
                return new Vector3(-186f, 85f, 1017);
                // return new Vector3(184, DataManager.GetHeight(184, 87) + 0.5f, 87f);
                // return new Vector3(-177.8567f, 14.10289f, 276.0712f);
            }
        }
    }
}
