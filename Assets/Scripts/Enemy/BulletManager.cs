using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance { get; private set; }

    [SerializeField] private GameObject bulletPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (bulletPrefab == null)
        {
            bulletPrefab = CreateDefaultBulletPrefab();
        }
    }

    public void SpawnBullet(Vector3 startPosition, Vector3 direction, float speed, float lifetime)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, startPosition, Quaternion.identity);
        Rigidbody rb = bulletObj.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = bulletObj.AddComponent<Rigidbody>();
        }

        rb.velocity = direction.normalized * speed;

        // ÿ©“®ηχÿΚ
        Destroy(bulletObj, lifetime);

        Debug.Log($"ÿ¶ÿ¬ÿ[–‹‰— {startPosition}ÿC•ϋÿό {direction.normalized}");
    }

    private GameObject CreateDefaultBulletPrefab()
    {
        GameObject bullet = new GameObject("DefaultBullet");
        
        SphereCollider collider = bullet.AddComponent<SphereCollider>();
        collider.radius = 0.3f;

        Rigidbody rb = bullet.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        MeshFilter meshFilter = bullet.AddComponent<MeshFilter>();
        meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

        MeshRenderer meshRenderer = bullet.AddComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        meshRenderer.material = mat;

        bullet.SetActive(false);

        return bullet;
    }
}
