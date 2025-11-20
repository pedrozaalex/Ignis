using System.Numerics;

namespace Ignis.Engine.Core;

public static class NumericsExtensions
{
    public static Vector3 ToEulerAngles(this Quaternion q)
    {
        Vector3 eulerAngles;

        // Roll (X-axis rotation)
        double sinrCosp = 2 * (q.W * q.X + q.Y * q.Z);
        double cosrCosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
        eulerAngles.X = (float)Math.Atan2(sinrCosp, cosrCosp);

        // Pitch (Y-axis rotation)
        double sinp = 2 * (q.W * q.Y - q.Z * q.X);
        if (Math.Abs(sinp) >= 1)
        {
            eulerAngles.Y = (float)(Math.PI / 2 * Math.Sign(sinp)); // Use 90 degrees if out of range
        }
        else
        {
            eulerAngles.Y = (float)Math.Asin(sinp);
        }

        // Yaw (Z-axis rotation)
        double sinyCosp = 2 * (q.W * q.Z + q.X * q.Y);
        double cosyCosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        eulerAngles.Z = (float)Math.Atan2(sinyCosp, cosyCosp);

        return eulerAngles;
    }
    
    public static void ExtractRotation(this Matrix4x4 matrix, out Quaternion rotation)
    {
        // Normalize the matrix to remove scaling
        Vector3 scale;
        scale.X = new Vector3(matrix.M11, matrix.M12, matrix.M13).Length();
        scale.Y = new Vector3(matrix.M21, matrix.M22, matrix.M23).Length();
        scale.Z = new Vector3(matrix.M31, matrix.M32, matrix.M33).Length();

        Matrix4x4 rotationMatrix = new Matrix4x4
        (
            matrix.M11 / scale.X, matrix.M12 / scale.X, matrix.M13 / scale.X, 0,
            matrix.M21 / scale.Y, matrix.M22 / scale.Y, matrix.M23 / scale.Y, 0,
            matrix.M31 / scale.Z, matrix.M32 / scale.Z, matrix.M33 / scale.Z, 0,
            0, 0, 0, 1
        );

        rotation = Quaternion.CreateFromRotationMatrix(rotationMatrix);
    }
}