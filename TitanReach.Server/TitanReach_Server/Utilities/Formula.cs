using System;
using System.Linq;
using System.Text.RegularExpressions;
using TitanReach_Server.Model;
using TitanReach_Server.Utilities;

namespace TitanReach_Server
{

    public class Formula
    {


        public static int GetIndex(int needle, int[] haystack)
        {
            for (int i = 0; i < haystack.Length; i++)
                if (haystack[i] == needle)
                    return i;

            return 0;
        }

        public static int GetIndex(int needle, ushort[] haystack)
        {
            for (int i = 0; i < haystack.Length; i++)
                if (haystack[i] == needle)
                    return i;

            return 0;
        }

        public static int CombatLevel(int dex, int str, int def, int hp, int range, int magic)
        {
            int melleeLevel = (int)(((dex + str + def + hp) - 18.0) * (99.0 / (396.0 - 18.0)) + 1.0);
            int magicLevel = (int)(((2.0 * magic + def + hp) - 18.0) * (99.0 / (396.0 - 18.0)) + 1.0);
            int rangeLevel = (int)(((2.0 * range + def + hp) - 18.0) * (99.0 / (396.0 - 18.0)) + 1.0);
            int combatLevel = 1;
            if (melleeLevel > combatLevel) combatLevel = melleeLevel;
            if (magicLevel > combatLevel) combatLevel = magicLevel;
            if (rangeLevel > combatLevel) combatLevel = rangeLevel;
            if (combatLevel > 99.9) return 100;
            return combatLevel;
        }
        public static string StripEmail(string s)
        {
            return Regex.Replace(s, @" ^ ([\w\.\-] +)@([\w\-] +)((\.(\w){ 2,3})+)$", String.Empty);
        }

        public static string Strip(string s)
        {
            return Regex.Replace(s, "[^a-zA-Z0-9 ]", String.Empty);
        }
        public static bool InRange(Vector3 one, Vector3 two, int dist)
        {
            if ((int)one.X - dist <= (int)two.X && (int)one.X + dist >= (int)two.X)
                if ((int)one.Z - dist <= (int)two.Z && (int)one.Z + dist >= (int)two.Z)
                    return true;

            return false;
        }

        public static bool InRadius(Player p1, Player p2, int dist) {
            return InRadius(p1.transform.position, p2.transform.position, dist);
        }
        public static bool InRadius(Vector3 one, Vector3 two, int dist)
        {
            return (one - two).Magnitude() <= (int)dist;
        }

        public static bool InRadius(Vector3 one, Vector3 two, float dist)
        {
            return (one - two).Magnitude() <= dist;
        }

        public static bool InRadiusXZ(Vector3 one, Vector3 two, int dist)
        {
            one.Y = 0;
            two.Y = 0;
            return (one - two).Magnitude() < (int)dist;
        }

        public static float Lerp(float firstFloat, float secondFloat, float ratio)
        {
            return firstFloat * (1 - ratio) + secondFloat * ratio;
        }

        public static Vector3 Lerp(Vector3 firstVector, Vector3 secondVector, float ratio)
        {
            float retX = Lerp(firstVector.X, secondVector.X, ratio);
            float retY = Lerp(firstVector.Y, secondVector.Y, ratio);
            float retZ = Lerp(firstVector.Z, secondVector.Z, ratio);

            return new Vector3(retX, retY, retZ);
        }

        public static bool CapsuleCollision(Vector3 ATip, Vector3 Abase, float ARadius, Vector3 BTip, Vector3 Bbase, float BRadius)
        {
            // capsule A:
            Vector3 a_Normal = (ATip - Abase);
            a_Normal.Normalize();
            Vector3 a_LineEndOffset = a_Normal * ARadius;
            Vector3 a_A = Abase + a_LineEndOffset;
            Vector3 a_B = ATip - a_LineEndOffset;

            // capsule B:
            Vector3 b_Normal = (BTip - Bbase);
            b_Normal.Normalize();
            Vector3 b_LineEndOffset = b_Normal * BRadius;
            Vector3 b_A = Bbase + b_LineEndOffset;
            Vector3 b_B = BTip - b_LineEndOffset;

            // vectors between line endpoints:
            Vector3 v0 = b_A - a_A;
            Vector3 v1 = b_B - a_A;
            Vector3 v2 = b_A - a_B;
            Vector3 v3 = b_B - a_B;

            // squared distances:
            float d0 = v0.DotProduct(v0);
            float d1 = v1.DotProduct(v1);
            float d2 = v2.DotProduct(v2);
            float d3 = v3.DotProduct(v3);

            // select best potential endpoint on capsule A:
            Vector3 bestA;
            if (d2 < d0 || d2 < d1 || d3 < d0 || d3 < d1)
            {
                bestA = a_B;
            }
            else
            {
                bestA = a_A;
            }

            // select point on capsule B line segment nearest to best potential endpoint on A capsule:
            Vector3 bestB = ClosestPointOnLineSegment(b_A, b_B, bestA);

            // now do the same for capsule A segment:
            bestA = ClosestPointOnLineSegment(a_A, a_B, bestB);


            Vector3 penetration_normal = bestA - bestB;
            float len = penetration_normal.Magnitude();
            penetration_normal.Normalize();
            float penetration_depth = ARadius + BRadius - len;
            bool intersects = penetration_depth > 0;

            return intersects;
        }

        public static Vector3 ClosestPointOnLineSegment(Vector3 A, Vector3 B, Vector3 Point)
        {
            Vector3 AB = B - A;
            float t = (Point - A).DotProduct(AB) / AB.DotProduct(AB);
            return A + AB * MathF.Min(MathF.Max(t, 0.0f), 1);
        }

        public static float Evaluate1DCubicBezier(float t, float p0, float p1, float p2, float p3)
        {
            float e0 = MathF.Pow(1 - t, 3) * p0;
            float e1 = 3 * MathF.Pow(1 - t, 2) * t * p1;
            float e2 = 3 * (1 - t) * MathF.Pow(t, 2) * p2;
            float e3 = MathF.Pow(t, 3) * p3;
            return e0 + e1 + e2 + e3;
        }

        public static float Evaluate1DCubicBezier(float t, float p1, float p2) // assumes p0 = 0, p3 = 1
        {
            float e1 = 3 * MathF.Pow(1 - t, 2) * t * p1;
            float e2 = 3 * (1 - t) * MathF.Pow(t, 2) * p2;
            float e3 = MathF.Pow(t, 3);
            return e1 + e2 + e3;
        }

        public static float Evaluate1DSquareBezier(float t, float p0, float p1, float p2)
        {
            float e0 = MathF.Pow(1 - t, 2) * p0;
            float e1 = 2 * MathF.Pow(1 - t, 2) * t * p1;
            float e2 = t * t * p2;
            return e0 + e1 + e2;
        }

        public static float Evaluate1DSquareBezier(float t, float p1) // assumes p0 = 0, p2 = 1
        {
            float e1 = 2 * MathF.Pow(1 - t, 2) * t * p1;
            float e2 = t * t;
            return e1 + e2;
        }

    }
}
