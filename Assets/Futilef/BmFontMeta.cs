using System.Collections.Generic;
using System.IO;
using System;

using UnityEngine;

namespace Futilef {
	public unsafe struct BmGlyphMeta {
		// id			4	uint	0+c*20	These fields are repeated until all characters have been described
		public UInt32 id;
		// x			2	uint	4+c*20
		// y			2	uint	6+c*20
		public UInt32 x, y;
		// width		2	uint	8+c*20
		// height		2	uint	10+c*20
		public UInt32 width, height;
		// xoffset		2	int		12+c*20
		// yoffset		2	int		14+c*20
		public Int32 xOffset, yOffset;
		// xadvance		2	int		16+c*20
		public Int32 xAdvance;
		// page			1	uint	18+c*20
		public UInt32 page;
		// chnl			1	uint	19+c*20

		public static BmGlyphMeta *Init(BmGlyphMeta *self, byte[] buf, ref int i) {
			// id			4	uint	0+c*20	These fields are repeated until all characters have been described
			self->id = Bit.ReadUInt32(buf, ref i);
			// x			2	uint	4+c*20
			// y			2	uint	6+c*20
			self->x = Bit.ReadUInt16(buf, ref i);
			self->y = Bit.ReadUInt16(buf, ref i);
			// width		2	uint	8+c*20
			// height		2	uint	10+c*20
			self->width = Bit.ReadUInt16(buf, ref i);
			self->height = Bit.ReadUInt16(buf, ref i);
			// xoffset		2	int		12+c*20
			// yoffset		2	int		14+c*20
			self->xOffset = Bit.ReadInt16(buf, ref i);
			self->yOffset = Bit.ReadInt16(buf, ref i);
			// xadvance		2	int		16+c*20
			self->xAdvance = Bit.ReadInt16(buf, ref i);
			// page			1	uint	18+c*20
			self->page = Bit.ReadUInt8(buf, ref i);
			// chnl			1	uint	19+c*20
			Bit.ReadUInt8(buf, ref i);

//			Debug.Log(this);
			return self;
		}

		public override string ToString() {
			return string.Format("[BmGlyph: id={0}, x={1}, y={2}, width={3}, height={4}, xOffset={5}, yOffset={6}, xAdvance={7}, page={8}]", id, x, y, width, height, xOffset, yOffset, xAdvance, page);
		}
	}

	public class BmFontMeta {
		// fontSize		2	int		0
		public Int16 fontSize;
		// bitField		1	bits	2	bit 0: smooth, bit 1: unicode, bit 2: italic, bit 3: bold, bit 4: fixedHeigth, bits 5-7: reserved
		// charSet		1	uint	3
		// stretchH		2	uint	4
		// aa			1	uint	6
		// paddingUp	1	uint	7
		// paddingRight	1	uint	8
		// paddingDown	1	uint	9
		// paddingLeft	1	uint	10
		// spacingHoriz	1	uint	11
		public byte spacingHoriz;
		// spacingVert	1	uint	12
		public byte spacingVert;
		// outline		1	uint	13	added with version 2
		// fontName		n+1	string	14	null terminated string with length n
		public string fontName;

		// lineHeight	2	uint	0
		public UInt16 lineHeight;
		// base			2	uint	2
		// scaleW		2	uint	4
		// scaleH		2	uint	6
		public UInt16 scaleW, scaleH;
		// pages		2	uint	8
		public UInt16 pages;
		// bitField		1	bits	10	bits 0-6: reserved, bit 7: packed
		// alphaChnl	1	uint	11
		// redChnl		1	uint	12
		// greenChnl	1	uint	13
		// blueChnl		1	uint	14

		// pageNames	p*(n+1)	strings	0	p null terminated strings, each with length n
		public string[] pageNames;

		public Dictionary<UInt32, BmGlyphMeta> glyphDict = new Dictionary<UInt32, BmGlyphMeta>();
		public Dictionary<UInt64, Int16> kerningDict = new Dictionary<UInt64, Int16>();

		public BmFontMeta(byte[] buf) {
			int i = 0;

//			Should.Equal("Bit.ReadUInt8(buf, ref i)", Bit.ReadUInt8(buf, ref i), 'B');
//			Should.Equal("Bit.ReadUInt8(buf, ref i)", Bit.ReadUInt8(buf, ref i), 'M');
//			Should.Equal("Bit.ReadUInt8(buf, ref i)", Bit.ReadUInt8(buf, ref i), 'F');
//			Should.Equal("Bit.ReadUInt8(buf, ref i)", Bit.ReadUInt8(buf, ref i), 3);
//
//			Should.Equal("Bit.ReadUInt8(buf, ref i)", Bit.ReadUInt8(buf, ref i), 1);
			var length = Bit.ReadInt32(buf, ref i);

			// fontSize		2	int		0
			fontSize = Bit.ReadInt16(buf, ref i);
			// bitField		1	bits	2	bit 0: smooth, bit 1: unicode, bit 2: italic, bit 3: bold, bit 4: fixedHeigth, bits 5-7: reserved
			Bit.ReadUInt8(buf, ref i);
			// charSet		1	uint	3
			Bit.ReadUInt8(buf, ref i);
			// stretchH		2	uint	4
			Bit.ReadUInt16(buf, ref i);
			// aa			1	uint	6
			Bit.ReadUInt8(buf, ref i);
			// paddingUp	1	uint	7
			Bit.ReadUInt8(buf, ref i);
			// paddingRight	1	uint	8
			Bit.ReadUInt8(buf, ref i);
			// paddingDown	1	uint	9
			Bit.ReadUInt8(buf, ref i);
			// paddingLeft	1	uint	10
			Bit.ReadUInt8(buf, ref i);
			// spacingHoriz	1	uint	11
			spacingHoriz = Bit.ReadUInt8(buf, ref i);
			// spacingVert	1	uint	12
			spacingVert = Bit.ReadUInt8(buf, ref i);
			// outline		1	uint	13	added with version 2
			Bit.ReadUInt8(buf, ref i);
			// fontName		n+1	string	14	null terminated string with length n
			fontName = Bit.ReadString(buf, ref i, length - 15);
//			Should.Equal("Bit.ReadUInt8(buf, ref i)", Bit.ReadUInt8(buf, ref i), '\0');

			Debug.Log(fontName);

//			Should.Equal("Bit.ReadUInt8(buf, ref i)", Bit.ReadUInt8(buf, ref i), 2);
			length = Bit.ReadInt32(buf, ref i);

			// lineHeight	2	uint	0
			lineHeight = Bit.ReadUInt16(buf, ref i);
			// base			2	uint	2
			Bit.ReadUInt16(buf, ref i);
			// scaleW		2	uint	4
			// scaleH		2	uint	6
			scaleW = Bit.ReadUInt16(buf, ref i);
			scaleH = Bit.ReadUInt16(buf, ref i);
			// pages		2	uint	8
			pages = Bit.ReadUInt16(buf, ref i);
			// bitField		1	bits	10	bits 0-6: reserved, bit 7: packed
			Bit.ReadUInt8(buf, ref i);
			// alphaChnl	1	uint	11
			Bit.ReadUInt8(buf, ref i);
			// redChnl		1	uint	12
			Bit.ReadUInt8(buf, ref i);
			// greenChnl	1	uint	13
			Bit.ReadUInt8(buf, ref i);
			// blueChnl		1	uint	14
			Bit.ReadUInt8(buf, ref i);

//			Should.Equal("Bit.ReadUInt8(buf, ref i)", Bit.ReadUInt8(buf, ref i), 3);
			length = Bit.ReadInt32(buf, ref i);
			pageNames = Bit.ReadString(buf, ref i, length).Split(new [] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
//			Should.Equal("pageNames.Length", pageNames.Length, pages);
			foreach (var n in pageNames) {
				Debug.Log(n);
			}

//			Should.Equal("Bit.ReadUInt8(buf, ref i)", Bit.ReadUInt8(buf, ref i), 4);
			length = Bit.ReadInt32(buf, ref i);
			for (int j = 0; j < length; j += 20) {
//				var glyph = new BmGlyph(stream);
//				glyphDict.Add(glyph.id, glyph);
			}
//			Debug.Log(glyphDict.Count);

//			if (IsEnd()) return;

//			Should.Equal("Bit.ReadUInt8(buf, ref i)", Bit.ReadUInt8(buf, ref i), 5);
			length = Bit.ReadInt32(buf, ref i);
			for (int j = 0; j < length; j += 10) {
				UInt64 first = Bit.ReadUInt32(buf, ref i);
				UInt64 second = Bit.ReadUInt32(buf, ref i);
				Int16 amount = Bit.ReadInt16(buf, ref i);

				UInt64 key = (first << 32) | second;
				kerningDict.Add(key, amount);

//				Debug.LogFormat("{0}-{1}: {2}", first, second, amount);
			}
		}

		public bool TryGetGlyph(UInt32 id, out BmGlyphMeta glyph) {
			return glyphDict.TryGetValue(id, out glyph);
		}

		public bool HasGlyph(UInt32 id) {
			return glyphDict.ContainsKey(id);
		}

		public BmGlyphMeta GetGlyph(UInt32 id) {
			return glyphDict[id];
		}

		public int GetKerning(UInt32 first, UInt32 second) {
			UInt64 key = (first << 32) | second;
			return kerningDict.ContainsKey(key) ? kerningDict[key] : 0;
		}

		public override string ToString() {
			return string.Format("[BmFont: fontSize={0}, spacingHoriz={1}, spacingVert={2}, fontName={3}, lineHeight={4}, scaleW={5}, scaleH={6}, pages={7}, pageNames={8}, _glyphDict={9}, _kerningDict={10}]", fontSize, spacingHoriz, spacingVert, fontName, lineHeight, scaleW, scaleH, pages, pageNames, glyphDict, kerningDict);
		}
	}
}