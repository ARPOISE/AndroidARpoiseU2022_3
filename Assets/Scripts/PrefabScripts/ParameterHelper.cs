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
using System.Globalization;
using System.Linq;
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
            if (float.TryParse(value.Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out floatValue))
            {
                return floatValue;
            }
        }
        return defaultValue;
    }
    public static bool? SetParameter(bool setValue, string value, bool? defaultValue)
    {
        if (setValue && !string.IsNullOrWhiteSpace(value))
        {
            bool boolValue;
            if (bool.TryParse(value, out boolValue))
            {
                return boolValue;
            }
        }
        return defaultValue;
    }
    public static Vector3? SetParameter(bool setValue, string value, Vector3? defaultValue)
    {
        if (setValue && !string.IsNullOrWhiteSpace(value))
        {
            Vector3 vector3Value;
            if (TryParse(value, out vector3Value))
            {
                return vector3Value;
            }
        }
        return defaultValue;
    }
    public static bool TryParse(string input, out Vector3 value)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            float floatValue;
            var values = input.Split(',').Select(s => s.Trim()).ToArray();
            var x = (float)(values.Length > 0 && float.TryParse(values[0].Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out floatValue) ? floatValue : 0);
            var y = (float)(values.Length > 1 && float.TryParse(values[1].Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out floatValue) ? floatValue : 0);
            var z = (float)(values.Length > 2 && float.TryParse(values[2].Trim().Replace("+", string.Empty), NumberStyles.Float, CultureInfo.InvariantCulture, out floatValue) ? floatValue : 0);
            value = new Vector3(x, y, z);
            return true;
        }
        value = Vector3.zero;
        return false;
    }

    public static string ToString(Vector3 value)
    {
        return $"{value.x.ToString("F1", CultureInfo.InvariantCulture)}, {value.y.ToString("F1", CultureInfo.InvariantCulture)}, {value.z.ToString("F1", CultureInfo.InvariantCulture)}";
    }
}
