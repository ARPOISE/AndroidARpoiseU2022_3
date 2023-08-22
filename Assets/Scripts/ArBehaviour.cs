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

#if UNITY_IOS_unused
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);
            DeepLinkReceiverIsAlive(); // Let the App Controller know it's ok to call URLOpened now.
#endif
            StartCoroutine(nameof(GetPosition));
            StartCoroutine(nameof(GetData));
            StartCoroutine(nameof(TakeScreenshotRoutine));
        }
        #endregion

        #region Update
        private long _lastSecond = -1;
        protected override void Update()
        {
            var minute = DateTime.Now.Hour * 60 + DateTime.Now.Minute;

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
                var second = DateTime.Now.Ticks / 10000000L;
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

        #region iOS deep link

#if UNITY_IOS_unused
        public string LinkUrl;

        [DllImport("__Internal")]
        private static extern void DeepLinkReceiverIsAlive();
        [System.Serializable]
        public class StringEvent : UnityEvent { }
        public StringEvent urlOpenedEvent;
        public bool dontDestroyOnLoad = true;

        public void URLOpened(string url)
        {
            LinkUrl = url;
            Debug.Log("Link url" + LinkUrl);
        }
#endif
        #endregion
    }
}
