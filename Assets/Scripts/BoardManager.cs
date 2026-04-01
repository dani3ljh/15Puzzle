using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Handles UI and animations for the puzzle
/// </summary>
public class BoardManager : MonoBehaviour
{
	[Header("Board")]
	[SerializeField] private int width;
	[SerializeField] private int height;
	[SerializeField] private Vector2 backgroundSize;
	[SerializeField] private bool shuffleOnStart;

	[Header("Canvas")]
	[SerializeField] private RectTransform background;
	[SerializeField] private Transform tileFolder;
	[SerializeField] private GameObject tilePrefab;

	[Header("Win Effect")]
	[SerializeField] private ParticleSystem[] confetti;

	private LogicManager logic;
	private Dictionary<Tile, Coroutine> activeAnimations = new();
	private List<Color> groupColors;
	private List<(Tile tile, Vector2Int from, Vector2Int to)?> moveQueue;

	private void Start()
	{
		moveQueue = new();

		ResetBoard();

		// Shuffle with animation
		if (shuffleOnStart)
			StartCoroutine(ShuffleAnimate());
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
			moveQueue.Add(logic.MoveUp());
		else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
			moveQueue.Add(logic.MoveDown());
		else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
			moveQueue.Add(logic.MoveLeft());
		else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
			moveQueue.Add(logic.MoveRight());
		else if (Input.GetKeyDown(KeyCode.R))
		{
			ResetBoard();
			StartCoroutine(ShuffleAnimate());
			if (logic.CheckWin())
				WinEffect();
		}

		if (moveQueue.Count > 0)
		{
			(Tile tile, Vector2Int from, Vector2Int to)? move = moveQueue[0];
			moveQueue.RemoveAt(0);

			if (move == null)
				return;

			HandleMove(move.Value.tile, move.Value.from, move.Value.to);
			if (move.Value.tile.GetCorrect(logic.GetWidth()))
			{
				if (logic.CheckWin())
					WinEffect();
			}
		}
	}

	private void WinEffect()
	{
		for (int i = 0; i <confetti.Length; i++)
			confetti[i].Play();
	}

	public void ButtonPress(int x, int y)
	{
		Vector2Int gapIndicies = logic.GetGapIndices();

		// print($"pressing button ({x}, {y}), gap: {gapIndicies}");

		if (x != gapIndicies.x && y != gapIndicies.y)
			return;
		
		if (x == gapIndicies.x)
		{
			if (y < gapIndicies.y)
			{
				for (int i = 0; i < gapIndicies.y - y; i++)
					moveQueue.Add(logic.MoveDown());
			}
			else
			{
				for (int i = 0; i < y - gapIndicies.y; i++)
					moveQueue.Add(logic.MoveUp());
			}
		}
		else
		{
			if (x < gapIndicies.x)
			{
				for (int i = 0; i < gapIndicies.x - x; i++)
					moveQueue.Add(logic.MoveRight());
			}
			else
			{
				for (int i = 0; i < x - gapIndicies.x; i++)
					moveQueue.Add(logic.MoveLeft());
			}
		}
	}
	
	private List<Color> GenerateGroupColors(int count)
	{
		List<Color> colors = new();
		
		for (int i = 0; i < count; i++)
		{
			float hue = (float)i / count;
			float saturation = 0.9f;
			float value = 0.8f;
			
			colors.Add(Color.HSVToRGB(hue, saturation, value));
		}
		
		return colors;
	}

	private void ResetBoard()
	{
		if (logic != null)
		{
			for (int y = 0; y < logic.board.GetLength(0); y++)
			{
				for (int x = 0; x < logic.board.GetLength(1); x++)
				{
					if (logic.board[y, x])
						Destroy(logic.board[y, x].gameObject);
				}
			}
			logic = null;
		}

		if (height <= 0 || width <= 0)
			throw new Exception("Board Size improper size");

		logic = new LogicManager(width, height);

		if (backgroundSize.x <= 0 || backgroundSize.y <= 0)
			throw new Exception("Background Size improper size");

		background.sizeDelta = backgroundSize;
		
		groupColors = GenerateGroupColors(width + height - 2);

		// Create tiles
		for (int i = 1; i < width * height; i++)
		{
			int x = (i - 1) % width;
			int y = (i - 1) / width;

			Tile tile = Instantiate(tilePrefab, tileFolder).GetComponent<Tile>();
			tile.SetNumber(i);
			tile.gameObject.name = $"Tile {i}";
			tile.board = this;
			tile.x = x;
			tile.y = y;
			
			// Assign group colors
			for (int j = 0; j < groupColors.Count; j++)
			{
				// first row then first column
				if (j % 2 == 0 && y == j / 2 || j % 2 == 1 && x == j / 2)
				{
					tile.SetBackgroundColor(groupColors[j]);
					break;
				}
			}

			logic.board[y, x] = tile;

			SetTileInstant(tile, x, y);
			tile.UpdateLabelColor(width);
		}
	}

	private void HandleMove(Tile tile, Vector2Int from, Vector2Int to)
	{
		tile.x = to.x;
		tile.y = to.y;

		Vector2 targetPos = GetTilePosition(to.x, to.y);
		PlayTileAnimation(tile, targetPos, 0.15f);

		tile.UpdateLabelColor(width);
	}

	private IEnumerator ShuffleAnimate()
	{
		logic.Shuffle();

		// Animate all tiles to their shuffled positions
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				Tile tile = logic.board[y, x];
				if (tile == null) continue;

				tile.x = x;
				tile.y = y;

				Vector2 targetPos = GetTilePosition(x, y);
				PlayTileAnimation(tile, targetPos, 0.3f); // slower for shuffle effect
				tile.UpdateLabelColor(width);
			}
		}

		// wait for all animations to finish
		while (activeAnimations.Count > 0)
			yield return null;
	}

	private Vector2 GetTilePosition(int x, int y)
	{
		float tileWidth = backgroundSize.x / width;
		float tileHeight = backgroundSize.y / height;

		float posX = -backgroundSize.x / 2f + tileWidth / 2f + x * tileWidth;
		float posY = backgroundSize.y / 2f - tileHeight / 2f - y * tileHeight;

		return new Vector2(posX, posY);
	}

	private void SetTileInstant(Tile tile, int x, int y)
	{
		RectTransform rect = tile.GetComponent<RectTransform>();
		float tileWidth = backgroundSize.x / width;
		float tileHeight = backgroundSize.y / height;

		rect.localScale = Vector3.one;
		rect.sizeDelta = new Vector2(tileWidth, tileHeight);
		rect.anchoredPosition = GetTilePosition(x, y);
	}

	private void PlayTileAnimation(Tile tile, Vector2 targetPos, float duration)
	{
		RectTransform rect = tile.GetComponent<RectTransform>();

		// Snap if already animating
		if (activeAnimations.ContainsKey(tile))
		{
			StopCoroutine(activeAnimations[tile]);
			// rect.anchoredPosition = targetPos;
			activeAnimations.Remove(tile);
		}

		Coroutine anim = StartCoroutine(AnimateTile(tile, targetPos, duration));
		activeAnimations[tile] = anim;
	}

	private IEnumerator AnimateTile(Tile tile, Vector2 targetPos, float duration)
	{
		RectTransform rect = tile.GetComponent<RectTransform>();
		Vector2 startPos = rect.anchoredPosition;
		float time = 0f;

		while (time < duration)
		{
			time += Time.deltaTime;
			float t = time / duration;
			t = t * t * (3f - 2f * t); // smoothstep easing

			rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
			yield return null;
		}

		rect.anchoredPosition = targetPos;
		activeAnimations.Remove(tile);
	}

	[ContextMenu("Print Board State")]
	private void PrintBoardState()
	{
		StringBuilder output = new StringBuilder();

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (logic.board[y, x] == null)
					output.Append("nu, ");
				else
					output.Append(logic.board[y, x].GetNumber().ToString("D2") + ", ");
			}
			output.Append("\n");
		}

		print(output.ToString());
	}
}