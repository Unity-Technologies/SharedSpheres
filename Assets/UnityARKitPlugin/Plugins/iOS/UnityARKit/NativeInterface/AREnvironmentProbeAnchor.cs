using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using AOT;


namespace UnityEngine.XR.iOS
{

	public enum UnityAREnvironmentTexturing
	{
		UnityAREnvironmentTexturingNone,
		UnityAREnvironmentTexturingManual,
		UnityAREnvironmentTexturingAutomatic
	}


	public struct UnityAREnvironmentProbeAnchorData 
	{

		public IntPtr ptrIdentifier;

		/**
	 		The transformation matrix that defines the anchor's rotation, translation and scale in world coordinates.
			 */
		public UnityARMatrix4x4 transform;

		public IntPtr cubemapPtr;

		public Vector3 probeExtent;

	};


	public class AREnvironmentProbeAnchor 
	{

		UnityAREnvironmentProbeAnchorData envProbeAnchorData;

		public AREnvironmentProbeAnchor(UnityAREnvironmentProbeAnchorData uarepad)
		{
			envProbeAnchorData = uarepad;
		}

		public string identifier { get { return Marshal.PtrToStringAuto(envProbeAnchorData.ptrIdentifier); } }

		public Matrix4x4 transform { 
			get { 
				Matrix4x4 matrix = new Matrix4x4 ();
				matrix.SetColumn (0, envProbeAnchorData.transform.column0);
				matrix.SetColumn (1, envProbeAnchorData.transform.column1);
				matrix.SetColumn (2, envProbeAnchorData.transform.column2);
				matrix.SetColumn (3, envProbeAnchorData.transform.column3);
				return matrix;
			}
		}

		public Cubemap Cubemap
		{
			get 
			{ 
				if (envProbeAnchorData.cubemapPtr != IntPtr.Zero) {
					return Cubemap.CreateExternalTexture (0, TextureFormat.R8, false, envProbeAnchorData.cubemapPtr);
				} else {
					return null;
				}
			}

		}

		public Vector3 Extent
		{
			get { return envProbeAnchorData.probeExtent; }
		}
	}


	public partial class UnityARSessionNativeInterface
	{
		// Object Anchors
		public delegate void AREnvironmentProbeAnchorAdded(AREnvironmentProbeAnchor anchorData);
		public static event AREnvironmentProbeAnchorAdded AREnvironmentProbeAnchorAddedEvent;

		public delegate void AREnvironmentProbeAnchorUpdated(AREnvironmentProbeAnchor anchorData);
		public static event AREnvironmentProbeAnchorUpdated AREnvironmentProbeAnchorUpdatedEvent;

		public delegate void AREnvironmentProbeAnchorRemoved(AREnvironmentProbeAnchor anchorData);
		public static event AREnvironmentProbeAnchorRemoved AREnvironmentProbeAnchorRemovedEvent;


		delegate void internal_AREnvironmentProbeAnchorAdded(UnityAREnvironmentProbeAnchorData anchorData);
		delegate void internal_AREnvironmentProbeAnchorUpdated(UnityAREnvironmentProbeAnchorData anchorData);
		delegate void internal_AREnvironmentProbeAnchorRemoved(UnityAREnvironmentProbeAnchorData anchorData);

		public UnityAREnvironmentProbeAnchorData AddEnvironmentProbeAnchor(UnityAREnvironmentProbeAnchorData anchorData)
		{
			#if !UNITY_EDITOR && UNITY_IOS
			return SessionAddEnvironmentProbeAnchor(m_NativeARSession, anchorData);
			#else
			return new UnityAREnvironmentProbeAnchorData();
			#endif
		}


#if !UNITY_EDITOR && UNITY_IOS

		#region Environment Probe Anchors
		[MonoPInvokeCallback(typeof(internal_AREnvironmentProbeAnchorAdded))]
		static void _envprobe_anchor_added(UnityAREnvironmentProbeAnchorData anchor)
		{
			if (AREnvironmentProbeAnchorAddedEvent != null)
			{
				AREnvironmentProbeAnchor arEnvProbeAnchor = new AREnvironmentProbeAnchor(anchor);
				AREnvironmentProbeAnchorAddedEvent(arEnvProbeAnchor);
			}
		}

		[MonoPInvokeCallback(typeof(internal_AREnvironmentProbeAnchorUpdated))]
		static void _envprobe_anchor_updated(UnityAREnvironmentProbeAnchorData anchor)
		{
			if (AREnvironmentProbeAnchorUpdatedEvent != null)
			{
				AREnvironmentProbeAnchor arEnvProbeAnchor = new AREnvironmentProbeAnchor(anchor);
				AREnvironmentProbeAnchorUpdatedEvent(arEnvProbeAnchor);
			}
		}

		[MonoPInvokeCallback(typeof(internal_AREnvironmentProbeAnchorRemoved))]
		static void _envprobe_anchor_removed(UnityAREnvironmentProbeAnchorData anchor)
		{
			if (AREnvironmentProbeAnchorRemovedEvent != null)
			{
				AREnvironmentProbeAnchor arEnvProbeAnchor = new AREnvironmentProbeAnchor(anchor);
				AREnvironmentProbeAnchorRemovedEvent(arEnvProbeAnchor);
			}
		}
		#endregion

		[DllImport("__Internal")]
		private static extern void session_SetEnvironmentProbeAnchorCallbacks(IntPtr nativeSession, internal_AREnvironmentProbeAnchorAdded envprobeAnchorAddedCallback, 
		internal_AREnvironmentProbeAnchorUpdated envprobeAnchorUpdatedCallback, 
		internal_AREnvironmentProbeAnchorRemoved envprobeAnchorRemovedCallback);

		[DllImport("__Internal")]
		private static extern UnityAREnvironmentProbeAnchorData SessionAddEnvironmentProbeAnchor (IntPtr nativeSession, UnityAREnvironmentProbeAnchorData anchorData);


#endif //!UNITY_EDITOR && UNITY_IOS




	}
}