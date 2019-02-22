using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameService.ClientComponent;

namespace FateUnity.Components.UI {
	public sealed class UIAnimator : MonoBehaviour {
		private sealed class UIEasePosition : TimelineEaseAction {
			private readonly RectTransform _ref;
			private readonly Vector2 _from;
			private readonly Vector2 _to;

			public UIEasePosition(RectTransform transform, Vector2 to, float duration, TimelineEaseType easeType) :
				base(easeType, duration) {
				_ref = transform;
				_from = transform.anchoredPosition;
				_to = to;
			}

			public override void Ease(float step) {
				_ref.anchoredPosition = Vector2.Lerp(_from, _to, step);
			}

			public override void Final() {
				_ref.anchoredPosition = _to;
			}
		}

		private sealed class UIEaseRotation : TimelineEaseAction {
			private readonly RectTransform _ref;
			private readonly float _from;
			private readonly float _to;

			public UIEaseRotation(RectTransform transform, float toAngle, float duration, TimelineEaseType easeType) :
				base(easeType, duration)  {
				_ref = transform;
				_from = transform.eulerAngles.z;
				_to = toAngle;
			}

			public override void Ease(float step) {
				var euler = _ref.eulerAngles;
				euler.z = Mathf.Lerp(_from, _to, step);
				_ref.eulerAngles = euler;
			}

			public override void Final() {
				var euler = _ref.eulerAngles;
				euler.z = _to;
				_ref.eulerAngles = euler;
			}
		}

		private sealed class UIEaseScale : TimelineEaseAction {
			private readonly RectTransform _ref;
			private readonly Vector2 _from;
			private readonly Vector2 _to;

			public UIEaseScale(RectTransform transform, Vector2 toScale, float duration, TimelineEaseType easeType) :
				base(easeType, duration)  {
				_ref = transform;
				_from = transform.localScale;
				_to = toScale;
			}

			public override void Ease(float step) {
				_ref.localScale = Vector2.Lerp(_from, _to, step);
			}

			public override void Final() {
				_ref.localScale = _to;
			}
		}

		private sealed class UIEaseSize : TimelineEaseAction {
			private RectTransform _ref;
			private Vector2 _from;
			private Vector2 _to;

			public UIEaseSize(RectTransform transform, Vector2 toSize, float duration, TimelineEaseType easeType) :
				base(easeType, duration)  {
				_ref = transform;
				_from = transform.sizeDelta;
				_to = toSize;
			}

			public override void Ease(float step) {
				_ref.sizeDelta = Vector2.Lerp(_from, _to, step);
			}

			public override void Final() {
				_ref.sizeDelta = _to;
			}
		}

		private sealed class UIGraphicEaseColor : TimelineEaseAction {
			private readonly Graphic _ref;
			private readonly Color _from;
			private readonly Color _to;

			public UIGraphicEaseColor(Graphic graphic, Color to, float duration, TimelineEaseType easeType) :
				base(easeType, duration)  {
				_ref = graphic;
				_from = graphic.color;
				_to = to;
			}

			public override void Ease(float step) {
				_ref.color = Color.Lerp(_from, _to, step);
			}

			public override void Final() {
				_ref.color = _to;
			}
		}

		private Timeline _timeline;
		private TimelineConcurrentActions _concurrentActions = null;

		public UIAnimator() {
			_timeline = new Timeline();
		}

		public void AbortAllActions() {
			_timeline.Abort();
		}

		public void SkipAllActions() {
			_timeline.Skip();
		}

		public void DeclareConcurrentActionsBegin() {
			_concurrentActions = new TimelineConcurrentActions();
		}

		public void DeclareConcurrentActionsEnd() {
			_timeline.PushAction(_concurrentActions);
			_concurrentActions = null;
		}

		public void EasePosition(Vector2 to, float duration, TimelineEaseType easeType = TimelineEaseType.Linear) {
			var action = new UIEasePosition(GetComponent<RectTransform>(), to, duration, easeType);
			if (_concurrentActions != null) {
				_concurrentActions.AddAction(action);
			} else {
				_timeline.PushAction(action);
			}
		}

		public void EaseColor(Color color, float duration, TimelineEaseType easeType = TimelineEaseType.Linear) {
			var components = GetComponentsInChildren<Graphic>(true);
			var cActions = new TimelineConcurrentActions();
			foreach (var graphicComponent in components) {
				var action = new UIGraphicEaseColor(graphicComponent, color, duration, easeType);
				cActions.AddAction(action);
			}
			if (_concurrentActions != null) {
				_concurrentActions.AddAction(cActions);
			} else {
				_timeline.PushAction(cActions);
			}
		}

		public void CallFunc(Action func) {
			if (_concurrentActions != null) {
				_concurrentActions.AddAction(new TimelineCallbackAction(func));
			} else {
				_timeline.PushAction(new TimelineCallbackAction(func));
			}
		}

		private void Update() {
			_timeline.Update(Time.deltaTime);
		}
	}
}
