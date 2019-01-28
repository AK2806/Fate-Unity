using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameService.ClientComponent;

[RequireComponent(typeof(RectTransform))]
public sealed class RectTransformController : MonoBehaviour {
	private sealed class UIEasePosition : TimelineEaseAction {
		private readonly RectTransform _ref;
		private readonly Vector2 _from;
		private readonly Vector2 _to;

		public UIEasePosition(RectTransform transform, Vector2 to, float duration, TimelineEaseType easeType) :
			base(easeType, duration) {
			_ref = transform;
			_from = new Vector2(transform.localPosition.x, transform.localPosition.y);
			_to = to;
		}

		public override void Ease(float step) {
			var pos = Vector2.Lerp(_from, _to, step);
			_ref.localPosition = new Vector3(pos.x, pos.y, _ref.localPosition.z);
		}

		public override void Final() {
			_ref.localPosition = new Vector3(_to.x, _to.y, _ref.localPosition.z);
		}
	}

	private sealed class UIEaseRotation : TimelineEaseAction {
		private RectTransform _ref;

		public UIEaseRotation(RectTransform transform, float duration, TimelineEaseType easeType) :
			base(easeType, duration)  {
			_ref = transform;
		}

		public override void Ease(float step) {

		}

		public override void Final() {
			
		}
	}

	private sealed class UIEaseScale : TimelineEaseAction {
		private RectTransform _ref;

		public UIEaseScale(RectTransform transform, float duration, TimelineEaseType easeType) :
			base(easeType, duration)  {
			_ref = transform;
		}

		public override void Ease(float step) {

		}

		public override void Final() {
			
		}
	}

	private sealed class UIEaseSize : TimelineEaseAction {
		private RectTransform _ref;

		public UIEaseSize(RectTransform transform, float duration, TimelineEaseType easeType) :
			base(easeType, duration)  {
			_ref = transform;
		}

		public override void Ease(float step) {

		}

		public override void Final() {
			
		}
	}

	private RectTransform _rectTransform;
	private Timeline _timeline;

	public RectTransformController() {
		_timeline = new Timeline();
	}

	public void EasePosition(Vector2 to, float duration, TimelineEaseType easeType = TimelineEaseType.Linear) {
		_timeline.AppendAction(new UIEasePosition(_rectTransform, to, duration, easeType));
	}

	private void Awake() {
		_rectTransform = GetComponent<RectTransform>();
	}

	private void Update() {
		_timeline.Update(Time.deltaTime);
	}

}
