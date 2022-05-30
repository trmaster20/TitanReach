namespace TitanReach_Server.Model
{
    public class Transform
    {

        public Transform(Vector3 pos, Vector3 rot, Vector3 sca)
        {
            this.position = pos;
            this.rotation = rot;
            this.scale = sca;
        }

        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }
}
