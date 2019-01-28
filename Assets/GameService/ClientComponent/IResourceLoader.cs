using System.Collections;
using System.Collections.Generic;
using GameService.Util;
using GameCore;

namespace GameService.ClientComponent {
    public interface IResourceLoader {
        void Load(AssetReference res);
        bool IsLoaded(AssetReference res);
        void Release(AssetReference res);
    }
}