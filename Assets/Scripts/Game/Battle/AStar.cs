using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Strategy;

namespace Strategy
{
	
	public class AStar : MonoBehaviour
    {
		
		//search for a path, through walkable tile only
		//for normal movement, return the path in a list of hexTile
        public static List<Tile> SearchWalkableTile(byte[,] costMap, Tile originTile, Tile destTile)
        {
			
			List<Tile> closeList=new List<Tile>();
			List<Tile> openList=new List<Tile>();
			
			Tile currentTile=originTile;
			
			float currentLowestF=Mathf.Infinity;
			int id=0;
			int i=0;
			
			while(true){
				
				//if we have reach the destination
				if(currentTile==destTile) break;
				
				//move currentNode to closeList;
				closeList.Add(currentTile);
				currentTile.aStar.listState=TileAStar._AStarListState.Close;
				
				//loop through the neighbour of current loop, calculate  score and stuff
				currentTile.aStar.ProcessWalkableNeighbour(destTile);
				
				//put all neighbour in openlist
				foreach(Tile neighbour in currentTile.aStar.GetNeighbourList(true)){
                    if (
                        neighbour.x>costMap.GetUpperBound(0)||
                        neighbour.y>costMap.GetUpperBound(1)||
                        costMap[neighbour.x, neighbour.y] == 255 ||
                        neighbour.unit != null)
                    {
                        continue;
                    }
					if(neighbour.aStar.listState==TileAStar._AStarListState.Unassigned || neighbour==destTile){
						//~ //set the node state to open
						neighbour.aStar.listState=TileAStar._AStarListState.Open;
						openList.Add(neighbour);
					}
				}
				
				//clear the current node, before getting a new one, so we know if there isnt any suitable next node
				currentTile=null;
				
				
				currentLowestF=Mathf.Infinity;
				id=0;
				for(i=0; i<openList.Count; i++){
					if(openList[i].aStar.scoreF<currentLowestF){
						currentLowestF=openList[i].aStar.scoreF;
						currentTile=openList[i];
						id=i;
					}
				}
				
				
				//if there's no node left in openlist, path doesnt exist
				if(currentTile==null) {
					break;
				}

				openList.RemoveAt(id);
			}
			
			if(currentTile==null){
                float tileSize = TileManager.GetTileSize() * TileManager.GetGridToTileSizeRatio();
				currentLowestF=Mathf.Infinity;
				for(i=0; i<closeList.Count; i++){
					float dist=Vector3.Distance(destTile.GetPos(), closeList[i].GetPos());
					if(dist<currentLowestF){
						currentLowestF=dist;
						currentTile=closeList[i];
						if(dist<tileSize*1.5f) break;
					}
				}
			}
			
			List<Tile> path=new List<Tile>();
			while(currentTile!=null){
				if(currentTile==originTile || currentTile==currentTile.aStar.parent) break;
				path.Add(currentTile);
				currentTile=currentTile.aStar.parent;
			}
			
			path=InvertTileArray(path);
			
			ResetGraph(destTile, openList, closeList);
			
			return path;
		}

        //get all the tiles within certain distance from a given tile
        public static List<Tile> GetTilesInRange(byte[,]costMap, Tile srcTile, int range)
        {
            List<Tile> closeList = new List<Tile>();
            List<Tile> openList = new List<Tile>();
            
            srcTile.aStar.scoreG = 0;
            openList.Add(srcTile);

            while (openList.Count > 0)
            {
                Tile currentTile = openList[0];
                if (currentTile == null)
                    break;
                openList.RemoveAt(0);

                if (currentTile.aStar.scoreG > range)
                    continue;

                int foundInClose = -1;
                for (int i = 0; i < closeList.Count; i++)
                {
                    if (closeList[i].id == currentTile.id)
                    {
                        foundInClose = i;
                        break;
                    }
                }
                if (foundInClose != -1)
                {
                    if (closeList[foundInClose].aStar.scoreG <= currentTile.aStar.scoreG)
                        continue;
                    closeList[foundInClose] = currentTile;
                }
                else
                    closeList.Add(currentTile);

                List<Tile> neighbourList = currentTile.aStar.GetNeighbourList();
                for (int m = 0; m < neighbourList.Count; m++)
                {
                    Tile neighbour = neighbourList[m];
                    int cost = 255;
                    if(neighbour.x<costMap.GetUpperBound(0)&&
                        neighbour.y<costMap.GetUpperBound(1))
                        cost = (int)costMap[neighbour.x, neighbour.y];
                    
                    neighbour.aStar.scoreG = currentTile.aStar.scoreG + cost;
                    neighbour.aStar.parent = currentTile;
                    openList.Add(neighbour);
                }

            }

            return closeList;

        }
		
		//search the shortest path through all tile reagardless of status
		//this is used to accurately calculate the distance between 2 tiles in term of tile
		//distance calculated applies for line traverse thru walkable tiles only, otherwise it can be calculated using the coordinate
		public static int GetDistance(Tile srcTile, Tile targetTile){
			List<Tile> closeList=new List<Tile>();
			List<Tile> openList=new List<Tile>();
			
			Tile currentTile=srcTile;
			if(srcTile==null) Debug.Log("src tile is null!!!");
			
			float currentLowestF=Mathf.Infinity;
			int id=0;
			int i=0;
			
			while(true){
				//if we have reach the destination
				if(currentTile==targetTile) break;
				
				//move currentNode to closeList;
				closeList.Add(currentTile);
				currentTile.aStar.listState=TileAStar._AStarListState.Close;
				
				//loop through all neighbours, regardless of status 
				//currentTile.ProcessAllNeighbours(targetTile);
				currentTile.aStar.ProcessWalkableNeighbour(targetTile);
				
				//put all neighbour in openlist
				foreach(Tile neighbour in currentTile.aStar.GetNeighbourList()){
					if(neighbour.unit!=null && neighbour!=targetTile) continue;
					if(neighbour.aStar.listState==TileAStar._AStarListState.Unassigned) {
						//set the node state to open
						neighbour.aStar.listState=TileAStar._AStarListState.Open;
						openList.Add(neighbour);
					}
				}
				
				currentTile=null;
				
				currentLowestF=Mathf.Infinity;
				id=0;
				for(i=0; i<openList.Count; i++){
					if(openList[i].aStar.scoreF<currentLowestF){
						currentLowestF=openList[i].aStar.scoreF;
						currentTile=openList[i];
						id=i;
					}
				}
				
				if(currentTile==null) return -1;
				
				openList.RemoveAt(id);
			}
			
			
			int counter=0;
			while(currentTile!=null){
				counter+=1;
				currentTile=currentTile.aStar.parent;
			}
			
			ResetGraph(targetTile, openList, closeList);
			
			return counter-1;
		}
		
		
		
		private static List<Vector3> InvertArray(List<Vector3> p){
			List<Vector3> pInverted=new List<Vector3>();
			for(int i=0; i<p.Count; i++){
				pInverted.Add(p[p.Count-(i+1)]);
			}
			return pInverted;
		}
		
		private static List<Tile> InvertTileArray(List<Tile> p){
			List<Tile> pInverted=new List<Tile>();
			for(int i=0; i<p.Count; i++){
				pInverted.Add(p[p.Count-(i+1)]);
			}
			return pInverted;
		}
		
		
		//reset all the tile, called after a search is complete
		private static void ResetGraph(Tile hTile, List<Tile> oList, List<Tile> cList){
			hTile.aStar.listState=TileAStar._AStarListState.Unassigned;
			hTile.aStar.parent=null;
			
			foreach(Tile tile in oList){
				tile.aStar.listState=TileAStar._AStarListState.Unassigned;
				tile.aStar.parent=null;
			}
			foreach(Tile tile in cList){
				tile.aStar.listState=TileAStar._AStarListState.Unassigned;
				tile.aStar.parent=null;
			}
		}
	}
	

}