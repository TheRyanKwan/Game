// Assets/Scripts/Player/PlayerTeleport.cs
using UnityEngine;
using System.Collections.Generic;

public class PlayerTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("Maximum horizontal distance to search for teleport locations.")]
    public float maxTeleportDistance = 10f;

    [Tooltip("Maximum height difference (up or down) for valid teleport spots.")]
    public float maxHeightDifference = 5f;

    [Tooltip("Width of the detection cone in front of the player.")]
    public float detectionWidth = 3f;

    [Tooltip("Layer mask for platforms/ground (use 'Wall' layer).")]
    public LayerMask groundLayer;

    [Tooltip("Minimum distance from ledge edge to place player.")]
    public float ledgeOffset = 0.5f;

    [Tooltip("Number of rays to cast for detection.")]
    public int rayCount = 15;

    [Tooltip("Height to check above player for clearance.")]
    public float clearanceHeight = 2f;

    [Header("Visual Feedback")]
    [Tooltip("VFX for teleport start (optional).")]
    public GameObject teleportStartVFX;

    [Tooltip("VFX for teleport end (optional).")]
    public GameObject teleportEndVFX;

    private CharacterController controller;
    private PlayerMovement playerMovement;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();

        if (controller == null)
        {
            Debug.LogError("PlayerTeleport requires a CharacterController component.");
            enabled = false;
            return;
        }

        if (groundLayer.value == 0)
        {
            Debug.LogWarning("Ground LayerMask is not set. Please assign it in the Inspector.");
        }
    }

    /// <summary>
    /// Finds the furthest valid teleport position in front of the player.
    /// Called externally by Skill2.
    /// </summary>
    public Vector3? FindTeleportPosition()
    {
        Vector3 playerPosition = transform.position;
        Vector3 forwardDirection = transform.forward;

        List<TeleportCandidate> candidates = new List<TeleportCandidate>();

        for (int i = 0; i < rayCount; i++)
        {
            float progress = (float)i / (rayCount - 1);
            float horizontalDistance = progress * maxTeleportDistance;

            Vector3[] offsets = new Vector3[]
            {
                Vector3.zero,
                transform.right * detectionWidth * 0.5f,
                -transform.right * detectionWidth * 0.5f
            };

            foreach (Vector3 offset in offsets)
            {
                Vector3 checkPosition = playerPosition + forwardDirection * horizontalDistance + offset;

                for (float heightOffset = maxHeightDifference; heightOffset >= -maxHeightDifference; heightOffset -= 0.5f)
                {
                    Vector3 rayStart = checkPosition + Vector3.up * (controller.height + heightOffset);
                    RaycastHit hit;

                    if (Physics.Raycast(rayStart, Vector3.down, out hit, maxHeightDifference * 2 + controller.height, groundLayer))
                    {
                        Vector3 landingPosition = hit.point + Vector3.up * (controller.height / 2);

                        if (IsValidTeleportPosition(landingPosition, hit))
                        {
                            float distance = Vector3.Distance(
                                new Vector3(playerPosition.x, 0, playerPosition.z),
                                new Vector3(landingPosition.x, 0, landingPosition.z));

                            candidates.Add(new TeleportCandidate
                            {
                                position = landingPosition,
                                distance = distance,
                                hitInfo = hit
                            });
                        }

                        break;
                    }
                }
            }
        }

        if (candidates.Count > 0)
        {
            candidates.Sort((a, b) => b.distance.CompareTo(a.distance));
            Vector3 furthestPosition = candidates[0].position;
            furthestPosition = AdjustForLedge(furthestPosition, candidates[0].hitInfo);
            return furthestPosition;
        }

        return null;
    }

    /// <summary>
    /// Executes the teleportation to the target position.
    /// Called externally by Skill2.
    /// </summary>
    public void ExecuteTeleport(Vector3 targetPosition)
    {
        if (teleportStartVFX != null)
            Instantiate(teleportStartVFX, transform.position, Quaternion.identity);

        controller.enabled = false;
        transform.position = targetPosition;
        controller.enabled = true;

        if (playerMovement != null)
            playerMovement.ResetVerticalVelocity();

        if (teleportEndVFX != null)
            Instantiate(teleportEndVFX, targetPosition, Quaternion.identity);

        Debug.Log("Teleported to: " + targetPosition);
    }

    bool IsValidTeleportPosition(Vector3 position, RaycastHit groundHit)
    {
        float horizontalDistance = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(position.x, 0, position.z));

        if (horizontalDistance < 1f) return false;

        Vector3 startPos = transform.position + Vector3.up * (controller.height / 2);
        Vector3 direction = (position - startPos).normalized;
        float pathDistance = Vector3.Distance(startPos, position);

        RaycastHit pathHit;
        if (Physics.SphereCast(startPos, controller.radius * 0.8f, direction, out pathHit, pathDistance, groundLayer))
        {
            if (pathHit.collider != groundHit.collider &&
                Vector3.Dot(pathHit.normal, Vector3.up) < 0.7f)
                return false;
        }

        for (float heightCheck = 0.2f; heightCheck <= controller.height; heightCheck += controller.height / 3f)
        {
            Vector3 rayStart = transform.position + Vector3.up * heightCheck;
            Vector3 rayEnd = position + Vector3.up * heightCheck;
            Vector3 rayDir = (rayEnd - rayStart).normalized;
            float rayDist = Vector3.Distance(rayStart, rayEnd);

            RaycastHit wallHit;
            if (Physics.Raycast(rayStart, rayDir, out wallHit, rayDist, groundLayer))
            {
                if (wallHit.collider != groundHit.collider &&
                    Vector3.Dot(wallHit.normal, Vector3.up) < 0.7f)
                    return false;
            }
        }

        if (Physics.CheckSphere(position, controller.radius * 0.9f, groundLayer)) return false;

        RaycastHit ceilingHit;
        if (Physics.Raycast(position, Vector3.up, out ceilingHit, clearanceHeight, groundLayer))
        {
            if (ceilingHit.distance < controller.height) return false;
        }

        return true;
    }

    Vector3 AdjustForLedge(Vector3 position, RaycastHit groundHit)
    {
        Vector3 forwardDir = transform.forward;
        Vector3 forwardCheckStart = position + Vector3.up * 0.1f + forwardDir * ledgeOffset;

        RaycastHit forwardHit;
        if (!Physics.Raycast(forwardCheckStart, Vector3.down, out forwardHit, 1f, groundLayer))
            position -= forwardDir * ledgeOffset;

        return position;
    }

    private class TeleportCandidate
    {
        public Vector3 position;
        public float distance;
        public RaycastHit hitInfo;
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Vector3 playerPos = transform.position;
        Vector3 forward = transform.forward;

        Vector3 leftBound = playerPos + forward * maxTeleportDistance - transform.right * detectionWidth * 0.5f;
        Vector3 rightBound = playerPos + forward * maxTeleportDistance + transform.right * detectionWidth * 0.5f;

        Gizmos.DrawLine(playerPos, leftBound);
        Gizmos.DrawLine(playerPos, rightBound);
        Gizmos.DrawLine(leftBound, rightBound);
    }
}
