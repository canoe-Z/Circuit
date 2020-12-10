using UnityEngine;
using System.Collections;

public class SnapshotCameraTest : MonoBehaviour
{
	public GameObject prefab;
	public Color backgroundColor = Color.clear;
	public Vector3 position = new Vector3(0, 0, 1);
	public Vector3 rotation = new Vector3(345.8529f, 313.8297f, 14.28433f);
	public Vector3 scale = new Vector3(1, 1, 1);

	private SnapshotCamera snapshotCamera;
	private Texture2D texture;

	void Start()
	{
		snapshotCamera = SnapshotCamera.
			MakeSnapshotCamera("SnapshotLayer");

		UpdatePreview();
	}

	void OnGUI()
	{
		GUI.TextField(new Rect(10, 5, 275, 21), "Press \"Spacebar\" to save the snapshot");

		if (texture != null)
		{
			GUI.backgroundColor = Color.clear;
			GUI.Box(new Rect(10, 32, texture.width, texture.height), texture);
		}
	}

	public void UpdatePreview()
	{
		if (prefab != null)
		{
			// Destroy the texture to prevent a memory leak
			// For a bit of fun you can try removing this and watching the memory profiler while for example continuously changing the rotation to trigger UpdatePreview()
			Object.Destroy(texture);

			// Take a new snapshot of the objectToSnapshot
			texture = snapshotCamera.TakePrefabSnapshot(prefab, backgroundColor,
				position, Quaternion.Euler(rotation), scale, width: 1024, height: 1024);
		}
	}

	void Update()
	{
		UpdatePreview();

		// Save a PNG of the snapshot when pressing space
		if (Input.GetKeyUp(KeyCode.Space))
		{

			System.IO.FileInfo fi = SnapshotCamera.SavePNG(texture);

			Debug.Log(string.Format("Snapshot {0} saved to {1}", fi.Name, fi.DirectoryName));
		}
	}
}
