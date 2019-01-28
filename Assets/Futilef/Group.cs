namespace Futilef {
	public unsafe struct Group {
		public int tag;
		#if FDB
		public static readonly int Type = Fdb.NewType("Group");
		public int type;
		#endif

		public fixed float pos[3];
		public fixed float scl[2];
		public float rot;

		public bool isTransformDirty;
		public bool isDepthDirty;

		public fixed float mat[6];
		public fixed float concatMat[6];
		public float *parentMat;

		public PtrLst childLst;

		public static void Init(Group *self) {
			self->tag = Tag.Group;

			Vec3.Zero(self->pos);
			Vec2.One(self->scl);
			self->rot = 0;
			self->isTransformDirty = true;
			self->isDepthDirty = true;

			Mat2D.Identity(self->mat);
			Mat2D.Identity(self->concatMat);
			self->parentMat = null;

			PtrLst.Init(&self->childLst);
		}

		/**
		 * parentMat: null -> no change
		 *            same pointer -> parent transform changed
		 *            diff pointer -> parent changed
		 */
		public static void Draw(Group *self, float *parentMat, bool isParentTransformDirty) {
			var childLst = &self->childLst;
			int childCount = childLst->count;
			void **childArr = childLst->arr;

			// sort by depth if any child depth is changed
			for (int i = 0; i < childCount; i += 1) {
				if (Node.IsDepthDirty(childArr[i])) {
					Algo.MergeSort(childArr, childCount, Node.DepthCmp);
					break;
				}
			}

			float *mat = self->mat;
			float *concatMat = self->concatMat;

			bool isTransformDirty = self->isTransformDirty;
			if (isTransformDirty) {
				self->isTransformDirty = false;
				Mat2D.FromScalingRotationTranslation(mat, self->pos, self->scl, self->rot);
			}

			if (isParentTransformDirty) {
				self->parentMat = parentMat;
			}

			if (isTransformDirty || isParentTransformDirty) {
				isParentTransformDirty = true;
				if (parentMat == null) {
					Mat2D.Copy(concatMat, mat);
				} else {
					Mat2D.Mul(concatMat, parentMat, mat);
				}
			}

			for (int i = 0; i < childCount; i += 1) {
				Node.Draw(childArr[i], concatMat, isParentTransformDirty);
			}
		}
	}
}

