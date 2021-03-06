using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenerateZone : MonoBehaviour
{
    UnityEvent spawnNewZone = new UnityEvent();
    public GameObject[] camps;
    public GameObject[] zones;

    int previousCamp = -1;

    public Vector3 offset;
    Respawn playerRespawn;
    ScoreManager scoreManager;
    BulletInventory bulletInventory;
    Timer timer;
    AudioSource audioSource;
    bool portalTriggered = false;

    private void Start()
    {
        playerRespawn = FindObjectOfType<Respawn>();
        scoreManager = FindObjectOfType<ScoreManager>();
        timer = FindObjectOfType<Timer>();
        bulletInventory = FindObjectOfType<BulletInventory>();
        audioSource = GetComponent<AudioSource>();

        spawnNewZone.AddListener(scoreManager.UpdateScore);
        spawnNewZone.AddListener(timer.GetClearZoneTime);
        spawnNewZone.AddListener(bulletInventory.NewRound);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !portalTriggered)
        {
            portalTriggered = true;
            SpawnZone();
            spawnNewZone.Invoke();
            Rigidbody rb = other.GetComponent<Rigidbody>();
            rb.velocity = Vector3.Scale(rb.velocity, new Vector3(1f, 0f, 1f));
        }
        if (other.CompareTag("Obstacle"))
        {
            if (other.TryGetComponent(out ObstacleInstance obstacle))
                obstacle.Explode();
        }
    }

    public void SpawnZone()
    {
        LevelStats.ZonesCompleted++;

        GameObject instance = LevelStats.ZonesCompleted % LevelStats.zonesPerArea == 0 ? 
            camps[GetRandomCamp()] : zones[Random.Range(0, zones.Length)];

        audioSource.Play();
        Vector3 location = LevelStats.ZonesCompleted * offset;
        GameObject newGameObject = Instantiate(instance, location, Quaternion.Euler(Vector3.up * -180f));
        playerRespawn.UpdateIncrement(offset.y / 1.1f);
        playerRespawn.UpdateRespawnPoint(newGameObject.transform.Find("StartPosition").transform);
        playerRespawn.RespawnPlayer();

        AboveVoidCheck.isInside = true;
    }

    int GetRandomCamp()
    {
        int randomCamp = previousCamp; 
        while (randomCamp == previousCamp)
        {
            randomCamp = Random.Range(0, camps.Length);
        }

        previousCamp = randomCamp;
        return randomCamp;
    }
}
