namespace Moonlit.DeadliestCatch.Gameplay.MiniGames.CrabPotPulling
{
    using GameData.MiniGames.CrabPotPulling;
    using Gameplay.GameEvents;
    using UserInterface.Panels;
    using Messages;
    using System;
    using UnityEngine;
    using Zenject;

    public sealed class SubMiniGamePull : ISubMiniGame, IMessageSender
    {
        #region Public Types
        public sealed class OnOpenArgs : GameEventArgs
        {
            public float RopeLengthLeft { get; }

            public OnOpenArgs(float ropeLengthLeft)
            {
                RopeLengthLeft = ropeLengthLeft;
            }
        }

        public sealed class OnCloseArgs : GameEventArgs
        {
            //left empty on purpose
        }

        public sealed class OnCompleteArgs : GameEventArgs
        {
            //left empty on purpose
        }

        public sealed class OnTickArgs : GameEventArgs
        {
            public float RopeLengthLeft { get; }

            public OnTickArgs(float ropeLengthLeft)
            {
                RopeLengthLeft = ropeLengthLeft;
            }
        }
        #endregion Public Types

        #region Public Variables
        public bool IsActive { get; private set; }

        public event EventHandler OnOpen;
        public event EventHandler OnClose;
        public event EventHandler OnComplete;
        public event EventHandler<OnTickArgs> OnTick;
        #endregion Public Variables

        #region Public Methods
        [Inject]
        public SubMiniGamePull(CrabPotPullingTools tools, SubMiniGamePullData data)
        {
            _Data = data;
            _Tools = tools;
        }

        public void Open()
        {
            IsActive = true;

            _Tools.Hook.transform.SetParent(_Tools.StaticParent);

            _Tools.Hook.SetActive(true);
            _Tools.Rope.SetActive(true);

            var initialRopeLength = (_Tools.Hook.transform.position - _Tools.Camera.position).magnitude;
            _RopeLengthData = new RopeLengthData(initialRopeLength);

            OpenPanel();

            OnOpen?.Invoke(this, new OnOpenArgs(_RopeLengthData.RopeLength));
        }

        public void Close()
        {
            IsActive = false;

            ClosePanel();

            OnClose?.Invoke(this, new OnCloseArgs());
        }

        public void Complete()
        {
            OnComplete?.Invoke(this, new OnCompleteArgs());
        }

        public void Pull()
        {
            if (!IsActive) { return; }

            _RopeLengthData.PullRope(Time.deltaTime * _Data.PullingSpeed);

            if (_RopeLengthData.RopeLength < _Data.MinRopeLength) { Complete(); return; }

            _Tools.Rope.ScrollTexture(Time.deltaTime * _Data.RopeScrollSpeed);

            var hookDirection = (_Tools.Hook.transform.position - _Tools.Camera.position).normalized;
            var newHookDirection = hookDirection * _RopeLengthData.RopeLength;
            _Tools.Hook.transform.position = _Tools.Camera.position + newHookDirection;

            OnTick?.Invoke(this, new OnTickArgs(_RopeLengthData.RopeLength));
        }
        #endregion Public Methods

        #region Private Variables
        private readonly SubMiniGamePullData _Data;
        private readonly CrabPotPullingTools _Tools;
        private RopeLengthData _RopeLengthData;
        #endregion Private Variables

        #region Private Methods
        private void OpenPanel()
        {
            var panelProps = new CrabPotPullingPanelProperties(_RopeLengthData);
            this.Signal(new CrabPotPullingPanel.ShowMessage() { Properties = panelProps });
        }

        private void ClosePanel()
        {
            this.Signal<CrabPotPullingPanel.HideMessage>();
        }
        #endregion Private Methods
    }
}