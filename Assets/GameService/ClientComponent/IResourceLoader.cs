using System.Collections;
using System.Collections.Generic;
using GameService.Util;
using GameCore;

namespace GameService.ClientComponent {
    public interface IResourceLoader {
        void AsyncReload(AssetReference[] resources); // clear loaded resources and load new resources asynchronously
        float GetLoadingProgress();
    }
}