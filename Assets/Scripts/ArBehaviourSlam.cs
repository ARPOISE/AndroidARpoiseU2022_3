/*
ArBehaviourSlam.cs - MonoBehaviour for ARpoise - Simultaneous localization and mapping (SLAM) - handling.

Copyright (C) 2019, Tamiko Thiel and Peter Graf - All Rights Reserved

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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public class ArBehaviourSlam : ArBehaviourImage
    {
        private readonly List<GameObject> _imageSceneObjects = new List<GameObject>();
        private readonly List<GameObject> _slamSceneObjects = new List<GameObject>();
        public readonly List<TriggerObject> VisualizedSlamObjects = new List<TriggerObject>();

        #region Start
        protected override void Start()
        {
            base.Start();
        }
        #endregion

        public List<TriggerObject> AvailableSlamObjects
        {
            get
            {
                var result = new List<TriggerObject>();

                foreach (var slamObject in SlamObjects.Where(x => x.poi != null && x.layerWebUrl == LayerWebUrl))
                {
                    var maximumCount = slamObject.poi.MaximumCount;
                    if (maximumCount > 0)
                    {
                        var count = VisualizedSlamObjects.Where(x => x.poi != null && x.poi.id == slamObject.poi.id).Count();
                        if (count >= maximumCount)
                        {
                            continue;
                        }
                    }
                    result.Add(slamObject);
                }
                return result;
            }
        }

        public string AllAugmentsPlaced
        {
            get
            {
                foreach (var slamObject in SlamObjects.Where(x => x.poi != null && x.layerWebUrl == LayerWebUrl))
                {
                    var allAugmentsPlaced = slamObject.poi.AllAugmentsPlaced;
                    if (!string.IsNullOrEmpty(allAugmentsPlaced))
                    {
                        return allAugmentsPlaced;
                    }
                }
                return "All augments placed.";
            }
        }

        public string RequestedDetectionMode
        {
            get
            {
                foreach (var slamObject in SlamObjects.Where(x => x.poi != null && x.layerWebUrl == LayerWebUrl))
                {
                    var requestedDetectionMode = slamObject.poi.RequestedDetectionMode;
                    if (!string.IsNullOrEmpty(requestedDetectionMode))
                    {
                        return requestedDetectionMode;
                    }
                }
                return null;
            }
        }

        #region Update
        protected override void Update()
        {
            base.Update();
            var slamObjectsAvailable = AvailableSlamObjects.Any();
            foreach (var sceneObject in _imageSceneObjects)
            {
                var active = !IsSlam && HasTriggerImages;
                if (sceneObject != null && sceneObject.activeSelf != active)
                {
                    sceneObject.SetActive(active);
                    //Debug.Log($"{sceneObject.name} {active}");
                }
            }
            foreach (var sceneObject in _slamSceneObjects)
            {
                if (sceneObject != null && sceneObject.activeSelf != slamObjectsAvailable)
                {
                    sceneObject.SetActive(slamObjectsAvailable);
                }
            }
        }
        #endregion
    }
}
