/*
ArObjectState.cs - ArObject state for ARpoise.

Copyright (C) 2018, Tamiko Thiel and Peter Graf - All Rights Reserved

ARpoise - Augmented Reality point of interest service environment 

This file is part of ARpoise.

    ARpoise is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    ARpoise is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with ARpoise.  If not, see <https://www.gnu.org/licenses/>.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
ARpoise, see www.ARpoise.com/

*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public class ArObjectState
    {
        public volatile bool IsDirty = false;

        private readonly List<ArAnimation> _onCreateAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _onFollowAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _onFocusAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _inFocusAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _onClickAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _billboardAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _inMinutesAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _whenActiveAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _whenActivatedAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _whenDeactivatedAnimations = new List<ArAnimation>();
        private readonly List<ArAnimation> _onRandomAnimations = new List<ArAnimation>();

        private ArAnimation[] _allAnimations = null;
        private ArAnimation[] AllAnimations
        {
            get
            {
                if (_allAnimations == null)
                {
                    var allAnimations = new List<ArAnimation>();
                    allAnimations.AddRange(_onCreateAnimations);
                    allAnimations.AddRange(_onFollowAnimations);
                    allAnimations.AddRange(_onFocusAnimations);
                    allAnimations.AddRange(_inFocusAnimations);
                    allAnimations.AddRange(_onClickAnimations);
                    allAnimations.AddRange(_inMinutesAnimations);
                    allAnimations.AddRange(_whenActiveAnimations);
                    allAnimations.AddRange(_whenActivatedAnimations);
                    allAnimations.AddRange(_whenDeactivatedAnimations);
                    allAnimations.AddRange(_onRandomAnimations);
                    _allAnimations = allAnimations.ToArray();
                    _animationsWithName = null;
                }
                return _allAnimations;
            }
            set
            {
                _allAnimations = value;
                _animationsWithName = null;
            }
        }
        private ArAnimation[] _animationsWithName = null;
        public ArAnimation[] AnimationsWithName
        {
            get
            {
                if (_animationsWithName == null)
                {
                    _animationsWithName = AllAnimations.Where(x => !string.IsNullOrWhiteSpace(x.Name) && !x.Name.StartsWith(nameof(ArAnimation.RandomDelay))).ToArray();
                }
                return _animationsWithName;
            }
        }
        private readonly List<ArObject> _arObjects = new List<ArObject>();
        public IEnumerable<ArObject> ArObjects { get { return _arObjects; } }
        public List<ArObject> ArObjectsToDelete { get; private set; }
        public List<ArObject> ArObjectsToPlace { get; private set; }
        public List<ArObject> ArObjectsRelative { get; private set; }
        public List<Poi> ArPois { get; private set; }
        public static HashSet<long> PoisToActivate { get; private set; }
        public static HashSet<long> PoisToDeactivate { get; private set; }

        public ArObjectState()
        {
            ArObjectsToDelete = new List<ArObject>();
            ArObjectsToPlace = null;
            ArObjectsRelative = null;
            ArPois = new List<Poi>();
            PoisToActivate = new HashSet<long>();
            PoisToDeactivate = new HashSet<long>();
        }

        public void SetArObjectsToPlace()
        {
            var arObjectsToPlace = new HashSet<ArObject>(ArObjects.Where(x => !x.IsRelative));
            var queue = new Queue<ArObject>(arObjectsToPlace);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var child in current.ArObjects.Where(x => !x.IsRelative && !arObjectsToPlace.Contains(x)))
                {
                    arObjectsToPlace.Add(child);
                    queue.Enqueue(child);
                }
            }

            ArObjectsToPlace = arObjectsToPlace.ToList();
            ArObjectsRelative = ArObjects.Where(x => x.IsRelative).ToList();
        }

        public void AddOnCreateAnimation(ArAnimation animation)
        {
            _onCreateAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddOnFollowAnimation(ArAnimation animation)
        {
            _onFollowAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddOnFocusAnimation(ArAnimation animation)
        {
            _onFocusAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddOnRandomAnimation(ArAnimation animation)
        {
            _onRandomAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddInFocusAnimation(ArAnimation animation)
        {
            _inFocusAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddInMinutesAnimation(ArAnimation animation)
        {
            _inMinutesAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddWhenActiveAnimation(ArAnimation animation)
        {
            _whenActiveAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddWhenActivatedAnimation(ArAnimation animation)
        {
            //Debug.Log($"_whenActivatedAnimations '{animation.Name}', PoiId {animation.PoiId} added");
            _whenActivatedAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddWhenDeactivatedAnimation(ArAnimation animation)
        {
            //Debug.Log($"_whenDeactivatedAnimations '{animation.Name}', PoiId {animation.PoiId} added");
            _whenDeactivatedAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddOnClickAnimation(ArAnimation animation)
        {
            _onClickAnimations.Add(animation);
            AllAnimations = null;
        }

        public void AddBillboardAnimation(ArAnimation animation)
        {
            _billboardAnimations.Add(animation);
            AllAnimations = null;
        }

        private void RemoveFromAnimations(ArObject arObject)
        {
            _billboardAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _onCreateAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _onFollowAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _onFocusAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _inFocusAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _onClickAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _inMinutesAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _whenActiveAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _whenActivatedAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _whenDeactivatedAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            _onRandomAnimations.RemoveAll(x => arObject.Id == x.PoiId);
            AllAnimations = null;
        }

        public void Add(ArObject arObject)
        {
            _arObjects.Add(arObject);
        }

        public void DestroyArObjects(List<ArObject> arObjects)
        {
            foreach (var arObject in arObjects)
            {
                RemoveFromAnimations(arObject);
                foreach (var child in arObject.ArObjects)
                {
                    RemoveFromAnimations(child);
                }
                _arObjects.Remove(arObject);
                UnityEngine.Object.Destroy(arObject.WrapperObject);
                arObject.WrapperObject = null;
            }
            SetArObjectsToPlace();
        }

        public void DestroyArObject(ArObject arObject)
        {
            RemoveFromAnimations(arObject);
            foreach (var child in arObject.ArObjects)
            {
                RemoveFromAnimations(child);
            }
            _arObjects.Remove(arObject);
            UnityEngine.Object.Destroy(arObject.WrapperObject);
            arObject.WrapperObject = null;
            SetArObjectsToPlace();
        }

        public void DestroyArObjects()
        {
            foreach (var arObject in ArObjectsToDelete)
            {
                DestroyArObject(arObject);
            }
            ArObjectsToDelete.Clear();
        }

        public int Count => _arObjects.Count;

        public int CountArObjects(List<ArObject> arObjects = null)
        {
            if (arObjects == null)
            {
                arObjects = _arObjects;
            }
            var result = arObjects.Count;
            foreach (var arObject in arObjects)
            {
                result += CountArObjects(arObject.ArObjects);
            }
            return result;
        }

        public int NumberOfAnimations => AllAnimations.Length;

        public int NumberOfActiveAnimations => AllAnimations.Where(x => x.IsActive).Count();

        public bool RemoteActivate(string animationName, long startTicks, long nowTicks)
        {
            bool rc = false;
            foreach (var animation in AnimationsWithName.Where(x => animationName == x.Name))
            {
                if (!animation.IsActive)
                {
                    rc = true;
                    animation.Activate(startTicks, nowTicks, true);
                }
            }
            return rc;
        }

        public bool HandleAnimations(ArBehaviourArObject arBehaviour, long startTicks, long nowTicks)
        {
            if (_billboardAnimations.Count > 0)
            {
                Transform transform;
                foreach (var arAnimation in _billboardAnimations)
                {
                    var wrapper = arAnimation.Wrapper;
                    if (wrapper != null && (transform = wrapper.transform) != null)
                    {
                        transform.LookAt(Camera.main.transform);
                    }
                }
            }

            HashSet<ArAnimation> inFocusAnimationsToStop = null;
            if (_onFocusAnimations.Count > 0 || _inFocusAnimations.Count > 0)
            {
                inFocusAnimationsToStop = new HashSet<ArAnimation>(_inFocusAnimations.Where(x => x.IsActive));
                var ray = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0f));

                RaycastHit[] raycastHits = Physics.RaycastAll(ray, Mathf.Infinity);
                for (int i = 0; i < raycastHits.Length; i++)
                {
                    var objectHit = raycastHits[i].transform.gameObject;
                    if (objectHit != null)
                    {
                        foreach (var arAnimation in _onFocusAnimations.Where(x => objectHit.Equals(x.AnimatedObject)))
                        {
                            if (!arAnimation.IsActive)
                            {
                                arAnimation.Activate(startTicks, nowTicks);
                            }
                        }

                        foreach (var arAnimation in _inFocusAnimations.Where(x => objectHit.Equals(x.AnimatedObject)))
                        {
                            if (!arAnimation.IsActive)
                            {
                                arAnimation.Activate(startTicks, nowTicks);
                            }
                            inFocusAnimationsToStop.Remove(arAnimation);
                        }
                    }
                }
            }

            HashSet<ArAnimation> inMinutesAnimationsToStop = null;
            if (_inMinutesAnimations.Count > 0)
            {
                inMinutesAnimationsToStop = new HashSet<ArAnimation>(_inMinutesAnimations.Where(x => x.IsActive));
                foreach (var arAnimation in _inMinutesAnimations.Where(x => x.ShouldBeActive()))
                {
                    if (!arAnimation.IsActive)
                    {
                        arAnimation.Activate(startTicks, nowTicks);
                    }
                    inMinutesAnimationsToStop.Remove(arAnimation);
                }
            }

            HashSet<ArAnimation> whenActiveAnimationsToStop = null;
            if (_whenActiveAnimations.Count > 0)
            {
                whenActiveAnimationsToStop = new HashSet<ArAnimation>(_whenActiveAnimations.Where(x => x.IsActive));
                foreach (var arAnimation in _whenActiveAnimations)
                {
                    if (arAnimation.AnimatedObject.activeSelf)
                    {
                        whenActiveAnimationsToStop.Remove(arAnimation);
                    }
                }
            }

            var hasHit = false;
            if (_onClickAnimations.Count > 0 && Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit[] raycastHits = Physics.RaycastAll(ray, Mathf.Infinity);
                for (int i = 0; i < raycastHits.Length; i++)
                {
                    var objectHit = raycastHits[i].transform.gameObject;
                    if (objectHit != null)
                    {
                        foreach (var arAnimation in _onClickAnimations.Where(x => objectHit.Equals(x.AnimatedObject)))
                        {
                            hasHit = true;
                            if (!arAnimation.IsActive)
                            {
                                arAnimation.Activate(startTicks, nowTicks);
                            }
                        }
                    }
                }
            }

            if (_whenActivatedAnimations.Count > 0)
            {
                foreach (var arAnimation in _whenActivatedAnimations)
                {
                    if (PoisToActivate.Contains(arAnimation.PoiId))
                    {
                        var deactivation = _whenDeactivatedAnimations.FirstOrDefault(x => x.IsActive && x.PoiId == arAnimation.PoiId);
                        if (deactivation != null)
                        {
                            continue;
                        }
                        PoisToActivate.Remove(arAnimation.PoiId);
                        if (!arAnimation.IsActive)
                        {
                            var arGameObject = arAnimation.AnimatedObject;
                            if (arGameObject != null && !arGameObject.activeSelf)
                            {
                                arGameObject.SetActive(true);
                            }
                            //Debug.Log($"Animation {arAnimation.Name}, PoiId {arAnimation.PoiId} is being activated");
                            arAnimation.Activate(startTicks, nowTicks);
                        }
                    }
                }
            }

            if (_whenDeactivatedAnimations.Count > 0)
            {
                foreach (var arAnimation in _whenDeactivatedAnimations)
                {
                    if (PoisToDeactivate.Contains(arAnimation.PoiId))
                    {
                        PoisToDeactivate.Remove(arAnimation.PoiId);
                        var arGameObject = arAnimation.AnimatedObject;
                        if (!arAnimation.IsActive && arGameObject != null && arGameObject.activeSelf)
                        {
                            //Debug.Log($"Animation {arAnimation.Name}, PoiId {arAnimation.PoiId} is being activated");
                            arAnimation.Activate(startTicks, nowTicks);
                        }
                    }
                }
            }

            if (_onRandomAnimations.Count > 0)
            {
                foreach (var arAnimation in _onRandomAnimations)
                {
                    if (!arAnimation.IsActive && nowTicks > arAnimation.NextActivation.Ticks)
                    {
                        arAnimation.Activate(startTicks, nowTicks);
                    }
                }
            }

            var isToBeDestroyed = false;
            var animations = AllAnimations;
            var animationsWithName = AnimationsWithName;
            for (int i = 0; i < animations.Length; i++)
            {
                var animation = animations[i];
                if (inFocusAnimationsToStop != null && inFocusAnimationsToStop.Contains(animation))
                {
                    animation.Stop(startTicks, nowTicks);
                    inFocusAnimationsToStop.Remove(animation);
                }
                else if (inMinutesAnimationsToStop != null && inMinutesAnimationsToStop.Contains(animation))
                {
                    animation.Stop(startTicks, nowTicks);
                    inMinutesAnimationsToStop.Remove(animation);
                }
                else if (whenActiveAnimationsToStop != null && whenActiveAnimationsToStop.Contains(animation))
                {
                    animation.Stop(startTicks, nowTicks);
                    whenActiveAnimationsToStop.Remove(animation);
                }
                else
                {
                    animation.Animate(startTicks, nowTicks);
                }

                if (animation.JustStopped)
                {
                    foreach (var animationName in animation.FollowedBy)
                    {
                        if (nameof(RefreshRequest.ReloadLayerData).Equals(animationName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var refreshRequest = new RefreshRequest() { layerName = nameof(RefreshRequest.ReloadLayerData) };
                            arBehaviour.RequestRefresh(refreshRequest);
                            break;
                        }
                        if (animation.HandleOpenUrl(animationName))
                        {
                            continue;
                        }
                        if (animation.HandleSetActive(animationName, true))
                        {
                            continue;
                        }
                        foreach (var animationToFollow in animationsWithName.Where(x => animationName == x.Name))
                        {
                            if (!animationToFollow.IsActive)
                            {
                                if (animationToFollow.ArEventType != ArEventType.WhenActive || animationToFollow.AnimatedObject.activeSelf)
                                {
                                    animationToFollow.Activate(startTicks, nowTicks);
                                }
                            }
                        }
                    }
                }
                if (!isToBeDestroyed && animation.IsToBeDestroyed)
                {
                    isToBeDestroyed = true;
                }
            }

            if (isToBeDestroyed)
            {
                var toBeDestroyed = animations.Where(x => x.IsToBeDestroyed).ToArray();
                foreach (var arAnimation in toBeDestroyed)
                {
                    var arObject = ArObjects.FirstOrDefault(x => x.Id == arAnimation.PoiId);
                    if (arObject != null)
                    {
                        DestroyArObject(arObject);
                    }
                }
            }

            if (!hasHit && Input.GetMouseButtonDown(0))
            {
                arBehaviour.TakeScreenshot = true;
            }
            return hasHit;
        }

        public List<ArObject> ArObjectsToBeDuplicated()
        {
            List<ArObject> result = null;
            foreach (var arAnimation in AllAnimations.Where(x => x.IsToBeDuplicated))
            {
                arAnimation.IsToBeDuplicated = false;
                foreach (var arObject in ArObjects.Where(x => x.Id == arAnimation.PoiId))
                {
                    if (result == null)
                    {
                        result = new List<ArObject>();
                    }
                    result.Add(arObject);
                }
            }
            return result?.Distinct().ToList();
        }

        public void HandleApplicationSleep(bool shouldSleep)
        {
            foreach (var arAnimation in AllAnimations)
            {
                arAnimation.HandleApplicationSleep(shouldSleep);
            }
        }

        public void Replace(GameObject oldObject, GameObject newObject)
        {
            foreach (var arAnimation in AllAnimations.Where(x => x.AnimatedObject == oldObject))
            {
                arAnimation.Replace(oldObject, newObject);
            }
        }
    }
}
