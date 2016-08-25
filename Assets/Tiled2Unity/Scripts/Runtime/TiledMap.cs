using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Strategy;

namespace Tiled2Unity
{
    public class TiledMap : MonoBehaviour
    {
        public enum MapOrientation
        {
            Orthogonal,
            Isometric,
            Staggered,
            Hexagonal,
        }

        public enum MapStaggerAxis
        {
            X,
            Y,
        }

        public enum MapStaggerIndex
        {
            Odd,
            Even,
        }

        public List<Tile> tileList = new List<Tile>();
        private Tile[,] tileArray;

        public MapOrientation Orientation = MapOrientation.Orthogonal;
        public MapStaggerAxis StaggerAxis = MapStaggerAxis.X;
        public MapStaggerIndex StaggerIndex = MapStaggerIndex.Odd;
        public int HexSideLength = 0;

        public int NumLayers = 0;
        public int NumTilesWide = 0;
        public int NumTilesHigh = 0;
        public int TileWidth = 0;
        public int TileHeight = 0;
        public float ExportScale = 1.0f;

        // Note: Because maps can be isometric and staggered we simply can't multply tile width (or height) by number of tiles wide (or high) to get width (or height)
        // We rely on the exporter to calculate the width and height of the map
        public int MapWidthInPixels = 0;
        public int MapHeightInPixels = 0;

        public float GetMapWidthInPixelsScaled()
        {
            return this.MapWidthInPixels * this.transform.lossyScale.x * this.ExportScale;
        }

        public float GetMapHeightInPixelsScaled()
        {
            return this.MapHeightInPixels * this.transform.lossyScale.y * this.ExportScale;
        }

        public Rect GetMapRect()
        {
            Vector2 pos_w = this.gameObject.transform.position;
            float width = this.MapWidthInPixels;
            float height = this.MapHeightInPixels;
            return new Rect(pos_w.x, pos_w.y - height, width, height);
        }

        public Rect GetMapRectInPixelsScaled()
        {
            Vector2 pos_w = this.gameObject.transform.position;
            float widthInPixels = GetMapWidthInPixelsScaled();
            float heightInPixels = GetMapHeightInPixelsScaled();
            return new Rect(pos_w.x, pos_w.y - heightInPixels, widthInPixels, heightInPixels);
        }

        public bool AreTilesStaggered()
        {
            // Hex and Iso Staggered maps both use "staggered" tiles
            return this.Orientation == MapOrientation.Staggered || this.Orientation == MapOrientation.Hexagonal;
        }


        public void Init()
        {
            Component[] tiles;
            tiles = GetComponentsInChildren(typeof(Tile));

            tileArray = new Tile[NumTilesWide, NumTilesHigh];
            if (tiles != null)
            {
                foreach (Tile tile in tiles)
                {
                    tile.id = tile.x * NumTilesHigh + tile.y;
                    tile.aStar = new TileAStar(tile);

                    tileArray[tile.x, tile.y] = tile;
                }

                for (int i = 0; i < NumTilesWide; i++)
                {
                    for (int j = 0; j < NumTilesHigh; j++)
                    {
                        tileList.Add(tileArray[i, j]);

                        List<Tile> neighbourList = new List<Tile>();
                        if (i > 0)
                            neighbourList.Add(tileArray[i - 1, j]);
                        if (j > 0)
                            neighbourList.Add(tileArray[i, j - 1]);
                        if (i < NumTilesWide - 1)
                            neighbourList.Add(tileArray[i + 1, j]);
                        if (j < NumTilesHigh - 1)
                            neighbourList.Add(tileArray[i, j + 1]);
                        if (tileArray[i, j] == null)
                            Debug.Log("null tile:" + i + "," + j);
                        else
                            tileArray[i, j].aStar.SetNeighbourList(neighbourList);

                    }
                }
            }
        }

        public byte[,] GetCostGrid(IDictionary<string, byte> TerrainToCost)
        {
            byte[,] grid = new byte[NumTilesHigh, NumTilesWide];
            for (int i = 0; i < NumTilesHigh; i++)
            {
                for (int j = 0; j < NumTilesWide; j++)
                {
                    byte cost = 0;
                    if (tileArray[i, j] != null)
                    {
                        if (TerrainToCost.TryGetValue(tileArray[i, j].Terrain, out cost))
                            grid[i, j] = cost;
                        else
                            grid[i, j] = 255;
                    }
                }
            }
            return grid;
        }

        //get a direct distance, regardless of the walkable state
        public int GetDistance(Tile tile1, Tile tile2)
        {
            return (int)(Mathf.Abs(tile1.x - tile2.x) + Mathf.Abs(tile1.y - tile2.y));
        }

        //private void OnDrawGizmosSelected()
        //{
        //    Vector3 pos_w = this.gameObject.transform.position;
        //    Vector3 topLeft = Vector3.zero + pos_w;
        //    Vector3 topRight = new Vector3(GetMapWidthInPixelsScaled(), 0) + pos_w;
        //    Vector3 bottomRight = new Vector3(GetMapWidthInPixelsScaled(), -GetMapHeightInPixelsScaled()) + pos_w;
        //    Vector3 bottomLeft = new Vector3(0, -GetMapHeightInPixelsScaled()) + pos_w;

        //    // To make gizmo visible, even when using depth-shader shaders, we decrease the z depth by the number of layers
        //    float depth_z = -1.0f * this.NumLayers;
        //    pos_w.z += depth_z;
        //    topLeft.z += depth_z;
        //    topRight.z += depth_z;
        //    bottomRight.z += depth_z;
        //    bottomLeft.z += depth_z;

        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawLine(topLeft, topRight);
        //    Gizmos.DrawLine(topRight, bottomRight);
        //    Gizmos.DrawLine(bottomRight, bottomLeft);
        //    Gizmos.DrawLine(bottomLeft, topLeft);
        //}
    }
}
