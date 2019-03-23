﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FlipCrinRob.Scripts
{
	[RequireComponent(typeof(ControllerTransforms))]
	public class ObjectSelection : MonoBehaviour
	{
		#region 01 Inspector and Variables
		public ControllerTransforms Controller { get; private set; }
		public bool RStay { get; set; }
		public bool LStay { get; set; }
		public enum ControllerEnum
		{
			Left,
			Right
		}
		private GameObject lTarget;
		private GameObject rTarget;
		private bool lSelectPrevious;
		private bool rSelectPrevious;
		private bool lGrabPrevious;
		private bool rGrabPrevious;
		[HideInInspector] public ControllerEnum controllerEnum;
		[HideInInspector] public GameObject grabObject;
		[HideInInspector] public GameObject lMidPoint;
		[HideInInspector] public GameObject rMidPoint;
		[HideInInspector] public GameObject lFocusObject;
		[HideInInspector] public GameObject rFocusObject;
		[HideInInspector] public SelectableObject rSelectableObject;
		[HideInInspector] public SelectableObject lSelectableObject;
		[HideInInspector] public LineRenderer lLr;
		[HideInInspector] public LineRenderer rLr;
		[HideInInspector] public bool disableSelection;
		
		[TabGroup("Selection Settings")] [Range(0f, 180f)] public float gaze = 60f;
		[TabGroup("Selection Settings")] [Range(0f, 180f)] public float manual = 25f;
		[TabGroup("Selection Settings")] public bool setSelectionRange;		
		[TabGroup("Selection Settings")] [ShowIf("setSelectionRange")] [Indent] [Range(0f, 250f)] public float selectionRange = 25f;		
		[TabGroup("Selection Settings")] public bool disableLeftHand;
		[TabGroup("Selection Settings")] public bool disableRightHand;
		
		[TabGroup("Object Lists")] public List<GameObject> globalList;
		[TabGroup("Object Lists")] public List<GameObject> gazeList;
		[TabGroup("Object Lists")] public List<GameObject> rHandList;
		[TabGroup("Object Lists")] public List<GameObject> lHandList;

		[TabGroup("Aesthetics and References")] [Space(5)] [Indent] [SerializeField] [Range(0f, 30f)] public int quality = 15;
		#endregion
		private void Start ()
		{
			Controller = GetComponent<ControllerTransforms>();
			
			SetupGameObjects();
			
			lLr = Controller.LeftControllerTransform().gameObject.AddComponent<LineRenderer>();
			rLr = Controller.RightControllerTransform().gameObject.AddComponent<LineRenderer>();
			
			Setup.LineRender(lLr, Controller.lineRenderMat, .005f, false);
			Setup.LineRender(rLr, Controller.lineRenderMat, .005f, false);
		}
		private void SetupGameObjects()
		{
			lMidPoint = new GameObject("MidPoint/Left");
			rMidPoint = new GameObject("MidPoint/Right");
			lTarget = new GameObject("TargetLineRender/Left");
			rTarget = new GameObject("Target/LineRender/Right");
			
			SetupMidpoints(lMidPoint.transform, Controller.LeftControllerTransform());
			SetupMidpoints(rMidPoint.transform, Controller.RightControllerTransform());
		}
		private static void SetupMidpoints(Transform m, Transform p)
		{
			m.position = p.position;
			m.parent = p;
			m.localRotation = new Quaternion(0,0,0,0);
		}
		private void Update () 
		{		
			SortLists();

			if (!disableSelection)
			{
				lFocusObject = ObjectMethods.FindFocusObject(lHandList, lTarget, Controller.LeftControllerTransform());
				rFocusObject = ObjectMethods.FindFocusObject(rHandList, rTarget, Controller.RightControllerTransform());
				lSelectableObject = ObjectMethods.FindSelectableObject(lFocusObject);
				rSelectableObject = ObjectMethods.FindSelectableObject(rFocusObject);
			}

			ObjectMethods.DrawLineRenderer(lLr, lFocusObject, lMidPoint, Controller.LeftControllerTransform(),lTarget, quality, disableSelection);
			ObjectMethods.DrawLineRenderer(rLr, rFocusObject, rMidPoint, Controller.RightControllerTransform(),rTarget, quality, disableSelection);

			DrawDebugLines(Controller.debugActive);
		}
		private void FixedUpdate()
		{
			ObjectMethods.Manipulation(lFocusObject, lSelectableObject, Controller.LeftGrab(), lGrabPrevious, Controller.LeftControllerTransform(), lMidPoint.transform);
			ObjectMethods.Manipulation(rFocusObject, rSelectableObject, Controller.RightGrab(), rGrabPrevious, Controller.RightControllerTransform(), rMidPoint.transform);
			
			lGrabPrevious = Controller.LeftGrab();
			rGrabPrevious = Controller.RightGrab();
		}

		private void LateUpdate()
		{
			ObjectMethods.Selection(lFocusObject, lSelectableObject, Controller.LeftSelect(), lSelectPrevious);
			ObjectMethods.Selection(rFocusObject, rSelectableObject, Controller.RightSelect(), rSelectPrevious);
		
			lSelectPrevious = Controller.LeftSelect();
			rSelectPrevious = Controller.RightSelect();
		}

		private void SortLists()
		{
			lHandList.Sort(SortBy.FocusObjectL);
			rHandList.Sort(SortBy.FocusObjectR);
		}
		
		private void DrawDebugLines(bool b)
		{
			if (!b) return;
			
			foreach (var g in gazeList)
			{
				//Debug.DrawLine(Controller.CameraPosition(), g.transform.position, Color.cyan);
			}
			foreach (var g in lHandList)
			{
				//Debug.DrawLine(Controller.LeftControllerTransform().position, g.transform.position, Color.blue);
			}
			foreach (var g in rHandList)
			{
				//Debug.DrawLine(Controller.RightControllerTransform().position, g.transform.position, Color.blue);
			}
			if (lFocusObject != null)
			{
				var position = lMidPoint.transform.position;
				//Debug.DrawLine(position, lTarget.transform.position, Color.red);
				//Debug.DrawLine(Controller.LeftControllerTransform().position, position, Color.red);
				Debug.DrawLine(Controller.LeftControllerTransform().position, lFocusObject.transform.position, Color.green);
			}
			if (rFocusObject != null)
			{
				var position = rMidPoint.transform.position;
				//Debug.DrawLine(position, rTarget.transform.position, Color.red);
				//Debug.DrawLine(Controller.RightControllerTransform().position, position, Color.red);
				Debug.DrawLine(Controller.RightControllerTransform().position, rFocusObject.transform.position, Color.green);
			}
		}
	}
}
