namespace Moonlit.DeadliestCatch.Gameplay.MiniGames.CrabPotPulling
{
    using System;

    /// <summary>
    /// interface for all sub-minigames connected to crab pot pulling
    /// </summary>
    public interface ISubMiniGame
    {
        #region Public Variables
        event EventHandler OnOpen;
        event EventHandler OnClose;
        event EventHandler OnComplete;

        bool IsActive { get; }
        #endregion Public Variables

        #region Public Methods
        void Open();
        void Close();
        void Complete();
        #endregion Public Methods
    }
}