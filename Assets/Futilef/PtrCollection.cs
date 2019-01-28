namespace Futilef {
	public static unsafe class PtrCollection {
		public static void ShiftBase(void *self, long shift) {
			switch (*(int *)self) {
			case Tag.PtrLst: PtrLst.ShiftBase((PtrLst *)self, shift); return;
			case Tag.PtrIntDict: PtrIntDict.ShiftBase((PtrIntDict *)self, shift); return;
			}
		}
	}
}

