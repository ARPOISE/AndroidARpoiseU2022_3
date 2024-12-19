/*
ArpoiseObjectCrystal.cs - A script handling an 'object crystal' for ARpoise.

Copyright (C) 2024, Tamiko Thiel and Peter Graf - All Rights Reserved

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
using UnityEngine;

public class ArpoiseObjectCrystal : MonoBehaviour
{
    // The prefabs to be crystallized,
    // they have to be set before you create the asset bundle of the crystal
    // and have to be in the same asset bundle as the crystal
    //
    public GameObject CrystalObject0;
    public GameObject CrystalObject1;
    public GameObject CrystalObject2;
    public GameObject CrystalObject3;

    #region Crystal parameters
    public Vector3 LowerLeft = new Vector3(-45, -45, -45);
    public Vector3 UpperRight = new Vector3(45, 45, 45);
    public Vector3 Step = new Vector3(5, 5, 5);
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
    }

    private readonly List<GameObject> _crystalObjects = new List<GameObject>();
    private readonly List<GameObject> _instantiatedCrystalObjects = new List<GameObject>();

    protected void Start()
    {
        foreach (var crystalObject in new[] { CrystalObject0, CrystalObject1, CrystalObject2, CrystalObject3 })
        {
            if (crystalObject != null)
            {
                _crystalObjects.Add(crystalObject);
            }
        }
        if (_crystalObjects.Count < 1 || Step.x <= 0 || Step.y <= 0 || Step.z <= 0)
        {
            return;
        }
        int index = 0;
        for (float x = LowerLeft.x; x <= UpperRight.x; x += Step.x)
        {
            for (float y = LowerLeft.y; y <= UpperRight.y; y += Step.y)
            {
                for (float z = LowerLeft.z; z <= UpperRight.z; z += Step.z)
                {
                    var position = new Vector3(x, y, z);
                    GameObject crystalObject = _crystalObjects[index++ % _crystalObjects.Count];

                    crystalObject = Instantiate(crystalObject, position, crystalObject.transform.rotation, transform);
                    crystalObject.transform.parent = transform;
                    crystalObject.transform.LookAt(Camera.main.transform);

                    _instantiatedCrystalObjects.Add(crystalObject);
                }
            }
        }
    }

    protected void Update()
    {
        var cameraTransform = Camera.main.transform;
        foreach (var crystalObject in _instantiatedCrystalObjects)
        {
            crystalObject.transform.LookAt(cameraTransform);
        }
    }
}
