﻿/*
ArBehaviourImage.cs - MonoBehaviour for ARpoise image handling.

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

using System;
using UnityEngine;

namespace com.arpoise.arpoiseapp
{
    public class ArBehaviourImage : ArBehaviour
    {
        #region Globals

        public GameObject FitToScanOverlay;

        #endregion

        #region Start
        protected override void Start()
        {
            base.Start();

#if iOsArvosU2022_3
            if (InfoPanel != null)
            {
                var showInfoPanel = PlayerPrefs.GetString(nameof(InfoPanelIsActive));
                if (!false.ToString().Equals(showInfoPanel))
                {
                    var infoPanel = InfoPanel.GetComponent<InfoPanel>();
                    if (infoPanel != null)
                    {
                        infoPanel.Setup(this);
                        InfoPanel.SetActive(true);
                    }
                }
            }
#endif
        }
        #endregion

        #region Update
        protected override void Update()
        {
            base.Update();

            if ((IsHumanBody || IsSlam) && FitToScanOverlay != null && FitToScanOverlay.activeSelf)
            {
                FitToScanOverlay.SetActive(false);
            }
        }
        #endregion
    }
}
