using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class ARMapArea : MonoBehaviour 
{
	public Text statusText;

	public ARWorldMap mappedWorld {get; private set; }
	public bool worldMapSaved {get; private set; }

	private bool hasStartedMapping;
	private bool mappingDone;

	private ARWorldMappingStatus currentMapStatus;

	void OnEnable () 
	{
		statusText.text = "Look around with device to map area";
		hasStartedMapping = false;
		mappingDone = false;
		currentMapStatus = ARWorldMappingStatus.ARWorldMappingStatusNotAvailable;
		//hook worldmapping status
		UnityARSessionNativeInterface.ARFrameUpdatedEvent += OnWorldMapStatusChange;

	}

	void OnDisable()
	{
		//unhook worldmapping status
		UnityARSessionNativeInterface.ARFrameUpdatedEvent -= OnWorldMapStatusChange;

	}

	void OnWorldMapStatusChange(UnityARCamera cam)
	{

	 if (!hasStartedMapping)
	 {
	     hasStartedMapping = true;
	 }
	 if (!mappingDone) 
	 {
			currentMapStatus = cam.worldMappingStatus;

			if (currentMapStatus != ARWorldMappingStatus.ARWorldMappingStatusMapped && currentMapStatus != ARWorldMappingStatus.ARWorldMappingStatusExtending) 
			{
				statusText.text = "Look around with device to map area";
			} 
			else if (currentMapStatus == ARWorldMappingStatus.ARWorldMappingStatusMapped)
			{
				statusText.text = "Area mapped! Saving...";
			}
	 }

	}


	public IEnumerator MapArea()
	{
		//while (world map status != mapped)
		while (currentMapStatus != ARWorldMappingStatus.ARWorldMappingStatusMapped)
		{
			yield return null;
		}


		worldMapSaved = false;
		//unhook world mapping status
		UnityARSessionNativeInterface.ARFrameUpdatedEvent -= OnWorldMapStatusChange;

		//save current map async(WorldMapCreated)
		UnityARSessionNativeInterface.GetARSessionNativeInterface().GetCurrentWorldMapAsync(WorldMapCreated);

		while (!worldMapSaved)
		{
			yield return null;
		}

		statusText.text = "";
	}


	void WorldMapCreated(ARWorldMap worldMap)
	{
		mappedWorld = worldMap;
		worldMapSaved = true;
	}

}
