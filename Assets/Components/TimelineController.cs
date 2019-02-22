﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Math = System.Math;

namespace FateUnity.Components {
	public sealed class Timeline {
		private Queue<TimelineAction> _actions;
		private TimelineAction _performingAction = null;

		public Timeline() {
			_actions = new Queue<TimelineAction>();
		}

		public void PushAction(TimelineAction action) {
			_actions.Enqueue(action);
		}

		public void Abort() {
			_performingAction = null;
			_actions.Clear();
		}

		public void Skip() {
			_performingAction.Final();
			_performingAction = null;
			while (_actions.Count > 0) {
				var action = _actions.Dequeue();
				action.Final();
			}
		}

		public void Update(float deltaTime) {
			if (_actions.Count > 0 && _performingAction == null) {
				_performingAction = _actions.Dequeue();
			}
			if (_performingAction != null) {
				_performingAction.Action(deltaTime);
				if (_performingAction.IsActionCompleted) {
					_performingAction.Final();
					_performingAction = null;
				}
			}
		}
	}

	public enum TimelineEaseType {
		In = 1, Out = 2, InOut = 3,

		Linear = 4, 
		Quad = 8, QuadIn = 9, QuadOut = 10, QuadInOut = 11, 
		Cubic = 12, CubicIn = 13, CubicOut = 14, CubicInOut = 15, 
		Quart = 16, QuartIn = 17, QuartOut = 18, QuartInOut = 19, 
		Quint = 20, QuintIn = 21, QuintOut = 22, QuintInOut = 23, 
		Sine = 24, SineIn = 25, SineOut = 26, SineInOut = 27, 
		Expo = 28, ExpoIn = 29, ExpoOut = 30, ExpoInOut = 31, 
		Circ = 32, CircIn = 33, CircOut = 34, CircInOut = 35, 
		Back = 36, BackIn = 37, BackOut = 38, BackInOut = 39, 
		Elastic = 40, ElasticIn = 41, ElasticOut = 42, ElasticInOut = 43, 
		Bounce = 44, BounceIn = 45, BounceOut = 46, BounceInOut = 47
	}
	
	public abstract class TimelineAction {
		
		private bool _actionCompleted = false;

		public bool IsActionCompleted { get { return _actionCompleted; } }

		protected void MarkComplete() {
			_actionCompleted = true;
		}

		public abstract void Action(float deltaTime);
		public abstract void Final();
	}

	public sealed class TimelineWaitAction : TimelineAction {
		private readonly float _waitTime;
		private float _time = 0.0f;

		public TimelineWaitAction(float time) {
			_waitTime = time;
		}

		public override void Action(float deltaTime) {
			_time += deltaTime;
			if (_time >= _waitTime) {
				MarkComplete();
			}
		}

		public override void Final() { }
	}

	public sealed class TimelineConcurrentActions : TimelineAction {
		private readonly LinkedList<TimelineAction> _concurrentActions;

		public TimelineConcurrentActions() {
			_concurrentActions = new LinkedList<TimelineAction>();
		}

		public void ClearActions() {
			_concurrentActions.Clear();
		}

		public void AddAction(TimelineAction action) {
			_concurrentActions.AddLast(action);
		}

		public override void Action(float deltaTime) {
			var node = _concurrentActions.First;
			while (node != null) {
				var nextNode = node.Next;
				var action = node.Value;
				action.Action(deltaTime);
				if (action.IsActionCompleted) {
					action.Final();
					_concurrentActions.Remove(node);
				}
				node = nextNode;
			}
			if (_concurrentActions.Count == 0) MarkComplete();
		}

		public override void Final() { }
	}
	
	public sealed class TimelineRepeatActions : TimelineAction {
		private readonly Queue<TimelineAction> _repeatActions;
		private readonly int _repeatCount;
		private TimelineAction _performingAction = null;
		private TimelineAction _firstAction = null;
		private int _repeatNumber = 0;

		public TimelineRepeatActions(int repeatCount = -1) {
			_repeatActions = new Queue<TimelineAction>();
			_repeatCount = repeatCount;
		}

		public void ClearActions() {
			_repeatActions.Clear();
		}

		public void StopRepeat() {
			MarkComplete();
		}

		public void PushAction(TimelineAction action) {
			_repeatActions.Enqueue(action);
			if (_firstAction == null) _firstAction = action;
		}

		public override void Action(float deltaTime) {
			if (_repeatActions.Count <= 0) {
				MarkComplete();
			} else {
				if (_performingAction == null) {
					_performingAction = _repeatActions.Dequeue();
					_repeatActions.Enqueue(_performingAction);
					if (_firstAction == _performingAction) {
						++_repeatNumber;
						if (_repeatNumber > _repeatCount) {
							MarkComplete();
							return;
						}
					}
				}
				if (_performingAction != null) {
					_performingAction.Action(deltaTime);
					if (_performingAction.IsActionCompleted) {
						_performingAction.Final();
						_performingAction = null;
					}
				}
			}
		}

		public override void Final() { }
	}
	
	public sealed class TimelineCallbackAction : TimelineAction {
		private readonly Action _callback;

		public TimelineCallbackAction(Action callback) {
			_callback = callback;
		}

		public override void Action(float deltaTime) {
			MarkComplete();
		}

		public override void Final() {
			_callback();
		}
	}

	public abstract class TimelineEaseAction : TimelineAction {
		private const float Pi = 3.14159265359f, HalfPi = 1.57079632679f, TwoPi = 6.28318530718f;

		private static float Step(TimelineEaseType esType, float t) {
			switch (esType) {
				case TimelineEaseType.Quad:
				case TimelineEaseType.QuadIn:       return t * t;
				case TimelineEaseType.QuadOut:      return t * (2 - t);
				case TimelineEaseType.QuadInOut:    return (t *= 2) < 1 ? .5f * t * t : .5f * (1 - (t - 1) * (t - 3));
				
				case TimelineEaseType.Cubic:
				case TimelineEaseType.CubicIn:      return t * t * t;
				case TimelineEaseType.CubicOut:     return ((t -= 1) * t * t + 1);
				case TimelineEaseType.CubicInOut:   return (t *= 2) < 1 ? .5f * t * t * t : .5f * ((t -= 2) * t * t + 2);
				
				case TimelineEaseType.Quart:
				case TimelineEaseType.QuartIn:      return t * t * t * t;
				case TimelineEaseType.QuartOut:     return 1 - (t -= 1) * t * t * t;
				case TimelineEaseType.QuartInOut:   return (t *= 2) < 1 ? .5f * t * t * t * t : .5f * (2 - (t -= 2) * t * t * t);
				
				case TimelineEaseType.Quint:
				case TimelineEaseType.QuintIn:      return t * t * t * t * t;
				case TimelineEaseType.QuintOut:     return ((t -= 1) * t * t * t * t + 1);
				case TimelineEaseType.QuintInOut:   return (t *= 2) < 1 ? .5f * t * t * t * t * t : .5f * ((t -= 2) * t * t * t * t + 2);
				
				case TimelineEaseType.Sine:
				case TimelineEaseType.SineIn:       return 1 - (float)Math.Cos(t * HalfPi);
				case TimelineEaseType.SineOut:      return (float)Math.Sin(t * HalfPi);
				case TimelineEaseType.SineInOut:    return .5f * (1 - (float)Math.Cos(t * Pi));
				
				case TimelineEaseType.Expo:
				case TimelineEaseType.ExpoIn:       return (float)Math.Exp(7 * (t - 1));
				case TimelineEaseType.ExpoOut:      return 1 - (float)Math.Exp(-7 * t);
				case TimelineEaseType.ExpoInOut:    return (t *= 2) < 1 ? .5f * (float)Math.Exp(7 * (t - 1)) : .5f * (2 - (float)Math.Exp(-7 * (t - 1)));
				
				case TimelineEaseType.Circ:
				case TimelineEaseType.CircIn:       return 1 - (float)Math.Sqrt(1 - t * t);
				case TimelineEaseType.CircOut:      return (float)Math.Sqrt(1 - (t -= 1) * t);
				case TimelineEaseType.CircInOut:    return (t *= 2) < 1 ? .5f * (1 - (float)Math.Sqrt(1 - t * t)) : .5f * ((float)Math.Sqrt(1 - (t -= 2) * t) + 1);
				
				case TimelineEaseType.Back:
				case TimelineEaseType.BackIn:       return t * t * (2.70158f * t - 1.70158f);
				case TimelineEaseType.BackOut:      return (t -= 1) * t * (2.70158f * t + 1.70158f) + 1;
				case TimelineEaseType.BackInOut:    return (t *= 2) < 1 ? .5f * (t * t * (3.5949095f * t - 2.5949095f)) : .5f * ((t -= 2) * t * (3.5949095f * t + 2.5949095f) + 2);
				
				case TimelineEaseType.Elastic:
				case TimelineEaseType.ElasticIn:    return (float)( -Math.Exp(7 * (t -= 1)) * Math.Sin((t - 0.075) * 20.9439510239) );
				case TimelineEaseType.ElasticOut:   return (float)( Math.Exp(-7 * t) * Math.Sin((t - 0.075) * 20.9439510239) + 1 );
				case TimelineEaseType.ElasticInOut: return (t *= 2) < 1 ? (float)(-.5 * Math.Exp(7 * (t -= 1)) * Math.Sin((t - 0.1125) * 13.962634016)) : (float)(Math.Exp(-7 * (t -= 1)) * Math.Sin((t - 0.1125) * 13.962634016) * .5 + 1);
				
				case TimelineEaseType.Bounce:
				case TimelineEaseType.BounceIn:     return 1 - Step(TimelineEaseType.BounceOut, 1 - t);
				case TimelineEaseType.BounceOut:    return t < 0.363636363636f ? 7.5625f * t * t : t < 0.727272727273f ? 7.5625f * (t -= 0.545454545455f) * t + .75f : t < 0.909090909091f ? 7.5625f * (t -= 0.818181818182f) * t + .9375f : 7.5625f * (t -= 0.954545454545f) * t + .984375f;
				case TimelineEaseType.BounceInOut:  return (t *= 2) < 1 ? .5f * (1 - Step(TimelineEaseType.BounceOut, 1 - t)) : .5f * (Step(TimelineEaseType.BounceOut, t - 1) + 1);
			}

			return t;
		}

		private readonly TimelineEaseType _easeType;
		private readonly float _duration;
		private float _time;
		private float _step;
		
		public TimelineEaseAction(TimelineEaseType easeType, float duration) {
			_easeType = easeType;
			_duration = duration;
		}

		public sealed override void Action(float deltaTime) {
			_time += deltaTime;
			if (_time >= _duration) {
				MarkComplete();
			} else {
				float step = Step(_easeType, _time / _duration);
				Ease(step);
			}
		}

		public abstract void Ease(float step);
	}
}