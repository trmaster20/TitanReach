using System;
using System.Collections.Generic;
using System.Text;

namespace TRShared.Data
{
    public class DamageCalculater
    {
        public enum DamageType
        {
            MELEE = 0,
            RANGED = 1,
            MAGIC = 2
        }

        private static readonly float SLM = 0.12f;      // Strength Level Multiplier
        private static readonly float WSM = 0.08f;      // Weapon Strength Multiplier
        private static readonly float SWM = 0.001f;     // SLM/WSM Multiplier
        private static readonly float SO = 1.0f;        // Strength Offset

        private static readonly float ALM = 0.16f;      // Attack Level Multiplier
        private static readonly float WAM = 0.1f;       // Weapon Attack Multiplier
        private static readonly float AWM = 0.004f;     // ALM/WAM Multiplier
        private static readonly float AO = 0.0f;        // Attack Offset

        private static readonly float DLM = 0.12f;      // Defence Level Multiplier
        private static readonly float ADM = 0.06f;      // Armour Defence Multiplier
        private static readonly float DAM = 0.001f;     // ALM/WAM Multiplier
        private static readonly float DO = 0.0f;        // Defence Offset

        // SL - Strength Level
        // WSB - Weapon Strength Bonus
        // AL - Attack Level
        // WAB - Weapon Attack Bonus
        // DL - Attack Level
        // ADB - Armour Defence Bonus

        public enum DamageProfiles
        {
            LINEAR = 0,
            SWORD = 1,
            GREATSWORD = 2,
            BATTLEAXE = 3,
            DAGGER = 4
        }

        public struct DamageProfile
        {
            public float p0;
            public float p1;
            public float p2;
            public float p3;

            public DamageProfile(float p0, float p1, float p2, float p3)
            {
                this.p0 = p0;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
            }
        }

        public static DamageProfile LinearProfile = new DamageProfile(0, 1 / 3.0f, 2 / 3.0f, 1);
        public static DamageProfile SwordProfile = new DamageProfile(0, 1 / 3.0f, 2 / 3.0f, 1);
        public static DamageProfile GreatSwordProfile = new DamageProfile(0, 0.9f, 0.1f, 1);
        public static DamageProfile BattleAxeProfile = new DamageProfile(0, 0, 1, 1);
        public static DamageProfile DaggerProfile = new DamageProfile(0, 0, 0.6f, 1);


        private static System.Random random = new System.Random();

        private static float GetMaxHit(int SL, int WSB)
        {
            return ((SL * SLM) + (WSB * WSM) + (SL * WSB * SWM) + SO) * 10.0f;
        }

        private static float GetAttackBonus(int AL, int WAB)
        {
            return (AL * ALM * 30) + (WAB * WAM) + (AL * WAB * AWM) + AO;
        }

        private static float GetDefenceBonus(int DL, int ADB)
        {
            return (DL * DLM) + (ADB * ADM) + (DL * ADB * DAM) + DO;
        }

        private static float GetHitChance(int AL, int WAB, int DL, int ADB)
        {
            return 1.0f / (1.0f + (GetDefenceBonus(DL, ADB) / GetAttackBonus(AL, WAB)));
        }

        private static float DiceRoll()
        {
            return (float)random.NextDouble();
        }

        private static float DiceRoll(float multiplyer)
        {
            float pureRandom = (float)random.NextDouble();
            float newRoll = 2 * (1 - pureRandom) * pureRandom * multiplyer + pureRandom * pureRandom;
            // Server.Log("Roll Check: " + multiplyer + "  " + pureRandom + "  " + newRoll);
            return newRoll;
        }

        private static float DiceRollForced(float force)
        {
            return force;
        }

        private static float DiceRollForced(float multiplyer, float force)
        {
            float pureRandom = force;
            float newRoll = 2 * (1 - pureRandom) * pureRandom * multiplyer + pureRandom * pureRandom;
            // Server.Log("Roll Check: " + multiplyer + "  " + pureRandom + "  " + newRoll);
            return newRoll;
        }


        private static float GetRollMultiplyer(int AL, int WAB, int DL, int ADB)
        {
            if (AL == 0) AL = 1;
            if (WAB == 0) WAB = 1;
            if (DL == 0) DL = 1;
            if (ADB == 0) ADB = 1;

            float LevelPercent = AL / (float)DL;
            float BonusPercent = WAB / (float)ADB;

            LevelPercent = LevelPercent > 2 ? 2 : LevelPercent;
            BonusPercent = BonusPercent > 2 ? 2 : BonusPercent;

            return (LevelPercent + BonusPercent) / 4;
        }

        private static int CalculateDamage(float maxHit, float dr, DamageProfiles profile)
        {
            DamageProfile dp = LinearProfile;
            switch (profile)
            {
                case DamageProfiles.LINEAR:
                    dp = LinearProfile;
                    break;
                case DamageProfiles.SWORD:
                    dp = SwordProfile;
                    break;
                case DamageProfiles.GREATSWORD:
                    dp = GreatSwordProfile;
                    break;
                case DamageProfiles.BATTLEAXE:
                    dp = BattleAxeProfile;
                    break;
                case DamageProfiles.DAGGER:
                    dp = DaggerProfile;
                    break;
            }

            //1d bezier curve calculation
            //float normalisedDamage = Formula.Evaluate1DCubicBezier(dr, dp.p1, dp.p2);

            // TODO - think about rounding here
            return (int)(dr * maxHit);
        }

        private static int GetDamageForceRoll(int AL, int AB, int SL, int SB, int DL, int DB, float roll)
        {
            float hitChance = GetHitChance(AL, AB, DL, DB);
            float hitRoll = DiceRollForced(roll);
            if (hitRoll < hitChance)
            {
                float maxHit = GetMaxHit(SL, SB);
                float rollMultiplyer = GetRollMultiplyer(AL, AB, DL, DB);
                //Server.Log(rollMultiplyer);
                //Server.Log(AL + " " + AB + " " + DL + " " + DB);
                //float damageRoll = DiceRoll();
                float damageRoll = DiceRollForced(rollMultiplyer, roll);

                int damage = CalculateDamage(maxHit, damageRoll, DamageProfiles.LINEAR);
                return damage;
            }
            return 0;
        }

        private static int GetDamage(int AL, int AB, int SL, int SB, int DL, int DB)
        {
            float hitChance = GetHitChance(AL, AB, DL, DB);
            float hitRoll = DiceRoll();
            if (hitRoll < hitChance)
            {
                float maxHit = GetMaxHit(SL, SB);
                float rollMultiplyer = GetRollMultiplyer(AL, AB, DL, DB);
                //Server.Log(rollMultiplyer);
                //Server.Log(AL + " " + AB + " " + DL + " " + DB);
                //float damageRoll = DiceRoll();
                float damageRoll = DiceRoll(rollMultiplyer);

                int damage = CalculateDamage(maxHit, damageRoll, DamageProfiles.LINEAR);
                return damage;
            }
            return 0;
        }

        public static int CalculateAttack(int AL, int AB, int PL, int PB, int DL, int DB)
        {
            int damage = GetDamage(AL, AB, PL, PB, DL, DB);
            return damage;

        }

        public static int CalculateAttackForceRoll(int AL, int AB, int PL, int PB, int DL, int DB, float roll)
        {
            int damage = GetDamageForceRoll(AL, AB, PL, PB, DL, DB, roll);
            return damage;

        }
    }
}
