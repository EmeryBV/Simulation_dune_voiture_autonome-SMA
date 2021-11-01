using System.Collections;
using System.Collections.Generic;
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

    public float distView = 3.0f;

    public int visibility = 3;


    void Update()
    {
        if (body_looking)
        {
            Vector3 tar_dir = db_moves[1].position - tr_body.position;
            Vector3 new_dir = Vector3.RotateTowards(tr_body.forward, tar_dir, rotate_speed * Time.deltaTime / 2, 0f);
            new_dir.y = 0;
            tr_body.transform.rotation = Quaternion.LookRotation(new_dir);
        }

        if (moving)
        {
            float step;
            step = move_speed * Time.deltaTime / tile_s.getWeight();
            
            
            transform.position = Vector3.MoveTowards(transform.position, db_moves[0].position, step);
            var tdist = Vector3.Distance(tr_body.position, db_moves[0].position);
            if (tdist < 0.001f)
            {
                tile_s.db_chars.Remove(this);
                tile_s = tar_tile_s.db_path_lowest[num_tile];
                tile_s.db_chars.Add(this);
                if (moving_tiles && num_tile < tar_tile_s.db_path_lowest.Count - 1)
                {
                    num_tile++;
                    var tpos = tar_tile_s.db_path_lowest[num_tile].transform.position;
                    if (big) //Large chars//
                    {
                        tpos = new Vector3(0, 0, 0);
                        tpos += tar_tile_s.db_path_lowest[num_tile].transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[1].tile_s.transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[2].tile_s.transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[1].tile_s.db_neighbors[2].tile_s.transform.position;
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
                    if (gm_s.find_path == efind_path.once_per_turn || gm_s.find_path == efind_path.max_tiles)
                        gm_s.find_paths_static(this);
                    gm_s.hover_tile(selected_tile_s);
                }

                Debug.Log("NumTile : " + num_tile + "\n");
                bool bouchon = false;
                int dist = Mathf.Min(num_tile + visibility, tar_tile_s.db_path_lowest.Count);
                for(int i = num_tile; i < dist; i++)
                {
                    if (tar_tile_s.db_path_lowest[num_tile].bouchon)
                    {
                        bouchon = true;
                        break;
                    }   
                }

                if (bouchon)
                {
                    num_tile = 0;
                    gm_s.find_paths_weighted(this, tar_tile_s);
                    bouchon = false;
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

        move_speed = this.move_speed;

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
            tpos += tar_tile_s.db_path_lowest[num_tile].transform.position;
        }
        else
        if (big)
        {
            tpos += tar_tile_s.db_path_lowest[num_tile].transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[1].tile_s.transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[2].tile_s.transform.position + tar_tile_s.db_path_lowest[num_tile].db_neighbors[1].tile_s.db_neighbors[2].tile_s.transform.position;
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
