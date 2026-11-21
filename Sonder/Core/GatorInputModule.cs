using Sonder.Managers;
using Sonder.Menu;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Sonder.Core;

public class GatorInputModule : BaseInputModule
{
	private const float RayLength = 5f;

	private const float HitRadius = 0.012f;

	private const int MenuLayerMask = 4;

	private PointerEventData _pointerData;

	private MenuButton _hovered;

	private bool _triggerHeld;

	private int _lastProcessedFrame = -1;

	public override void Process()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		if (Time.frameCount == _lastProcessedFrame)
		{
			return;
		}
		_lastProcessedFrame = Time.frameCount;
		if (!MenuManager.IsOpen || (Object)(object)MenuManager.RightHandPointer == (Object)null || (Object)(object)MenuManager.PointerLine == (Object)null)
		{
			ClearHover();
			return;
		}
		if (_pointerData == null)
		{
			_pointerData = new PointerEventData(eventSystem);
		}
		Transform rightHandPointer = MenuManager.RightHandPointer;
		Vector3 position = rightHandPointer.position;
		Vector3 forward = rightHandPointer.forward;
		Vector3 fallback = position + forward * 5f;
		MenuButton menuButton = null;
		RaycastHit val = default(RaycastHit);
		if (Physics.SphereCast(position, 0.012f, forward, out val, 5f, 4))
		{
			menuButton = ((Component)val.collider).GetComponentInParent<MenuButton>();
			fallback = val.point;
		}
		else
		{
			fallback = ClampToMenuPlane(position, forward, fallback);
		}
		MenuManager.PointerLine.SetPosition(0, position);
		MenuManager.PointerLine.SetPosition(1, fallback);
		if ((Object)(object)MenuManager.PointerCursor != (Object)null)
		{
			MenuManager.PointerCursor.position = fallback;
			MenuManager.PointerCursor.localScale = Vector3.one * (((Object)(object)menuButton != (Object)null) ? 1.6f : 1f);
		}
		bool flag = (Object)(object)ControllerInputPoller.instance != (Object)null && ((ControllerInputPoller)ControllerInputPoller.instance).rightControllerTriggerButton;
		ControllerInputPoller instance = ControllerInputPoller.instance;
		MenuButton.FavoriteModifier = (Keyboard.current != null && ((ButtonControl)Keyboard.current.fKey).isPressed) || ((Object)(object)instance != (Object)null && instance.leftControllerPrimaryButton);
		DispatchHover(menuButton, flag && !_triggerHeld);
		_triggerHeld = flag;
	}

	private void DispatchHover(MenuButton hitButton, bool clicked)
	{
		if ((Object)(object)hitButton != (Object)(object)_hovered)
		{
			if ((Object)(object)_hovered != (Object)null)
			{
				ExecuteEvents.Execute<IPointerExitHandler>(((Component)_hovered).gameObject, (BaseEventData)(object)_pointerData, ExecuteEvents.pointerExitHandler);
			}
			_hovered = hitButton;
			if ((Object)(object)_hovered != (Object)null)
			{
				ExecuteEvents.Execute<IPointerEnterHandler>(((Component)_hovered).gameObject, (BaseEventData)(object)_pointerData, ExecuteEvents.pointerEnterHandler);
			}
		}
		if (clicked && (Object)(object)_hovered != (Object)null)
		{
			ExecuteEvents.Execute<IPointerClickHandler>(((Component)_hovered).gameObject, (BaseEventData)(object)_pointerData, ExecuteEvents.pointerClickHandler);
		}
	}

	public void ClearHover()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		if ((Object)(object)_hovered != (Object)null)
		{
			if (_pointerData == null)
			{
				_pointerData = new PointerEventData(eventSystem);
			}
			ExecuteEvents.Execute<IPointerExitHandler>(((Component)_hovered).gameObject, (BaseEventData)(object)_pointerData, ExecuteEvents.pointerExitHandler);
			_hovered = null;
		}
		_triggerHeld = false;
	}

	private static Vector3 ClampToMenuPlane(Vector3 origin, Vector3 direction, Vector3 fallback)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		Transform menuCanvas = MenuManager.MenuCanvas;
		if ((Object)(object)menuCanvas == (Object)null)
		{
			return fallback;
		}
		Plane val = default(Plane);
		val = new Plane(menuCanvas.forward, menuCanvas.position);
		Ray val2 = default(Ray);
		val2 = new Ray(origin, direction);
		float num = default(float);
		if (!val.Raycast(val2, out num) || num <= 0f || num > 5f)
		{
			return fallback;
		}
		Vector3 point = val2.GetPoint(num);
		Vector3 val3 = menuCanvas.InverseTransformPoint(point);
		Rect rect = ((Component)menuCanvas).GetComponent<RectTransform>().rect;
		if (val3.x < rect.xMin || val3.x > rect.xMax || val3.y < rect.yMin || val3.y > rect.yMax)
		{
			return fallback;
		}
		return point;
	}
}
