namespace Futilef {
	public static unsafe class Node {
		public static bool IsDepthDirty(void *node) {
			switch (*(int *)node) {
			case Tag.TpSprite: return ((TpSprite *)node)->isDepthDirty;
			case Tag.Group: return ((Group *)node)->isDepthDirty;
			}
			return false;
		}

		public static float *GetPosition(void *node) {
			switch (*(int *)node) {
			case Tag.TpSprite: return ((TpSprite *)node)->pos;
			case Tag.Group: return ((Group *)node)->pos;
			}
			return null;
		}

		public static void Draw(void *node, float *parentMat, bool isParentTransformDirty) {
			#if FDB
			Should.NotNull("node", node);
			#endif
			switch (*(int *)node) {
			case Tag.TpSprite: TpSprite.Draw((TpSprite *)node, parentMat, isParentTransformDirty); return;
			case Tag.Group: Group.Draw((Group *)node, parentMat, isParentTransformDirty); return;
			}
		}

		public static int DepthCmp(void *node1, void *node2) {
			float depth1 = GetPosition(node1)[2], depth2 = GetPosition(node2)[2];
			return depth1 - depth2 < 0 ? 1 : -1;
		}
	}
}

