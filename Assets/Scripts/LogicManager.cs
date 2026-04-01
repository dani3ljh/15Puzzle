using System;
using UnityEngine;

/// <summary>
/// Handles logic for the puzzle
/// </summary>
public class LogicManager
{
	public Tile[,] board;

	private int width;
	private int height;
	private BoardShuffler shuffler;

	public int GetWidth()
	{
		return width;
	}

	public int GetHeight()
	{
		return height;
	}

	public LogicManager(int width, int height)
	{
		this.width = width;
		this.height = height;

		board = new Tile[height, width];
		shuffler = new(this);
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

	public bool CheckWin()
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (board[y, x] == null)
					continue;
				if (!board[y, x].GetCorrect(width))
					return false;
			}
		}

		return true;
	}

	public (Tile, Vector2Int, Vector2Int)? MoveUp() => TryMove(Vector2Int.up);
	public (Tile, Vector2Int, Vector2Int)? MoveLeft() => TryMove(Vector2Int.right);
	public (Tile, Vector2Int, Vector2Int)? MoveDown() => TryMove(Vector2Int.down);
	public (Tile, Vector2Int, Vector2Int)? MoveRight() => TryMove(Vector2Int.left);

	public void Shuffle() => shuffler.ShuffleUniform();
}