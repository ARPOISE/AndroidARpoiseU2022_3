/*
ArpoisePoiRain.cs - A script handling a 'poi rain' for ARpoise.

Copyright (C) 2025, Tamiko Thiel and Peter Graf - All Rights Reserved

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
using com.arpoise.arpoiseapp;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ArpoisePoiRain : ArpoisePoiStructure
{
    #region Rain parameters
    public int OffsetX = 0; // Move the center of the rain in X direction
    public int OffsetZ = 0; // Move the center of the rain in Z direction
    public float DropHeight = 5; // Height to drop from, some random component is added below
    public float AreaSize = 10; // The dimensions of the area the rain happens in
    public int StartSecond = 0; // The number of seconds after the load of the layer to start
    public int EndSecond = 600; // The number of seconds to run after the start
    public float NumberOfNewRainObjectsPerSecond = 2.5f; // How many objects are to be created every second
    #endregion

    public override void SetParameter(bool setValue, string label, string value)
    {
        if (label.Equals(nameof(OffsetX)))
        {
            OffsetX = ParameterHelper.SetParameter(setValue, value, OffsetX).Value;
        }
        else if (label.Equals(nameof(OffsetZ)))
        {
            OffsetZ = ParameterHelper.SetParameter(setValue, value, OffsetZ).Value;
        }
        else if (label.Equals(nameof(DropHeight)))
        {
            DropHeight = ParameterHelper.SetParameter(setValue, value, DropHeight).Value;
        }
        else if (label.Equals(nameof(AreaSize)))
        {
            AreaSize = ParameterHelper.SetParameter(setValue, value, AreaSize).Value;
        }
        else if (label.Equals(nameof(StartSecond)))
        {
            StartSecond = ParameterHelper.SetParameter(setValue, value, StartSecond).Value;
        }
        else if (label.Equals(nameof(EndSecond)))
        {
            EndSecond = ParameterHelper.SetParameter(setValue, value, EndSecond).Value;
        }
        else if (label.Equals(nameof(NumberOfNewRainObjectsPerSecond)))
        {
            NumberOfNewRainObjectsPerSecond = ParameterHelper.SetParameter(setValue, value, NumberOfNewRainObjectsPerSecond).Value;
        }
        else
        {
            base.SetParameter(setValue, label, value);
        }
    }

    private long _millisecondAtStart;

    protected void Start()
    {
        _millisecondAtStart = DateTime.Now.Ticks / 10000;
    }

    protected override void Update()
    {
        if (gameObject.activeSelf)
        {
            var doFade = false;
            if (ArObjects is null)
            {
                SeedRandom(GetInstanceID());
                ArObjects = new List<ArObject>();
                doFade = true;
            }

            if (Pois.Count > 0)
            {
                long millisecond = DateTime.Now.Ticks / 10000 - _millisecondAtStart;
                if (millisecond / 1000 < StartSecond)
                {
                    return;
                }
                if (millisecond / 1000 >= EndSecond)
                {
                    return;
                }

                var expectedRainObjects = (millisecond - StartSecond * 1000) * NumberOfNewRainObjectsPerSecond / 1000;
                while (ArObjects.Count < expectedRainObjects && (MaxNofObjects < 0 || ArObjects.Count < MaxNofObjects))
                {
                    var poi = Pois[Random.Next(Pois.Count)];
                    var poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
                    var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
                    if (arObjectState is null || poiObject is null)
                    {
                        return;
                    }

                    Vector3 position = new Vector3(
                    UnityEngine.Random.Range(-1000 * AreaSize / 2, 1000 * AreaSize / 2) / 1000.0f + OffsetX,
                    UnityEngine.Random.Range(-1000 * DropHeight / 5, 1000 * DropHeight / 5) / 1000.0f + DropHeight,
                    UnityEngine.Random.Range(-1000 * AreaSize / 2, 1000 * AreaSize / 2) / 1000.0f + OffsetZ
                    );

                    var result = ArBehaviour.CreateArObject(
                        arObjectState,
                        poiObject.gameObject,
                        null,
                        transform,
                        poiObject.poi,
                        ArBehaviourArObject.ArObjectId,
                        out var rainObject,
                        out var arObject
                        );

                    if (rainObject != null)
                    {
                        if (!rainObject.activeSelf)
                        {
                            rainObject.SetActive(true);
                        }
                        rainObject.transform.localPosition = position;
                    }
                    if (arObject != null)
                    {
                        Add(arObject);
                    }
                }
                if (doFade)
                {
                    Fade(); // Set the initial fade value
                }
            }
        }

        base.Update();
    }
}
