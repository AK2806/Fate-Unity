namespace Futilef {
	public unsafe struct TpSprite {
		public int tag;
		#if FDB
		public static readonly int Type = Fdb.NewType("TpSpriteNode");
		public int type;
		#endif
		public int id;

		public fixed float pos[3];
		public fixed float scl[2];
		public float rot;

		public bool isTransformDirty;
		public bool isDepthDirty;

		public fixed float color[4];
		public fixed float verts[4 * 2];
		public fixed float uvs[4 * 2];

		public TpSpriteMeta *spriteMeta;

		public static void Init(TpSprite *self, TpSpriteMeta *spriteMeta) {
			#if FDB
			Should.NotNull("self", self);
			Should.NotNull("spriteMeta", spriteMeta);
			self->type = Type;
			#endif
			self->tag = Tag.TpSprite;

			Vec3.Zero(self->pos);
			Vec2.One(self->scl);
			self->rot = 0;
			self->isTransformDirty = true;  // delay regen verts to first Draw

			Vec4.Set(self->color, 1, 1, 1, 0);
			TpSpriteMeta.FillUvs(spriteMeta, self->uvs);

			self->spriteMeta = spriteMeta;
		}

		public static void SetMeta(TpSprite *self, TpSpriteMeta *meta) {
			self->isTransformDirty = true;
			TpSpriteMeta.FillUvs(meta, self->uvs);
			self->spriteMeta = meta;
		}

		public static void SetPosition(TpSprite *self, float x, float y, float z) {
			#if FDB
			Should.NotNull("self", self);
			Should.TypeEqual("self", self->type, Type);
			#endif
			Vec3.Set(self->pos, x, y, z);
			self->isTransformDirty = true;
		}

		public static void SetRotation(TpSprite *self, float rotation) {
			#if FDB
			Should.NotNull("self", self);
			Should.TypeEqual("self", self->type, Type);
			#endif
			self->rot = rotation;
			self->isTransformDirty = true;
		}

		public static void SetScale(TpSprite *self, float x, float y) {
			#if FDB
			Should.NotNull("self", self);
			Should.TypeEqual("self", self->type, Type);
			#endif
			Vec2.Set(self->scl, x, y);
			self->isTransformDirty = true;
		}

		public static void SetAlpha(TpSprite *self, float a) {
			#if FDB
			Should.NotNull("self", self);
			Should.TypeEqual("self", self->type, Type);
			#endif
			self->color[3] = a;
		}

		public static void SetTint(TpSprite *self, float r, float g, float b) {
			#if FDB
			Should.NotNull("self", self);
			Should.TypeEqual("self", self->type, Type);
			#endif
			Vec3.Set(self->color, r, g, b);
		}

		// v0 - v1
		// |  \ |
		// v3 - v2
		public static bool Raycast(TpSprite *self, float x, float y) {
			float *verts = self->verts;
			float v0x = verts[0], v0y = verts[1], v1x = verts[2], v1y = verts[3];
			float v2x = verts[4], v2y = verts[5], v3x = verts[6], v3y = verts[7];
			float area = Area(v0x, v0y, v1x, v1y, v2x, v2y) + Area(v0x, v0y, v2x, v2y, v3x, v3y);
			float sum = Area(x, y, v0x, v0y, v1x, v1y);
//			UnityEngine.Debug.LogFormat("{2} cast {0} {1}, area {3} sum1 {4}", x, y, self->id, area, sum);
			if (sum > area) return false;
			sum += Area(x, y, v1x, v1y, v2x, v2y);
//			UnityEngine.Debug.LogFormat("\t{2} area {3} sum2 {4}", x, y, self->id, area, sum);
			if (sum > area) return false;
			sum += Area(x, y, v2x, v2y, v3x, v3y);
//			UnityEngine.Debug.LogFormat("\t{2} area {3} sum3 {4}", x, y, self->id, area, sum);
			if (sum > area) return false;
			sum += Area(x, y, v3x, v3y, v0x, v0y);
//			UnityEngine.Debug.LogFormat("\t{2} area {3} sum4 {4}", x, y, self->id, area, sum);
			if (sum > area) return false;
//			UnityEngine.Debug.LogFormat("{2} hit", x, y, self->id);
			return true;
		}
		static float Area(float p0x, float p0y, float p1x, float p1y, float p2x, float p2y) {
			p0x = (p0x * (p1y - p2y) + p1x * (p2y - p0y) + p2x * (p0y - p1y)) * .5f;
			if (p0x < 0) return -p0x;
			return p0x;
		}

		public static void Draw(TpSprite *self, float *parentMat, bool isParentTransformDirty) {
			#if FDB
			Should.NotNull("self", self);
			Should.TypeEqual("self", self->type, Type);
			Should.NotNull("self->spriteMeta", self->spriteMeta);
			#endif
			var bat = DrawCtx.GetBatch(self->spriteMeta->atlas->name);
			int vertIdx = bat.vertCount, triIdx = bat.triCount;
			bat.RequestQuota(4, 6);

			float *verts = self->verts;
			float *uvs = self->uvs;

			if (self->isTransformDirty) {
				float *mat = stackalloc float[6];
				Mat2D.FromScalingRotationTranslation(mat, self->pos, self->scl, self->rot);
				TpSpriteMeta.FillQuad(self->spriteMeta, mat, self->verts);
			}

			var bVerts = bat.verts; var bUvs = bat.uvs;
			float z = self->pos[2];
			bVerts[vertIdx    ].Set(verts[0], verts[1], z);
			bVerts[vertIdx + 1].Set(verts[2], verts[3], z);
			bVerts[vertIdx + 2].Set(verts[4], verts[5], z);
			bVerts[vertIdx + 3].Set(verts[6], verts[7], z);

			bUvs[vertIdx    ].Set(uvs[0], uvs[1]);
			bUvs[vertIdx + 1].Set(uvs[2], uvs[3]);
			bUvs[vertIdx + 2].Set(uvs[4], uvs[5]);
			bUvs[vertIdx + 3].Set(uvs[6], uvs[7]);

			float *color = self->color;
			var bColor = Vec4.Color(color, 0.5f);
			var bColors = bat.colors;
			bColors[vertIdx    ] = bColor;
			bColors[vertIdx + 1] = bColor;
			bColors[vertIdx + 2] = bColor;
			bColors[vertIdx + 3] = bColor;

			var bTris = bat.tris;
			bTris[triIdx]     = vertIdx;
			bTris[triIdx + 1] = vertIdx + 1;
			bTris[triIdx + 2] = vertIdx + 2;
			bTris[triIdx + 3] = vertIdx;
			bTris[triIdx + 4] = vertIdx + 2;
			bTris[triIdx + 5] = vertIdx + 3;
		}

		public static int DepthCmp(void *a, void *b) {
			return ((TpSprite *)a)->pos[2] - ((TpSprite *)b)->pos[2] < 0 ? 1 : -1;
		}
	}
}
