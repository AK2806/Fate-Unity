using System.Collections.Generic;
using Debug = UnityEngine.Debug;
using Cam = UnityEngine.Camera;
using OnTouchDelegate = System.Action<int, float, float>;

namespace Futilef {
	public unsafe class GpController {
		public static class ImgAttr { public const int Interactable = 0, Position = 1, Rotation = 2, Scale = 3, Alpha = 4, Tint = 5, ImgId = 6, End = 7; }
		public static class CamAttr { public const int Position = 0, Zoom = 1; }

		#region CMD
		class Cmd { }

		class StartRepeatCmd : Cmd { public int id; public Queue<Cmd> cmdQueue; }
		class StopRepeatCmd : Cmd { public int id; }

		class WaitCmd : Cmd { public float time; }

		class ImgCmd : Cmd { public int id; }
		class AddImgCmd : ImgCmd { public int imgId; }
		class RmImgCmd : ImgCmd { }
		class SetImgAttrCmd : ImgCmd { public int imgAttrId; public object[] args; }
		class SetImgAttrEasedCmd : SetImgAttrCmd { public float duration; public int esType; }

		class SetCamAttrCmd : Cmd { public int camAttrId; public object[] args; }
		class SetCamAttrEasedCmd : SetCamAttrCmd { public float duration; public int esType; }
		#endregion

		#region ESJOB
		class EsJob { public float time, duration; public int esType; public int repeatID; public virtual void Apply(float step) {} public virtual void Finish() {} }

		class EsImgJob            : EsJob    { public TpSprite *node; }
		class EsSetImgPositionJob : EsImgJob { public float x, y, z, dx, dy, dz; public override void Apply(float step) { TpSprite.SetPosition(node, x + dx * step, y + dy * step, z + dz * step); } public override void Finish() { TpSprite.SetPosition(node, x + dx, y + dy, z + dz); } }
		class EsSetImgRotationJob : EsImgJob { public float r, dr;               public override void Apply(float step) { TpSprite.SetRotation(node, r + dr * step); }                               public override void Finish() { TpSprite.SetRotation(node, r + dr); } }
		class EsSetImgScaleJob    : EsImgJob { public float x, dx, y, dy;        public override void Apply(float step) { TpSprite.SetScale(node, x + dx * step, y + dy * step); }                   public override void Finish() { TpSprite.SetScale(node, x + dx, y + dy); } }
		class EsSetImgAlphaJob    : EsImgJob { public float a, da;               public override void Apply(float step) { TpSprite.SetAlpha(node, a + da * step); }                                  public override void Finish() { TpSprite.SetAlpha(node, a + da); } }
		class EsSetImgTintJob     : EsImgJob { public float r, g, b, dr, dg, db; public override void Apply(float step) { TpSprite.SetTint(node, r + dr * step, g + dg * step, b + db * step); }     public override void Finish() { TpSprite.SetTint(node, r + dr, g + dg, b + db); } }

		class EsCamJob            : EsJob    { public Cam cam; }
		class EsSetCamPositionJob : EsCamJob { public float x, y, dx, dy; public override void Apply(float step) { cam.transform.position = new UnityEngine.Vector3(x + dx * step, y + dy * step, -10); } public override void Finish() { cam.transform.position = new UnityEngine.Vector3(x + dx, y + dy, -10); } }
		class EsSetCamZoomJob     : EsCamJob { public float s, ds;        public override void Apply(float step) { cam.orthographicSize = s + ds * step; }                                                public override void Finish() { cam.orthographicSize = s + ds; } }
		#endregion

		readonly Queue<Cmd> cmdQueue = new Queue<Cmd>();
		readonly LinkedList<EsJob> esJobList = new LinkedList<EsJob>();

		float time;
		float waitEndTime = -1, lastEsEndTime;

		readonly Dictionary<int, Queue<Cmd>> repeatDict = new Dictionary<int, Queue<Cmd>>();
		Queue<Cmd> repeatCmdQueue = null;
		readonly Dictionary<int, float> repeatWaitEndTimeDict = new Dictionary<int, float>();
		readonly Dictionary<int, float> repeatLastEsEndTimeDict = new Dictionary<int, float>();

		readonly Dictionary<int, OnTouchDelegate> nodeTouchHandlerDict = new Dictionary<int, OnTouchDelegate>();

		PtrIntDict *nodeDict = PtrIntDict.New();
		Pool *spritePool = Pool.New();
		PtrLst *spritePtrLst = PtrLst.New();

		bool needDepthSort;

		readonly Cam cam;

		public GpController(Cam cam) {
			this.cam = cam;
		}

		public void Dispose() {
			cmdQueue.Clear();
			esJobList.Clear();
			PtrIntDict.Decon(nodeDict); Mem.Free(nodeDict); nodeDict = null;
			Pool.Decon(spritePool); Mem.Free(spritePool); spritePool = null;
			PtrLst.Decon(spritePtrLst); Mem.Free(spritePtrLst); spritePtrLst = null;
			DrawCtx.Dispose();

			Debug.Log("Clean up GPC");
		}

		public void Update(float deltaTime) {
			time += deltaTime;

			// execute commands
			while (time >= waitEndTime && cmdQueue.Count > 0) {
				var cmd = cmdQueue.Dequeue();
				
				switch (cmd.GetType().Name) {
					case "StartRepeatCmd":     StartRepeat(cmd as StartRepeatCmd); break;
					case "StopRepeatCmd":      StopRepeat(cmd as StopRepeatCmd, deltaTime); break;
					case "WaitCmd":            Wait(cmd as WaitCmd); break;
					case "AddImgCmd":          AddImg(cmd as AddImgCmd); break;
					case "RmImgCmd":           RmImg(cmd as RmImgCmd); break;
					case "SetImgAttrCmd":      SetImgAttr(cmd as SetImgAttrCmd); break;
					case "SetImgAttrEasedCmd": SetImgAttrEased(cmd as SetImgAttrEasedCmd); break;
					case "SetCamAttrCmd":      SetCamAttr(cmd as SetCamAttrCmd); break;
					case "SetCamAttrEasedCmd": SetCamAttrEased(cmd as SetCamAttrEasedCmd); break;
				}
			}

			foreach (var kv in repeatDict) {
				int id = kv.Key;
				var queue = kv.Value;
				var popCmds = new Queue<Cmd>();
				while (time >= repeatWaitEndTimeDict[id] && queue.Count > 0) {
					var cmd = queue.Dequeue();
					bool discard = false;
					switch (cmd.GetType().Name) {
						case "WaitCmd":            Repeat_Wait(id, cmd as WaitCmd); break;
						case "SetCamAttrCmd":      SetCamAttr(cmd as SetCamAttrCmd); break;
						case "SetCamAttrEasedCmd": Repeat_SetCamAttrEased(id, cmd as SetCamAttrEasedCmd); break;
						case "SetImgAttrCmd": {
							var command = cmd as SetImgAttrCmd;
							if (PtrIntDict.Contains(nodeDict, command.id)) SetImgAttr(command);
							else discard = true;
							break;
						}
						case "SetImgAttrEasedCmd": {
							var command = cmd as SetImgAttrEasedCmd;
							if (PtrIntDict.Contains(nodeDict, command.id)) Repeat_SetImgAttrEased(id, command);
							else discard = true;
							break;
						}
					}
					if (!discard) popCmds.Enqueue(cmd);
				}
				while (popCmds.Count > 0) queue.Enqueue(popCmds.Dequeue());
			}

			// execute easing jobs
			for (var node = esJobList.First; node != null;) {
				var next = node.Next;
				var job = node.Value;
				if ((job.time += deltaTime) > job.duration) {
					job.Finish();
					esJobList.Remove(node);
				} else {
					job.Apply(Es.Ease(job.esType, job.time / job.duration));
				}
				node = next;
			}

			// sort
			if (needDepthSort) { 
				needDepthSort = false; 
				Algo.MergeSort(spritePtrLst->arr, spritePtrLst->count, TpSprite.DepthCmp);
			}

			var arr = (TpSprite **)spritePtrLst->arr;

			// mouse (0 for left button, 1 for right button, 2 for the middle button)
			for (int m = 0; m <= 2; m += 1) {
				int phase = TchPhase.FromUnityMouse(m);
				if (phase == TchPhase.None) continue;
				var pos = cam.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
				float x = pos.x, y = pos.y;
				for (int i = spritePtrLst->count - 1; i >= 0; i -= 1) {
					int id = arr[i]->id;
					if (nodeTouchHandlerDict.ContainsKey(id) && TpSprite.Raycast(arr[i], x, y)) {  // has a handler && pos in sprite
						nodeTouchHandlerDict[id].Invoke(TchPhase.FromUnityMouse(m), x, y);
						break;
					}
				}
			}

			// touch
			var touches = UnityEngine.Input.touches;
			foreach (var touch in touches) {
				var pos = cam.ScreenToWorldPoint(touch.position);
				float x = pos.x, y = pos.y;
				for (int i = spritePtrLst->count - 1; i >= 0; i -= 1) {
					int id = arr[i]->id;
					if (nodeTouchHandlerDict.ContainsKey(id) && TpSprite.Raycast(arr[i], x, y)) {  // has a handler && pos in sprite
						nodeTouchHandlerDict[id].Invoke(TchPhase.FromUnityTouch(touch.phase), x, y);
						break;
					}
				}
			}

			// draw
			DrawCtx.Start();
			for (int i = 0, end = spritePtrLst->count; i < end; i += 1) {
				Node.Draw(arr[i], null, false);
			}
			DrawCtx.Finish();
		}

		public void Skip() {
			for (var node = esJobList.First; node != null;) {
				var next = node.Next;
				var job = node.Value;
				if (job.repeatID == -1) {
					job.Finish();
					esJobList.Remove(node);
				}
				node = next;
			}
			lastEsEndTime = waitEndTime = time;
			while (cmdQueue.Count > 0) {
				var cmd = cmdQueue.Dequeue();
				switch (cmd.GetType().Name) {
					case "StartRepeatCmd":     StartRepeat(cmd as StartRepeatCmd); break;
					case "StopRepeatCmd":      StopRepeat(cmd as StopRepeatCmd); break;
					case "AddImgCmd":          AddImg(cmd as AddImgCmd); break;
					case "RmImgCmd":           RmImg(cmd as RmImgCmd); break;
					case "SetImgAttrCmd":      SetImgAttr(cmd as SetImgAttrCmd); break;
					case "SetImgAttrEasedCmd": SetImgAttr(cmd as SetImgAttrCmd); break;
					case "SetCamAttrCmd":      SetCamAttr(cmd as SetCamAttrCmd); break;
					case "SetCamAttrEasedCmd": SetCamAttr(cmd as SetCamAttrCmd); break;
				}
			}
		}

		public void RepeatDeclareBegin() {
			repeatCmdQueue = new Queue<Cmd>();
		}

		public void RepeatDeclareEnd(int id) {
			cmdQueue.Enqueue(new StartRepeatCmd{ id = id, cmdQueue = repeatCmdQueue });
			repeatCmdQueue = null;
		}

		public void StopRepeat(int id) {
			cmdQueue.Enqueue(new StopRepeatCmd{ id = id });
		}

		void StartRepeat(StartRepeatCmd cmd) {
			repeatDict.Add(cmd.id, cmd.cmdQueue);
			repeatWaitEndTimeDict.Add(cmd.id, 0.0f);
			repeatLastEsEndTimeDict.Add(cmd.id, 0.0f);
		}

		void StopRepeat(StopRepeatCmd cmd, float deltaTime = -1.0f) {
			if (cmd.id != -1) {
				repeatDict.Remove(cmd.id);
				for (var node = esJobList.First; node != null;) {
					var next = node.Next;
					var job = node.Value;
					if (job.repeatID == cmd.id) {
						if (deltaTime >= 0) job.Apply(Es.Ease(job.esType, (job.time + deltaTime) / job.duration));
						esJobList.Remove(node);
					}
					node = next;
				}
			} else {
				repeatDict.Clear();
				for (var node = esJobList.First; node != null;) {
					var next = node.Next;
					var job = node.Value;
					if (job.repeatID != -1) {
						if (deltaTime >= 0) job.Apply(Es.Ease(job.esType, (job.time + deltaTime) / job.duration));
						esJobList.Remove(node);
					}
					node = next;
				}
			}
		}

		public void Wait(float time = -1) {
			if (repeatCmdQueue != null) {
				repeatCmdQueue.Enqueue(new WaitCmd{ time = time });
			} else {
				cmdQueue.Enqueue(new WaitCmd{ time = time });
			}
		}

		void Repeat_Wait(int id, WaitCmd cmd) {
			if (cmd.time < 0) {  // wait for all animation to finish
				if (repeatLastEsEndTimeDict[id] > repeatWaitEndTimeDict[id]) repeatWaitEndTimeDict[id] = repeatLastEsEndTimeDict[id];
			} else {
				float endTime = time + cmd.time;
				if (endTime > repeatWaitEndTimeDict[id]) repeatWaitEndTimeDict[id] = endTime;
			}
		}

		void Wait(WaitCmd cmd) {
			if (cmd.time < 0) {  // wait for all animation to finish
				if (lastEsEndTime > waitEndTime) waitEndTime = lastEsEndTime;
			} else {
				float endTime = time + cmd.time;
				if (endTime > waitEndTime) waitEndTime = endTime;
			}
		}

		public void AddImg(int id, int imgId) {
			cmdQueue.Enqueue(new AddImgCmd{ id = id, imgId = imgId });
		}
		void AddImg(AddImgCmd cmd) {
			#if FDB
			Should.False("nodeIdxDict.ContainsKey(cmd.id)", PtrIntDict.Contains(nodeDict, cmd.id));
			Should.True("Res.HasSpriteMeta(cmd.imgId)", Res.HasSpriteMeta(cmd.imgId));
			#endif
			var node = (TpSprite *)Pool.Alloc(spritePool, sizeof(TpSprite));
			TpSprite.Init(node, Res.GetSpriteMeta(cmd.imgId));
			node->id = cmd.id;

			if (spritePool->shift != 0) {
				PtrLst.ShiftBase(spritePtrLst, spritePool->shift);
				PtrIntDict.ShiftBase(nodeDict, spritePool->shift);
				foreach (var esJob in esJobList) {
					if (esJob is EsImgJob) {
						var esTpSpriteJob = (EsImgJob)esJob;
						esTpSpriteJob.node = (TpSprite *)((byte *)esTpSpriteJob.node + spritePool->shift);
					}
				}
				needDepthSort = true;
				spritePool->shift = 0;
			}
			PtrLst.Push(spritePtrLst, node);
			PtrIntDict.Set(nodeDict, cmd.id, node);
		}

		public void RmImg(int id) {
			cmdQueue.Enqueue(new RmImgCmd{ id = id });
		}
		void RmImg(RmImgCmd cmd) {
			#if FDB
			if (cmd.id >= 0) Should.True("nodeIdxDict.ContainsKey(cmd.id)", PtrIntDict.Contains(nodeDict, cmd.id));
			#endif
			if (cmd.id < 0) {
				nodeTouchHandlerDict.Clear();
				esJobList.Clear();
				PtrIntDict.Clear(nodeDict);
				PtrLst.Clear(spritePtrLst);
				Pool.Clear(spritePool);
			} else {
				if (nodeTouchHandlerDict.ContainsKey(cmd.id)) nodeTouchHandlerDict.Remove(cmd.id);
				void *node = PtrIntDict.Remove(nodeDict, cmd.id);
				for (var lsNode = esJobList.First; lsNode != null;) {
					var next = lsNode.Next;
					var esJob = lsNode.Value;
					if (esJob is EsImgJob) {
						var esTpSpriteJob = (EsImgJob)esJob;
						if (esTpSpriteJob.node == node) esJobList.Remove(lsNode);
					}
					lsNode = next;
				}
				PtrLst.Remove(spritePtrLst, node);
				Pool.Free(spritePool, node);
			}
		}

		public void SetImgInteractable(int id, OnTouchDelegate onTouch) {
			cmdQueue.Enqueue(new SetImgAttrCmd{ id = id, imgAttrId = ImgAttr.Interactable, args = new object[] { onTouch } });
		}
		void SetImgInteractable(SetImgAttrCmd cmd) {
			int id = cmd.id;
			var onTouch = (OnTouchDelegate)cmd.args[0];

			if (onTouch == null) {
				if (nodeTouchHandlerDict.ContainsKey(id)) nodeTouchHandlerDict.Remove(id);
			} else nodeTouchHandlerDict[id] = onTouch;
		}

		public void SetImgId(int id, int imgId) {
			if (repeatCmdQueue != null) {
				repeatCmdQueue.Enqueue(new SetImgAttrCmd{ id = id, imgAttrId = ImgAttr.ImgId, args = new object[] { imgId } });
			} else {
				cmdQueue.Enqueue(new SetImgAttrCmd{ id = id, imgAttrId = ImgAttr.ImgId, args = new object[] { imgId } });
			}
		}
		public void SetImgAttr(int id, int imgAttrId, params object[] args) {
			if (repeatCmdQueue != null) {
				repeatCmdQueue.Enqueue(new SetImgAttrCmd{ id = id, imgAttrId = imgAttrId, args = args });
			} else {
				cmdQueue.Enqueue(new SetImgAttrCmd{ id = id, imgAttrId = imgAttrId, args = args });
			}
		}
		void SetImgAttr(SetImgAttrCmd cmd) {
			#if FDB
			Should.True("nodeIdxDict.ContainsKey(cmd.id)", PtrIntDict.Contains(nodeDict, cmd.id));
			Should.InRange("cmd.imgAttrId", cmd.imgAttrId, 0, ImgAttr.End - 1);
			#endif
			var img = (TpSprite *)PtrIntDict.Get(nodeDict, cmd.id);
			var args = cmd.args;
			switch (cmd.imgAttrId) {
			case ImgAttr.Interactable: SetImgInteractable(cmd); break;
			case ImgAttr.Position:     TpSprite.SetPosition(img, (float)args[0], (float)args[1], (float)args[2]); needDepthSort = true; break;
			case ImgAttr.Rotation:     TpSprite.SetRotation(img, (float)args[0]); break;
			case ImgAttr.Scale:        TpSprite.SetScale(img, (float)args[0], (float)args[1]); break;
			case ImgAttr.Alpha:        TpSprite.SetAlpha(img, (float)args[0]); break;
			case ImgAttr.Tint:         TpSprite.SetTint(img, (float)args[0], (float)args[1], (float)args[2]); break;
			case ImgAttr.ImgId:        TpSprite.SetMeta(img, Res.GetSpriteMeta((int)args[0])); break;
			}
		}

		public void SetImgAttrEased(int id, int imgAttrId, float duration, int esType, params object[] args) {
			if (repeatCmdQueue != null) {
				repeatCmdQueue.Enqueue(new SetImgAttrEasedCmd{ id = id, imgAttrId = imgAttrId, duration = duration, esType = esType, args = args });
			} else {
				cmdQueue.Enqueue(new SetImgAttrEasedCmd{ id = id, imgAttrId = imgAttrId, duration = duration, esType = esType, args = args });
			}
		}

		void Repeat_SetImgAttrEased(int id, SetImgAttrEasedCmd cmd) {
			#if FDB
			Should.True("nodeIdxDict.ContainsKey(cmd.id)", PtrIntDict.Contains(nodeDict, cmd.id));
			Should.InRange("cmd.imgAttrId", cmd.imgAttrId, 0, ImgAttr.End - 1);
			Should.GreaterThan("cmd.duration", cmd.duration, 0);
			Should.InRange("cmd.esType", cmd.esType, 0, EsType.End - 1);
			#endif
			float endTime = time + cmd.duration;
			if (endTime > repeatLastEsEndTimeDict[id]) repeatLastEsEndTimeDict[id] = endTime;

			var img = (TpSprite *)PtrIntDict.Get(nodeDict, cmd.id);
			var args = cmd.args;
			switch (cmd.imgAttrId) {
			case ImgAttr.Position: esJobList.AddLast(new EsSetImgPositionJob{ node = img, duration = cmd.duration, esType = cmd.esType, repeatID = id, x = img->pos[0],   dx = (float)args[0] - img->pos[0], y = img->pos[1], dy = (float)args[1] - img->pos[1], z = img->pos[2], dz = 0 }); break;// (float)args[2] - img->pos[2] }); break;
			case ImgAttr.Rotation: esJobList.AddLast(new EsSetImgRotationJob{ node = img, duration = cmd.duration, esType = cmd.esType, repeatID = id, r = img->rot,      dr = (float)args[0] - img->rot }); break;
			case ImgAttr.Scale:    esJobList.AddLast(new EsSetImgScaleJob{    node = img, duration = cmd.duration, esType = cmd.esType, repeatID = id, x = img->scl[0],   dx = (float)args[0] - img->scl[0], y = img->scl[1], dy = (float)args[1] - img->scl[1] }); break;
			case ImgAttr.Alpha:    esJobList.AddLast(new EsSetImgAlphaJob{    node = img, duration = cmd.duration, esType = cmd.esType, repeatID = id, a = img->color[3], da = (float)args[0] - img->color[3] }); break;
			case ImgAttr.Tint:     esJobList.AddLast(new EsSetImgTintJob{     node = img, duration = cmd.duration, esType = cmd.esType, repeatID = id, r = img->color[0], dr = (float)args[0] - img->color[0], g = img->color[1], dg = (float)args[1] - img->color[1], b = img->color[2], db = (float)args[2] - img->color[2] }); break;
			}
		}

		void SetImgAttrEased(SetImgAttrEasedCmd cmd) {
			#if FDB
			Should.True("nodeIdxDict.ContainsKey(cmd.id)", PtrIntDict.Contains(nodeDict, cmd.id));
			Should.InRange("cmd.imgAttrId", cmd.imgAttrId, 0, ImgAttr.End - 1);
			Should.GreaterThan("cmd.duration", cmd.duration, 0);
			Should.InRange("cmd.esType", cmd.esType, 0, EsType.End - 1);
			#endif
			float endTime = time + cmd.duration;
			if (endTime > lastEsEndTime) lastEsEndTime = endTime;

			var img = (TpSprite *)PtrIntDict.Get(nodeDict, cmd.id);
			var args = cmd.args;
			switch (cmd.imgAttrId) {
			case ImgAttr.Position: esJobList.AddLast(new EsSetImgPositionJob{ node = img, duration = cmd.duration, esType = cmd.esType, repeatID = -1, x = img->pos[0],   dx = (float)args[0] - img->pos[0], y = img->pos[1], dy = (float)args[1] - img->pos[1], z = img->pos[2], dz = 0 }); break;// (float)args[2] - img->pos[2] }); break;
			case ImgAttr.Rotation: esJobList.AddLast(new EsSetImgRotationJob{ node = img, duration = cmd.duration, esType = cmd.esType, repeatID = -1, r = img->rot,      dr = (float)args[0] - img->rot }); break;
			case ImgAttr.Scale:    esJobList.AddLast(new EsSetImgScaleJob{    node = img, duration = cmd.duration, esType = cmd.esType, repeatID = -1, x = img->scl[0],   dx = (float)args[0] - img->scl[0], y = img->scl[1], dy = (float)args[1] - img->scl[1] }); break;
			case ImgAttr.Alpha:    esJobList.AddLast(new EsSetImgAlphaJob{    node = img, duration = cmd.duration, esType = cmd.esType, repeatID = -1, a = img->color[3], da = (float)args[0] - img->color[3] }); break;
			case ImgAttr.Tint:     esJobList.AddLast(new EsSetImgTintJob{     node = img, duration = cmd.duration, esType = cmd.esType, repeatID = -1, r = img->color[0], dr = (float)args[0] - img->color[0], g = img->color[1], dg = (float)args[1] - img->color[1], b = img->color[2], db = (float)args[2] - img->color[2] }); break;
			}
		}

		public void SetCamAttr(int camAttrId, params object[] args) {
			if (repeatCmdQueue != null) {
				repeatCmdQueue.Enqueue(new SetCamAttrCmd{ camAttrId = camAttrId, args = args });
			} else {
				cmdQueue.Enqueue(new SetCamAttrCmd{ camAttrId = camAttrId, args = args });
			}
		}
		void SetCamAttr(SetCamAttrCmd cmd) {
			var args = cmd.args;
			switch (cmd.camAttrId) {
			case CamAttr.Position: cam.transform.position = new UnityEngine.Vector3((float)args[0], (float)args[1], -10); break;
			case CamAttr.Zoom:     cam.orthographicSize = (float)args[0]; break;
			}
		}

		public void SetCamAttrEased(int camAttrId, float duration, int esType, params object[] args) {
			if (repeatCmdQueue != null) {
				repeatCmdQueue.Enqueue(new SetCamAttrEasedCmd{ camAttrId = camAttrId, duration = duration, esType = esType, args = args });
			} else {
				cmdQueue.Enqueue(new SetCamAttrEasedCmd{ camAttrId = camAttrId, duration = duration, esType = esType, args = args });
			}
		}
		void Repeat_SetCamAttrEased(int id, SetCamAttrEasedCmd cmd) {
			float endTime = time + cmd.duration;
			if (endTime > repeatLastEsEndTimeDict[id]) repeatLastEsEndTimeDict[id] = endTime;

			var args = cmd.args;
			switch (cmd.camAttrId) {
			case CamAttr.Position: esJobList.AddLast(new EsSetCamPositionJob{ cam = cam, duration = cmd.duration, esType = cmd.esType, repeatID = id, x = cam.transform.position.x, y = cam.transform.position.y, dx = (float)args[0] - cam.transform.position.x, dy = (float)args[1] - cam.transform.position.y }); break;
			case CamAttr.Zoom:     esJobList.AddLast(new EsSetCamZoomJob{     cam = cam, duration = cmd.duration, esType = cmd.esType, repeatID = id, s = cam.orthographicSize, ds = (float)args[0] - cam.orthographicSize }); break;
			}
		}
		void SetCamAttrEased(SetCamAttrEasedCmd cmd) {
			float endTime = time + cmd.duration;
			if (endTime > lastEsEndTime) lastEsEndTime = endTime;

			var args = cmd.args;
			switch (cmd.camAttrId) {
			case CamAttr.Position: esJobList.AddLast(new EsSetCamPositionJob{ cam = cam, duration = cmd.duration, esType = cmd.esType, repeatID = -1, x = cam.transform.position.x, y = cam.transform.position.y, dx = (float)args[0] - cam.transform.position.x, dy = (float)args[1] - cam.transform.position.y }); break;
			case CamAttr.Zoom:     esJobList.AddLast(new EsSetCamZoomJob{     cam = cam, duration = cmd.duration, esType = cmd.esType, repeatID = -1, s = cam.orthographicSize, ds = (float)args[0] - cam.orthographicSize }); break;
			}
		}
	}
}