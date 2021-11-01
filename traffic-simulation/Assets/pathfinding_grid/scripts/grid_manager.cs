using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;







#if UNITY_EDITOR
[CustomEditor(typeof(grid_manager)), CanEditMultipleObjects]
class grid_editor : Editor
{
    public override void OnInspectorGUI()
    {
        grid_manager gm_s = (grid_manager)target;
        if (GUILayout.Button("Make Grid"))
            gm_s.make_grid();
        if (GUILayout.Button("Make Circle"))
            gm_s.make_circle();

        DrawDefaultInspector();
    }
}
#endif


public class grid_manager : MonoBehaviour
{
    public efind_path find_path;
    public Vector2 v2_grid;
    public RectTransform rt;
    public GridLayoutGroup glg;
    public GameObject go_pref_tile;
    public List<character> listCar; 
    public List<tile> db_tiles;
    public tile fuel_tiles;
    public List<int> db_direction_order;
    public  List<tile> tile_d;//destination
    public Dictionary< character, List<tile> > car_paths = new Dictionary<character, List<tile>>();
    public List<tile> gasStation = new List<tile>();
    
    
    void Update()
    {

        foreach (var car in listCar)
        {
            
            if ((car.moving || car.tile_s == car.selected_tile_s) && (car.fuelLevel >=30|| car.searchFuel)) continue;
           
            tile tileDestination;
          
            
            if (car.fuelLevel >= 30)
            {
                if (!car.haveSaveDestination)
                {
                    do
                    {
                        int indexx = Random.Range(0, tile_d.Count);
                        tileDestination = tile_d[indexx];
                    } while (car.lastDestination == tileDestination);
                    
                    tileDestination.isDestination = true;
                    tileDestination.setColor(car.colorDestination) ;
                    
                    find_paths_weighted(car, tileDestination);
                    car.move_tile(tileDestination);
                }
                else
                {
                    find_paths_weighted(car, car.tempDestination);
                    car.move_tile(car.tempDestination);
                    car.haveSaveDestination = false;
                }
            }
            else
            {
                car.tempDestination = car.tar_tile_s;
                find_paths_weighted(car, fuel_tiles);
                car.move_tile(fuel_tiles);
                car.haveSaveDestination = true;
                car.searchFuel = true;
            }
            
        }
    } 

    public void find_paths_weighted(character tchar, tile tar_tile_s)
    {
        var ttile = tchar.tile_s;
        for (int x = 0; x < db_tiles.Count; x++)
            db_tiles[x].db_path_lowest.Clear(); //Clear all previous lowest paths for this char//
        
        int up = (int)ttile.v2xy.x - (int)tar_tile_s.v2xy.x;
        int right = (int)tar_tile_s.v2xy.y - (int)ttile.v2xy.y;
        int down = (int)tar_tile_s.v2xy.x - (int)ttile.v2xy.x;
        int left = (int)ttile.v2xy.y - (int)tar_tile_s.v2xy.y;

        db_direction_order.Clear();
        if (up >= right && up >= down && up >= left)
        {
            db_direction_order.Add(0);
            db_direction_order.Add(1);
            db_direction_order.Add(2);
            db_direction_order.Add(3);
        }
        else
        if (right >= up && right >= down && right >= left)
        {
            db_direction_order.Add(1);
            db_direction_order.Add(2);
            db_direction_order.Add(3);
            db_direction_order.Add(0);
        }
        else
        if (down >= up && down >= right && down >= left)
        {
            db_direction_order.Add(2);
            db_direction_order.Add(3);
            db_direction_order.Add(0);
            db_direction_order.Add(1);
        }
        else
        //if (left >= up && left >= right && left >= down)
        {
            db_direction_order.Add(3);
            db_direction_order.Add(0);
            db_direction_order.Add(1);
            db_direction_order.Add(2);
        }

        List<tile> db_tpath = new List<tile>();
        find_next_path_weighted(tchar, ttile, db_tpath, tar_tile_s);
        if (!car_paths.ContainsKey(tchar))
            car_paths.Add(tchar, new List<tile>(tar_tile_s.db_path_lowest));
        else
            car_paths[tchar] = new List<tile>(tar_tile_s.db_path_lowest);
            
        tchar.shortestPath = car_paths[tchar];
        
    }

    void find_next_path_weighted(character tchar, tile ttile, List<tile> db_tpath, tile tar_tile_s)
    {
            for(int x = 0; x < ttile.db_neighbors.Count; x++)
            {
                var donum = db_direction_order[x];
                var ntile = ttile.db_neighbors[donum].tile_s;
                if (ttile.db_neighbors[donum].tile_s != null && !db_tpath.Contains(ntile) && !ttile.db_neighbors[donum].blocked) //Check if tile, if not already used, if not blocked//
                {
                    if (pathWeight(tar_tile_s.db_path_lowest, tchar) == 0 || pathWeight(db_tpath, tchar) < pathWeight(tar_tile_s.db_path_lowest, tchar))
                    {
                        if (pathWeight(ntile.db_path_lowest, tchar) == 0 || pathWeight(db_tpath, tchar) + 1 < pathWeight(ntile.db_path_lowest, tchar))
                        {
                            if ((!tchar.big) || (tchar.big && ntile.db_neighbors[1].tile_s != null && !ntile.db_neighbors[1].blocked && ntile.db_neighbors[2].tile_s != null && !ntile.db_neighbors[2].blocked && ntile.db_neighbors[1].tile_s.db_neighbors[2].tile_s != null && !ntile.db_neighbors[1].tile_s.db_neighbors[2].blocked && !ntile.db_neighbors[2].tile_s.db_neighbors[1].blocked))
                            {
                                ntile.db_path_lowest.Clear();
                                for (int i = 0; i < db_tpath.Count; i++)
                                    ntile.db_path_lowest.Add(db_tpath[i]);

                                ntile.db_path_lowest.Add(ntile);
                                
                                if (ttile != tar_tile_s)
                                    find_next_path_weighted(tchar, ntile, ntile.db_path_lowest, tar_tile_s);
                           
                            }
                        }
                    }
                }
            }
    }


    private float pathWeight(List<tile> path, character tchar)
    {
        float weight = 0;

            for (int i = 0; i < path.Count; i++)
                if (dist(path[i].v2xy, tchar.tile_s.v2xy) < tchar.distView)
                    weight += path[i].getWeight();
                else weight += 1;

        return weight;
    }

        
    private float dist(Vector2 v1, Vector2 v2)
        {
            float dist1 = Mathf.Abs(v1.x - v2.x),
                dist2 = Mathf.Abs(v1.y - v2.y);

            if (dist1 >= dist2)
                return dist1;
            else
                return dist2;
        }

    
    public void make_grid()
    {
        glg.enabled = true;

        //Clear Old Tiles//
        for (int i = 0; i < db_tiles.Count; i++)
            DestroyImmediate(db_tiles[i].gameObject);
        db_tiles.Clear();

        float twidth = (glg.cellSize.y + glg.spacing.y) * v2_grid.y;
        rt.sizeDelta = new Vector2(twidth, glg.cellSize.x);

        for (int x = 0; x < v2_grid.x; x++)
        {
            for (int y = 0; y < v2_grid.y; y++)
            {
                var tgo = (GameObject) Instantiate(go_pref_tile, go_pref_tile.transform.position, go_pref_tile.transform.rotation, go_pref_tile.transform.parent);
                tgo.SetActive(true);
                tgo.name = "tile_" + x + "_" + y;
                var ttile = tgo.GetComponent<tile>();
                ttile.v2xy = new Vector2(x, y);
                db_tiles.Add(ttile);
            }
        }

        for (int x = 0; x < db_tiles.Count; x++)
        {
            for (int y = 0; y < db_tiles.Count; y++)
            {
                if (db_tiles[x].v2xy.x - db_tiles[y].v2xy.x == 1 && db_tiles[x].v2xy.y == db_tiles[y].v2xy.y)
                    db_tiles[x].db_neighbors[0].tile_s = db_tiles[y]; //Up//
                else
                    if (db_tiles[x].v2xy.x == db_tiles[y].v2xy.x && db_tiles[y].v2xy.y - db_tiles[x].v2xy.y == 1)
                    db_tiles[x].db_neighbors[1].tile_s = db_tiles[y]; //Right//
                else
                    if (db_tiles[y].v2xy.x - db_tiles[x].v2xy.x == 1 && db_tiles[x].v2xy.y == db_tiles[y].v2xy.y)
                    db_tiles[x].db_neighbors[2].tile_s = db_tiles[y]; //Down//
                else
                    if (db_tiles[x].v2xy.x == db_tiles[y].v2xy.x && db_tiles[x].v2xy.y > db_tiles[y].v2xy.y)
                    db_tiles[x].db_neighbors[3].tile_s = db_tiles[y]; //Left//
            }
        }
    }


    public void make_circle()
    {
        glg.enabled = false;

        var pos_mid = db_tiles[0].transform.position + db_tiles[db_tiles.Count - 1].transform.position;
        pos_mid /= 2;
        var max_dist = Vector3.Distance(pos_mid, db_tiles[0].transform.position);
        var circle_dist = max_dist * 0.68f;

        var tcount = db_tiles.Count;
        for (int i = tcount - 1; i > -1; i--)
        {
            var ttile = db_tiles[i];
            var tdist = Vector3.Distance(pos_mid, ttile.transform.position);
            
            if (tdist > circle_dist)
            {
                db_tiles.Remove(ttile);
                DestroyImmediate(ttile.gameObject);
            }
        }
    }


    IEnumerator start_game()
    {
        yield return new WaitForSeconds(0.01f);
        
        foreach (character car in listCar)
        {
            var ttile = car.tile_s;
            ttile.db_chars.Add(car);
            var tpos = ttile.transform.position;

            if (car.big)
            {
                tpos = new Vector3(0, 0, 0);
                tpos += ttile.transform.position + ttile.db_neighbors[1].tile_s.transform.position +
                        ttile.db_neighbors[2].tile_s.transform.position +
                        ttile.db_neighbors[1].tile_s.db_neighbors[2].tile_s.transform.position;
                tpos /= 4;
            }

            car.transform.position = tpos;


        }
    }


    void Start()
    {
        StartCoroutine(start_game());
    }
}
