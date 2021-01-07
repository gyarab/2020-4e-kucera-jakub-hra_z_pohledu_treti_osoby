using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GamePhysics
{
    private const float GRAVITY = 10f;

    #region Rotation

    // Vrátí Quaternion, kterým směrem by se hráč měl natočit, když se chce otočit směrem, kterým jde
    public static Quaternion RotateTowardsMovementDirection(Quaternion rotation, Vector3 velocity, float rotationSpeed)
    {
        Vector3 velocityRotation = new Vector3(velocity.x, 0, velocity.z);
        if(velocityRotation == Vector3.zero)
        {
            return rotation;
        }

        return Quaternion.Slerp(rotation, Quaternion.LookRotation(velocityRotation, Vector3.up), rotationSpeed);
    }

    // Vrácí postupnou rotaci, aby se objekt natočil směrem k cíli
    public static Quaternion RotateTowardsTarget(Quaternion rotation, Vector3 position, Vector3 targetPosition, float rotationSpeed)
    {
        Vector3 direction = new Vector3 (targetPosition.x - position.x, 0, targetPosition.z - position.z).normalized;
        return Quaternion.Slerp(rotation, Quaternion.LookRotation(direction), rotationSpeed);
    }

    #endregion

    #region Movement

    // Posune objekt směrem k cíli, objekt může přejít přes cíl, neřeší Y-ovou souřadnici
    public static Vector3 MoveTowardsPositionNonYUnclamped(Vector3 position, Vector3 targetLocation, float speed)
    {
        Vector2 temp = new Vector2(targetLocation.x - position.x, targetLocation.z - position.z);
        temp = temp.normalized * speed;
        return new Vector3(temp.x, 0, temp.y);
    }

    // Posune objekt směrem k cíli, objekt může přejít přes cíl, neřeší Y-ovou souřadnici, navíc vrací počáteční vzdálenost od cíle
    public static Vector3 MoveTowardsPositionNonYUnclamped(Vector3 position, Vector3 targetLocation, float speed, out float initialDistance)
    {
        Vector2 temp = new Vector2(targetLocation.x - position.x, targetLocation.z - position.z);
        initialDistance = temp.magnitude;
        temp = temp.normalized * speed;
        return new Vector3(temp.x, 0, temp.y);
    }

    // Posune objekt směrem k cíli, objekt nepřejde přes cíl, neřeší Y-ovou souřadnici, navíc vrací počáteční vzdálenost od cíle
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

    // Posune objekt směrem k cíli, objekt nepřejde přes cíl, neřeší Y-ovou souřadnici, počáteční pozice by měla zůstat stejná, mění se procento ušlé vzdálenosti
    public static Vector3 MoveTowardsPositionProgressivelyNonYClamped(Vector3 position, Vector3 targetLocation, float progress)
    {
        return Vector3.Slerp(position, new Vector3(targetLocation.x, position.y, targetLocation.z), progress);
    }

    #endregion

    #region Gravity

    // Spočítá gravitační sílu
    public static float GetGravitationalForce(float timeSinceGrounded)
    {
        return (- GRAVITY) * Mathf.Pow(timeSinceGrounded, 2);
    }

    // Spočítá gravitační sílu s počáteční silou
    public static float GetGravitationalForceWithInitialVelocity(float timeSinceGrounded, float initialForce)
    {
        return initialForce * timeSinceGrounded - 0.5f * (GRAVITY) * Mathf.Pow(timeSinceGrounded, 2);
    }

    #endregion

    #region Grounded

    // Zjistí jestli objekt stojí na zemi, případně upraví jeho výšku, přesnější než raycast
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

    // Zjistí jestli objekt stojí na zemi, případně upraví jeho výšku
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

    // Zjistí jestli objekt stojí na zemi, případně upraví jeho výšku, pokud by výšková změna přesahovala stanovený limit, tak vyřeší kolize
    public static bool IsGroundedWithMaxStepDistanceAndCollisions(Vector3 rayPosition, float groundOffset, float rayOverhead, float maxStep, SphereCollider feetSphereCollider, Vector3 sphereColliderParentPosition, LayerMask layer, out Vector3 correction)
    {
        correction = Vector3.zero;
        Ray ray = new Ray(rayPosition, Vector3.down);
        RaycastHit[] hits = new RaycastHit[4];
        float rayDistance = groundOffset + rayOverhead;

        int countOfCollisions = Physics.SphereCastNonAlloc(ray, feetSphereCollider.radius, hits, rayDistance, layer);

        if (countOfCollisions < 1)
        {
            return false;
        }

        float maxYDelta = -rayDistance - rayOverhead;
        float currentDistanceFromGround;

        for (int i = 0; i < countOfCollisions; i++)
        {
            currentDistanceFromGround = groundOffset - feetSphereCollider.radius - hits[i].distance;

            if (currentDistanceFromGround >= maxYDelta)
            {
                maxYDelta = currentDistanceFromGround;
            }
        }

        if (maxYDelta < -rayOverhead)
        {
            return false;
        }
        else if (maxYDelta <= maxStep)
        {
            correction.y += maxYDelta;
        }
        else
        {
            for (int i = 0; i < countOfCollisions; i++)
            {
                Transform transform = hits[i].collider.transform;
                Vector3 dir;
                float magnitude;

                if (Physics.ComputePenetration(feetSphereCollider, sphereColliderParentPosition, Quaternion.identity, hits[i].collider, transform.position, transform.rotation, out dir, out magnitude))
                {
                    Vector3 penetrationVector = dir * magnitude;
                    correction += GetXZPlaneVector(penetrationVector, feetSphereCollider.radius);
                }
            }
        }

        return true;
    }

    #endregion

    #region Collisions

    // Spočítá kolize a upraví pozici objektu, ale Y-ová pozice objektu je zachována
    public static Vector3 ResolveCollisionsNonY(Vector3 position, Quaternion rotation, Vector3 sphereColliderPosition, SphereCollider sphereCollider, LayerMask excludeCaster)
    {
        Vector3 collisionCorectionVector = Vector3.zero;

        Collider[] overlaps = new Collider[4];
        int num = Physics.OverlapSphereNonAlloc(sphereColliderPosition, sphereCollider.radius, overlaps, excludeCaster);

        for (int i = 0; i < num; i++)
        {
            Transform transform = overlaps[i].transform;
            Vector3 direction;
            float magnitude;

            if (Physics.ComputePenetration(sphereCollider, position, rotation, overlaps[i], transform.position, transform.rotation, out direction, out magnitude))
            {
                Vector3 penetrationVector = direction * magnitude;
                string a = ("a: " + penetrationVector.x + ", " + penetrationVector.y + ", " + penetrationVector.z);
                penetrationVector = GetXZPlaneVector(penetrationVector, sphereCollider.radius); // TODO remove if not working
                string b = ("b: " + penetrationVector.x + ", " + penetrationVector.y + ", " + penetrationVector.z);

                if (float.IsNaN(penetrationVector.x))
                {
                    Debug.Log(a);
                    Debug.Log(b);
                } else
                {
                    collisionCorectionVector += penetrationVector;
                }
            }
        }

        return collisionCorectionVector;
    }

    // Převede Vector3 upravující pozici na Vector2 (Y-ová souřadnice je vynechána), funguje pouze pro Sphere Collidery
    public static Vector3 GetXZPlaneVector(Vector3 input, float radius)
    {
        if(radius <= 0)
        {
            throw new System.Exception("Radius is lesser or equal to zero");
        }

        if(input.x * input.z == 0)
        {
            return input;
        }

        if(Vector3.Magnitude(input) > radius)
        {
            return Vector3.zero;
        }

        Vector3 vector = new Vector3(Mathf.Abs(input.x), Mathf.Abs(input.y), Mathf.Abs(input.z));

        float horizontalCathetus = Mathf.Sqrt((vector.x * vector.x) + (vector.z * vector.z));
        float shorterHypotenuse = Vector3.Magnitude(vector);

        float hypotenuseDelta = radius - shorterHypotenuse;
        float yDistanceFromCenter = (vector.y * hypotenuseDelta) / shorterHypotenuse; // sphere center - [0;0]
        float rho = Mathf.Sqrt((radius * radius) - (yDistanceFromCenter * yDistanceFromCenter));
        float distanceFromYAxis = Mathf.Sqrt((hypotenuseDelta * hypotenuseDelta) - (yDistanceFromCenter * yDistanceFromCenter)); // x = 0 && z = 0; Triangle - hypotenuseDelta, yDistanceFromCenter, distanceFromYAxis;
        float missingPiece = rho - distanceFromYAxis - horizontalCathetus;
        float multiplier = 1 + (missingPiece / horizontalCathetus);

        Vector3 result = new Vector3(input.x, 0, input.z);

        return result * multiplier;
    }

    // Kolize řešená 3 raycasty
    public static Vector3 RaycastCollisionDetection(Vector3 position, Vector3[] normalizedDirections, float rayDistance, LayerMask layer)
    {
        Ray ray;
        RaycastHit hitInfo;
        Vector3 correction = Vector3.zero;

        for (int i = 0; i < normalizedDirections.Length; i++)
        {
            ray = new Ray(position, normalizedDirections[i]);

            if(Physics.Raycast(ray, out hitInfo, rayDistance, layer))
            {
                correction -= normalizedDirections[i] * (rayDistance - hitInfo.distance) / rayDistance;
            }
        }

        return correction;
    }

    #endregion
}
