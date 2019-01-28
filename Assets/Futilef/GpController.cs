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
		class EsJob { public float time, duration; public int esType; public virtual void Apply(float step) {} public virtual void Finish() {} }

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
			if (time > waitEndTime) {
				while (time > waitEndTime && cmdQueue.Count > 0) {
					var cmd = cmdQueue.Dequeue();
					
					switch (cmd.GetType().Name) {
						case "WaitCmd":            Wait(cmd as WaitCmd); break;
						case "AddImgCmd":          AddImg(cmd as AddImgCmd); break;
						case "RmImgCmd":           RmImg(cmd as RmImgCmd); break;
						case "SetImgAttrCmd":      SetImgAttr(cmd as SetImgAttrCmd); break;
						case "SetImgAttrEasedCmd": SetImgAttrEased(cmd as SetImgAttrEasedCmd); break;
						case "SetCamAttrCmd":      SetCamAttr(cmd as SetCamAttrCmd); break;
						case "SetCamAttrEasedCmd": SetCamAttrEased(cmd as SetCamAttrEasedCmd); break;
					}
				}	
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

		public void Wait(float time = -1) {
			cmdQueue.Enqueue(new WaitCmd{ time = time });
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
				PtrIntDict.Clear(nodeDict);
				PtrLst.Clear(spritePtrLst);
				Pool.Clear(spritePool);
			} else {
				if (nodeTouchHandlerDict.ContainsKey(cmd.id)) nodeTouchHandlerDict.Remove(cmd.id);
				void *node = PtrIntDict.Remove(nodeDict, cmd.id);
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
			cmdQueue.Enqueue(new SetImgAttrCmd{ id = id, imgAttrId = ImgAttr.ImgId, args = new object[] { imgId } });
		}
		public void SetImgAttr(int id, int imgAttrId, params object[] args) {
			cmdQueue.Enqueue(new SetImgAttrCmd{ id = id, imgAttrId = imgAttrId, args = args });
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
			cmdQueue.Enqueue(new SetImgAttrEasedCmd{ id = id, imgAttrId = imgAttrId, duration = duration, esType = esType, args = args });
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
			case ImgAttr.Position: esJobList.AddLast(new EsSetImgPositionJob{ node = img, duration = cmd.duration, esType = cmd.esType, x = img->pos[0],   dx = (float)args[0] - img->pos[0], y = img->pos[1], dy = (float)args[1] - img->pos[1], z = img->pos[2], dz = 0 }); break;// (float)args[2] - img->pos[2] }); break;
			case ImgAttr.Rotation: esJobList.AddLast(new EsSetImgRotationJob{ node = img, duration = cmd.duration, esType = cmd.esType, r = img->rot,      dr = (float)args[0] - img->rot }); break;
			case ImgAttr.Scale:    esJobList.AddLast(new EsSetImgScaleJob{    node = img, duration = cmd.duration, esType = cmd.esType, x = img->scl[0],   dx = (float)args[0] - img->scl[0], y = img->scl[1], dy = (float)args[1] - img->scl[1] }); break;
			case ImgAttr.Alpha:    esJobList.AddLast(new EsSetImgAlphaJob{    node = img, duration = cmd.duration, esType = cmd.esType, a = img->color[3], da = (float)args[0] - img->color[3] }); break;
			case ImgAttr.Tint:     esJobList.AddLast(new EsSetImgTintJob{     node = img, duration = cmd.duration, esType = cmd.esType, r = img->color[0], dr = (float)args[0] - img->color[0], g = img->color[1], dg = (float)args[1] - img->color[1], b = img->color[2], db = (float)args[2] - img->color[2] }); break;
			}
		}

		public void SetCamAttr(int camAttrId, params object[] args) {
			cmdQueue.Enqueue(new SetCamAttrCmd{ camAttrId = camAttrId, args = args });
		}
		void SetCamAttr(SetCamAttrCmd cmd) {
			var args = cmd.args;
			switch (cmd.camAttrId) {
			case CamAttr.Position: cam.transform.position = new UnityEngine.Vector3((float)args[0], (float)args[1], -10); break;
			case CamAttr.Zoom:     cam.orthographicSize = (float)args[0]; break;
			}
		}

		public void SetCamAttrEased(int camAttrId, float duration, int esType, params object[] args) {
			cmdQueue.Enqueue(new SetCamAttrEasedCmd{ camAttrId = camAttrId, duration = duration, esType = esType, args = args });
		}
		void SetCamAttrEased(SetCamAttrEasedCmd cmd) {
			float endTime = time + cmd.duration;
			if (endTime > lastEsEndTime) lastEsEndTime = endTime;

			var args = cmd.args;
			switch (cmd.camAttrId) {
			case CamAttr.Position: esJobList.AddLast(new EsSetCamPositionJob{ cam = cam, duration = cmd.duration, esType = cmd.esType, x = cam.transform.position.x, y = cam.transform.position.y, dx = (float)args[0] - cam.transform.position.x, dy = (float)args[1] - cam.transform.position.y }); break;
			case CamAttr.Zoom:     esJobList.AddLast(new EsSetCamZoomJob{     cam = cam, duration = cmd.duration, esType = cmd.esType, s = cam.orthographicSize, ds = (float)args[0] - cam.orthographicSize }); break;
			}
		}
	}
}