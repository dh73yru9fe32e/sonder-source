using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.InputSystem;

namespace Sonder.Managers;

public class PcRuntime : MonoBehaviour
{
	public static PcRuntime Instance { get; private set; }

	public bool ClickerEnabled { get; set; }

	private bool _clickWasDown;
	private float _clickReadyAt;

	private bool _playerResolved;
	private PropertyInfo _playerInstanceProp;
	private FieldInfo _playerInstanceField;

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Update()
	{
		if (ClickerEnabled)
		{
			MouseClicker();
		}
	}

	private Component GetGorillaPlayer()
	{
		if (!_playerResolved)
		{
			_playerResolved = true;
			Type t = Type.GetType("GorillaLocomotion.GTPlayer, Assembly-CSharp")
			      ?? Type.GetType("GorillaLocomotion.Player, Assembly-CSharp");
			if (t != null)
			{
				const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
				_playerInstanceProp = t.GetProperty("Instance", flags) ?? t.GetProperty("instance", flags);
				if (_playerInstanceProp == null)
				{
					_playerInstanceField = t.GetField("Instance", flags) ?? t.GetField("instance", flags) ?? t.GetField("_instance", flags);
				}
			}
		}
		if (_playerInstanceProp != null)
		{
			return _playerInstanceProp.GetValue(null, null) as Component;
		}
		if (_playerInstanceField != null)
		{
			return _playerInstanceField.GetValue(null) as Component;
		}
		return null;
	}

	private void MouseClicker()
	{
		if (Mouse.current == null)
		{
			return;
		}
		if (!Mouse.current.leftButton.isPressed)
		{
			_clickWasDown = false;
			return;
		}
		if (_clickWasDown || Time.time < _clickReadyAt)
		{
			return;
		}
		_clickWasDown = true;

		Vector2 mousePos = Mouse.current.position.ReadValue();
		if (MenuManager.IsOpen && MenuManager.ScreenMode && MenuManager.PointerOverMenu)
		{
			return;
		}

		Camera cam = MenuManager.EyeCamera;
		if ((Object)(object)cam == (Object)null)
		{
			return;
		}

		GorillaTagger tagger = GorillaTagger.Instance;
		GameObject triggerObject = tagger != null ? tagger.rightHandTriggerCollider : null;
		Collider trigger = triggerObject == null ? null : triggerObject.GetComponent<Collider>();
		if (trigger == null)
		{
			return;
		}

		Ray ray = cam.ScreenPointToRay(mousePos);
		RaycastHit[] hits = Physics.RaycastAll(ray, 1000f, ~0, QueryTriggerInteraction.Collide);
		if (hits == null || hits.Length == 0)
		{
			return;
		}
		Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

		foreach (RaycastHit hit in hits)
		{
			if (hit.collider == null)
			{
				continue;
			}
			if (!TryClick(hit.collider, trigger))
			{
				continue;
			}
			_clickReadyAt = Time.time + 0.1f;
			return;
		}
	}

	private bool TryClick(Collider target, Collider trigger)
	{
		bool clicked = false;
		foreach (MonoBehaviour mb in target.GetComponentsInParent<MonoBehaviour>())
		{
			if (mb == null || !mb.enabled)
			{
				continue;
			}
			Type type = mb.GetType();
			string typeName = type.Name;
			if (typeName.IndexOf("Button", StringComparison.OrdinalIgnoreCase) < 0 &&
			    typeName.IndexOf("Pressable", StringComparison.OrdinalIgnoreCase) < 0)
			{
				continue;
			}
			try
			{
				MethodInfo enter = FindMethod(type, "OnTriggerEnter", new Type[] { typeof(Collider) });
				if (enter != null)
				{
					enter.Invoke(mb, new object[] { trigger });
					clicked = true;
					continue;
				}
				string[] names = { "ButtonActivationWithHand", "ButtonActivation", "OnPress", "Press", "Activate", "OnClick", "Click" };
				foreach (string n in names)
				{
					MethodInfo m = FindMethod(type, n, Type.EmptyTypes);
					object[] args = new object[0];
					if (m == null)
					{
						m = FindMethod(type, n, new Type[] { typeof(bool) });
						args = new object[] { false };
					}
					if (m == null)
					{
						continue;
					}
					m.Invoke(mb, args);
					clicked = true;
					break;
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[Sonder] Mouse Clicker failed on " + typeName + ": " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message));
			}
		}
		return clicked;
	}

	private MethodInfo FindMethod(Type type, string name, Type[] args)
	{
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
		for (Type t = type; t != null && t != typeof(MonoBehaviour); t = t.BaseType)
		{
			MethodInfo m = t.GetMethod(name, flags, null, args, null);
			if (m != null)
			{
				return m;
			}
		}
		return null;
	}
}