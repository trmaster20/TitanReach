using System;

namespace TitanReach_Server.Model
{

    public class Vector3
    {

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public Vector3()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Vector3(float xVal, float yVal, float zVal)
        {
            X = xVal;
            Y = yVal;
            Z = zVal;
        }

        public Vector3(System.Numerics.Vector3 vec)
        {
            X = vec.X;
            Y = vec.Y;
            Z = vec.Z;
        }

        public static Vector3 operator +(Vector3 mv1, Vector3 mv2)
        {
            return new Vector3(mv1.X + mv2.X, mv1.Y + mv2.Y, mv1.Z + mv2.Z);
        }

        public static Vector3 operator -(Vector3 mv1, Vector3 mv2)
        {
            return new Vector3(mv1.X - mv2.X, mv1.Y - mv2.Y, mv1.Z - mv2.Z);
        }

        public static Vector3 operator -(Vector3 mv1, float var)
        {
            return new Vector3(mv1.X - var, mv1.Y - var, mv1.Z - var);
        }

        public static Vector3 operator *(Vector3 mv1, Vector3 mv2)
        {
            return new Vector3(mv1.X * mv2.X, mv1.Y * mv2.Y, mv1.Z * mv2.Z);
        }

        public static Vector3 operator *(Vector3 mv, float var)
        {
            return new Vector3(mv.X * var, mv.Y * var, mv.Z * var);
        }

        public static Vector3 operator %(Vector3 mv1, Vector3 mv2)
        {
            return new Vector3(mv1.Y * mv2.Z - mv1.Z * mv2.Y,
                                 mv1.Z * mv2.X - mv1.X * mv2.Z,
                                 mv1.X * mv2.Y - mv1.Y * mv2.X);
        }

        public float this[int key]
        {
            get
            {
                return GetValueByIndex(key);
            }
            set { SetValueByIndex(key, value); }
        }

        private void SetValueByIndex(int key, float value)
        {
            if (key == 0) X = value;
            else if (key == 1) Y = value;
            else if (key == 2) Z = value;
        }

        private float GetValueByIndex(int key)
        {
            if (key == 0) return X;
            if (key == 1) return Y;
            return Z;
        }

        public float DotProduct(Vector3 mv)
        {
            return X * mv.X + Y * mv.Y + Z * mv.Z;
        }

        public Vector3 ScaleBy(float value)
        {
            return new Vector3(X * value, Y * value, Z * value);
        }

        public Vector3 ComponentProduct(Vector3 mv)
        {
            return new Vector3(X * mv.X, Y * mv.Y, Z * mv.Z);
        }

        public void ComponentProductUpdate(Vector3 mv)
        {
            X *= mv.X;
            Y *= mv.Y;
            Z *= mv.Z;
        }

        public Vector3 VectorProduct(Vector3 mv)
        {
            return new Vector3(Y * mv.Z - Z * mv.Y,
                                 Z * mv.X - X * mv.Z,
                                 X * mv.Y - Y * mv.X);
        }

        public float ScalarProduct(Vector3 mv)
        {
            return X * mv.X + Y * mv.Y + Z * mv.Z;
        }

        public void AddScaledVector(Vector3 mv, float scale)
        {
            X += mv.X * scale;
            Y += mv.Y * scale;
            Z += mv.Z * scale;
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public float SquareMagnitude()
        {
            return X * X + Y * Y + Z * Z;
        }

        public void Trim(float size)
        {
            if (SquareMagnitude() > size * size)
            {
                Normalize();
                X *= size;
                Y *= size;
                Z *= size;
            }
        }

        public void Normalize()
        {
            float m = Magnitude();
            if (m > 0)
            {
                X = X / m;
                Y = Y / m;
                Z = Z / m;
            }
            else
            {
                X = 0;
                Y = 0;
                Z = 0;
            }
        }

        public Vector3 Inverted()
        {
            return new Vector3(-X, -Y, -Z);
        }

        public Vector3 Unit()
        {
            Vector3 result = this;
            result.Normalize();
            return result;
        }

        public void Clear()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public static float Distance(Vector3 mv1, Vector3 mv2)
        {
            return (mv1 - mv2).Magnitude();
        }



        public static Vector3 Zero()
        {
            return new Vector3(0f, 0f, 0f);
        }

        //public static Vector3[] ReturnAsVector3(MyVector3[] mv3)
        //{
        //  Vector3[] v3 = new Vector3[mv3.Length];

        //  return v3;
        //}



    }
}
