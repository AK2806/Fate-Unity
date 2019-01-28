using System.Collections.Generic;
using UnityEngine;

namespace Futilef {
	public static unsafe class Res {
		static readonly Dictionary<int, Texture2D> textureDict = new Dictionary<int, Texture2D>();

		static readonly PtrLst *atlasMetaLst = PtrLst.New();
		// static readonly PtrLst *spriteMetaLst = PtrLst.New();
		static readonly PtrIntDict *spriteMetaDict = PtrIntDict.New();
		// static readonly Dictionary<int, int> spriteMetaLstIdxDict = new Dictionary<int, int>();

		public static Texture2D GetTexture(int id) {
			if (textureDict.ContainsKey(id)) return textureDict[id];

			var texture = new Texture2D(0, 0, TextureFormat.RGBA32, false);
			texture.LoadImage(Resources.Load<TextAsset>(id + "i").bytes);
			textureDict[id] = texture;
			return texture;
		}

		public static void ReleaseTexture(int id) {
			if (!textureDict.ContainsKey(id)) return;
			var texture = textureDict[id];
			textureDict.Remove(id);
			Object.Destroy(texture);
		}

		public static void LoadAtlases(params int[] ids) {
			for (int i = 0, len = ids.Length; i < len; i += 1) {
				int id = ids[i];
				var atlasMeta = TpAtlasMeta.New(Resources.Load<TextAsset>(id.ToString()).text);
				PtrLst.Push(atlasMetaLst, atlasMeta);
				for (int j = 0, jlen = atlasMeta->spriteCount; j < jlen; j += 1) {
					var spriteMeta = atlasMeta->sprites + j;
					PtrIntDict.Set(spriteMetaDict, spriteMeta->name, spriteMeta);
					// spriteMetaLstIdxDict[spriteMeta->name] = spriteMetaLst->count;
					// PtrLst.Push(spriteMetaLst, spriteMeta);
				}
			}	
		}

		public static void ReleaseAtlases(params int[] ids) {
			for (int i = 0, len = ids.Length; i < len; i += 1) {
				int id = ids[i];
				var atlasMeta = (TpAtlasMeta *)atlasMetaLst->arr[i];
				if (atlasMeta->name == id) {
					ReleaseTexture(id);
					for (int j = 0, jlen = atlasMeta->spriteCount; j < jlen; j += 1) {
						var spriteMeta = atlasMeta->sprites + j;
						PtrIntDict.Remove(spriteMetaDict, spriteMeta->name);
					}
					TpAtlasMeta.Decon(atlasMeta); Mem.Free(atlasMeta);
					PtrLst.RemoveAt(atlasMetaLst, i);
					i -= 1;
				}
			}	
		}

		public static TpSpriteMeta *GetSpriteMeta(int id) {
			return (TpSpriteMeta *)PtrIntDict.Get(spriteMetaDict, id);
			// return (TpSpriteMeta *)spriteMetaLst->arr[spriteMetaLstIdxDict[id]];
		}

		public static bool HasSpriteMeta(int id) {
			return PtrIntDict.Contains(spriteMetaDict, id);
			// return spriteMetaLstIdxDict.ContainsKey(id);
		}
	}
}
