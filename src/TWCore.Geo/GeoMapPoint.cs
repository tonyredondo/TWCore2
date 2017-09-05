/*
Copyright 2015-2017 Daniel Adrian Redondo Suarez

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

using System;
using System.Runtime.CompilerServices;

namespace TWCore.Geo
{
    /// <summary>
    /// Defines a GeoMap point
    /// </summary>
    public class GeoMapPoint
    {
        private const int EarthRadius = 6371;
        private double _latitude, _longitude;

        #region Properties
        /// <summary>
        /// Latitude in degrees. -90 to 90
        /// </summary>
        public double Latitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _latitude; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value > 90) throw new ArgumentOutOfRangeException("value", "Latitude value must be <= 90");
                if (value < -90) throw new ArgumentOutOfRangeException("value", "Latitude value must be >= -90");
                _latitude = value;
            }
        }

        /// <summary>
        /// Longitude in degree. -180 to 180
        /// </summary>
        public double Longitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _longitude; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value > 180) throw new ArgumentOutOfRangeException("value", "Longitude value must be <= 180");
                if (value < -180) throw new ArgumentOutOfRangeException("value", "Longitude value must be >= -180");
                _longitude = value;
            }
        }
        #endregion

        #region .ctor
        /// <summary>
        /// Defines a GeoMap point
        /// </summary>
        /// <param name="latitude">Latitude value</param>
        /// <param name="longitude">Logintude value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GeoMapPoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the distance in kilometers between two GeoMapPoint
        /// </summary>
        /// <param name="point">GeoMapPoint to calculate the distance</param>
        /// <returns>Distance between both map points</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetDistanceInKm(GeoMapPoint point)
        {
            var dLat = (point.Latitude - Latitude).ToRad();
            var dLon = (point.Longitude - Longitude).ToRad();

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(Latitude.ToRad()) * Math.Cos(point.Latitude.ToRad()) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadius * c;
        }

        /// <summary>
        /// Gets the distance in miles between two GeoMapPoint
        /// </summary>
        /// <param name="point">GeoMapPoint to calculate the distance</param>
        /// <returns>Distance between both map points</returns>
        public double GetDistanceInMiles(GeoMapPoint point) => GetDistanceInKm(point) * 0.62;

        /// <summary>
        /// Gets the bearing from the current GeoMapPoint to another (the azimuth)
        /// </summary>
        /// <param name="point">GeoMapPoint to calculate the bearing</param>
        /// <returns>Bearing between both map points</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetBearing(GeoMapPoint point)
        {
            var dLon = (point.Longitude - Longitude).ToRad();

            var y = Math.Sin(dLon) * Math.Cos(point.Latitude.ToRad());
            var x = Math.Cos(Latitude.ToRad()) * Math.Sin(point.Latitude.ToRad()) -
                       Math.Sin(Latitude.ToRad()) * Math.Cos(point.Latitude.ToRad()) * Math.Cos(dLon);
            return Math.Atan2(y, x).ToBearing();
        }

        /// <summary>
        /// Gets the cardinal point to another GeoMapPoint using the bearing.
        /// </summary>
        /// <param name="point">GeoMapPoint to calculate the cardinal point</param>
        /// <returns>Cardinal point between both map points</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetCardinalPoint(GeoMapPoint point)
        {
            var bearing = GetBearing(point);
            if (bearing >= 0 && bearing < 22.5) return "N";
            if (bearing >= 22.5 && bearing < 67.5) return "NE";
            if (bearing >= 67.5 && bearing < 112.5) return "E";
            if (bearing >= 112.5 && bearing < 157.5) return "SE";
            if (bearing >= 157.5 && bearing < 202.5) return "S";
            if (bearing >= 202.5 && bearing < 247.5) return "SW";
            if (bearing >= 247.5 && bearing < 292.5) return "W";
            if (bearing >= 292.5 && bearing < 337.5) return "NW";
            if (bearing >= 337.5 && bearing < 360.1) return "N";
            return string.Empty;
        }
        #endregion
    }
}