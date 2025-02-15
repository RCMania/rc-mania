using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShootingController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] Transform shootPoint;      
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] List<GameObject> spawnedProjectiles = new List<GameObject>(); //SerializeField Only for Tests

    [Header("Shooting Settings")]
    [SerializeField] float reloadTime = 4f;
    private NetworkVariable<float> lastShootTime = new NetworkVariable<float>();

    private void Start()
    {
        if (IsServer)
        {
            lastShootTime.Value = Time.time;
        }
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        if (Input.GetMouseButtonDown(0))
        {
            ShootServerRpc();  
        }
    }

    [ServerRpc] 
    private void ShootServerRpc()
    {
        if (Time.time < (lastShootTime.Value + reloadTime)) { return; }  // Prevent shooting before reload time

        lastShootTime.Value = Time.time;  // Update the last shot time

        SpawnProjectile();
    }
    
    private void SpawnProjectile()
    {
        GameObject _projectileInstance = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
        _projectileInstance.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);


        spawnedProjectiles.Add(_projectileInstance);

        _projectileInstance.GetComponent<Projectile>().parent = this;

    }

    [ServerRpc(RequireOwnership = false)] 
    public void DestroyServerRpc(NetworkObjectReference projectileReference)
    {
        if (projectileReference.TryGet(out NetworkObject projectileNetworkObject))
        {
            var projectileInstance = projectileNetworkObject.gameObject;

            // Ensure it is removed from the list before being destroyed
            spawnedProjectiles.Remove(projectileInstance);

            //projectileNetworkObject.Despawn(true); //Despawn remove the objects but keeps it on the server (good if you want to revive the object).
            Destroy(projectileInstance);
        }
    }
}
