namespace Moonlit.DeadliestCatch.Gameplay.MiniGames.CrabPotPulling
{
    using Interaction;
    using Equipment;
    using UnityEngine;
    using GameEvents;
    using System;
    using Rewired;

    // TODO: Would be nice to have more than one slot on the Ship. @karol
    public sealed class GrapplingHookSlot
    {
        #region Public Types
        public sealed class HookTakenArgs : GameEventArgs
        {
            public GrapplingHook Hook { get; }

            public HookTakenArgs(GrapplingHook hook)
            {
                Hook = hook;
            }
        }

        public sealed class HookPutAwayArgs : GameEventArgs
        {
            public GrapplingHook Hook { get; }

            public HookPutAwayArgs(GrapplingHook hook)
            {
                Hook = hook;
            }
        }

        public sealed class MiniGameStartedArgs : GameEventArgs
        {
            public Player Player;

            public MiniGameStartedArgs(Player player)
            {
                Player = player;
            }
        }
        #endregion Public Types

        #region Public Variables
        public bool ContainsGrapplingHook => _Hook != null;
        public GrapplingHook Hook => _Hook;

        public EventHandler<HookTakenArgs> HookTaken;
        public EventHandler<HookPutAwayArgs> HookPutAway;
        public EventHandler<MiniGameStartedArgs> MiniGameStarted;
        #endregion Public Variables

        #region Public Methods
        public void AddGrapplingHook(IPickableItem item)
        {
            if (_Hook != null) { Debug.LogError("Trying to put away grappling hook, but the slot is already occupied"); return; }

            var hook = (GrapplingHook)item;

            if (hook == null) { Debug.LogError("Trying to put away an item that's not a grappling hook"); return; }

            _Hook = hook;
            HookPutAway?.Invoke(this, new HookPutAwayArgs(_Hook));
        }

        public IPickableItem RemoveGrapplingHook()
        {
            if (_Hook == null) { Debug.LogError("Trying to take null grappling hook"); return null; }

            var hook = _Hook;
            _Hook = null;

            HookTaken?.Invoke(this, new HookTakenArgs(hook));
            return hook;
        }

        public void TakeGrapplingHook(PlayerInteractionHand playerHand)
        {
            playerHand.Grab(RemoveGrapplingHook());
        }

        public void PutAwayGrapplingHook(PlayerInteractionHand playerHand)
        {
            AddGrapplingHook(playerHand.Drop());
        }

        public void StartMinigame(PlayerInteractionHand playerHand, Player player)
        {
            TakeGrapplingHook(playerHand);
            MiniGameStarted?.Invoke(this, new MiniGameStartedArgs(player));
        }
        #endregion Public Methods

        #region Private Variables
        private GrapplingHook _Hook;
        #endregion Private Variables
    }
}