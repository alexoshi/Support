using System;
using System.CodeDom;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;
//using System.Windows.Media.Media3D;

namespace Support
{
    public struct Frame
    {
        private Matrix4x4 mat;
        private bool inv = false;
        private Matrix4x4 iMat;

        public Frame()
        {
            mat = Matrix4x4.Identity;
            iMat = Matrix4x4.Identity;
            inv = true;
        }

        public Frame(System.Windows.Media.Media3D.Matrix3D matrix3D)
        {
            this = new Frame(
            (float)matrix3D.M11, (float)matrix3D.M12, (float)matrix3D.M13,
            (float)matrix3D.M21, (float)matrix3D.M22, (float)matrix3D.M23,
            (float)matrix3D.M31, (float)matrix3D.M32, (float)matrix3D.M33,
            (float)matrix3D.OffsetX, (float)matrix3D.OffsetY, (float)matrix3D.OffsetZ, (float)matrix3D.M44
            , true);
        }
        public Frame(System.Windows.Media.Media3D.Point3D point, System.Windows.Media.Media3D.Quaternion quaternion)
        {
            Matrix3D matrix3D = new Matrix3D();
            matrix3D.Rotate(quaternion);
            matrix3D.OffsetX = (float)point.X;
            matrix3D.OffsetY = (float)point.Y;
            matrix3D.OffsetZ = (float)point.Z;
            this = new Frame(
            (float)matrix3D.M11, (float)matrix3D.M12, (float)matrix3D.M13,
            (float)matrix3D.M21, (float)matrix3D.M22, (float)matrix3D.M23,
            (float)matrix3D.M31, (float)matrix3D.M32, (float)matrix3D.M33,
            (float)matrix3D.OffsetX, (float)matrix3D.OffsetY, (float)matrix3D.OffsetZ, (float)matrix3D.M44
            , true);
        }

        public Frame Delta(Matrix3D value)
        {
            if (!inv)
            {
                Matrix4x4.Invert(mat, out iMat);
                inv = true;
            }
            Frame transform_new = new(value);
            return new Frame(Matrix4x4.Multiply(transform_new.mat, iMat));
        }
        public Frame Inverse()
        {
            if (!inv)
            {
                Matrix4x4.Invert(mat, out iMat);
                inv = true;
            }
            return new(iMat);
        }

        public Frame(Matrix4x4 frame)
        {
            mat = frame;
            iMat = Matrix4x4.Identity;
            inv = false;
        }

        public Frame(
            float m11, float m12, float m13,
            float m21, float m22, float m23,
            float m31, float m32, float m33,
            float offsetX, float offsetY, float offsetZ, float m44 = 1, bool normal = false)
        {
            iMat = Matrix4x4.Identity;
            inv = false;
            mat = new Matrix4x4();
            float nx = normal ? 1 : MathF.Sqrt(m11 * m11 + m12 * m12 + m13 * m13);
            mat.M11 = m11 / nx;
            mat.M12 = m12 / nx;
            mat.M13 = m13 / nx;
            float ny = normal ? 1 : MathF.Sqrt(m21 * m21 + m22 * m22 + m23 * m23);
            mat.M21 = m21 / ny;
            mat.M22 = m22 / ny;
            mat.M23 = m23 / ny;
            float nz = normal ? 1 : MathF.Sqrt(m31 * m31 + m32 * m32 + m33 * m33);
            mat.M31 = m31 / nz;
            mat.M32 = m32 / nz;
            mat.M33 = m33 / nz;

            mat.M41 = offsetX;
            mat.M42 = offsetY;
            mat.M43 = offsetZ;
            mat.M44 = m44;


            if (!normal && ((MathF.Abs(Vector3.Dot(Nx, Ny)) + MathF.Abs(Vector3.Dot(Nx, Nz))) > 1e-12))
            {
                Vector3 n_x = Nx;
                Vector3 n_y = Ny;
                Vector3 n_z = Vector3.Cross(n_x, n_y);
                n_y = Vector3.Cross(n_z, n_x);

                n_y = Vector3.Normalize(n_y);
                n_z = Vector3.Normalize(n_z);

                mat.M21 = n_y.X;
                mat.M22 = n_y.Y;
                mat.M23 = n_y.Z;
                mat.M31 = n_z.X;
                mat.M32 = n_z.Y;
                mat.M33 = n_z.Z;

                Matrix4x4.Invert(mat, out iMat);
                inv = true;
            }

        }
        public static Frame Identity => new(Matrix4x4.Identity);

        public Vector3 Nx => new(mat.M11, mat.M12, mat.M13);
        public Vector3 Ny => new(mat.M21, mat.M22, mat.M23);
        public Vector3 Nz => new(mat.M31, mat.M32, mat.M33);
        public Vector3 Translation
        {
            set { mat.Translation = value; }
            get { return mat.Translation; }
        }
        public float Scale => 1.0f / mat.M44;

        public Frame(Kuka kuka)
        {
            this = new Frame(kuka.X, kuka.Y, kuka.Z, kuka.A, kuka.B, kuka.C, kuka.Scale);
        }
        public Frame(double x, double y, double z, double a, double b, double c, double scale = 1)
        {
            this = new Kuka(x, y, z, a, b, c, scale).GetFrame();
        }
        public Frame(string input)
        {
            this = new Kuka(input).GetFrame();
        }

        public System.Windows.Media.Media3D.Matrix3D GetMatrix3D()
        {
            return new System.Windows.Media.Media3D.Matrix3D(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23, mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }
        public System.Windows.Media.Media3D.Point3D GetPoint3D()
        {
            return new System.Windows.Media.Media3D.Point3D(mat.M41, mat.M42, mat.M43);
        }
        public Kuka GetKuka()
        {
            return new Kuka(this);
        }

        public Vector3 Transform(Vector3 vector3)
        {
            return Vector3.Transform(vector3, mat);
        }
        public Frame AppendTransform(Frame frame)
        {
            return new(Matrix4x4.Multiply(frame.mat, mat));
        }
        public Frame PrependTransform(Frame frame)
        {
            return new(Matrix4x4.Multiply(mat, frame.mat));
        }
    }

    public struct Kuka
    {
        public double X;
        public double Y;
        public double Z;
        public double A;
        public double B;
        public double C;
        public double Scale;

        public Kuka(double x, double y, double z, double a, double b, double c, double scale = 1)
        {
            X = x; Y = y; Z = z; A = a; B = b; C = c; Scale = scale;
        }
        public Kuka(Frame asi)
        {
            Vector3 translation = asi.Translation;
            X = translation.X;
            Y = translation.Y;
            Z = translation.Z;

            Scale = asi.Scale;

            Vector3 nx = asi.Nx;
            Vector3 ny = asi.Ny;
            Vector3 nz = asi.Nz;

            double Rxx = nx.X;
            double Ryx = ny.X;
            double Rzx = nz.X;
            double Rxy = nx.Y;
            double Ryy = ny.Y;
            double Rzy = nz.Y;
            double Rxz = nx.Z;
            //double Ryz = ny.Z;
            //double Rzz = nz.Z;

            double norm = Math.Sqrt(Rxx * Rxx + Rxy * Rxy);

            if (norm > 1e-5)
            {
                double sa = Rxy / norm;
                double ca = Rxx / norm;
                A = Math.Atan2(sa, ca) / Math.PI * 180;
                if (Math.Abs(Rxz) < 1e-12)
                    B = 0;
                else
                    B = Math.Atan2(-Rxz, ca * Rxx + sa * Rxy) / Math.PI * 180;
                C = Math.Atan2(sa * Rzx - ca * Rzy, -sa * Ryx + ca * Ryy) / Math.PI * 180;
            }
            else
            {
                A = 0;
                B = Math.Atan2(-Rxz, norm) / Math.PI * 180;
                if (B > -1e-12)
                    C = Math.Atan2(Ryx, Ryy) / Math.PI * 180;
                else
                    C = -Math.Atan2(Ryx, Ryy) / Math.PI * 180;
            }
        }
        public Kuka(string input)
        {
            string[] split = input.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length == 6)
            {
                this = new Kuka(System.Convert.ToDouble(split[0]), System.Convert.ToDouble(split[1]), System.Convert.ToDouble(split[2]), System.Convert.ToDouble(split[3]), System.Convert.ToDouble(split[4]), System.Convert.ToDouble(split[5]));
            }
            else if (split.Length == 7)
            {
                this = new Kuka(System.Convert.ToDouble(split[0]), System.Convert.ToDouble(split[1]), System.Convert.ToDouble(split[2]), System.Convert.ToDouble(split[3]), System.Convert.ToDouble(split[4]), System.Convert.ToDouble(split[5]), System.Convert.ToDouble(split[6]));
            }
            else
                throw new ArgumentException(input, nameof(input));
        }

        public string GetString(string? Format = null, string? Join = null, int Padleft =0, int PadRight=0)
        {
            return $"{X.ToString(Format).PadLeft(Padleft).PadRight(PadRight)}{Join ?? ""}{Y.ToString(Format).PadLeft(Padleft).PadRight(PadRight)}{Join ?? ""}{Z.ToString(Format).PadLeft(Padleft).PadRight(PadRight)}{Join ?? ""}{A.ToString(Format).PadLeft(Padleft).PadRight(PadRight)}{Join ?? ""}{B.ToString(Format).PadLeft(Padleft).PadRight(PadRight)}{Join ?? ""}{C.ToString(Format).PadLeft(Padleft).PadRight(PadRight)}{(Scale == 1 ? String.Empty : $"{Join ?? String.Empty}{Scale.ToString(Format).PadLeft(Padleft).PadRight(PadRight)}")}";
        }
        public Frame GetFrame()
        {
            double cc = Math.Cos(C / 180 * Math.PI);
            double sc = Math.Sin(C / 180 * Math.PI);
            double cb = Math.Cos(B / 180 * Math.PI);
            double sb = Math.Sin(B / 180 * Math.PI);
            double ca = Math.Cos(A / 180 * Math.PI);
            double sa = Math.Sin(A / 180 * Math.PI);


            double Rxx = ca * cb;
            double Rxy = -sa * cc + ca * sb * sc;
            double Rxz = sa * sc + ca * sb * cc;
            double Ryx = sa * cb;
            double Ryy = ca * cc + sa * sb * sc;
            double Ryz = -ca * sc + sa * sb * cc;
            double Rzx = -sb;
            double Rzy = cb * sc;
            double Rzz = cb * cc;

            return new Frame((float)Rxx, (float)Ryx, (float)Rzx, (float)Rxy, (float)Ryy, (float)Rzy, (float)Rxz, (float)Ryz, (float)Rzz, (float)X, (float)Y, (float)Z, (float)(1 / Scale), true);
        }
    }
}