/*
ArBehaviour.cs - MonoBehaviour for ARpoise.

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
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public class ArBehaviour : ArBehaviourUserInterface
    {
        #region Awake

        public static ArBehaviour Instance { get; private set; }

        public virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("ArBehaviour Awake() set Instance");

                Application.deepLinkActivated += onDeepLinkActivated;
                if (!string.IsNullOrEmpty(Application.absoluteURL))
                {
                    // Cold start and Application.absoluteURL not null so process Deep Link.
                    onDeepLinkActivated(Application.absoluteURL);
                }
                // Initialize DeepLink Manager global variable.
                else
                {
                    DeeplinkURL = string.Empty;
                }
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void onDeepLinkActivated(string url)
        {
            // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
            DeeplinkURL = url;
            Debug.Log("DeeplinkURL " + DeeplinkURL);

            // Decode the URL to determine action.
            // In this example, the application expects a link formatted like this:
            // arpoisedeeplink://arpoise?DeeplinkName
            var parts = url.Split('?');
            if (parts.Length > 1 )
            {
                DeeplinkName = parts[1].Trim();
            }
            else
            {
                DeeplinkName = string.Empty;
            }
            DeeplinkChanged = true;
            Debug.Log("DeeplinkName " + DeeplinkName);
        }
        #endregion

        #region Start
        protected override void Start()
        {
            base.Start();

#if QUEST_ARPOISE
            Debug.Log("QUEST_ARPOISE Start");
#endif
#if UNITY_EDITOR
            Debug.Log("UNITY_EDITOR Start");
#endif
            StartCoroutine(nameof(GetPosition));
            StartCoroutine(nameof(GetData));
            StartCoroutine(nameof(CheckWebRequestsRoutine));
        }
        #endregion

        #region Update
        private long _lastSecond = -1;
        protected override void Update()
        {
            var now = DateTime.Now;
            var minute = now.Hour * 60 + now.Minute;

            var shouldNotSleep = ApplicationSleepStartMinute < 0 || ApplicationSleepEndMinute < 0
                   || (ApplicationSleepStartMinute <= ApplicationSleepEndMinute && (minute < ApplicationSleepStartMinute || minute >= ApplicationSleepEndMinute))
                   || (ApplicationSleepStartMinute > ApplicationSleepEndMinute && (minute < ApplicationSleepStartMinute && minute >= ApplicationSleepEndMinute));
            if (shouldNotSleep)
            {
                if (ApplicationIsSleeping)
                {
                    ApplicationIsSleeping = false;
                    ArObjectState?.HandleApplicationSleep(false);
                }
            }
            else
            {
                if (!ApplicationIsSleeping)
                {
                    ApplicationIsSleeping = true;
                    ArObjectState?.HandleApplicationSleep(true);
                }
            }

            if (ApplicationIsSleeping)
            {
                var second = now.Ticks / TimeSpan.TicksPerSecond;
                if (second == _lastSecond)
                {
                    return;
                }
                _lastSecond = second;
                ArObjectState?.HandleApplicationSleep(true);
            }

            base.Update();
        }
        #endregion
    }
}
