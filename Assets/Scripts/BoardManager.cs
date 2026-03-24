using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
	[Header("Board")]
	[SerializeField] private int width;
	[SerializeField] private int height;
	[SerializeField] private Vector2 backgroundSize;

	[Header("Canvas")]
	[SerializeField] private RectTransform background;
	[SerializeField] private Transform tileFolder;
	[SerializeField] private GameObject tilePrefab;

	private LogicManager logic;

	private Dictionary<Tile, Coroutine> activeAnimations = new Dictionary<Tile, Coroutine>();

	private void Start()
	{
		if (height <= 0 || width <= 0)
			throw new Exception("Board Size improper size");

		logic = new LogicManager(width, height);

		if (backgroundSize.x <= 0 || backgroundSize.y <= 0)
			throw new Exception("Background Size improper size");

		background.sizeDelta = backgroundSize;

		// Create tiles
		for (int i = 1; i < width * height; i++)
		{
			int x = (i - 1) % width;
			int y = (i - 1) / width;

			Tile tile = Instantiate(tilePrefab, tileFolder).GetComponent<Tile>();
			tile.SetNumber(i);
			tile.gameObject.name = $"Tile {i}";
			tile.x = x;
			tile.y = y;

			logic.board[y, x] = tile;

			SetTileInstant(tile, x, y);
			tile.UpdateColor(width);
		}

		// Shuffle with animation
		StartCoroutine(ShuffleAnimate());
	}

	private void Update()
	{
		(Tile tile, Vector2Int from, Vector2Int to)? move = null;

		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
			move = logic.MoveUp();
		else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
			move = logic.MoveDown();
		else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
			move = logic.MoveLeft();
		else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
			move = logic.MoveRight();

		if (move.HasValue)
		{
			HandleMove(move.Value.tile, move.Value.from, move.Value.to);
		}
	}

	private void HandleMove(Tile tile, Vector2Int from, Vector2Int to)
	{
		tile.x = to.x;
		tile.y = to.y;

		Vector2 targetPos = GetTilePosition(to.x, to.y);
		PlayTileAnimation(tile, targetPos, 0.15f);

		tile.UpdateColor(width);
	}

	private IEnumerator ShuffleAnimate()
	{
		logic.ShuffleByRandomMoves();

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
				tile.UpdateColor(width);
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
			rect.anchoredPosition = targetPos;
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