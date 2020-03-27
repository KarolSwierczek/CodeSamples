namespace Moonlit.DeadliestCatch.UserInterface
{
    using UnityEngine;

    /// <summary>
    /// object that can be dragged and droppend and contains content of type T
    /// </summary>
    /// <typeparam name="T">type of content</typeparam>
    public abstract class DragAndDropObject<T> : MonoBehaviour
    {
        #region Public Types
        public enum ObjectType
        {
            Swap,
            DragOnly,
            DropOnly
        }
        #endregion Public Types

        #region Public Variables
        public bool CanSource => Interactable && _ObjectType != ObjectType.DropOnly && _HasContent;
        public bool CanReceive => Interactable && _ObjectType != ObjectType.DragOnly;
        public bool Interactable { get; set; } = true;
        public T Content { get; protected set; }
        #endregion Public Variables

        #region Public Methods
        public virtual void OnDragStarted()
        {
        }

        public virtual void OnDragEnded()
        {
            if (_ObjectType == ObjectType.DragOnly) { return; }

            if (_CanRemove) { RemoveContent(); }
            else { _CanRemove = true; }
        }

        public virtual void OnDrag(Vector2 position)
        {
        }

        public virtual void OnDrop(DragAndDropObject<T> sourceObject)
        {
            var newContent = sourceObject.Content;
            if (_ObjectType == ObjectType.Swap && CanSource && sourceObject.CanReceive) { sourceObject.OnSwap(Content); }
            AddContent(newContent);
        }

        public virtual void OnDropInvalid()
        {
            _CanRemove = false;
        }

        public virtual void OnSwap(T content)
        {
            AddContent(content);
            _CanRemove = false;
        }

        public virtual void AddContent(T content)
        {
            Content = content;
            _HasContent = true;
        }

        public virtual void RemoveContent()
        {
            _HasContent = false;
            Content = default;
        }
        #endregion Public Methods

        #region Inspector Variables
        [SerializeField] protected ObjectType _ObjectType;
        #endregion Inspector Variables

        #region Private Variables
        protected bool _CanRemove = true;
        protected bool _HasContent = false;
        #endregion Private Variables
    }
}