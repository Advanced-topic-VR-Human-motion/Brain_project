using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuaternionExtension
{
    public static void Decompose(this Quaternion quaternion, Vector3 direction, out Quaternion swing, out Quaternion twist)
    {
        Vector3 vector = new Vector3(quaternion.x, quaternion.y, quaternion.z);
        Vector3 projection = Vector3.Project(vector, direction);

        twist = new Quaternion(projection.x, projection.y, projection.z, quaternion.w).normalized;
        swing = quaternion * Quaternion.Inverse(twist);
    }

    public static Quaternion Constrain(this Quaternion quaternion, float angle)
    {
        float magnitude = Mathf.Sin(0.5F * angle);
        float sqrMagnitude = magnitude * magnitude;

        Vector3 vector = new Vector3(quaternion.x, quaternion.y, quaternion.z);

        if (vector.sqrMagnitude > sqrMagnitude)
        {
            vector = vector.normalized * magnitude;

            quaternion.x = vector.x;
            quaternion.y = vector.y;
            quaternion.z = vector.z;
            quaternion.w = Mathf.Sqrt(1.0F - sqrMagnitude) * Mathf.Sign(quaternion.w);
        }

        return quaternion;
    }

    public static Quaternion ConstrainUpDown(this Quaternion quaternion, float angleUpDown)
    {
        float magnitudeUpDown = Mathf.Sin(0.5F * angleUpDown);
        float sqrMagnitudeUpDown = magnitudeUpDown * magnitudeUpDown;

        Vector3 vector = new Vector3(quaternion.x, quaternion.y, quaternion.z);

        //apply up down constraint
        if (vector.sqrMagnitude > sqrMagnitudeUpDown)
        {
            //only apply to up down direction (y)
            vector.y = vector.y > 0 ? magnitudeUpDown : -magnitudeUpDown;

            quaternion.x = vector.x;
            quaternion.y = vector.y;
            quaternion.z = vector.z;
            quaternion.w = Mathf.Sqrt(1.0F - sqrMagnitudeUpDown) * Mathf.Sign(quaternion.w);
        }
        
        return quaternion;

    }

    public static Quaternion ConstrainLeftRight(this Quaternion quaternion, float angleLeftRight)
    {
        float magnitudeLeftRight = Mathf.Sin(0.5F * angleLeftRight);
        float sqrMagnitudeLeftRight = magnitudeLeftRight * magnitudeLeftRight;

        Vector3 vector = new Vector3(quaternion.x, quaternion.y, quaternion.z);

        //apply left right constraint
        if (vector.sqrMagnitude > sqrMagnitudeLeftRight)
        {
            //only apply to left right direction (x and z)
            vector.x = vector.x > 0 ? magnitudeLeftRight : -magnitudeLeftRight;
            vector.z = vector.z > 0 ? magnitudeLeftRight : -magnitudeLeftRight;

            quaternion.x = vector.x;
            quaternion.y = vector.y;
            quaternion.z = vector.z;
            quaternion.w = Mathf.Sqrt(1.0F - sqrMagnitudeLeftRight) * Mathf.Sign(quaternion.w);
        }
        quaternion.Normalize();
        return quaternion;

    }
}
