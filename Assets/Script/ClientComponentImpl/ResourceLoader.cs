using System;
using System.Collections;
using System.Collections.Generic;
using Futilef;
using GameService.Util;
using GameService.ClientComponent;
using GameCore;

namespace ClientComponentImpl {
    public sealed class ResourceLoader : IResourceLoader {
        public void Load(AssetReference res) {

        }

        public bool IsLoaded(AssetReference res) {
            return false;
        }

        public void Release(AssetReference res) {

        }
    }
}