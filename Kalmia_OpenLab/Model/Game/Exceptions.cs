﻿using System;

using Kalmia_OpenLab.Model.Reversi;

namespace Kalmia_OpenLab.Model.Game
{
    internal class IllegalMoveException : Exception
    {
        const string MESSAGE = "{0} sent move {1} but it was illegal move.";
        public IllegalMoveException(IPlayer player, BoardCoordinate move) : base(string.Format(MESSAGE, player, move)) { }
    }
}
