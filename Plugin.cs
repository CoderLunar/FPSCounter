using System;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using Utilla;

namespace FpsCounter
{

	[BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
	[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
	public class Plugin : BaseUnityPlugin
	{
		public void Awake()
		{
			var harmony = new Harmony("com.lunar.gorillatag.lunarfpscounter");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

	[HarmonyPatch(typeof(GorillaLocomotion.Player))]
	[HarmonyPatch("FixedUpdate", MethodType.Normal)]
	public class MainClass
	{
		static bool gripDown;
		static GameObject canvasObj = null;
		public static float timer, refresh, avgFramerate;
		public static string display = "{0} FPS";
		static GameObject canvasTextObj = null;
		public static Text fps;

		static void Prefix (GorillaLocomotion.Player __instance)
		{
			List<InputDevice> list = new List<InputDevice>();
			InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller, list);
			list[0].TryGetFeatureValue(CommonUsages.gripButton, out gripDown);

			if (gripDown && canvasObj == null)
			{
				Draw();
			}

			else if (!gripDown && canvasObj != null)
			{
				GameObject.Destroy(canvasObj);
				canvasObj = null;
			}

			if (gripDown && canvasObj != null)
			{
				canvasObj.transform.position = __instance.leftHandTransform.position;
				canvasObj.transform.rotation = __instance.leftHandTransform.rotation;
			}
		}

		public static void Draw()
		{
			canvasObj = new GameObject();
			Canvas canvas = canvasObj.AddComponent<Canvas>();
			CanvasScaler canvasScaler  = canvasObj.AddComponent<CanvasScaler>();
			canvas.renderMode = RenderMode.WorldSpace;
			canvasScaler.dynamicPixelsPerUnit = 1000;

			canvasTextObj = new GameObject();
			canvasTextObj.transform.parent = canvasObj.transform;
			Text fps = canvasTextObj.AddComponent<Text>();
			fps.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
			fps.fontSize = 1;
			fps.alignment = TextAnchor.MiddleCenter;
			fps.resizeTextForBestFit = true;
			fps.resizeTextMinSize = 0;
			float timelapse = Time.smoothDeltaTime;
			timer = timer <= 0 ? refresh : timer -= timelapse;
			if(timer <=0) avgFramerate = (int) (1f / timelapse);
			fps.text = string.Format(display, avgFramerate.ToString());
			RectTransform fpsTransform = fps.GetComponent<RectTransform>();
			fpsTransform.localPosition = Vector3.zero;
			fpsTransform.position = new Vector3(0.04f, 0f, 0f);
			fpsTransform.sizeDelta = new Vector2(0.28f, 0.05f);
			fpsTransform.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		}
	}
}
