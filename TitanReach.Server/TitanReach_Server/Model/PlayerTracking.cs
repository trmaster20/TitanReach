using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace TitanReach_Server.Model {
    public class PlayerTracking {
        private const int VELOCITY_CHECK_DURATION = 30000000;
        private const float VELOCITY_CAP = 0.12f;
        private const float LARGE_CAP = 0.2f;
        private const int PACKET_THRESHOLD = 350;
        private const int PACKET_THRESHOLD_TIME = 100000000;

        private Player _player;
        private List<float> _velocities;
        private long _lastMovement = DateTime.Now.Ticks;
        private long _lastVelocityCheck = DateTime.Now.Ticks;
        public Vector3 _lastVelocityCheckLocation;

        private long _packetThresholdTime = DateTime.Now.Ticks;
        private int _packetThreshold;
        private static Dictionary<uint, DateTime> nextStuckTime = new Dictionary<uint, DateTime>();

        public long LastRoll = DateTime.Now.Ticks;
        public long LastTP = DateTime.Now.Ticks;
        private int _lastLand;

        private List<String> interactTracking;
        private int lastInteractTime = 0;
        public List<String> attackTracking;
        private int lastAttackTime = 0;
        public bool tracked = false;

        public PlayerTracking(Player player) {
            _player = player;
            _velocities = new List<float>();
            interactTracking = new List<string>();
            attackTracking = new List<string>();
        }

        public int PacketThreshold {

            get => _packetThreshold;
            set {
                _packetThreshold = value;
                if (DateTime.Now.Ticks - _packetThresholdTime > PACKET_THRESHOLD_TIME) {
                    if (_packetThreshold > PACKET_THRESHOLD)
                        _player.Msg("Warning: Packet threshhold exceeded " + _packetThreshold + "/" + PACKET_THRESHOLD + " <- TELL UNRAVEL");
                    _packetThresholdTime = DateTime.Now.Ticks;
                    _packetThreshold = 0;
                }
            }
        }

        public void TrackPlayer(int time) {
            if (!StartTracking()) {
                return;
            }

            Timer t = new Timer();
            t.Interval = time * 1000;
            t.AutoReset = false;
            t.Elapsed += new ElapsedEventHandler(TrackingTimerElapsed);
            t.Start();
        }

        private void TrackingTimerElapsed(object sender, ElapsedEventArgs e) {
            EndTracking();
        }

        public void EndTracking() {
            if (!tracked) {
                return;
            }

            StopTracking();

            string path = _player.Name + " tracking.txt";
            try {
                SaveTrackingToFile(path);

            } catch (Exception ex) {
                Server.Error(ex.Message);
            }

            Discord.SendFileDB(path, _player.Name + " tracking data");
            
            Timer t = new Timer();
            t.Interval = 2000;
            t.AutoReset = false;
            t.Elapsed +=  delegate { DeleteTracking(path); };
            t.Start();
        }

        private void DeleteTracking(string path) {
            try {
                File.Delete(path);
            } catch (Exception ex) {
                Server.Error(ex.Message);
            }
        }

        private void SaveTrackingToFile(string path){
            List<string> allLines = new List<string>();
            allLines.Add("Logs for " + _player.Name);
            allLines.Add("Object Tracking for " + _player.Name);
            allLines.AddRange(interactTracking);
            allLines.Add("Attack Tracking for " + _player.Name);
            allLines.AddRange(attackTracking);

            File.WriteAllLines(path, allLines);
        }

        private bool StartTracking() {
            if (tracked) {
                return false;
            }

            tracked = true;
            interactTracking.Clear();
            attackTracking.Clear();
            return true;
        }

        private void StopTracking() {
            tracked = false;
        }

        public void TrackInteraction(string objectName) {
            if (!tracked) {
                return;
            }

            int newInteractTime = Environment.TickCount;

            string info = "";
            info += "[" + Environment.TickCount + "] ";
            info += "Interact: " + objectName;

            if (lastInteractTime != 0) {
                int timeDiff = newInteractTime - lastInteractTime;
                info += "(" + timeDiff +"ms since last interact)"; 
            }
            lastInteractTime = newInteractTime;
            interactTracking.Add(info);
        }

        public void TrackAttacking(string objectName) {
            if (!tracked) {
                return;
            }

            int newAttackTime = Environment.TickCount;

            string info = "";
            info += "[" + Environment.TickCount + "] ";
            info += "Attack: " + objectName;

            if (lastAttackTime != 0) {
                int timeDiff = newAttackTime - lastAttackTime;
                info += "(" + timeDiff + "ms since last attack)";
            }
            lastAttackTime = newAttackTime;
            attackTracking.Add(info);
        }

        public bool IsTooFast(Vector3 newPosition, int LandID) {
            if(LandID != _lastLand)
            {
                _lastLand = LandID;
                _velocities.Clear();
                _lastVelocityCheck = DateTime.Now.Ticks;
                _lastVelocityCheckLocation = _player.transform.position;
                return false;
            }
            TrackVelocity(newPosition);

            if (DateTime.Now.Ticks - _lastVelocityCheck >= VELOCITY_CHECK_DURATION) {
                float averageVelocity = CalcAverageVelocity();
                _velocities.Clear();
                float cap = VELOCITY_CAP;
                if (LastRoll + 50000000 > _lastVelocityCheck || LastTP + 50000000 > _lastVelocityCheck) {
                    cap = LARGE_CAP;
                }
                if (averageVelocity > cap ) {//&& _player.Rank != Utilities.Rank.ADMIN) {
                    _lastVelocityCheck = DateTime.Now.Ticks;
                    _player.transform.position = _lastVelocityCheckLocation;
                  //  _lastVelocityCheckLocation = _player.transform.position;
                    Server.Log("Player " + _player.UID + ": " + _player.Name + " is moving too fast, check for speedhacking: " + averageVelocity);
                    return true;
                }

                _lastVelocityCheck = DateTime.Now.Ticks;
                _lastVelocityCheckLocation = _player.transform.position;
                return false;
            }

            return false;
        }

        public static TimeSpan StuckCooldown(uint id) {
            if (!nextStuckTime.ContainsKey(id))
                return TimeSpan.Zero;

            return nextStuckTime[id] - DateTime.Now;
        }

        public static void UpdateStuckTime(uint id) {
            nextStuckTime[id] = DateTime.Now.AddMinutes(10);
        }

        private void TrackVelocity(Vector3 newPosition) {
            float diffX = _player.transform.position.X - newPosition.X;
            float diffZ = _player.transform.position.Z - newPosition.Z;
            double timeDiff = Math.Round((DateTime.Now.Ticks - _lastMovement) / 100000.0);
            if (timeDiff == 0 || timeDiff == double.NaN) {
                _lastMovement = DateTime.Now.Ticks;
                return;
            }

            double velocity = Math.Sqrt((double)(diffX * diffX + diffZ * diffZ)) / timeDiff;

            _velocities.Add((float) velocity);
            _lastMovement = DateTime.Now.Ticks;
        }

        private float CalcAverageVelocity() {
            if(_velocities.Count <= 0) {
                return 0;
            }
            return _velocities.Sum() / _velocities.Count;
        }


    }
}
