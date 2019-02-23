using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameService.ClientComponent;

namespace FateUnity.Components.UI {
	public sealed class UIAnimator : MonoBehaviour {
		private sealed class UIEasePosition : TimelineEaseAction {
			private RectTransform _ref;
			private Vector2 _from;
			private Vector2 _to;

			public UIEasePosition(RectTransform transform, Vector2 to, float duration, TimelineEaseType easeType) :
				base(easeType, duration) {
				_ref = transform;
				_to = to;
			}

			protected override TimelineEaseAction Copy(TimelineEaseType easeType, float duration) {
				return new UIEasePosition(_ref, _to, duration, easeType);
			}

			protected override void Ease(float step) {
				_ref.anchoredPosition = Vector2.Lerp(_from, _to, step);
			}

			protected override void Init() {
				_from = _ref.anchoredPosition;
			}

			protected override void Final() {
				_ref.anchoredPosition = _to;
			}
		}

		private sealed class UIEaseRotation : TimelineEaseAction {
			private RectTransform _ref;
			private float _from;
			private float _to;

			public UIEaseRotation(RectTransform transform, float toAngle, float duration, TimelineEaseType easeType) :
				base(easeType, duration)  {
				_ref = transform;
				_to = toAngle;
			}

			protected override TimelineEaseAction Copy(TimelineEaseType easeType, float duration) {
				return new UIEaseRotation(_ref, _to, duration, easeType);
			}

			protected override void Ease(float step) {
				var euler = _ref.eulerAngles;
				euler.z = Mathf.Lerp(_from, _to, step);
				_ref.eulerAngles = euler;
			}

			protected override void Init() {
				_from = _ref.eulerAngles.z;
			}

			protected override void Final() {
				var euler = _ref.eulerAngles;
				euler.z = _to;
				_ref.eulerAngles = euler;
			}
		}

		private sealed class UIEaseScale : TimelineEaseAction {
			private RectTransform _ref;
			private Vector2 _from;
			private Vector2 _to;

			public UIEaseScale(RectTransform transform, Vector2 toScale, float duration, TimelineEaseType easeType) :
				base(easeType, duration)  {
				_ref = transform;
				_to = toScale;
			}

			protected override TimelineEaseAction Copy(TimelineEaseType easeType, float duration) {
				return new UIEaseScale(_ref, _to, duration, easeType);
			}

			protected override void Ease(float step) {
				_ref.localScale = Vector2.Lerp(_from, _to, step);
			}

			protected override void Init() {
				_from = _ref.localScale;
			}

			protected override void Final() {
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
				_to = toSize;
			}

			protected override TimelineEaseAction Copy(TimelineEaseType easeType, float duration) {
				return new UIEaseSize(_ref, _to, duration, easeType);
			}

			protected override void Ease(float step) {
				_ref.sizeDelta = Vector2.Lerp(_from, _to, step);
			}

			protected override void Init() {
				_from = _ref.sizeDelta;
			}

			protected override void Final() {
				_ref.sizeDelta = _to;
			}
		}

		private sealed class UIGraphicEaseColor : TimelineEaseAction {
			private Graphic _ref;
			private Color _from;
			private Color _to;

			public UIGraphicEaseColor(Graphic graphic, Color to, float duration, TimelineEaseType easeType) :
				base(easeType, duration)  {
				_ref = graphic;
				_to = to;
			}

			protected override TimelineEaseAction Copy(TimelineEaseType easeType, float duration) {
				return new UIGraphicEaseColor(_ref, _to, duration, easeType);
			}

			protected override void Ease(float step) {
				_ref.color = Color.Lerp(_from, _to, step);
			}

			protected override void Init() {
				_from = _ref.color;
			}

			protected override void Final() {
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
			var graphicComp = GetComponent<Graphic>();
			if (graphicComp == null) return;
			var action = new UIGraphicEaseColor(graphicComp, color, duration, easeType);
			if (_concurrentActions != null) {
				_concurrentActions.AddAction(action);
			} else {
				_timeline.PushAction(action);
			}
		}

		public void Wait(float time) {
			var action = new TimelineWaitAction(time);
			if (_concurrentActions != null) {
				_concurrentActions.AddAction(action);
			} else {
				_timeline.PushAction(action);
			}
		}

		public void CallFunc(Action func) {
			var action = new TimelineCallbackAction(func);
			if (_concurrentActions != null) {
				_concurrentActions.AddAction(action);
			} else {
				_timeline.PushAction(action);
			}
		}

		public void ResetAsSequenceMode(bool abort = false) {
			_timeline.ResetAsSequenceMode(abort);
		}

		public void ResetAsRepeatMode(bool abort = false) {
			_timeline.ResetAsRepeatMode(abort);
		}

		private void Update() {
			_timeline.Update(Time.deltaTime);
		}
	}
}
