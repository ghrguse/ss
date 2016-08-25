using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Strategy;

namespace Strategy
{
	[Tiled2Unity.CustomTiledImporter]
	public class CustomImporter_TerrainTiles : Tiled2Unity.ICustomTiledImporter
	{
	    public void HandleCustomProperties(GameObject gameObject,
	        IDictionary<string, string> customProperties)
	    {
			Debug.Log("Load Obj");
	        if (customProperties.ContainsKey("Terrain"))
			{
	            // Add the terrain tile game object
				Tile tile = gameObject.AddComponent<Tile>();
	            tile.Terrain = customProperties["Terrain"];
	            //tile.TileNote = "Reimport";
				tile.x = Mathf.FloorToInt(gameObject.transform.position.x);
				tile.y = -Mathf.FloorToInt(gameObject.transform.position.y);

				Debug.Log(string.Format("terrain:{0},self:{1},{2},parent:{3},{4}",tile.Terrain,
				                        gameObject.transform.position.x,
				                        gameObject.transform.position.y,
				                        gameObject.transform.parent.position.x,
				                        gameObject.transform.parent.position.y));
			}

	    }

	    public void CustomizePrefab(GameObject prefab)
	    {
	        // Do nothing
	    }
	}
}
