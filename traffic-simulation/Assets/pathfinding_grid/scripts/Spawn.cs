using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public character car;
    public grid_manager gm_s;

    public List<tile> spawnTiles;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Time.fixedTime % 5 == 0)
        {
            int index = Random.Range(0, spawnTiles.Count);
            gm_s.listCar.Add( Instantiate(car, spawnTiles[index].transform.position, Quaternion.identity) );
        }
    }
}
