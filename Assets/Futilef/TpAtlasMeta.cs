namespace Futilef {
	public unsafe struct TpAtlasMeta {
		#if FDB
		public static readonly int Type = Fdb.NewType("TpAtlasMeta");
		public int type;
		#endif
		public int name;
		public fixed float size[2];

		public int spriteCount;
		public TpSpriteMeta *sprites;

		public static TpAtlasMeta *New(string str) {
			#if FDB
			Should.NotNullOrEmpty("str", str);
			#endif
			return Init((TpAtlasMeta *)Mem.Malloc(sizeof(TpAtlasMeta)), str);
		}

		public static TpAtlasMeta *Init(TpAtlasMeta *self, string str) {
			#if FDB
			Should.NotNull("self", self);
			Should.NotNullOrEmpty("str", str);
			self->type = Type;
			#endif
			string[] segs = str.Split(',');
			int *nums = stackalloc int[segs.Length];
			for (int j = 0, end = segs.Length; j < end; j += 1) {
				nums[j] = int.Parse(segs[j]);
			}

			int i = 0;
			self->name = nums[i++];
			self->size[0] = nums[i++];
			self->size[1] = nums[i++];
			self->spriteCount = nums[i++];
			self->sprites = (TpSpriteMeta *)Mem.Malloc(self->spriteCount * sizeof(TpSpriteMeta));
			for (int j = 0, end = self->spriteCount; j < end; j += 1) {
				var sprite = self->sprites + j;
				#if FDB
				sprite->type = TpSpriteMeta.Type;
				#endif
				sprite->atlas = self;
				sprite->name = nums[i++];
				sprite->rotated = nums[i++] != 0;
				sprite->size[0] = nums[i++];
				sprite->size[1] = nums[i++];
				sprite->pivot[0] = nums[i++];
				sprite->pivot[1] = nums[i++];
				sprite->quad[0] = nums[i++];
				sprite->quad[1] = nums[i++];
				sprite->quad[2] = nums[i++];
				sprite->quad[3] = nums[i++];
				sprite->uv[0] = nums[i++];
				sprite->uv[1] = nums[i++];
				sprite->uv[2] = nums[i++];
				sprite->uv[3] = nums[i++];
				sprite->border[0] = nums[i++];
				sprite->border[1] = nums[i++];
				sprite->border[2] = nums[i++];
				sprite->border[3] = nums[i++];
			}

			return self;
		}

		public static void Decon(TpAtlasMeta *self) {
			#if FDB
			Should.NotNull("self", self);
			Should.TypeEqual("self", self->type, Type);
			Should.NotNull("self->sprites", self->sprites);
			self->type = Fdb.NullType;
			for (int j = 0, end = self->spriteCount; j < end; j += 1) {
				var sprite = self->sprites + j;
				Should.TypeEqual("sprite->type", sprite->type, TpSpriteMeta.Type);
				sprite->type = Fdb.NullType;
			}
			#endif
			self->name = -1;
			Mem.Free(self->sprites); self->sprites = null;
		}
	}

	public unsafe struct TpSpriteMeta {
		#if FDB
		public static readonly int Type = Fdb.NewType("TpSpriteMeta");
		public int type;
		#endif
		public TpAtlasMeta *atlas;

		public int name;
		public bool rotated;
		public fixed float size[2], pivot[2];
		public fixed float quad[4], uv[4], border[4];

		// v0 - v1
		// |  \ |
		// v3 - v2
		public static void FillQuad(TpSpriteMeta *self, float *mat, float *verts) {
			#if FDB
			Should.NotNull("self", self);
			Should.NotNull("verts", verts);
			Should.NotNull("mat", mat);
			Should.TypeEqual("self", self->type, Type);
			Should.NotNull("self->atlas", self->atlas);
			Should.TypeEqual("self->atlas->type", self->atlas->type, TpAtlasMeta.Type);
			#endif
			float *pivot = self->pivot;
			float pivotX = pivot[0], pivotY = pivot[1];
			float *quad = self->quad;
			float quadX = quad[0], quadY = quad[1], quadW = quad[2], quadH = quad[3];
			Vec2.TransformMat2D(verts,     mat, -pivotX + quadX,         pivotY - quadY);
			Vec2.TransformMat2D(verts + 2, mat, -pivotX + quadX + quadW, pivotY - quadY);
			Vec2.TransformMat2D(verts + 4, mat, -pivotX + quadX + quadW, pivotY - quadY - quadH);
			Vec2.TransformMat2D(verts + 6, mat, -pivotX + quadX,         pivotY - quadY - quadH);

		}

		public static void FillUvs(TpSpriteMeta *self, float *uvs) {
			#if FDB
			Should.NotNull("self", self);
			Should.NotNull("uvs", uvs);
			Should.TypeEqual("self", self->type, Type);
			Should.NotNull("self->atlas", self->atlas);
			Should.TypeEqual("self->atlas->type", self->atlas->type, TpAtlasMeta.Type);
			#endif
			float *size = self->atlas->size;
			float invSizeX = 1 / size[0], invSizeY = 1 / size[1];
			float *uv = self->uv;
			float uvX = uv[0], uvY = uv[1], uvW = uv[2], uvH = uv[3];
			if (self->rotated) {
				Vec2.Set(uvs    , (uvX + uvW) * invSizeX,  -uvY        * invSizeY);
				Vec2.Set(uvs + 2, (uvX + uvW) * invSizeX, (-uvY - uvH) * invSizeY);
				Vec2.Set(uvs + 4,  uvX        * invSizeX, (-uvY - uvH) * invSizeY);
				Vec2.Set(uvs + 6,  uvX        * invSizeX,  -uvY        * invSizeY);
			} else {
				Vec2.Set(uvs    ,  uvX        * invSizeX,  -uvY        * invSizeY);
				Vec2.Set(uvs + 2, (uvX + uvW) * invSizeX,  -uvY        * invSizeY);
				Vec2.Set(uvs + 4, (uvX + uvW) * invSizeX, (-uvY - uvH) * invSizeY);
				Vec2.Set(uvs + 6,  uvX        * invSizeX, (-uvY - uvH) * invSizeY);
			}
		}
	}
}