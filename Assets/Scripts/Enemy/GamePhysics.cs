using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GamePhysics // TODO rename to Physics And Movement
{
    private const float GRAVITY = 10f;

    public static Quaternion RotateTowardsMovementDirection(Quaternion rotation, Vector3 velocity, float rotationSpeed)
    {
        Vector3 velocityRotation = new Vector3(velocity.x, 0, velocity.z);
        if(velocityRotation == Vector3.zero)
        {
            return rotation;
        }
        return Quaternion.Slerp(rotation, Quaternion.LookRotation(velocityRotation), rotationSpeed);
    }

    public static Quaternion RotateTowardsTarget(Quaternion rotation, Vector3 position, Vector3 targetPosition, float rotationSpeed)
    {
        Vector3 direction = new Vector3 (targetPosition.x - position.x, 0, targetPosition.z - position.z).normalized;
        return Quaternion.Slerp(rotation, Quaternion.LookRotation(direction), rotationSpeed);
    }

    // TODO change return bool?
    public static Vector3 MoveTowardsPositionNonYUnclamped(Vector3 position, Vector3 targetLocation, float speed)
    {
        Vector2 temp = new Vector2(targetLocation.x - position.x, targetLocation.z - position.z);
        temp = temp.normalized * speed;
        return new Vector3(temp.x, 0, temp.y);
    }

    // TODO change return bool?
    public static Vector3 MoveTowardsPositionNonYUnclamped(Vector3 position, Vector3 targetLocation, float speed, out float initialDistance)
    {
        Vector2 temp = new Vector2(targetLocation.x - position.x, targetLocation.z - position.z);
        initialDistance = temp.magnitude;
        temp = temp.normalized * speed;
        return new Vector3(temp.x, 0, temp.y);
    }

    // TODO change return bool?
    public static Vector3 MoveTowardsPositionNonYClamped(Vector3 position, Vector3 targetLocation, float speed, out float initialDistance)
    {
        Vector2 temp = new Vector2(targetLocation.x - position.x, targetLocation.z - position.z);
        initialDistance = temp.magnitude;
        if (initialDistance <= speed)
        {
            return new Vector3(targetLocation.x - position.x, 0, targetLocation.z - position.z);
        }

        temp = temp.normalized * speed;
        return new Vector3(temp.x, 0, temp.y);
    }

    public static float GetGravitationalForce(float timeSinceGrounded)
    {
        return (- GRAVITY) * Mathf.Pow(timeSinceGrounded, 2);
    }

    public static bool IsGroundedSphereCast(Vector3 position, float sphereRadius, float groundOffset, float rayOverhead, LayerMask layer, out float yCorrection)
    {
        Ray ray = new Ray(position, Vector3.down);

        RaycastHit tempHit = new RaycastHit();
        if (Physics.SphereCast(ray, sphereRadius, out tempHit, groundOffset + rayOverhead, layer))
        {
            yCorrection = groundOffset - sphereRadius - tempHit.distance;
            return true;
        }
        else
        {
            yCorrection = 0;
            return false;
        }
    }

    public static bool IsGroundedRayCast(Vector3 position, float groundOffset, float rayOverhead, LayerMask layer, out float yCorrection)
    {
        Ray ray = new Ray(position, Vector3.down);

        RaycastHit tempHit = new RaycastHit();
        if (Physics.Raycast(ray, out tempHit, groundOffset + rayOverhead, layer))
        {
            yCorrection = groundOffset - tempHit.distance;
            return true;
        }
        else
        {
            yCorrection = 0;
            return false;
        }
    }

    public static Vector3 ResolveCollisions(Vector3 position, Quaternion rotation, Vector3 collisionCenter, SphereCollider sphereCollider, LayerMask excludeCaster)
    {
        Vector3 collisionCorectionVector = Vector3.zero;

        Collider[] overlaps = new Collider[4];
        int num = Physics.OverlapSphereNonAlloc(collisionCenter, sphereCollider.radius, overlaps, excludeCaster);

        for (int i = 0; i < num; i++)
        {
            Transform t = overlaps[i].transform;
            Vector3 dir;
            float dist;

            if (Physics.ComputePenetration(sphereCollider, position, rotation, overlaps[i], t.position, t.rotation, out dir, out dist))
            {
                Vector3 penetrationVector = dir * dist;
                collisionCorectionVector += penetrationVector;
            }
        }

        // 2D vector?
        return GetXZPlaneVector(collisionCorectionVector, sphereCollider.radius);
    }

    public static Vector3 GetXZPlaneVector(Vector3 vector, float radius)
    {
        if(vector.x * vector.z == 0)
        {
            return Vector3.zero;
        }

        float horizontalCathetus = Mathf.Sqrt((vector.x * vector.x) + (vector.z * vector.z));
        float shorterHypotenuse = Vector3.Magnitude(vector);

        float hypotenuseDelta = radius - shorterHypotenuse;
        float yDistanceFromCenter = (vector.y * hypotenuseDelta) / shorterHypotenuse; // sphere center - [0;0]
        float rho = Mathf.Sqrt((radius * radius) - (yDistanceFromCenter * yDistanceFromCenter));
        float distanceFromYAxis = Mathf.Sqrt((hypotenuseDelta * hypotenuseDelta) - (yDistanceFromCenter * yDistanceFromCenter)); // x = 0 && z = 0; Triangle - hypotenuseDelta, yDistanceFromCenter, distanceFromYAxis;
        float missingPiece = rho - horizontalCathetus;
        float multiplier = 1 + (missingPiece / horizontalCathetus);

        Vector3 result = new Vector3(vector.x, 0, vector.z);

        return result * multiplier;

        /*if (vector.y == 0) // TODO remove old and probably not functioning code?
        {
            return vector;
        }
        if (vector.x == 0 && vector.z == 0)
        {
            return Vector3.zero;
        }

        float k;
        Vector3 result;

        k = Mathf.Pow(vector.y, 2) / (Mathf.Pow(vector.x, 2) + Mathf.Pow(vector.z, 2));
        result = new Vector3(vector.x * (k + 1), 0, vector.z * (k + 1));

        return vector + result;*/
    }
}
