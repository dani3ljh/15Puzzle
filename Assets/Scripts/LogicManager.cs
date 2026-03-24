using System;
using System.Collections.Generic;
using UnityEngine;

public class LogicManager
{
	public Tile[,] board;
	private int width;
	private int height;

	public LogicManager(int width, int height)
	{
		this.width = width;
		this.height = height;

		board = new Tile[height, width];
	}

	private Vector2Int GetGapIndices()
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (board[y, x] == null)
					return new Vector2Int(x, y);
			}
		}

		throw new Exception("No gap found");
	}

	private (Tile tile, Vector2Int from, Vector2Int to)? TryMove(Vector2Int tileOffset)
	{
		Vector2Int gap = GetGapIndices();

		Vector2Int target = gap + tileOffset;

		// bounds check
		if (target.x < 0 || target.x >= width || target.y < 0 || target.y >= height)
			return null;

		Tile movingTile = board[target.y, target.x];

		// swap in array
		board[gap.y, gap.x] = movingTile;
		board[target.y, target.x] = null;

		return (movingTile, target, gap);
	}

	public (Tile, Vector2Int, Vector2Int)? MoveUp() => TryMove(Vector2Int.up);
	public (Tile, Vector2Int, Vector2Int)? MoveLeft() => TryMove(Vector2Int.right);
	public (Tile, Vector2Int, Vector2Int)? MoveDown() => TryMove(Vector2Int.down);
	public (Tile, Vector2Int, Vector2Int)? MoveRight() => TryMove(Vector2Int.left);

	public void ShuffleByRandomMoves(int moves = 100)
	{
		// start from solved (gap in bottom-right)
		Tile[,] solved = new Tile[height, width];
		List<Tile> tiles = new List<Tile>();
		for (int y = 0; y < height; y++)
			for (int x = 0; x < width; x++)
				if (board[y, x] != null)
					tiles.Add(board[y, x]);

		int idx = 0;
		for (int y = 0; y < height; y++)
			for (int x = 0; x < width; x++)
			{
				if (board[y, x] != null)
				{
					Tile t = tiles[idx++];
					t.x = x;
					t.y = y;
					solved[y, x] = t;
				}
			}

		board = solved;

		// find gap
		Vector2Int gap = GetGapIndices();

		System.Random rnd = new System.Random();

		// perform random moves
		for (int i = 0; i < moves; i++)
		{
			List<Vector2Int> possibleMoves = new List<Vector2Int>();

			// check neighbors
			Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
			foreach (var d in dirs)
			{
				Vector2Int target = gap + d;
				if (target.x >= 0 && target.x < width && target.y >= 0 && target.y < height)
					possibleMoves.Add(target);
			}

			if (possibleMoves.Count == 0) continue;

			// pick random neighbor to slide into gap
			Vector2Int moveTile = possibleMoves[rnd.Next(possibleMoves.Count)];
			SwapTiles(moveTile.x, moveTile.y, gap.x, gap.y);

			// update gap
			gap = moveTile;
		}
	}

	private void SwapTiles(int x1, int y1, int x2, int y2)
	{
		Tile tile1 = board[y1, x1];
		Tile tile2 = board[y2, x2];

		// swap in array
		board[y1, x1] = tile2;
		board[y2, x2] = tile1;

		// update tile coordinates
		if (tile1 != null)
		{
			tile1.x = x2;
			tile1.y = y2;
		}

		if (tile2 != null)
		{
			tile2.x = x1;
			tile2.y = y1;
		}
	}
}