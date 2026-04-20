using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles UI and animations for the puzzle
/// </summary>
public class BoardManager : MonoBehaviour
{
	[Header("Board")]
	[SerializeField] private int width;
	[SerializeField] private int height;

	[Header("Canvas Objects")]
	[SerializeField] private Transform canvas;
	[SerializeField] private RectTransform background;
	[SerializeField] private Transform tileFolder;

	[Header("Canvas Labels")]
	[SerializeField] private TMP_Text timerLabel;
	[SerializeField] private TMP_Text moveCountLabel;

	[Header("Label Colors")]
	[SerializeField] private Color normalColor;
	[SerializeField] private Color finishedColor;

	[Header("Prefabs")]
	[SerializeField] private GameObject tilePrefab;
	[SerializeField] private GameObject confirmationWindowPrefab;

	[Header("Win Effect")]
	[SerializeField] private ParticleSystem[] confetti;

	private LogicManager logic;
	private Dictionary<Tile, Coroutine> activeAnimations = new();
	private List<Color> groupColors;
	private Queue<(Tile tile, Vector2Int from, Vector2Int to)?> moveQueue;
	private int moveCount;
	private float timeElapsed;
	private bool hasShuffled;
	private bool timerRunning;
	private ConfirmationWindow confirmationWindow;
	public WindowType? windowType; //temp public
	
	public enum WindowType
	{
		Shuffle,
		Reset
	}

	private void Start()
	{
		// default to (1, 1) to make empty screen if there's an error getting playerprefs
		width = PlayerPrefs.GetInt("width", 1);
		height = PlayerPrefs.GetInt("height", 1);

		moveQueue = new();

		ResetBoard();
	}

	private void Update()
	{
		if (timerRunning)
		{
			timeElapsed += Time.deltaTime;
			int minutes = (int)(timeElapsed / 60);
			float seconds = timeElapsed % 60;
			timerLabel.text = $"{minutes:D2}:{seconds:00.0}";
		}

		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
			moveQueue.Enqueue(logic.MoveUp());
		else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
			moveQueue.Enqueue(logic.MoveDown());
		else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
			moveQueue.Enqueue(logic.MoveLeft());
		else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
			moveQueue.Enqueue(logic.MoveRight());
		else if (Input.GetKeyDown(KeyCode.R))
			TryResetBoard();
		else if (Input.GetKeyDown(KeyCode.T))
			TryShuffleBoard();
		else if (Input.GetKeyDown(KeyCode.Escape) && confirmationWindow != null)
			DestroyConfirmationWindow();
		else if (Input.GetKeyDown(KeyCode.Return) && confirmationWindow != null)
		{
			if (windowType == WindowType.Reset)
				ResetBoard();
			else if (windowType == WindowType.Shuffle)
				ShuffleBoard();
			DestroyConfirmationWindow();
		}

		if (moveQueue.Count > 0)
		{
			(Tile tile, Vector2Int from, Vector2Int to)? move = moveQueue.Dequeue();

			if (move == null)
				return;
			
			SetMoves(moveCount + 1);
			
			if (hasShuffled && !timerRunning)
				timerRunning = true;

			HandleMove(move.Value.tile, move.Value.from, move.Value.to);

			if (move.Value.tile.GetCorrect(logic.GetWidth()) && logic.CheckWin() && hasShuffled)
				Win();
		}
	}

	private void Win()
	{
		hasShuffled = false;
		timerRunning = false;
		timerLabel.color = finishedColor;
		moveCountLabel.color = finishedColor;
		for (int i = 0; i <confetti.Length; i++)
			confetti[i].Play();
	}

	private void SetMoves(int moves)
	{
		moveCount = moves;
		moveCountLabel.text = $"{moves}";
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
					moveQueue.Enqueue(logic.MoveDown());
			}
			else
			{
				for (int i = 0; i < y - gapIndicies.y; i++)
					moveQueue.Enqueue(logic.MoveUp());
			}
		}
		else
		{
			if (x < gapIndicies.x)
			{
				for (int i = 0; i < gapIndicies.x - x; i++)
					moveQueue.Enqueue(logic.MoveRight());
			}
			else
			{
				for (int i = 0; i < x - gapIndicies.x; i++)
					moveQueue.Enqueue(logic.MoveLeft());
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
	
	public void TryShuffleBoard()
	{
		
		if (confirmationWindow != null)
			DestroyConfirmationWindow();
		
		if (!timerRunning)
		{
			ShuffleBoard();
			return;
		}
		
		confirmationWindow = Instantiate(confirmationWindowPrefab, canvas).GetComponent<ConfirmationWindow>();
		confirmationWindow.SetHeaders("Shuffle Board?", "This will scramble the board and reset your timer.");
		confirmationWindow.SetCallbacks(() =>
		{
			DestroyConfirmationWindow();
			ShuffleBoard();
		}, DestroyConfirmationWindow);
		windowType = WindowType.Shuffle;
		// PrintWindowType();
	}
	
	private void DestroyConfirmationWindow()
	{
		Destroy(confirmationWindow.gameObject);
		windowType = null;
		// PrintWindowType();
	}

	public void ShuffleBoard()
	{
		// print("shuffling");
		hasShuffled = true;
		timeElapsed = 0;
		timerRunning = false;

		timerLabel.text = "00:00.0";
		timerLabel.color = normalColor;

		SetMoves(0);
		moveCountLabel.color = normalColor;
		
		StartCoroutine(ShuffleAnimate());

		if (logic.CheckWin())
			Win();
	}

	public void TryResetBoard()
	{
		if (confirmationWindow != null)
			DestroyConfirmationWindow();

		if (!hasShuffled)
		{
			ResetBoard();
			return;
		}
		
		confirmationWindow = Instantiate(confirmationWindowPrefab, canvas).GetComponent<ConfirmationWindow>();
		confirmationWindow.SetHeaders("Reset Board?", "This will solve the board and end your time.");
		confirmationWindow.SetCallbacks(() =>
		{
			DestroyConfirmationWindow();
			ResetBoard();
		}, DestroyConfirmationWindow);
		windowType = WindowType.Reset;
		// PrintWindowType();
	}

	public void ResetBoard()
	{
		hasShuffled = false;
		timeElapsed = 0;
		timerRunning = false;

		timerLabel.text = "00:00.0";
		timerLabel.color = normalColor;

		SetMoves(0);
		moveCountLabel.color = normalColor;

		if (logic == null)
		{
			InitializeBoard();
			return;
		}

		StartCoroutine(ResetAnimate());
	}
	
	private void InitializeBoard()
	{
		if (height <= 0 || width <= 0)
			throw new Exception("Board Size improper size");

		logic = new LogicManager(width, height);

		if (background.sizeDelta.x <= 0 || background.sizeDelta.y <= 0)
			throw new Exception("Background Size improper size");

		groupColors = GenerateGroupColors(width + height - 2);

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

			for (int j = 0; j < groupColors.Count; j++)
			{
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

	private IEnumerator ResetAnimate()
	{
		logic.ResetToSolved();

		// Animate all tiles to solved positions
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				Tile tile = logic.board[y, x];
				if (tile == null) continue;

				tile.x = x;
				tile.y = y;

				Vector2 targetPos = GetTilePosition(x, y);
				PlayTileAnimation(tile, targetPos, 0.25f);

				tile.UpdateLabelColor(width);
			}
		}

		while (activeAnimations.Count > 0)
			yield return null;
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
		float tileWidth = background.sizeDelta.x / width;
		float tileHeight = background.sizeDelta.y / height;

		float posX = -background.sizeDelta.x / 2f + tileWidth / 2f + x * tileWidth;
		float posY = background.sizeDelta.y / 2f - tileHeight / 2f - y * tileHeight;

		return new Vector2(posX, posY);
	}

	private void SetTileInstant(Tile tile, int x, int y)
	{
		RectTransform rect = tile.GetComponent<RectTransform>();
		float tileWidth = background.sizeDelta.x / width;
		float tileHeight = background.sizeDelta.y / height;

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
}