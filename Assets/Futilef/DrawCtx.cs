using System.Collections.Generic;
using UnityEngine;

namespace Futilef {
	public static class DrawCtx {
		public const int BackgroundQueue = 1000, GeometryQueue = 2000, TransparentQueue = 3000, OverlayQueue = 4000;

		static readonly Shader DefaultShader = Shader.Find("Futilef/Basic");

		static LinkedList<DrawBat> prevBatches = new LinkedList<DrawBat>();
		static LinkedList<DrawBat> activeBatches = new LinkedList<DrawBat>();
		static readonly LinkedList<DrawBat> inactiveBatches = new LinkedList<DrawBat>();

		static int curQueue;
		static DrawBat curBatch;

		public static void Start() {
			curQueue = TransparentQueue + 1;
			curBatch = null;
		}

		public static void Finish() {
			for (var node = prevBatches.First; prevBatches.Count > 0; node = prevBatches.First) {
				node.Value.Deactivate();
				inactiveBatches.AddLast(node.Value);
				prevBatches.RemoveFirst();
			}

			// Same as prevBatches.AddRange(activeBatches); activeBatches.Clear();
			var swap = prevBatches;
			prevBatches = activeBatches;
			activeBatches = swap;

			if (curBatch != null) {
				curBatch.Close();
				curBatch = null;
			}
		}

		public static DrawBat GetBatch(int textureId) {
			return GetBatch(DefaultShader, Res.GetTexture(textureId));
		}

		public static DrawBat GetBatch(Shader shader, Texture2D texture) {
			if (curBatch != null) {
				if (curBatch.shader == shader && curBatch.texture == texture) return curBatch;
				curBatch.Close();
			}

			curQueue += 1;

			// if there is a prevBatch that matches
			for (var node = prevBatches.First; node != null; node = node.Next) {
				curBatch = node.Value;
				if (curBatch.shader == shader && curBatch.texture == texture) {
					prevBatches.Remove(node);
					curBatch.Open(curQueue);
					activeBatches.AddLast(curBatch);
					return curBatch;
				}
			}

			// if there is an inactiveBatch that matches
			for (var node = inactiveBatches.First; node != null; node = node.Next) {
				curBatch = node.Value;
				if (curBatch.shader == shader && curBatch.texture == texture) {
					inactiveBatches.Remove(node);
					curBatch.Activate();
					curBatch.Open(curQueue);
					activeBatches.AddLast(curBatch);
					return curBatch;
				}
			}

			// create a new batch
			curBatch = new DrawBat(shader, texture);
			curBatch.Open(curQueue);
			activeBatches.AddLast(curBatch);
			return curBatch;
		}

		public static void Dispose() {
			foreach (var batch in activeBatches) batch.Dispose(); activeBatches.Clear();
			foreach (var batch in prevBatches) batch.Dispose(); prevBatches.Clear();
			foreach (var batch in inactiveBatches) batch.Dispose(); inactiveBatches.Clear();
		}
	}
}
