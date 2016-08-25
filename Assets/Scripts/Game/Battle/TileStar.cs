using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Strategy
{
	
	public class TileAStar
	{
		public Tile tile;
		
		//neighbour list is only setup once the application is playing
		private List<Tile> neighbourList = new List<Tile>();
		public void SetNeighbourList(List<Tile> list) { neighbourList = list; }
		
		private List<Tile> disconnectedNeighbourList = new List<Tile>();
		
		public enum _AStarListState { Unassigned, Open, Close };
		public _AStarListState listState = _AStarListState.Unassigned;
		
		public Tile parent = null;
		public float scoreG;
		public float scoreH;
		public float scoreF;
		public float tempScoreG;
		public int rangeRequired;	//the range-cost required to move to this tile, for grid with height variance
		
		
		public TileAStar(Tile t) { tile = t; }
		
		public bool IsNeighbourDisconnected(Tile tile)
		{
			return disconnectedNeighbourList.Contains(tile) ? true : false;
		}
		
		public void DisconnectNeighbour(Tile tile)
		{
			if (neighbourList.Contains(tile))
			{
				neighbourList.Remove(tile);
				disconnectedNeighbourList.Add(tile);
			}
		}
		public void ConnectNeighbour(Tile tile)
		{
			if (disconnectedNeighbourList.Contains(tile))
			{
				neighbourList.Add(tile);
				disconnectedNeighbourList.Remove(tile);
			}
		}
		
		
		
		public List<Tile> GetNeighbourList(bool walkableOnly = false)
		{
			List<Tile> newList = new List<Tile>();
			if (walkableOnly)
			{
				for (int i = 0; i < neighbourList.Count; i++)
				{
					if (neighbourList[i].unit == null) newList.Add(neighbourList[i]);
				}
			}
			else
			{
				for (int i = 0; i < neighbourList.Count; i++) newList.Add(neighbourList[i]);
				for (int i = 0; i < disconnectedNeighbourList.Count; i++) newList.Add(disconnectedNeighbourList[i]);
			}
			return newList;
		}
		
		
		//call during a serach to scan through neighbour, check their score against the position passed
		//process walkable neighbours only, used to search for a walkable path via A*
		public void ProcessWalkableNeighbour(Tile targetTile)
		{
			for (int i = 0; i < neighbourList.Count; i++)
			{
				TileAStar neighbour = neighbourList[i].aStar;
				if ((neighbour.tile.unit == null) || neighbour.tile == targetTile)
				{
					//if the neightbour state is clean (never evaluated so far in the search)
					if (neighbour.listState == _AStarListState.Unassigned)
					{
						//check the score of G and H and update F, also assign the parent to currentNode
						neighbour.scoreG = scoreG + 1;
						neighbour.scoreH = Vector3.Distance(neighbour.tile.GetPos(), targetTile.GetPos());
						neighbour.UpdateScoreF();
						neighbour.parent = tile;
					}
					//if the neighbour state is open (it has been evaluated and added to the open list)
					else if (neighbour.listState == _AStarListState.Open)
					{
						//calculate if the path if using this neighbour node through current node would be shorter compare to previous assigned parent node
						tempScoreG = scoreG + 1;
						if (neighbour.scoreG > tempScoreG)
						{
							//if so, update the corresponding score and and reassigned parent
							neighbour.parent = tile;
							neighbour.scoreG = tempScoreG;
							neighbour.UpdateScoreF();
						}
					}
				}
			}
		}
		
		
		public void UpdateScoreF() { scoreF = scoreG + scoreH; }
	}
}
