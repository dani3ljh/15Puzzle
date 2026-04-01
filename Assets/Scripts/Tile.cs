using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the GameObject representing a tile in the puzzle
/// </summary>
public class Tile : MonoBehaviour
{
	[SerializeField] private Text label;
	[SerializeField] private Color correctColor;
	[SerializeField] private Color normalColor;
	[SerializeField] private Image background;

	[HideInInspector] public BoardManager board;
	[HideInInspector] public int x;
	[HideInInspector] public int y;

	private int number;

	public void SetNumber(int number)
	{
		this.number = number;
		label.text = number.ToString();
	}

	public int GetNumber()
	{
		return number;
	}

	public bool GetCorrect(int width)
	{
		int correctX = (number - 1) % width;
		int correctY = (number - 1) / width;

		return x == correctX && y == correctY;
	}

	public void UpdateLabelColor(int width)
	{
		label.color = GetCorrect(width) ? correctColor : normalColor;
	}

	public void SetBackgroundColor(Color color)
	{
		background.color = color;
	}

	public void OnButtonPressed() => board.ButtonPress(x, y);
}