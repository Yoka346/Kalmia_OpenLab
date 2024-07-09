using System;

namespace Kalmia_OpenLab.SDLWrapper
{
    internal class SDLException : Exception
    {
        public SDLException(int errCode) : base($"SDL error occured. Error code: {errCode}") { }
        public SDLException(string message) : base(message) { }
    }
}
