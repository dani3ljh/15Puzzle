using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
	[SerializeField] private Text label;
	[SerializeField] private Color correctColor;
	[SerializeField] private Color normalColor;

	[HideInInspector] public int x;
	[HideInInspector] public int y;

	private int number;

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void SetNumber(int number)
	{
		this.number = number;
		label.text = number.ToString();
	}

	public int GetNumber()
	{
		return number;
	}

	public void UpdateColor(int width)
	{
		int correctX = (number - 1) % width;
		int correctY = (number - 1) / width;

		if (x == correctX && y == correctY)
		{
			label.color = correctColor;
		}
		else
		{
			label.color = normalColor;
		}
	}
}
