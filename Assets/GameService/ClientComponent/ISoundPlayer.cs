using GameService.Util;
using GameCore;

namespace GameService.ClientComponent {
    public interface ISoundPlayer {
        void PlayBGM(AssetReference music);
        void StopBGM();
        void PlayBGS(AssetReference sound);
        void StopBGS();
        void PlaySE(AssetReference sound);
    }
}