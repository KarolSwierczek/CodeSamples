using SimFactor.BWPSimulator.Data;
using System;
using System.Collections.Generic;
using SimFactor.BWPSimulator.Systems;
using UnityEngine;
using Zenject;

namespace SimFactor.BWPSimulator.UI
{
    public sealed class MenuTabsManager : IMenuTabsManager
    {
        [Inject] private readonly LoginWindow _loginWindow;
        [Inject] private readonly QuitApplicationDialogEntity _quitDialog;
        [Inject] private readonly IApplicationSession _applicationSession;

        private event Action<TabChangedArgs> TabChanged;

        private List<IMenuTab> _menuTabs = new List<IMenuTab>();
        private IMenuTab _activeTab = null;
        private IMenuTab _nextTab = null;

        MenuTabType IMenuTabsManager.CurrentTabType => _activeTab.TabType;
        MenuTabType IMenuTabsManager.NextTabType => _nextTab.TabType;

        event Action<TabChangedArgs> IMenuTabsManager.ActiveTabChanged
        {
            add { TabChanged += value; }
            remove { TabChanged -= value; }
        }

        void IMenuTabsManager.SetActiveTab(MenuTabType tabType)
        {
            switch (tabType)
            {
                case MenuTabType.NONE:
                    return;
                case MenuTabType.APP_QUIT:
                    _quitDialog.ShowDialog();
                    return;
                case MenuTabType.Demo:
                    GetTab(tabType).Open(); //todo: this is a hotfix
                    return;
                case MenuTabType.Shooting:
                    _applicationSession.ClearTrainee();
                    break;
            }
            
            SetActiveTab(GetTab(tabType));
        }

        void IMenuTabsManager.RegisterTab(IMenuTab tab)
        {
            _menuTabs.Add(tab);
        }

        private IMenuTab GetTab(MenuTabType tabType)
        {
            foreach (var tab in _menuTabs)
            {
                if (tab.TabType == tabType)
                {
                    return tab;
                }
            }

            throw new ArgumentException("There is no registered menu tab of type: " + tabType);
        }

        private void SetActiveTab(IMenuTab tab)
        {
            var previousTabType = _activeTab?.TabType ?? MenuTabType.NONE;

            if (_activeTab != null)
            {
                if (_activeTab == tab)
                {
                    return;
                }

                _activeTab.Close();             
            }
            _nextTab = tab;
            tab.Open();
            _activeTab = tab;
            _nextTab = null;
            TabChanged?.Invoke(new TabChangedArgs(tab.TabType, tab.TitleTerm));

            if (tab.RequiredUserAccess != UserType.None)
            {
                _loginWindow.Open(tab.RequiredUserAccess, tab.TabType, previousTabType);
            }
        }

        public class TabChangedArgs
        {
            public MenuTabType TabType { get; }
            public string Term { get; }

            public TabChangedArgs(MenuTabType tabType, string term)
            {
                TabType = tabType;
                Term = term;
            }
        }
    }
}
