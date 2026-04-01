using System.Collections.Generic;

/// <summary>
/// Handles shuffling the puzzle
/// </summary>
public class BoardShuffler
{
    private LogicManager logic;
    private System.Random rnd;
    
    public BoardShuffler(LogicManager logic)
    {
        this.logic = logic;
        rnd = new();
    }
    
	public void ShuffleUniform()
	{
		bool isLinear = logic.GetWidth() == 1 || logic.GetHeight() == 1;

		// Collect tiles
		List<Tile> tiles = CollectTiles();

		// Shuffle only if board is 2D
		if (!isLinear)
			ShuffleTiles(tiles);

		// Place tiles and random gap
		Tile[,] newBoard = PlaceTilesWithRandomGap(tiles);

		// Fix parity for 2D boards
		if (!isLinear && !IsSolvable(newBoard))
			FixParity(newBoard);

		logic.board = newBoard;
	}

	private List<Tile> CollectTiles()
	{
        List<Tile> tiles = new();
		for (int y = 0; y < logic.GetHeight(); y++)
			for (int x = 0; x < logic.GetWidth(); x++)
				if (logic.board[y, x] != null)
					tiles.Add(logic.board[y, x]);
		return tiles;
	}

	private void ShuffleTiles(List<Tile> tiles)
	{
		for (int i = tiles.Count - 1; i > 0; i--)
		{
			int j = rnd.Next(i + 1);
			(tiles[i], tiles[j]) = (tiles[j], tiles[i]);
		}
	}

	private Tile[,] PlaceTilesWithRandomGap(List<Tile> tiles)
	{
		int totalSlots = logic.GetWidth() * logic.GetHeight();
		int gapIndex = rnd.Next(totalSlots);

		Tile[,] newBoard = new Tile[logic.GetHeight(), logic.GetWidth()];
		int tileIndex = 0;

		for (int i = 0; i < totalSlots; i++)
		{
			int x = i % logic.GetWidth();
			int y = i / logic.GetWidth();

			if (i == gapIndex)
				newBoard[y, x] = null;
			else
			{
				Tile t = tiles[tileIndex++];
				t.x = x;
				t.y = y;
				newBoard[y, x] = t;
			}
		}

		return newBoard;
	}

	private bool IsSolvable(Tile[,] board)
	{
		List<int> numbers = new();
		int gapRow = 0;
		
		for (int y = 0; y < logic.GetHeight(); y++)
		{
			for (int x = 0; x < logic.GetWidth(); x++)
			{
				if (board[y, x] == null)
					gapRow = y;
				else
					numbers.Add(board[y, x].GetNumber());
			}
		}
		
		int inversions = 0;
		
		for (int i = 0; i < numbers.Count; i++)
			for (int j = i + 1; j < numbers.Count; j++)
				if (numbers[i] > numbers[j])
					inversions++;
		
		if (logic.GetWidth() % 2 == 1)
			return inversions % 2 == 0;
		else
		{
			int blankFromBottom = logic.GetHeight() - gapRow;
			
			if (blankFromBottom % 2 == 0)
				return inversions % 2 == 1;
			else
				return inversions % 2 == 0;
		}
	}
	
	private void FixParity(Tile[,] board)
	{
		Tile a = null, c = null;
		
		for (int y = 0; y < logic.GetHeight(); y++)
		{
			for (int x = 0; x < logic.GetWidth(); x++)
			{
				if (board[y, x] != null)
				{
					if (a == null)
						a = board[y, x];
					else
					{
						c = board[y, x];
						break;
					}
				}
			}
			if (c != null)
				break;
		}
		
		int ax = a.x, ay = a.y;
		int cx = c.x, cy = c.y;

		board[ay, ax] = c;
		board[cy, cx] = a;
		
		a.x = cx;
		a.y = cy;
		
		c.x = ax;
		c.y = ay;
	}
}