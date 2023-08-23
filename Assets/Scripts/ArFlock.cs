/*
ArFlock.cs - Flocking behaviour for an entire flock for ARpoise.

    The code is derived from the video
    https://www.youtube.com/watch?v=a7GkPNMGz8Y
    by Holistic3d, aka Professor Penny de Byl.

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
using UnityEngine;

public class ArFlock : MonoBehaviour
{
    #region Parameters
    private float? _neighbourDistance;
    public float NeighbourDistance
    {
        get => _neighbourDistance.HasValue ? _neighbourDistance.Value : 2.5f;
        set => _neighbourDistance = value;
    }

    private float? _minNeighbourDistance;
    public float MinNeighbourDistance
    {
        get => _minNeighbourDistance.HasValue ? _minNeighbourDistance.Value : 1.8f;
        set => _minNeighbourDistance = value;
    }

    private float? _speedFactor;
    public float SpeedFactor
    {
        get => _speedFactor.HasValue ? _speedFactor.Value : 1f;
        set => _speedFactor = value;
    }

    private float? _rotationSpeed;
    public float RotationSpeed
    {
        get => _rotationSpeed.HasValue ? _rotationSpeed.Value : 4f;
        set => _rotationSpeed = value;
    }

    private float? _minimumSpeed;
    public float MinimumSpeed
    {
        get => _minimumSpeed.HasValue ? _minimumSpeed.Value : .7f;
        set => _minimumSpeed = value;
    }

    private float? _maximumSpeed;
    public float MaximumSpeed
    {
        get => _maximumSpeed.HasValue ? _maximumSpeed.Value : 2f;
        set => _maximumSpeed = value;
    }

    private float? _applyRulesPercentage;
    public float ApplyRulesPercentage
    {
        get => _applyRulesPercentage.HasValue ? _applyRulesPercentage.Value : 20f;
        set => _applyRulesPercentage = value;
    }

    public virtual void SetParameter(bool setValue, string label, string value)
    {
        if (label.Equals(nameof(NeighbourDistance)))
        {
            _neighbourDistance = SetParameter(setValue, value, (float?)null);
        }
        else if (label.Equals(nameof(MinNeighbourDistance)))
        {
            _minNeighbourDistance = SetParameter(setValue, value, (float?)null);
        }
        else if (label.Equals(nameof(SpeedFactor)))
        {
            _speedFactor = SetParameter(setValue, value, (float?)null).Value;
        }
        else if (label.Equals(nameof(RotationSpeed)))
        {
            _rotationSpeed = SetParameter(setValue, value, (float?)null).Value;
        }
        else if (label.Equals(nameof(ApplyRulesPercentage)))
        {
            _applyRulesPercentage = SetParameter(setValue, value, (float?)null).Value;
        }
        else if (label.Equals(nameof(MinimumSpeed)))
        {
            _minimumSpeed = SetParameter(setValue, value, (float?)null).Value;
        }
        else if (label.Equals(nameof(MaximumSpeed)))
        {
            _maximumSpeed = SetParameter(setValue, value, (float?)null).Value;
        }
    }

    protected int? SetParameter(bool setValue, string value, int? defaultValue)
    {
        if (setValue && !string.IsNullOrWhiteSpace(value))
        {
            int intValue;
            if (int.TryParse(value, out intValue))
            {
                return intValue;
            }
        }
        return defaultValue;
    }

    protected float? SetParameter(bool setValue, string value, float? defaultValue)
    {
        if (setValue && !string.IsNullOrWhiteSpace(value))
        {
            float floatValue;
            if (float.TryParse(value, out floatValue))
            {
                return floatValue;
            }
        }
        return defaultValue;
    }
    #endregion

    public Vector3 GoalPosition;

    //set the size of the bounding box to keep the fish within.
    //its actual side length will be twice the values given here
    public Vector3 SwimLimits = new Vector3(42, 42, 42);

    private GameObject[] _allFish = null;
    public GameObject[] AllFish { get => _allFish; set => _allFish = value; }

    protected void Start()
    {
        GoalPosition = transform.position;
        RenderSettings.fogColor = Camera.main.backgroundColor;
        RenderSettings.fogDensity = 0.03F;
        RenderSettings.fog = false;
    }
}
