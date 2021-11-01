using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class character : MonoBehaviour
{
    public grid_manager gm_s;
    public bool big;
    public bool body_looking;
    public bool moving;
    public bool moving_tiles;
    public float move_speed = 2f;
    public float rotate_speed = 6f;
    public Color col;
    public Transform tr_body;
    public tile tile_s;
    public tile tar_tile_s;
    public tile selected_tile_s;
    public List<Transform> db_moves;
    public int max_tiles = 7;
    public int num_tile;
    public List<tile> db_tiles;
    public float distView = 3.0f;
    public int visibility = 3;
    public AudioSource fluid;
    public AudioSource calm;
    private bool isInJam = false;
    bool bouchon = false;
    public  tile lastDestination;
    public List<tile> shortestPath;
    public TextMeshPro music;
    private int weight =1;
    public float fuelLevel = 50;
    public float maxFuel = 0.0f;
    public bool searchFuel = false;
    public tile tempDestination;
    public bool haveSaveDestination = false;
    public TextMeshPro fuelDisplay;
    public Color colorDestination = Color.magenta;

    private void Start()
    {
        music.SetText( "Playing cheerful music" );
        
    }

 
    void Update()
    {
        fuelDisplay.SetText( "Fuel : " + ( fuelLevel * 100 / maxFuel ).ToString("0.0") + "%" );

        if (tile_s.bouchon)
        {
            if (!isInJam)
            {
                //play calme
                isInJam = true;
                music.SetText( "Playing calm music" );

            }
        }
        else
        {
            if (isInJam)
            {
                //play dynamique ?/arreter musique
                isInJam = false;
                music.SetText( "Playing cheerful music" );

            }
        }
        
        if (body_looking)
        {
            Vector3 tar_dir = db_moves[1].position - tr_body.position;
            Vector3 new_dir = Vector3.RotateTowards(tr_body.forward, tar_dir, rotate_speed * Time.deltaTime / 2, 0f);
            new_dir.y = 0;
            tr_body.transform.rotation = Quaternion.LookRotation(new_dir);
        }

        if (moving )
        {
            float step;
            step = move_speed * Time.deltaTime / tile_s.getWeight();
            
            
            transform.position = Vector3.MoveTowards(transform.position, db_moves[0].position, step);
            var tdist = Vector3.Distance(tr_body.position, db_moves[0].position);
            if (tdist < 0.001f)
            {
                // print(fuelLevel);
                if (tile_s.db_chars[tile_s.db_chars.Count - 1] != shortestPath[num_tile]) fuelLevel -= tile_s.getWeight();
                tile_s.db_chars.Remove(this);
                tile_s = shortestPath[num_tile];
                tile_s.db_chars.Add(this);

                if (tile_s.isGasStation)
                {
                    fuelLevel = maxFuel;
                    searchFuel = false;
                }

                if (fuelLevel <= 0)
                {
                    // print("Je me détruis ");
                    tile_s.bouchon = true;
                    Destroy(gameObject);
                }
                   
                
                bouchon = false;
                
                int dist = Mathf.Min(num_tile + visibility, shortestPath.Count);
                for (int i = num_tile; i < dist; i++)
                {
                    // Debug.Log("I : " + i + "\n");
                    if (shortestPath[i].bouchon)
                    {
                        bouchon = true;
                        break;
                    }
                }

                if (bouchon)
                {
                    num_tile = -1;
                    gm_s.find_paths_weighted(this, tar_tile_s);
                    // Debug.Log("Nouveau chemin calcul\n");
                    bouchon = false;

                }

                if (moving_tiles && num_tile < shortestPath.Count - 1)
                {
                    num_tile++;
                    var tpos = shortestPath[num_tile].transform.position;
                    if (big) //Large chars//
                    {
                        tpos = new Vector3(0, 0, 0);
                        tpos += shortestPath[num_tile].transform.position + shortestPath[num_tile].db_neighbors[1].tile_s.transform.position + shortestPath[num_tile].db_neighbors[2].tile_s.transform.position + shortestPath[num_tile].db_neighbors[1].tile_s.db_neighbors[2].tile_s.transform.position;
                        tpos /= 4; //Takes up 4 tiles//
                    }
                    tpos.y = transform.position.y;
                    db_moves[0].position = tpos;
                    db_moves[1].position = tpos;

                }
                else
                {
                    db_moves[4].gameObject.SetActive(false);
                    moving = false;
                    moving_tiles = false;
                    tar_tile_s.isDestination = false;
                    lastDestination = tar_tile_s;
                    // if (gm_s.find_path == efind_path.once_per_turn || gm_s.find_path == efind_path.max_tiles)
                        // gm_s.find_paths_static(this);
                    // gm_s.hover_tile(selected_tile_s);
                }
            }
        }
    }


    public void move_tile(tile ttile)
    {

        num_tile = 0;
        tar_tile_s = ttile;

        //0 - body_move, 1 - body_look, 2 - head_look, 3 - eyes_look, target tile marker
        db_moves[0].parent = null;
        db_moves[1].parent = null;
        db_moves[4].parent = null;
        
        var tpos = new Vector3(0, 0, 0);
        if (!big)
        {
            tpos = tar_tile_s.transform.position;
        }
        else
        if (big)
        {
            tpos += tar_tile_s.transform.position + tar_tile_s.db_neighbors[1].tile_s.transform.position + tar_tile_s.db_neighbors[2].tile_s.transform.position + tar_tile_s.db_neighbors[1].tile_s.db_neighbors[2].tile_s.transform.position;
            tpos /= 4;
        }

        db_moves[4].position = tpos; //Tar Tile Marker//
        db_moves[4].gameObject.SetActive(true); //Tar Tile Marker//

        tpos = new Vector3(0, 0, 0);
        if (!big)
        {
            tpos += shortestPath[num_tile].transform.position;
        }
        else
        if (big)
        {
            tpos += shortestPath[num_tile].transform.position + shortestPath[num_tile].db_neighbors[1].tile_s.transform.position + shortestPath[num_tile].db_neighbors[2].tile_s.transform.position + shortestPath[num_tile].db_neighbors[1].tile_s.db_neighbors[2].tile_s.transform.position;
            tpos /= 4;
        }

        tpos.y = transform.position.y;
        db_moves[0].position = tpos;
        db_moves[1].position = tpos;

        moving = true;
        moving_tiles = true;
        body_looking = true;

        }
    
}
