using System;
using System.Collections;
using System.Collections.Generic;
using Futilef;
using GameService.Util;
using GameService.ClientComponent;
using GameCore;

namespace FateUnity.Script.ClientComponentImpl {
    public sealed class ResourceLoader : IResourceLoader {
        public void AsyncReload(AssetReference[] resources) {

        }

        public float GetLoadingProgress() {
            return 0.0f;
        }
        
        public SpriteMeta GetAssetMetaAsSprite(AssetReference reference) {
            return new SpriteMeta();
        }
    }
}