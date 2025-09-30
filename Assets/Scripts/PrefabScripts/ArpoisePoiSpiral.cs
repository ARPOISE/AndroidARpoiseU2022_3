/*
ArpoisePoiSpiral.cs - A script handling a 'poi spiral' for ARpoise.

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

public class ArpoisePoiSpiral : ArpoisePoiStructure
{
    #region Spiral parameters
    public Vector3 Center = new Vector3(0, 0, 10);
    public int Arms = 4;
    public float Diameter = 1f;
    public string Pattern = "Sunflower"; // "Sunflower" or "Galaxy"
    #endregion

    public override void SetParameter(bool setValue, string label, string value)
    {
        if (label.Equals(nameof(Center)))
        {
            Center = ParameterHelper.SetParameter(setValue, value, Center).Value;
        }
        else if (label.Equals(nameof(Arms)))
        {
            Arms = ParameterHelper.SetParameter(setValue, value, Arms).Value;
        }
        else if (label.Equals(nameof(Diameter)))
        {
            Diameter = ParameterHelper.SetParameter(setValue, value, Diameter).Value;
        }
        else if (label.Equals(nameof(Pattern)))
        {
            if (setValue)
            {
                Pattern = value;
            }
        }
        else
        {
            base.SetParameter(setValue, label, value);
        }
    }

    private readonly float GOLDEN_ANGLE = Mathf.PI * (3f - Mathf.Sqrt(5f));

    public List<Vector2> Disk(int N, float R = 1f, Vector2 center = default)
    {
        var pts = new List<Vector2>(N);
        for (int n = 0; n < N; n++)
        {
            float r = R * Mathf.Sqrt((n + 0.5f) / (float)N);
            float t = n * GOLDEN_ANGLE;
            pts.Add(center + new Vector2(Mathf.Cos(t), Mathf.Sin(t)) * r);
        }
        return pts;
    }

    private static float NextGaussian(System.Random rng)
    {
        double u1 = 1.0 - rng.NextDouble();
        double u2 = 1.0 - rng.NextDouble();
        double r = Math.Sqrt(-2.0 * Math.Log(u1));
        double phi = 2.0 * Math.PI * u2;
        return (float)(r * Math.Cos(phi));
    }

    // Sample r from Gamma(k=2, θ=scaleLength) == 2D exponential disk; truncated at Rmax
    private static float SampleExpDiskR(System.Random rng, float scaleLength, float Rmax)
    {
        for (int tries = 0; tries < 100; tries++)
        {
            double u1 = rng.NextDouble();
            double u2 = rng.NextDouble();
            double r = -scaleLength * Math.Log(u1 * u2);
            if (r <= Rmax) return (float)r;
        }
        return Rmax * Mathf.Sqrt((float)rng.NextDouble());
    }

    // Compress minor axis by (1 - barStrength) and rotate by barAngle
    private static Vector2 ApplyBar(Vector2 v, float barStrength, float barAngle)
    {
        // rotate into bar frame
        float c = Mathf.Cos(barAngle), s = Mathf.Sin(barAngle);
        float x = c * v.x + s * v.y;
        float y = -s * v.x + c * v.y;
        y *= (1f - 0.9f * barStrength); // cap compression so it never flattens completely
        // rotate back
        return new Vector2(c * x - s * y, s * x + c * y);
    }

    public struct Params
    {
        public int N;                 // total points (stars)
        public int arms;              // number of spiral arms (2..4 typical)
        public float R;               // outer radius
        public Vector2 center;        // center position

        // Spiral shape
        public float pitchDeg;        // arm pitch angle in degrees (10..30 typical)
        public float innerRadius;     // where arms start (avoid 0)
        public float armWidth;        // angular stddev (radians) around each arm (~0.15..0.35)

        // Radial density (exponential disk with scale length Rd)
        public float scaleLength;     // Rd; ~0.3..0.5 * R

        // Extras
        public float bulgeFraction;   // 0..0.5; fraction of N in central bulge
        public float bulgeSigma;      // bulge stddev (as absolute units)
        public float barStrength;     // 0..0.9; 0=none, 0.6=strong bar
        public float barAngleDeg;     // bar orientation
        public float barRadius;       // bar affects points with r < barRadius

        public int? seed;             // optional RNG seed for repeatability
    }

    public static List<Vector2> Galaxy(Params p)
    {
        // sensible defaults
        if (p.arms <= 0) p.arms = 2;
        if (p.R <= 0f) p.R = 10f;
        if (p.innerRadius <= 0f) p.innerRadius = 0.02f * p.R;
        if (p.pitchDeg <= 0f) p.pitchDeg = 15f;
        if (p.armWidth <= 0f) p.armWidth = 0.25f;         // radians
        if (p.scaleLength <= 0f) p.scaleLength = 0.35f * p.R;
        if (p.bulgeSigma <= 0f) p.bulgeSigma = 0.08f * p.R;
        if (p.barRadius <= 0f) p.barRadius = 0.35f * p.R;
        p.bulgeFraction = Mathf.Clamp01(p.bulgeFraction);
        p.barStrength = Mathf.Clamp01(p.barStrength);

        var rng = p.seed.HasValue ? new System.Random(p.seed.Value) : new System.Random();
        var pts = new List<Vector2>(p.N);

        int nBulge = Mathf.RoundToInt(p.bulgeFraction * p.N);
        int nDisk = Math.Max(0, p.N - nBulge);

        // Precompute constants
        float pitch = p.pitchDeg * Mathf.Deg2Rad;
        float b = Mathf.Tan(pitch);                 // r = a * exp(b * theta), pitch = atan(b)
        if (Mathf.Abs(b) < 1e-4f) b = 1e-4f;        // guard
        float twoPi = 2f * Mathf.PI;

        // --- Bulge (Gaussian 2D) ---
        for (int i = 0; i < nBulge; i++)
        {
            float x = p.bulgeSigma * NextGaussian(rng);
            float y = p.bulgeSigma * NextGaussian(rng);
            var v = new Vector2(x, y);
            // optional mild bar shaping inside barRadius
            if (p.barStrength > 0f && v.magnitude < p.barRadius)
                v = ApplyBar(v, p.barStrength, p.barAngleDeg * Mathf.Deg2Rad);
            pts.Add(p.center + v);
        }

        // --- Disk + Arms ---
        for (int i = 0; i < nDisk; i++)
        {
            // sample radius from 2D exponential disk: p(r) ∝ r * exp(-r/Rd)
            float r = SampleExpDiskR(rng, p.scaleLength, p.R);
            r = Mathf.Max(r, p.innerRadius * 1.0001f);

            // base angle from log-spiral equation
            float thetaBase = Mathf.Log(r / p.innerRadius) / b;

            // pick an arm and add angular scatter (arm thickness)
            int armIndex = rng.Next(p.arms);
            float theta = thetaBase + (armIndex * twoPi / p.arms) + p.armWidth * NextGaussian(rng);

            var v = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * r;

            // optional bar shaping in the inner region
            if (p.barStrength > 0f && r < p.barRadius)
                v = ApplyBar(v, p.barStrength, p.barAngleDeg * Mathf.Deg2Rad);

            pts.Add(p.center + v);
        }

        return pts;
    }

    protected override void Update()
    {
        if (gameObject.activeSelf)
        {
            if (ArObjects is null)
            {
                SeedRandom(GetInstanceID());
                ArObjects = new List<ArObject>();

                if (Pois.Count > 0)
                {
                    var poi = Pois[Random.Next(Pois.Count)];
                    var poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
                    var arObjectState = ArBehaviour != null ? ArBehaviour.ArObjectState : null;
                    if (arObjectState is null || poiObject is null || MaxNofObjects <= 0)
                    {
                        return;
                    }

                    var points = new List<Vector2>();
                    if (Pattern.Equals("Galaxy", StringComparison.OrdinalIgnoreCase))
                    {
                        points = Galaxy(new Params
                        {
                            center = new Vector2(Center.x, Center.y),
                            N = MaxNofObjects,
                            R = 0.5f * Diameter * Mathf.Sqrt(MaxNofObjects),
                            arms = Arms
                        });
                    }
                    else
                    {
                        points = Disk(MaxNofObjects, 0.5f * Diameter * Mathf.Sqrt(MaxNofObjects));
                    }

                    for (int i = 0; i < MaxNofObjects; i++)
                    {
                        poi = Pois[Random.Next(Pois.Count)];
                        poiObject = ArBehaviour?.AvailableCrystalObjects?.Find(x => x.poi.title == poi);
                        if (poiObject is not null)
                        {
                            var position = new Vector3(Center.x + points[i].x, Center.y + points[i].y, Center.z);

                            var result = ArBehaviour.CreateArObject(
                                arObjectState,
                                poiObject.gameObject,
                                null,
                                transform,
                                poiObject.poi,
                                poiObject.poi.id,
                                out var spiralObject,
                                out var arObject
                                );

                            if (spiralObject != null)
                            {
                                if (!spiralObject.activeSelf)
                                {
                                    spiralObject.SetActive(true);
                                }
                                spiralObject.transform.localPosition = position;
                            }
                            if (arObject != null)
                            {
                                Add(arObject);
                            }
                        }
                    }
                    Fade(); // Set the initial fade value
                }
            }
        }
        else
        {
            base.Update();
        }
    }
}
