using System;
using System.Collections.Generic;
using SimFactor.Hermes.Core.Input;
using SimFactor.Hermes.Data;
using SimFactor.Hermes.Utils;
using UnityEngine;
using Zenject;

namespace SimFactor.Hermes.Images
{
    public class ImageDisplayController : MonoBehaviour, IImageDisplayController
    {
        private IScrollInput _input;
        private DisplayInfo _displayInfo;
        
        private bool _initialized;
        
        private IImageScroller[] _imageScrollers;
        private ImageSetGroups _imageSetGroups;
        private int _currentImageSetIndex;
        
        void IImageDisplayController.SetImageSets(ImageSetGroups imageSetGroups)
        {
            if (imageSetGroups.GroupCount > _displayInfo.DisplayCount)
            {
                throw new ArgumentException(
                    $"Cannot display {imageSetGroups.GroupCount} image groups, because there are not enough displays!");
            }

            _imageSetGroups = imageSetGroups;
            _imageScrollers = new IImageScroller[imageSetGroups.GroupCount];

            for (var i = 0; i < imageSetGroups.GroupCount; i++)
            {
                if (imageSetGroups.Groups[i].Count <= 0)
                {
                    continue;
                }
                
                var scrollerSpecificDisplayInfo =
                    new DisplayInfo(new List<DisplayData> {_displayInfo.GetDisplayData(i)}, _displayInfo.DisplayMaxWidth);
                
                _imageScrollers[i] = new ImageScroller(scrollerSpecificDisplayInfo);
                _imageScrollers[i].SetSprites(imageSetGroups.Groups[i][0]);
            }

            _initialized = true;
        }

        void IImageDisplayController.Enable()
        {
            if (!_initialized)
            {
                throw new Exception("Image display controller cannot be enabled before initialization");
            }
            
            AddEventListeners();

            foreach (var imageScroller in _imageScrollers)
            {
                imageScroller?.Enable();
            }
        }

        void IImageDisplayController.Disable()
        {
            if (!_initialized)
            {
                throw new Exception("Image display controller cannot be disabled before initialization");
            }
            
            RemoveEventListeners();
            
            foreach (var imageScroller in _imageScrollers)
            {
                imageScroller?.Disable();
            }
        }

        [Inject]
        private void SetBindings(IScrollInput input, DisplayInfo displayInfo)
        {
            _input = input;
            _displayInfo = displayInfo;
        }

        private void OnImageSetChanged(DirectionType direction)
        {
            var value = direction == DirectionType.Right ? 1 : -1;
            _currentImageSetIndex = SpillOver(_currentImageSetIndex + value,  _imageSetGroups.SetCount);
            
            for (var i = 0; i < _imageSetGroups.GroupCount; i++)
            {
                if (_imageSetGroups.Groups[i].Count > _currentImageSetIndex 
                    && _imageSetGroups.Groups[i][_currentImageSetIndex] != null)
                {
                    _imageScrollers[i]?.SetSprites(_imageSetGroups.Groups[i][_currentImageSetIndex]);
                }
            }
        }

        private void AddEventListeners()
        {
            _input.ImageSetChange += OnImageSetChanged;
            
            foreach (var imageScroller in _imageScrollers)
            {
                if (imageScroller == null)
                {
                    continue;
                }
                
                imageScroller.ScrollEndReached += _input.OnScrollLimitReached;
                imageScroller.ScrollStartReached += _input.OnScrollLimitReached;

                _input.Scroll += imageScroller.Scroll;
            }
        }

        private void RemoveEventListeners()
        {
            _input.ImageSetChange -= OnImageSetChanged;
            
            foreach (var imageScroller in _imageScrollers)
            {
                if (imageScroller == null)
                {
                    continue;
                }
                
                imageScroller.ScrollEndReached -= _input.OnScrollLimitReached;
                imageScroller.ScrollStartReached -= _input.OnScrollLimitReached;

                _input.Scroll -= imageScroller.Scroll;
            }
        }

        private static int SpillOver(int value, int limit)
        {
            while (value < 0)
            {
                value += limit;
            }

            return value % limit;
        }
    }
}