//-----------------------------------------------------------------------
// <copyright file="AugmentedImageVisualizer.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

/*
ArvosVisualizer.cs - MonoBehaviour for handling detected image triggers of the ArFoundation version of image trigger ARpoise.

This file is part of ARpoise.

This file is derived from image trigger example of the Google ARCore SDK for Unity

https://github.com/google-ar/arcore-unity-sdk

The license of the original file is shown above.

For more information on 

Tamiko Thiel, see www.TamikoThiel.com/
Peter Graf, see www.mission-base.com/peter/
ARpoise, see www.ARpoise.com/

*/

namespace com.arpoise.arpoiseapp
{
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;

    public class ArvosVisualizer : MonoBehaviour
    {
        /// <summary>
        /// The AugmentedImage to visualize.
        /// </summary>
        public ARTrackable Image;

        /// <summary>
        /// The hit pose use to place the TriggerObject.
        /// </summary>
        public Pose? Pose = null;

        /// <summary>
        /// The object to visualize.
        /// </summary>
        public TriggerObject TriggerObject { get; set; }

        /// <summary>
        /// The behaviour.
        /// </summary>
        public ArBehaviourImage ArBehaviour { get; set; }

        public bool HasTimedOut { get; set; }

        private GameObject _gameObject = null;
        private bool _gameObjectCreated = false;
        private bool _first = true;

        public void SetActive()
        {
            if (_gameObject != null && !IsActive)
            {
                _gameObject.SetActive(true);

                var poi = TriggerObject?.poi;
                if (poi != null && poi?.animations?.whenActivated != null && poi.animations.whenActivated.Length > 0)
                {
                    ArObjectState.PoisToActivate.Add(TriggerObject.poi.id);
                    //Debug.Log($"Poi {TriggerObject.poi.id} is to be activated");
                }
            }
        }
        public void SetInActive()
        {
            if (_gameObject != null && IsActive)
            {
                var poi = TriggerObject?.poi;
                if (poi != null && poi?.animations?.whenDeactivated != null && poi.animations.whenDeactivated.Length > 0)
                {
                    ArObjectState.PoisToDeactivate.Add(poi.id);
                    //Debug.Log($"Animation '{poi.animations.whenDeactivated[0].name}', PoiId {poi.id}, is to be deactivated");
                }
                else
                {
                    _gameObject.SetActive(false);
                }
            }
        }

        //private long _lastSecond = DateTime.Now.Second;

        public bool IsActive
        {
            get
            {
                var arGameObject = _gameObject;
                while (arGameObject != null)
                {
                    if (!arGameObject.activeSelf)
                    {
                        //if (_lastSecond != DateTime.Now.Second)
                        //{
                        //    _lastSecond = DateTime.Now.Second;
                        //    Debug.Log($"Inactive '{arGameObject.name}'");
                        //}
                        return false;
                    }
                    if (arGameObject.transform.childCount == 0)
                    {
                        //if (_lastSecond != DateTime.Now.Second)
                        //{
                        //    _lastSecond = DateTime.Now.Second;
                        //    Debug.Log($"Active '{arGameObject.name}'");
                        //}
                        return true;
                    }
                    arGameObject = arGameObject.transform.GetChild(0).gameObject;
                }
                return true;
            }
        }

        public void Update()
        {
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState != null && TriggerObject != null && !_gameObjectCreated)
            {
                _gameObjectCreated = true;
                SetTransform();

                var result = ArBehaviour.CreateArObject(
                    arObjectState,
                    TriggerObject.gameObject,
                    null,
                    transform,
                    TriggerObject.poi,
                    TriggerObject.poi.id,
                    out _gameObject
                    );
                if (!string.IsNullOrWhiteSpace(result))
                {
                    ArBehaviour.ErrorMessage = result;
                    return;
                }
                if (_gameObject != null && !_gameObject.activeSelf)
                {
                    _gameObject.SetActive(true);
                }
            }
            if (_gameObject != null)
            {
                if (Pose != null)
                {
                    if (_first)
                    {
                        _first = false;
                        _gameObject.transform.position = Pose.Value.position;
                        _gameObject.transform.rotation = Pose.Value.rotation;
                    }
                }
                else
                {
                    Vector3 targetPosition = Image.transform.position;
                    Quaternion targetRotation = Image.transform.rotation;

                    var positionLerpFactor = TriggerObject?.poi?.PositionLerpFactor;
                    var rotationLerpFactor = TriggerObject?.poi?.RotationLerpFactor;

                    //Debug.Log($"Lerp factor {positionLerpFactor} {rotationLerpFactor} to position {ParameterHelper.ToString(targetPosition)}");

                    if (positionLerpFactor.HasValue && positionLerpFactor > 0
                        || rotationLerpFactor.HasValue && rotationLerpFactor > 0)
                    {
                        if (_first || HasTimedOut)
                        {
                            SetInitialTransform(targetPosition, targetRotation);
                        }
                        else
                        {
                            LerpTransform(targetPosition, targetRotation, positionLerpFactor, rotationLerpFactor);
                        }
                    }
                    else
                    {
                        _gameObject.transform.position = targetPosition;
                        _gameObject.transform.rotation = targetRotation;
                    }
                    HasTimedOut = _first = false;
                }
            }
        }

        private void SetTransform()
        {
            if (Pose != null)
            {
                transform.position = Pose.Value.position;
                transform.rotation = Pose.Value.rotation;
            }
            else
            {
                var pos = Image.transform.position;
                //Debug.Log($"Position update {pos.x.ToString("F1")}, {pos.y.ToString("F1")}, {pos.z.ToString("F1")}");

                transform.position = pos;
                transform.rotation = Image.transform.rotation;
            }
        }

        private void SetInitialTransform(Vector3 targetPosition, Quaternion targetRotation)
        {
            var targetPositionFactor = TriggerObject?.poi?.TargetPositionFactor;
            if (targetPositionFactor.HasValue && targetPositionFactor.Value > 0)
            {
                _gameObject.transform.position = targetPositionFactor.Value * (targetPosition - Camera.main.transform.position);
            }
            else
            {
                _gameObject.transform.position = targetPosition;
            }
            //Debug.Log($"Position is {ParameterHelper.ToString(_gameObject.transform.position)}");
            _gameObject.transform.rotation = targetRotation;
        }

        private void LerpTransform(Vector3 targetPosition, Quaternion targetRotation, float? positionLerpFactor, float? rotationLerpFactor)
        {
            //Debug.Log($"Lerp factor {positionLerpFactor} to position {ParameterHelper.ToString(targetPosition)}");

            if (positionLerpFactor.HasValue && positionLerpFactor > 0)
            {
                _gameObject.transform.position = Vector3.Lerp(_gameObject.transform.position, targetPosition, positionLerpFactor.Value / ArBehaviourArObject.FramesPerSecond);
            }
            if (rotationLerpFactor.HasValue && rotationLerpFactor > 0)
            {
                _gameObject.transform.rotation = Quaternion.Lerp(_gameObject.transform.rotation, targetRotation, rotationLerpFactor.Value / ArBehaviourArObject.FramesPerSecond);
            }
        }
    }
}
