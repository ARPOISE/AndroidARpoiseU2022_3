/*
ArpoisePoiCrystal.cs - A script handling an 'poi crystal' for ARpoise.

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
using System.Collections.Generic;
using UnityEngine;

public class ArpoisePoiCrystal : MonoBehaviour
{
    public ArBehaviourArObject ArBehaviour { get; set; }

    #region Crystal parameters
    public Vector3 LowerLeft = new Vector3(-45, -45, -45);
    public Vector3 UpperRight = new Vector3(45, 45, 45);
    public Vector3 Step = new Vector3(15, 15, 15);
    public string Poi = string.Empty;
    #endregion

    public void SetParameter(bool setValue, string label, string value)
    {
        if (label.Equals(nameof(LowerLeft)))
        {
            LowerLeft = ParameterHelper.SetParameter(setValue, value, LowerLeft).Value;
        }
        else if (label.Equals(nameof(UpperRight)))
        {
            UpperRight = ParameterHelper.SetParameter(setValue, value, UpperRight).Value;
        }
        else if (label.Equals(nameof(Step)))
        {
            Step = ParameterHelper.SetParameter(setValue, value, Step).Value;
        }
        else if (label.Equals(nameof(Poi)))
        {
            if (setValue)
            {
                Poi = value;
            }
        }
    }

    private bool _firstUpdate = true;

    protected void Start()
    {
    }

    public void CallUpdate()
    {
        Update();
    }
    protected void Update()
    {
        if (_firstUpdate)
        {
            _firstUpdate = false;

            var triggerObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == Poi);
            var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
            if (arObjectState is null || triggerObject is null || Step.x <= 0 || Step.y <= 0 || Step.z <= 0)
            {
                return;
            }

            for (float x = LowerLeft.x; x <= UpperRight.x; x += Step.x)
            {
                for (float y = LowerLeft.y; y <= UpperRight.y; y += Step.y)
                {
                    for (float z = LowerLeft.z; z <= UpperRight.z; z += Step.z)
                    {
                        var position = new Vector3(x, y, z);

                        var result = ArBehaviour.CreateArObject(
                            arObjectState,
                            triggerObject.gameObject,
                            null,
                            transform,
                            triggerObject.poi,
                            triggerObject.poi.id,
                            out var gameObject
                            );

                        if (gameObject != null)
                        {
                            if (!gameObject.activeSelf)
                            {
                                gameObject.SetActive(true);
                            }
                            gameObject.transform.position = position;
                        }
                    }
                }
            }
        }
    }
}
