﻿/*
ArBehaviourPosition.cs - MonoBehaviour for ARpoise position handling.

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
using System.Collections;
using System.Linq;
using UnityEngine;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

#if HAS_AR_FOUNDATION
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
#endif

namespace com.arpoise.arpoiseapp
{
    public interface IArpoiseBehaviour
    {
        bool SendToRemote(string animationName);
        float? DurationStretchFactor { get; }
        void TimeSync();
    }

    public class ArBehaviourPosition : ArBehaviourMultiUser, IArpoiseBehaviour
    {
        #region Globals

        public GameObject ArCamera = null;
        public string ErrorMessage { get; set; }
        public float RecordingFrameRate = 0;
        #endregion

        #region Protecteds

        protected static readonly long InitialSecond = DateTime.Now.Ticks / 10000000L;

#if HAS_AR_CORE
        protected const string AppName = "AR-vos";
#else
#if HAS_AR_KIT
        protected const string AppName = "AR-vos";
#else
        protected const string AppName = "ARpoise";
#endif
#endif
        protected const float PositionTolerance = 1.25f;
        protected int AreaSize = 0;
        protected int AreaWidth = 0;

        protected float FilteredLongitude = 0;
        protected float FilteredLatitude = 0;
        protected bool ApplyKalmanFilter = true;

        protected float? FixedDeviceLatitude = null;
        protected float? FixedDeviceLongitude = null;
        protected float InitialHeading = 0;
        protected float Heading = 0;
        protected float CurrentHeading = 0;
        protected double LocationTimestamp = 0;
        protected float LocationHorizontalAccuracy = 0;
        protected float LocationLongitude = 0;
        protected float LocationLatitude = 0;
        protected float OriginalLatitude = 0;
        protected float OriginalLongitude = 0;
        protected DeviceOrientation InitialDeviceOrientation = DeviceOrientation.LandscapeLeft;

        protected bool CameraIsInitializing = true;

        public virtual bool InfoPanelIsActive()
        {
            return false;
        }

        protected float UsedLatitude
        {
            get
            {
                var latitude = FixedDeviceLatitude;
                if (latitude.HasValue)
                {
                    return latitude.Value;
                }
                return FilteredLatitude;
            }
        }

        protected float UsedLongitude
        {
            get
            {
                var longitude = FixedDeviceLongitude;
                if (longitude.HasValue)
                {
                    return longitude.Value;
                }
                return FilteredLongitude;
            }
        }

        #endregion

        #region Start
        protected override void Start()
        {
            NowTicks = DateTime.Now.Ticks;
            base.Start();
        }
        #endregion

        #region Update
        protected override void Update()
        {
            base.Update();
            if (RecordingFrameRate > 0)
            {
                // If Unity recording is used, one frame time elapses between two calls to Update
                //
                NowTicks += (long)(10000000L / RecordingFrameRate);
            }
            else
            {
                NowTicks = DateTime.Now.Ticks; // No recording, use time from system
            }
        }
        #endregion

        #region PlaceArObjects

        private float _handledLatitude = 0;
        private float _handledLongitude = 0;

        // Calculate positions for all ar objects
        private void PlaceArObjects(ArObjectState arObjectState)
        {
            var arObjectsToPlace = arObjectState.ArObjectsToPlace;
            if (arObjectsToPlace != null)
            {
                var filteredLatitude = UsedLatitude;
                var filteredLongitude = UsedLongitude;

                if (!arObjectsToPlace.Any(x => x.IsDirty) && _handledLatitude == filteredLatitude && _handledLongitude == filteredLongitude)
                {
                    return;
                }
                _handledLatitude = filteredLatitude;
                _handledLongitude = filteredLongitude;

                foreach (var arObject in arObjectsToPlace)
                {
                    arObject.IsDirty = false;
                    if (arObject.Poi.visibilityRange > 0)
                    {
                        var distance = CalculateDistance(arObject.Latitude, arObject.Longitude, filteredLatitude, filteredLongitude);
                        var isVisible = distance <= PositionTolerance * arObject.Poi.visibilityRange;
                        if (isVisible != arObject.WrapperObject.activeSelf)
                        {
                            arObject.WrapperObject.SetActive(isVisible);
                        }
                    }
                    var latDistance = CalculateDistance(arObject.Latitude, arObject.Longitude, filteredLatitude, arObject.Longitude);
                    var lonDistance = CalculateDistance(arObject.Latitude, arObject.Longitude, arObject.Latitude, filteredLongitude);

                    if (arObject.Latitude < UsedLatitude)
                    {
                        if (latDistance > 0)
                        {
                            latDistance *= -1;
                        }
                    }
                    else
                    {
                        if (latDistance < 0)
                        {
                            latDistance *= -1;
                        }
                    }
                    if (arObject.Longitude < UsedLongitude)
                    {
                        if (lonDistance > 0)
                        {
                            lonDistance *= -1;
                        }
                    }
                    else
                    {
                        if (lonDistance < 0)
                        {
                            lonDistance *= -1;
                        }
                    }

                    if (AreaSize <= 0 && AreaWidth > 0)
                    {
                        AreaSize = AreaWidth;
                    }
                    if (AreaSize > 0)
                    {
                        if (AreaWidth <= 0)
                        {
                            AreaWidth = AreaSize;
                        }

                        var halfWidth = AreaWidth / 2f;
                        while (lonDistance > 0 && lonDistance > halfWidth)
                        {
                            lonDistance -= AreaWidth;
                        }
                        while (lonDistance < 0 && lonDistance < -halfWidth)
                        {
                            lonDistance += AreaWidth;
                        }

                        var halfSize = AreaSize / 2f;
                        while (latDistance > 0 && latDistance > halfSize)
                        {
                            latDistance -= AreaSize;
                        }
                        while (latDistance < 0 && latDistance < -halfSize)
                        {
                            latDistance += AreaSize;
                        }
                        var distanceToAreaBorder = Mathf.Min(Mathf.Abs(Mathf.Abs(latDistance) - halfSize), Mathf.Abs(Mathf.Abs(lonDistance) - halfWidth));
                        if (distanceToAreaBorder < 1)
                        {
                            // The object is less than 1 meter from the border, scale it down with the distance it has
                            arObject.Scale = distanceToAreaBorder;
                        }
                        else
                        {
                            arObject.Scale = 1;
                        }
                    }
                    else
                    {
                        arObject.Scale = 1;
                    }
                    arObject.TargetPosition = new Vector3(lonDistance, arObject.RelativeAltitude, latDistance);
                }
            }
        }
        #endregion

        #region GetPosition

        private LocationService _locationService = null;

        // A Coroutine for retrieving the current location
        //
        protected IEnumerator GetPosition()
        {
#if QUEST_ARPOISE
            // If in quest mode, set a fixed initial location and forget about the location service
            //
            {
                // EOF
                //FilteredLatitude = OriginalLatitude = 49.020586f;
                //FilteredLongitude = OriginalLongitude = 12.09294f;
                // Ay Corona!
                //FilteredLatitude = OriginalLatitude = 48.158601475435f;
                //FilteredLongitude = OriginalLongitude = 11.580199727856f;

                // Quest Default
                FilteredLatitude = OriginalLatitude = 48.158f;
                FilteredLongitude = OriginalLongitude = -11.58f;

                Debug.Log("QUEST_ARPOISE fixed location, lat " + OriginalLatitude + ", lon " + OriginalLongitude);

                var second = DateTime.Now.Ticks / 10000000L;
                var random = new System.Random((int)second);
                var nextMove = second + 5 + random.Next(0, 5);

                while (second > 0)
                {
                    second = DateTime.Now.Ticks / 10000000L;
                    if (second >= nextMove)
                    {
                        nextMove = second + 5 + random.Next(0, 5);

                        FilteredLatitude = OriginalLatitude + 0.000001f * random.Next(-15, 15);
                        FilteredLongitude = OriginalLongitude + 0.000001f * random.Next(-12, 12);
                        Debug.Log("QUEST_ARPOISE new location, lat " + FilteredLatitude + ", lon " + FilteredLongitude);
                    }
                    var arObjectState = ArObjectState;
                    if (arObjectState != null)
                    {
                        PlaceArObjects(arObjectState);
                    }
                    yield return new WaitForSeconds(.1f);
                }
            }
            // End of quest mode
#endif
#if AndroidArvosU2021_3_Test
            //Test mode, set a fixed initial location and forget about the location service
            //
            {
                InitialHeading = 0;
                FilteredLatitude = OriginalLatitude = 48.158f;
                FilteredLongitude = OriginalLongitude = 11.58f;

                Debug.Log("Fixed location, lat " + OriginalLatitude + ", lon " + OriginalLongitude);

                var second = DateTime.Now.Ticks / 10000000L;
                var random = new System.Random((int)second);
                var nextMove = second + 5 + random.Next(0, 5);

                while (second > 0)
                {
                    var arObjectState = ArObjectState;
                    if (arObjectState != null)
                    {
                        PlaceArObjects(arObjectState);
                    }
                    yield return new WaitForSeconds(.1f);
                }
            }
            // End of quest mode
#endif
#if UNITY_EDITOR
            // If in editor mode, set a fixed initial location and forget about the location service
            //
            {
                // MUC-HDK
                //FilteredLatitude = OriginalLatitude = 48.144f;
                //FilteredLongitude = OriginalLongitude = 11.586f;

                // MUC-AINMILLER
                FilteredLatitude = OriginalLatitude = 48.158526f;
                FilteredLongitude = OriginalLongitude = 11.578670f;

                // R-Bahnhof
                //FilteredLatitude = OriginalLatitude = 49.012142f;
                //FilteredLongitude = OriginalLongitude = 12.098089f;

                // DC SmithS...
                //FilteredLatitude = OriginalLatitude = 38.888411f;
                //FilteredLongitude = OriginalLongitude = -77.024183f;

                Debug.Log("UNITY_EDITOR fixed location, lat " + OriginalLatitude + ", lon " + OriginalLongitude);

                var second = DateTime.Now.Ticks / 10000000L;
                var random = new System.Random((int)second);
                var nextMove = second + 90000 + random.Next(0, 6);

                while (second > 0 || second <= 0)
                {
                    second = DateTime.Now.Ticks / 10000000L;
                    //if (second >= nextMove)
                    //{
                    //    nextMove = second + 6 + random.Next(0, 6);
                    //    FilteredLatitude = OriginalLatitude + 0.00001f * random.Next(-5, 5);
                    //    FilteredLongitude = OriginalLongitude + 0.00001f * random.Next(-4, 4);
                    //    Debug.Log("UNITY_EDITOR new location, lat " + FilteredLatitude + ", lon " + FilteredLongitude);
                    //}
                    //FilteredLatitude += 0.00001F;
                    var arObjectState = ArObjectState;
                    if (arObjectState != null)
                    {
                        PlaceArObjects(arObjectState);
                    }
                    yield return new WaitForSeconds(1f);
                }
            }
            // End of editor mode
#endif
            int nFails = 0;
            bool doInitialize = true;
            while (string.IsNullOrWhiteSpace(ErrorMessage))
            {
                while (InfoPanelIsActive())
                {
                    yield return new WaitForSeconds(.01f);
                }

                if (doInitialize)
                {
                    doInitialize = false;
                    Input.compass.enabled = true;

                    int maxWait = 4500;
#if UNITY_ANDROID
                    bool first = true;
                    while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && maxWait > 0)
                    {
                        if (first)
                        {
                            Permission.RequestUserPermission(Permission.FineLocation);
                            first = false;
                        }
                        yield return new WaitForSeconds(.01f);
                        maxWait--;
                    }
#endif
                    if (_locationService == null)
                    {
                        _locationService = new LocationService();
                        maxWait = 1000;
                        while (!_locationService.isEnabledByUser && maxWait > 0)
                        {
                            yield return new WaitForSeconds(.01f);
                            maxWait--;
                        }
                        if (!_locationService.isEnabledByUser)
                        {
                            ErrorMessage = $"Please enable the location service for the {AppName} app!";
                            yield break;
                        }
                    }

                    Input.location.Start(.1f, .1f);
                    yield return new WaitForSeconds(.2f);

                    maxWait = 3000;
                    while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
                    {
                        yield return new WaitForSeconds(.01f);
                        maxWait--;
                    }

                    if (maxWait < 1)
                    {
                        ErrorMessage = "Location service didn't initialize in 30 seconds.";
                        yield break;
                    }

                    if (Input.location.status == LocationServiceStatus.Failed)
                    {
                        if (++nFails > 10)
                        {
                            ErrorMessage = $"Please enable the location service for {AppName} app.";
                            yield break;
                        }
                        Input.location.Stop();
                        doInitialize = true;
                        continue;
                    }

                    FilteredLatitude = OriginalLatitude = Input.location.lastData.latitude;
                    FilteredLongitude = OriginalLongitude = Input.location.lastData.longitude;

                    InitialDeviceOrientation = Input.deviceOrientation;
                }

                // For the first 500 milliseconds we remember the initial camera heading
                if (CameraIsInitializing && StartTicks > 0 && DateTime.Now.Ticks > StartTicks + 5000000)
                {
                    CameraIsInitializing = false;
                }
                CurrentHeading = Input.compass.trueHeading;

                var setLocation = true;
                if (PositionUpdateInterval > 0)
                {
                    var now = DateTime.Now;
                    if (_nextPositionUpdate < now)
                    {
                        _nextPositionUpdate = now.AddMilliseconds(PositionUpdateInterval * 1000f);
                    }
                    else
                    {
                        setLocation = false;
                    }
                }

                if (setLocation)
                {
                    if (LocationLatitude != Input.location.lastData.latitude
                        || LocationLongitude != Input.location.lastData.longitude
                        || LocationTimestamp != Input.location.lastData.timestamp
                        || LocationHorizontalAccuracy != Input.location.lastData.horizontalAccuracy
                    )
                    {
                        LocationLatitude = Input.location.lastData.latitude;
                        LocationLongitude = Input.location.lastData.longitude;
                        LocationTimestamp = Input.location.lastData.timestamp;
                        LocationHorizontalAccuracy = Input.location.lastData.horizontalAccuracy;

                        KalmanFilter(LocationLatitude, LocationLongitude, LocationHorizontalAccuracy, (long)(1000L * LocationTimestamp));
                    }
                }

                var arObjectState = ArObjectState;
                if (arObjectState != null)
                {
                    PlaceArObjects(arObjectState);
                }
                yield return new WaitForSeconds(.01f);
            }
            yield break;
        }

        private DateTime _nextPositionUpdate = DateTime.MinValue;
        protected float PositionUpdateInterval = 0;

        public float? DurationStretchFactor { get; private set; }

        private float _timeSync;
        protected void TimeSync(float timeSync)
        {
            _timeSync = timeSync;
            if (_timeSync > 0)
            {
                DurationStretchFactor = 1;
            }
        }

        public void TimeSync()
        {
            if (_timeSync <= 0)
            {
                DurationStretchFactor = null;
                return;
            }
            var syncTime = CurrentSecond % ((long)_timeSync);
            if (syncTime < 0.75 * _timeSync)
            {
                var restTime = _timeSync - syncTime;
                DurationStretchFactor = restTime / _timeSync;
            }
            else
            {
                var restTime = _timeSync - syncTime;
                DurationStretchFactor = (_timeSync + restTime) / _timeSync;
            }
        }

        protected int ApplicationSleepStartMinute = -1;
        protected int ApplicationSleepEndMinute = -1;
        protected bool ApplicationIsSleeping = false;
        protected int AllowTakeScreenshot = -1;

#if HAS_AR_FOUNDATION
        public void EnableOcclusion(ArLayer layer)
        {
            if (layer is not null)
            {
#if UNITY_IOS
                var occlusionManager = ArCamera.GetComponent<AROcclusionManager>();
                if (occlusionManager is not null)
                {
                    occlusionManager.requestedEnvironmentDepthMode = layer.OcclusionEnvironmentDepthMode;
                    occlusionManager.requestedOcclusionPreferenceMode = layer.OcclusionPreferenceMode;
                    occlusionManager.requestedHumanStencilMode = layer.OcclusionHumanSegmentationStencilMode;
                    occlusionManager.requestedHumanDepthMode = layer.OcclusionHumanSegmentationDepthMode;
                }
#endif
            }
        }
#endif
#endregion

        #region Misc
        protected virtual IEnumerator GetData()
        {
            ErrorMessage = "ArBehaviourPosition.GetData needs to be overridden";
            yield break;
        }

        // Calculates the distance between two sets of coordinates, taking into account the curvature of the earth
        protected float CalculateDistance(float lat1, float lon1, float lat2, float lon2)
        {
            //  The Haversine formula according to Dr. Math.
            //  http://mathforum.org/library/drmath/view/51879.html

            //  dlon = lon2 - lon1
            //  dlat = lat2 - lat1
            //  a = (sin(dlat/2))^2 + cos(lat1) * cos(lat2) * (sin(dlon/2))^2
            //  c = 2 * atan2(sqrt(a), sqrt(1-a)) 
            //  d = R * c

            //  Where
            //    * dlon is the change in longitude
            //    * dlat is the change in latitude
            //    * c is the great circle distance in Radians.
            //    * R is the radius of a spherical Earth.
            //    * The locations of the two points in 
            //        spherical coordinates (longitude and 
            //        latitude) are lon1,lat1 and lon2, lat2.

            double dLat1 = lat1 * (Math.PI / 180.0);
            double dLon1 = lon1 * (Math.PI / 180.0);
            double dLat2 = lat2 * (Math.PI / 180.0);
            double dLon2 = lon2 * (Math.PI / 180.0);

            double dLon = dLon2 - dLon1;
            double dLat = dLat2 - dLat1;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLat / 2.0), 2.0) +
                       Math.Cos(dLat1) * Math.Cos(dLat2) *
                       Math.Pow(Math.Sin(dLon / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            // Distance.
            return (float)Math.Abs(6367000 * c);
        }

        private long _timeStampInMilliseconds;
        private readonly double _qMetersPerSecond = 3;
        private double _variance = -1;

        // Kalman filter processing for latitude and longitude
        private void KalmanFilter(double currentLatitude, double currentLongitude, double accuracy, long timeStampInMilliseconds)
        {
            if (!ApplyKalmanFilter)
            {
                FilteredLatitude = (float)currentLatitude;
                FilteredLongitude = (float)currentLongitude;
                return;
            }

            if (accuracy < 1)
            {
                accuracy = 1;
            }
            if (_variance < 0)
            {
                // if variance < 0, the object is unitialised, so initialise it with current values
                _timeStampInMilliseconds = timeStampInMilliseconds;
                FilteredLatitude = (float)currentLatitude;
                FilteredLongitude = (float)currentLongitude;
                _variance = accuracy * accuracy;
            }
            else
            {
                // apply Kalman filter
                long timeIncreaseInMilliseconds = timeStampInMilliseconds - _timeStampInMilliseconds;
                if (timeIncreaseInMilliseconds > 0)
                {
                    // time has moved on, so the uncertainty in the current position increases
                    _variance += timeIncreaseInMilliseconds * _qMetersPerSecond * _qMetersPerSecond / 1000;
                    _timeStampInMilliseconds = timeStampInMilliseconds;
                    // TO DO: USE VELOCITY INFORMATION HERE TO GET A BETTER ESTIMATE OF CURRENT POSITION
                }

                // Kalman gain matrix K = Covariance * Inverse(Covariance + MeasurementVariance)
                // NB: because K is dimensionless, it doesn't matter that variance has different units to lat and lon
                double k = _variance / (_variance + accuracy * accuracy);
                // apply K
                FilteredLatitude += (float)(k * (currentLatitude - FilteredLatitude));
                FilteredLongitude += (float)(k * (currentLongitude - FilteredLongitude));
                // new Covarariance  matrix is (IdentityMatrix - K) * Covariance 
                _variance = (1 - k) * _variance;
            }
        }
#endregion
    }
}