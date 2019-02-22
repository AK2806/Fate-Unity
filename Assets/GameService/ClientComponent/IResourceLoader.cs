using System.Collections;
using System.Collections.Generic;
using GameService.Util;
using GameCore;

namespace GameService.ClientComponent {
    public class SpriteMeta {
        public AssetReference reference;
        public Vec2 sizeInPixels;
        public Vec2 pivotNormalized;
    }

    public interface IResourceLoader {
        void AsyncReload(AssetReference[] resources); // clear loaded resources and load new resources asynchronously
        float GetLoadingProgress();
        SpriteMeta GetAssetMetaAsSprite(AssetReference reference);
    }
}