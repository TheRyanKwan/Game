// Assets/Scripts/Bullet/BulletManager.cs
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
    
    /// <summary>
    /// 創建子彈並附加行為。
    /// </summary>
    public Bullet SpawnBullet(Vector3 position, params IBulletBehavior[] behaviors)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        
        if (bullet == null)
        {
            bullet = bulletObj.AddComponent<Bullet>();
        }
        
        // 為子彈添加所有行為
        foreach (var behavior in behaviors)
        {
            bullet.AddBehavior(behavior);
        }
        
        return bullet;
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
        
        return bullet;
    }
}
