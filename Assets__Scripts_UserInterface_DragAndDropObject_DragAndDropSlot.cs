namespace Moonlit.DeadliestCatch.UserInterface
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// object that can be dragged and droppend and contains content of type T
    /// </summary>

    public abstract class DragAndDropSlot<T> : MonoBehaviour, IBeginDragHandler, IDropHandler, IEndDragHandler, IDragHandler
    {
        #region Public Methods
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!_Object.CanSource) { Debug.Log("cant source on begin drag"); Debug.Log(_Object.Interactable); Debug.Log(_Object.Content); return; }
            if (_SourceObject != null) { throw new System.Exception("Another object is already being dragged"); }

            _SourceObject = _Object;
            _Object.OnDragStarted();
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            if (_SourceObject == null) { return; }
            if (_SourceObject == _Object || !_Object.CanReceive) { _SourceObject.OnDropInvalid(); return; }
            _Object.OnDrop(_SourceObject);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (_SourceObject == null) { return; }
            _Object.OnDragEnded();
            _SourceObject = null;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_SourceObject == null) { return; }
            _Object.OnDrag(eventData.pointerCurrentRaycast.worldPosition);
        }
        #endregion Public Methods

        #region Unity Methods
        private void Awake()
        {
            _Object = GetComponent<DragAndDropObject<T>>();
        }

        private void OnDisable()
        {
            //OnEndDrag(default);
            _SourceObject = null;
        }
        #endregion Unity Methods

        #region Private Variables
        private DragAndDropObject<T> _Object;
        private static DragAndDropObject<T> _SourceObject;
        #endregion Private Variables
    }
}