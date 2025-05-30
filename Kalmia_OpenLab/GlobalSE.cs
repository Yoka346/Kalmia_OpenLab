using Kalmia_OpenLab.SDLWrapper;

namespace Kalmia_OpenLab
{
    internal class GlobalSE
    {
        const string CUSOR_SE_FILE_NAME = "cursor.ogg";
        const string BUTTON_PRESS_SE_FILE_NAME = "button_press.ogg";
        const string POP_UP_SE_FILE_NAME = "pop_up.ogg";

        public static MixerChunk CursorSE { get; }
        public static MixerChunk ButtonPressSE { get; }

        static GlobalSE()
        {
            CursorSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}{CUSOR_SE_FILE_NAME}");
            ButtonPressSE = MixerChunk.LoadWav($"{FilePath.SEDirPath}{BUTTON_PRESS_SE_FILE_NAME}");
        }

        public static void DisposeAll()
        {
            CursorSE.Dispose();
            ButtonPressSE.Dispose();
        }
    }
}
