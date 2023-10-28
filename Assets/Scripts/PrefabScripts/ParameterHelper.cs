/*
ParameterHelper.cs - A static helper script.

Copyright (C) 2023, Tamiko Thiel and Peter Graf - All Rights Reserved

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
using System.Collections.Generic;
using UnityEngine;

public static class ParameterHelper
{
    public static int? SetParameter(bool setValue, string value, int? defaultValue)
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
    public static float? SetParameter(bool setValue, string value, float? defaultValue)
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
}
