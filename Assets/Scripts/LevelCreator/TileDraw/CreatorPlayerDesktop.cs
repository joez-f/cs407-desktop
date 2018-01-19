﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public class CreatorPlayerDesktop : CreatorPlayer 
{
	// Buttons can be greyed out due to tool selection or undo/redo stack size.
	[SerializeField]
	private ToolbarButton undoButton;

	[SerializeField]
	private ToolbarButton redoButton;

	[SerializeField]
	private ToolbarButton fillButton;

	[SerializeField]
	private ToolbarButton rectFilledButton;

	[SerializeField]
	private ToolbarButton rectHollowButton;

	private Camera mainCam;

	protected override void Start()
	{
		base.Start();

		// Camera.main is slow so cache it.
		mainCam = Camera.main;
	}

	protected override void Update()
	{
		base.Update();

		if(Input.GetMouseButtonDown(0))
			StartDraw();

		if(Input.GetMouseButtonUp(0))
			StopDraw();
	}

	private Vector3 MouseToWorldPos()
	{
		Vector3 mousePos = new Vector3(Input.mousePosition.x, 
			Input.mousePosition.y, 10.0f);
		
		return mainCam.ScreenToWorldPoint(mousePos);
	}

	private Vector2 RoundVectorToInt(Vector2 vec)
	{
		return new Vector2(Mathf.RoundToInt(vec.x), Mathf.RoundToInt(vec.y));
	}

	// Position the preview block at mouse position.
	protected override void UpdatePreviewPos()
	{
		if(previewBlock != null)
		{
			Vector3 pos = MouseToWorldPos();

			pos.x = Mathf.RoundToInt(pos.x);
			pos.y = Mathf.RoundToInt(pos.y);
			pos.z = -5.0f;
			previewBlock.transform.position = pos;
		}
	}

	// Check if the Undo and Redo buttons need to be greyed out.
	public override void CheckHistory()
	{
		undoButton.SetVisible(undoStack.Count > 0);
		redoButton.SetVisible(redoStack.Count > 0);
	}

	// While still holding down the placement button, continually place or
	// remove tiles.
	protected override IEnumerator PencilDraw()
	{
		WaitForEndOfFrame wait = new WaitForEndOfFrame();

		HashSet<Vector2> tilePositions = new HashSet<Vector2>();
		HashSet<TileOperation> operations = new HashSet<TileOperation>();

		stopDrawing = false;

		while(!stopDrawing)
		{
			// Do not place tiles when mouse is on top of the UI elements.
			if(!EventSystem.current.IsPointerOverGameObject())
			{
				Vector2 tp = RoundVectorToInt(MouseToWorldPos());

				if(!tilePositions.Contains(tp))
				{
					tilePositions.Add(tp);

					operations = TryPlaceTile(operations, 
						activeTile.creatorPrefab, tp);
				}
			}

			yield return wait;
		}

		// Add the drawn tiles to the undo history.
		if (operations.Count > 0)
			AddUndoHistory(operations);
	}

	protected override IEnumerator Erase()
	{
		WaitForEndOfFrame wait = new WaitForEndOfFrame();

		HashSet<Vector2> tilePositions = new HashSet<Vector2>();
		HashSet<TileOperation> operations = new HashSet<TileOperation>();

		stopDrawing = false;

		while(!stopDrawing)
		{
			// Do not place tiles when mouse is on top of the UI elements.
			if(!EventSystem.current.IsPointerOverGameObject())
			{
				Vector2 tp = RoundVectorToInt(MouseToWorldPos());

				// Do not re-erase here if already erased here this operation.
				if(!tilePositions.Contains(tp))
				{
					tilePositions.Add(tp);
					operations = TryPlaceTile(operations, null, tp);
				}
			}

			yield return wait;
		}

		// Add the drawn tiles to the undo history.
		if (operations.Count > 0)
			AddUndoHistory(operations);
	}

	protected override void FloodFill()
	{
		throw new System.NotImplementedException();
	}

	/*
	protected override IEnumerator DrawHollowRect()
	{
		WaitForEndOfFrame wait = new WaitForEndOfFrame();

		Vector2 startPos = RoundVectorToInt(MouseToWorldPos());
		Vector2 endPos = Vector2.zero;

		HashSet<CreatorTile> previews = new HashSet<CreatorTile>();

		stopDrawing = false;

		while(!stopDrawing)
		{
			foreach(CreatorTile preview in previews)
			{
				if(preview != null)
					Destroy(preview.gameObject);
			}
			previews.Clear();

			endPos = RoundVectorToInt(MouseToWorldPos());

			previews = RectHelper(startPos, endPos, false, true);

			yield return wait;
		}

		foreach(CreatorTile preview in previews)
		{
			if(preview != null)
				Destroy(preview.gameObject);
		}
		previews.Clear();

		HashSet<CreatorTile> newTiles = RectHelper(startPos, endPos, false, false);
	}
	*/

	protected override IEnumerator DrawRect(bool filled)
	{
		WaitForEndOfFrame wait = new WaitForEndOfFrame();

		Vector2 startPos = RoundVectorToInt(MouseToWorldPos());
		Vector2 endPos = Vector2.zero;

		HashSet<CreatorTile> previews = new HashSet<CreatorTile>();

		stopDrawing = false;

		while(!stopDrawing)
		{
			//if(!EventSystem.current.IsPointerOverGameObject())
			//{
				foreach(CreatorTile preview in previews)
				{
					if(preview != null)
						Destroy(preview.gameObject);
				}
				previews.Clear();

				endPos = RoundVectorToInt(MouseToWorldPos());

				previews = RectHelper(startPos, endPos, filled, true);
			//}

			yield return wait;
		}

		foreach(CreatorTile preview in previews)
		{
			if(preview != null)
				Destroy(preview.gameObject);
		}
		previews.Clear();

		HashSet<CreatorTile> newTiles = RectHelper(startPos, endPos, filled, false);
	}

	public override void ClearAll()
	{
		HashSet<TileOperation> operations = new HashSet<TileOperation>();

		List<CreatorTile> tiles = CreatorPlayerWrapper.Get().GetTiles();

		foreach (CreatorTile tile in tiles)
			operations.Add(new TileOperation(null, tile, tile.transform.position));

		AddUndoHistory(operations);
	}

	// Grey out undo/redo buttons on desktop UI.
	public override void SetActiveTile(TileData tile)
	{
		base.SetActiveTile(tile);

		if(tile.IsUnitSize())
		{
			fillButton.SetVisible(true);
			rectFilledButton.SetVisible(true);
			rectHollowButton.SetVisible(true);
		}
		else
		{
			fillButton.SetVisible(false);
			rectFilledButton.SetVisible(false);
			rectHollowButton.SetVisible(false);
		}
	}
}